using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using IntelHexReader;
using MadWizard.WinUSBNet;

namespace DfuFwUpdater_cli
{
    class CommandLineOptions
    {
        [Option("about", HelpText = "Display About Information")]
        public bool PrintAbout { get; set; }

        [Option('l', "list", HelpText = "List the DFU devices currently connected.")]
        public bool ListDevices { get; set; }

        [Option('D', "download", HelpText = "FILE\nWrite firmware from FILE into device.")]
        public string DownloadFileName { get; set; }

        [Option("address", Default = "08000000", HelpText = "ADDRESS\nSpecify target address (hex format) for raw binary download on device.")]
        public string Address { get; set; }
    }

    class Program
    {
        static int exitCode = 0;
        static int Main(string[] args)
        {
            Parser.Default.ParseArguments<CommandLineOptions>(args).WithParsed(Run);

            return exitCode;
        }

        private static List<FirmwareBlock> IntelHexContent2Blocks(IntelHexContent content)
        {
            List<FirmwareBlock> blocks = new List<FirmwareBlock>();

            uint min_address = content.MemoryCells.Keys.Min();
            uint max_address = content.MemoryCells.Keys.Max();

            List<byte> blk_data_list = new List<byte>();
            uint blk_start_addr = 0;
            bool flag_in_blk = false;
            for (uint addr = min_address; addr <= max_address; addr++)
            {
                if (content.MemoryCells.ContainsKey(addr))
                {
                    if (flag_in_blk == false)
                    {
                        blk_start_addr = addr;
                        flag_in_blk = true;
                    }
                    blk_data_list.Add(content.MemoryCells[addr]);
                }

                if (((content.MemoryCells.ContainsKey(addr) == false) || (addr == max_address)) && flag_in_blk)
                {
                    byte[] blk_data = blk_data_list.ToArray();

                    blocks.Add(new FirmwareBlock(blk_start_addr, blk_data));

                    blk_data_list.Clear();
                    flag_in_blk = false;
                }
            }

            return blocks;
        }

        private static void Run(CommandLineOptions options)
        {
            if (options.PrintAbout)
            {
                Console.WriteLine(AboutInfo.Information);
                return;
            }

            if (options.ListDevices)
            {
                var deviceInfos = DfuDevice.GetAllDevicesInfo();
                if (deviceInfos.Count == 0)
                    Console.WriteLine("No DFU device found!");
                else
                {
                    foreach (var deviceInfo in deviceInfos)
                    {
                        Console.WriteLine($"Found DFU: [{deviceInfo.VID:X4}:{deviceInfo.PID:X4}], path={deviceInfo.DevicePath}");
                    }
                }
                return;
            }

            if (options.DownloadFileName == null)
            {
                Console.WriteLine("No operation executed.");
                return;
            }

            DfuDevice dfuDevice = null;

            try
            {
                uint address = Convert.ToUInt32(options.Address, 16);

                if (!File.Exists(options.DownloadFileName))
                    throw new ApplicationException("The specified file is not exist!");

                var deviceInfos = DfuDevice.GetAllDevicesInfo();
                if (deviceInfos.Count == 0)
                    throw new ApplicationException("No DFU device found!");

                USBDeviceInfo deviceInfo = deviceInfos[0];
                dfuDevice = new DfuDevice(deviceInfo);

                Console.Write($"Open {options.DownloadFileName} ...");
                List<FirmwareBlock> fwBlocks = null;
                FlashDataBuilder flashDataBuilder = new FlashDataBuilder(256, 256);
                if (Path.GetExtension(options.DownloadFileName).ToLower() == ".hex")
                {
                    Console.WriteLine("(Hex File)");
                    HexFileReader reader = new HexFileReader(options.DownloadFileName);
                    IntelHexContent hexContent = reader.Parse();

                    fwBlocks = IntelHexContent2Blocks(hexContent);

                    flashDataBuilder.AddData(hexContent.MemoryCells);
                }
                else
                {
                    Console.WriteLine("(Bin File)");
                    byte[] fileContent = File.ReadAllBytes(options.DownloadFileName);

                    fwBlocks = new List<FirmwareBlock>();
                    fwBlocks.Add(new FirmwareBlock(address, fileContent));

                    flashDataBuilder.AddData(address, fileContent);
                }

                uint min_address = flashDataBuilder.PageList.First().Key;
                var kv_last = flashDataBuilder.PageList.Last();
                uint max_address = kv_last.Key + kv_last.Value.data_length - 1;

                byte[] info_data = dfuDevice.GetInfo(0x00);
                byte usb_bootloader_ver = info_data[0];
                uint chip_id = ByteHelper.GetUInt32(info_data, 1);
                uint flash_size = ByteHelper.GetUInt32(info_data, 5);
                uint sram_size = ByteHelper.GetUInt32(info_data, 9);

                if ((chip_id & 0x3FFF) != 0x2980)
                    throw new ApplicationException($"Unknown Chip (0x{chip_id:X8}), Maybe use the latest software to solve the problem");

                if ((min_address < 0x08000000u) ||
                    (max_address > (0x08000000u + flash_size - 1)))
                {
                    throw new ApplicationException($"The content out of flash address range. The flash address range is 0x08000000-0x{(0x08000000u + flash_size - 1):X8}");
                }

                Console.WriteLine("Start Download...");
                Console.Write("Erase Chip...");
                dfuDevice.EraseChip();
                Console.WriteLine("OK");

                Console.Write("Writing...");
                foreach (var kv in flashDataBuilder.PageList)
                {
                    uint page_addr = kv.Key;
                    byte[] page_data_buf = kv.Value.buffer;
                    uint page_data_len = kv.Value.data_length;
                    page_data_len = ((page_data_len + 3) / 4) * 4;
                    dfuDevice.ProgramPage(page_addr, page_data_buf, 0, (int)page_data_len);
                }
                Console.WriteLine("OK");

                Console.Write("Verifying...");
                foreach (var block in fwBlocks)
                {
                    byte[] read_data = dfuDevice.ReadData(block.StartAddress, (uint)block.Data.Length);
                    for (int i = 0; i < block.Data.Length; i++)
                    {
                        if (read_data[i] != block.Data[i])
                        {
                            throw new ApplicationException($"Check Failed in address 0x{block.StartAddress + i:X8}");
                        }
                    }
                }
                Console.WriteLine("OK");

                Console.WriteLine("Download completed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR : {ex.Message}");
                exitCode = -1;
            }
            finally
            {
                if (dfuDevice != null)
                {
                    dfuDevice.Dispose();
                    dfuDevice = null;
                }
            }
        }
    }
}

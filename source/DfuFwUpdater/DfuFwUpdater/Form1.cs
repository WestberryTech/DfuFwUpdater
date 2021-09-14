using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IntelHexReader;
using MadWizard.WinUSBNet;

namespace DfuFwUpdater
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            btnSearchDevices_Click(sender, e);
        }

        private void btnSearchDevices_Click(object sender, EventArgs e)
        {
            var deviceInfos = DfuDevice.GetAllDevicesInfo();
            cbxDevices.Items.Clear();
            foreach (var info in deviceInfos)
            {
                cbxDevices.Items.Add(new NamedObject(info.DeviceDescription, info));
            }
            if ((cbxDevices.SelectedIndex < 0) && (cbxDevices.Items.Count > 0))
                cbxDevices.SelectedIndex = 0;
        }

        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select Hex file";
            ofd.Filter = "HEX File(*.hex)|*.hex";
            ofd.Multiselect = false;
            ofd.ValidateNames = true;
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                tboxFwFilePath.Text = ofd.FileName;
            }
        }

        private List<FirmwareBlock> IntelHexContent2Blocks(IntelHexContent content)
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

        private void StartDownload(USBDeviceInfo deviceInfo)
        {
            DfuDevice dfuDevice = null;
            try
            {
                dfuDevice = new DfuDevice(deviceInfo);

                this.Invoke(new Action(() => tboxLog.AppendText($"Open {tboxFwFilePath.Text} ...\r\n")));
                HexFileReader reader = new HexFileReader(tboxFwFilePath.Text);
                IntelHexContent hexContent = reader.Parse();

                List<FirmwareBlock> hexBlocks = IntelHexContent2Blocks(hexContent);
                FlashDataBuilder flashDataBuilder = new FlashDataBuilder(256, 256);
                flashDataBuilder.AddData(hexContent.MemoryCells);

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
                    throw new ApplicationException($"The content out of flash address range.\r\nThe flash address range is 0x08000000-0x{(0x08000000u + flash_size - 1):X8}");
                }


                this.Invoke(new Action(() => tboxLog.AppendText("Start Download...\r\n")));

                if (chkBoxEraseChip.Checked)
                {
                    this.Invoke(new Action(() => tboxLog.AppendText("Erase Chip...")));
                    dfuDevice.EraseChip();
                    this.Invoke(new Action(() => tboxLog.AppendText("OK\r\n")));
                }
                else
                {
                    this.Invoke(new Action(() => progressBar1.Value = 0));
                    this.Invoke(new Action(() => progressBar1.Maximum = flashDataBuilder.SectorList.Count));
                    this.Invoke(new Action(() => tboxLog.AppendText("Erase Sectors...")));
                    foreach (var kv in flashDataBuilder.SectorList)
                    {
                        uint erase_addr = kv.Key;
                        dfuDevice.ErasePages(erase_addr, 1);
                        this.Invoke(new Action(() => progressBar1.Value += 1));
                    }
                    this.Invoke(new Action(() => tboxLog.AppendText("OK\r\n")));
                }

                this.Invoke(new Action(() => progressBar1.Value = 0));
                this.Invoke(new Action(() => progressBar1.Maximum = flashDataBuilder.PageList.Count));
                this.Invoke(new Action(() => tboxLog.AppendText("Writing...")));
                foreach (var kv in flashDataBuilder.PageList)
                {
                    uint page_addr = kv.Key;
                    byte[] page_data_buf = kv.Value.buffer;
                    uint page_data_len = kv.Value.data_length;
                    page_data_len = ((page_data_len + 3) / 4) * 4;
                    dfuDevice.ProgramPage(page_addr, page_data_buf, 0, (int)page_data_len);
                    this.Invoke(new Action(() => progressBar1.Value += 1));
                }
                this.Invoke(new Action(() => tboxLog.AppendText("OK\r\n")));

                if (chkBoxVerify.Checked)
                {
                    this.Invoke(new Action(() => tboxLog.AppendText("Verifying...")));
                    foreach (var block in hexBlocks)
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
                    this.Invoke(new Action(() => tboxLog.AppendText("OK\r\n")));
                }

                this.Invoke(new Action(() => tboxLog.AppendText("Download completed.\r\n\r\n")));
            }
            catch (Exception ex)
            {
                this.Invoke(new Action(() => this.tboxLog.AppendText($"ERROR : {ex.Message}\r\n")));
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

        private async void btnDownload_Click(object sender, EventArgs e)
        {
            btnDownload.Enabled = false;
            btnSelectFile.Enabled = false;
            if (cbxDevices.SelectedIndex >= 0)
            {
                USBDeviceInfo deviceInfo = (USBDeviceInfo)(((NamedObject)cbxDevices.SelectedItem).Instance);
                await Task.Run(() => StartDownload(deviceInfo));
            }
            else
            {
                tboxLog.AppendText("Error: No DFU Device selected.\r\n");
            }
            btnDownload.Enabled = true;
            btnSelectFile.Enabled = true;
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            AboutBox1 aboutBox = new AboutBox1();
            aboutBox.ShowDialog();
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

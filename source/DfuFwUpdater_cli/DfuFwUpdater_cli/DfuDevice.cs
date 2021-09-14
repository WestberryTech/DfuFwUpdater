using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MadWizard.WinUSBNet;

namespace DfuFwUpdater_cli
{
    class DfuDevice : IDisposable
    {
        private USBDevice devInst = null;
        private bool disposed = false;

        public DfuDevice(USBDeviceInfo device_info)
        {
            devInst = new USBDevice(device_info);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                if (devInst != null)
                {
                    devInst.Dispose();
                    devInst = null;
                }
            }

            disposed = true;
        }

        ~DfuDevice()
        {
            Dispose(false);
        }

        public static List<USBDeviceInfo> GetAllDevicesInfo()
        {
            var devices_info = new List<USBDeviceInfo>();
            foreach (var dev_info in USBDevice.GetDevices("{89B0FDF0-3D22-4408-8393-32147BA508CE}"))
            {
                if ((dev_info.DeviceDescription == "WB Device in DFU Mode") && (dev_info.VID == 0x342d) && (dev_info.PID == 0xdfa0))
                {
                    devices_info.Add(dev_info);
                }
            }

            return devices_info;
        }

        private byte[] CommandTransfer(byte[] command)
        {
            Debug.WriteLine($"CommandTransfer command {BitConverter.ToString(command)}");
            USBInterface iface = devInst.Interfaces[0];
            iface.OutPipe.Write(command);
            if (command.Length % iface.OutPipe.MaximumPacketSize == 0)
            {
                iface.OutPipe.Write(new byte[1], 0, 0);     // 如果要发送的数据长度是端点最大包长的整数倍，则需要再发送一个0长度的数据包
            }
            byte[] buffer = new byte[4096];
            int bytesRead = iface.InPipe.Read(buffer);
            byte[] response = new byte[bytesRead];
            for (int i = 0; i < bytesRead; i++)
                response[i] = buffer[i];
            Debug.WriteLine($"CommandTransfer response {BitConverter.ToString(response)}");
            if (response[0] != command[0])
                throw new ApplicationException($"COMMAND 0x{command[0]:X2} RESPONSE ERROR（0x{response[0]:X2}）！！！");
            return response;
        }

        public byte[] GetInfo(byte info_id)
        {
            byte[] command = new byte[2] { 0x01, info_id };
            byte[] response = CommandTransfer(command);
            if (response[1] != 0x00)
                throw new ApplicationException($"DFU_CMD_GET_INFO COMMAND FAILED! (ERROR CODE: 0x{response[1]:X2})");
            byte[] info = new byte[response[2]];
            Array.Copy(response, 3, info, 0, info.Length);
            return info;
        }

        public void Erase(byte method, uint page_addr, uint num_pages)
        {
            byte[] command = new byte[12];
            command[0] = 0x5E;
            command[1] = method;
            command[2] = 0x00;
            command[3] = 0x00;
            ByteHelper.ConvertToBytes(page_addr, command, 4);
            ByteHelper.ConvertToBytes(num_pages, command, 8);
            byte[] response = CommandTransfer(command);
            if (response.Length != 2)
                throw new ApplicationException($"DFU_CMD_ERASE COMMAND RESPONSE LENGTH ERROR! ({response.Length})");
            if (response[1] != 0x00)
                throw new ApplicationException($"DFU_CMD_ERASE COMMAND FAILED! (ERROR CODE: 0x{response[1]:X2})");
        }

        public void EraseChip()
        {
            Erase(0xF0, 0x00, 0);
        }

        public void ErasePages(uint page_addr, uint num_pages)
        {
            Erase(0x00, page_addr, num_pages);
        }

        public void ProgramPage(uint page_addr, byte[] data, int offset, int count)
        {
            byte[] command = new byte[12 + count];
            command[0] = 0x6A;
            command[1] = 0x00;
            command[2] = 0x00;
            command[3] = 0x00;
            ByteHelper.ConvertToBytes(page_addr, command, 4);
            ByteHelper.ConvertToBytes((uint)count, command, 8);
            for (int idx = 0; idx < count; idx++)
                command[12 + idx] = data[offset + idx];
            byte[] response = CommandTransfer(command);
            if (response.Length != 2)
                throw new ApplicationException($"DFU_CMD_PROGRAM_PAGE COMMAND RESPONSE LENGTH ERROR! ({response.Length})");
            if (response[1] != 0x00)
                throw new ApplicationException($"DFU_CMD_PROGRAM_PAGE COMMAND FAILED! (ERROR CODE: 0x{response[1]:X2})");
        }

        private void CmdRead(uint addr, byte[] buffer, int offset, ushort count)
        {
            byte[] command = new byte[10];
            command[0] = 0x72;
            command[1] = 0x00;
            command[2] = 0x00;
            command[3] = 0x00;
            ByteHelper.ConvertToBytes((uint)addr, command, 4);
            ByteHelper.ConvertToBytes((ushort)count, command, 8);
            byte[] response = CommandTransfer(command);
            if (response[1] != 0x00)
                throw new ApplicationException($"DFU_CMD_READ COMMAND FAILED! (ERROR CODE: 0x{response[1]:X2})");

            Array.Copy(response, 2, buffer, offset, count);
        }

        public byte[] ReadData(uint address, uint count, Action<float> progress_cb = null)
        {
            byte[] buffer = new byte[count];
            if (progress_cb == null)
                progress_cb = (float x) => { return; };
            progress_cb(0.0f);
            uint index = 0;
            while (index < count)
            {
                uint read_len = count - index;
                if (read_len > 256)
                    read_len = 256;

                CmdRead(address + index, buffer, (int)index, (ushort)read_len);
                index += read_len;
                progress_cb(index * 1.0f / count);
            }
            return buffer;
        }

        public void Reset(uint delay_ms)
        {
            byte[] command = new byte[8];
            command[0] = 0x83;
            command[1] = 0x00;
            command[2] = 0x00;
            command[3] = 0x00;
            ByteHelper.ConvertToBytes(delay_ms, command, 4);
            byte[] response = CommandTransfer(command);
            if (response.Length != 2)
                throw new ApplicationException($"DFU_CMD_RESET COMMAND RESPONSE LENGTH ERROR! ({response.Length})");
            if (response[1] != 0x00)
                throw new ApplicationException($"DFU_CMD_RESET COMMAND FAILED! (ERROR CODE: 0x{response[1]:X2})");
        }

        public void Go(uint address, uint delay_ms)
        {
            byte[] command = new byte[12];
            command[0] = 0x9B;
            command[1] = 0x00;
            command[2] = 0x00;
            command[3] = 0x00;
            ByteHelper.ConvertToBytes(address, command, 4);
            ByteHelper.ConvertToBytes(delay_ms, command, 8);
            byte[] response = CommandTransfer(command);
            if (response.Length != 2)
                throw new ApplicationException($"DFU_CMD_GO COMMAND RESPONSE LENGTH ERROR! ({response.Length})");
            if (response[1] != 0x00)
                throw new ApplicationException($"DFU_CMD_GO COMMAND FAILED! (ERROR CODE: 0x{response[1]:X2})");
        }

    }
}

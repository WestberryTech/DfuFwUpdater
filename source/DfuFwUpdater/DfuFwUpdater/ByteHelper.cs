using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DfuFwUpdater
{
    public static class ByteHelper
    {
        public static void ConvertToBytes(uint value, byte[] buffer, int offset, bool bigEndian = false)
        {
            if (bigEndian)
            {
                buffer[offset] = (byte)(value >> 24);
                buffer[offset + 1] = (byte)(value >> 16);
                buffer[offset + 2] = (byte)(value >> 8);
                buffer[offset + 3] = (byte)value;
            }
            else
            {
                buffer[offset] = (byte)value;
                buffer[offset + 1] = (byte)(value >> 8);
                buffer[offset + 2] = (byte)(value >> 16);
                buffer[offset + 3] = (byte)(value >> 24);
            }
        }

        public static void ConvertToBytes(ushort value, byte[] buffer, int offset, bool bigEndian = false)
        {
            if (bigEndian)
            {
                buffer[offset] = (byte)(value >> 8);
                buffer[offset + 1] = (byte)value;
            }
            else
            {
                buffer[offset] = (byte)value;
                buffer[offset + 1] = (byte)(value >> 8);
            }
        }

        public static byte[] GetBytes(uint value, bool bigEndian = false)
        {
            byte[] buffer = new byte[4];
            if (bigEndian)
            {
                buffer[0] = (byte)(value >> 24);
                buffer[1] = (byte)(value >> 16);
                buffer[2] = (byte)(value >> 8);
                buffer[3] = (byte)value;
            }
            else
            {
                buffer[0] = (byte)value;
                buffer[1] = (byte)(value >> 8);
                buffer[2] = (byte)(value >> 16);
                buffer[3] = (byte)(value >> 24);
            }
            return buffer;
        }

        public static uint GetUInt32(byte[] buffer, int offset, bool bigEndian = false)
        {
            uint value = 0;
            if (bigEndian)
            {
                value |= (uint)buffer[offset] << 24;
                value |= (uint)buffer[offset + 1] << 16;
                value |= (uint)buffer[offset + 2] << 8;
                value |= (uint)buffer[offset + 3];
                return value;
            }
            else
            {
                value |= (uint)buffer[offset];
                value |= (uint)buffer[offset + 1] << 8;
                value |= (uint)buffer[offset + 2] << 16;
                value |= (uint)buffer[offset + 3] << 24;
                return value;
            }
        }
    }
}

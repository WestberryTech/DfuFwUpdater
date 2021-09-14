using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DfuFwUpdater
{
    public class FirmwareBlock
    {
        public uint StartAddress { get; }
        public byte[] Data { get; }

        public FirmwareBlock(uint start_addr, byte[] data)
        {
            this.StartAddress = start_addr;
            this.Data = data;
        }
    }
}

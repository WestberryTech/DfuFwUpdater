using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DfuFwUpdater_cli
{
    class FlashDataBuilder
    {
        public class PageData
        {
            public byte[] buffer;
            public uint data_length;
            public PageData(byte[] buffer, uint data_length)
            {
                this.buffer = buffer;
                this.data_length = data_length;
            }
        }
        public SortedList<uint, object> SectorList { get; private set; }
        public SortedList<uint, PageData> PageList { get; private set; }
        public uint SectorSize { get; private set; }
        public uint PageSize { get; private set; }

        public FlashDataBuilder(uint sector_size, uint page_size)
        {
            this.SectorSize = sector_size;
            this.PageSize = page_size;
            this.SectorList = new SortedList<uint, object>();
            this.PageList = new SortedList<uint, PageData>();
        }

        public void AddData(uint addr, byte data)
        {
            uint sector_addr = addr - (addr % this.SectorSize);
            if (!SectorList.ContainsKey(sector_addr))
            {
                SectorList.Add(sector_addr, null);
            }

            uint page_addr = addr - (addr % this.PageSize);
            if (!PageList.ContainsKey(page_addr))
            {
                PageList.Add(page_addr, new PageData(new byte[this.PageSize], 0));
            }
            PageData pageData = PageList[page_addr];
            uint offset = addr - page_addr;
            pageData.buffer[offset] = data;
            if (pageData.data_length <= offset)
            {
                pageData.data_length = offset + 1;
            }
        }

        public void AddData(uint addr, byte[] data)
        {
            for (uint i = 0; i < data.Length; i++)
            {
                AddData(addr + i, data[i]);
            }
        }

        public void AddData(Dictionary<uint, byte> data_cells)
        {
            foreach (var kv in data_cells)
            {
                AddData(kv.Key, kv.Value);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DfuFwUpdater
{
    public class NamedObject
    {
        public string Name { get; set; }
        public object Instance { get; set; }
        public NamedObject(string name, object instance)
        {
            this.Name = name;
            this.Instance = instance;
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}

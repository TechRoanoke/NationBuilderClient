using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NationBuilder
{
    // Tag retrieved from NB. 
    // Tag lookup is *case-sensitive*.
    public class NBTag : NBObject
    {
        public string name { get; set; }

        public override string ToString()
        {
            return this.name;
        }
    }
}
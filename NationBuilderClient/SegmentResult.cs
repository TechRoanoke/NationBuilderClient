using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace NationBuilder
{
    // Serialization format. 
    // Most NationBuilder APIs return lists in this format. 
    class SegmentResult<T>
    {
        // Cycle through the next poitners to get the results. 
        public string next { get; set; }
        public string prev { get; set; }

        public T[] results { get; set; }
    }
}
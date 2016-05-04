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
    // Represent the precincts in the nation. 
    // Also a serialization format 
    // http://nationbuilder.com/precincts_api
    public class NBPrecinct : NBObject
    {
        // NationBuilder's internal ID. 
        // This is meaningless outside of NB. 
        public long id { get; set; }
        
        // Code, hopefully that matches with a State Voter File. 
        // Sometimes codes are numbers with leading 0s. 
        public string code {get; set;}

        // Possible friendly name of the precinct. 
        public string name {get;set;}

        public override string ToString()
        {
            return string.Format("'{0}', code={1}, id={2}", name, code, id);
        }

        public string GetBestName()
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                return this.name;
            }
            if (!string.IsNullOrWhiteSpace(code))
            {
                return this.code;
            }
            return this.id.ToString();
        }
    }
}
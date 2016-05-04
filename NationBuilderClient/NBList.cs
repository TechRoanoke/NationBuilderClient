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
    // Represent a list in NationBuilder. 
    // Also a serialization format. See http://nationbuilder.com/lists_api for details. 
    public class NBList : NBObject
    {
        public string name { get; set; }
        public int count { get; set; }
        public long id { get; set; }

        public string slug { get; set; }

        // Get the people in the list
        //  GET https://trcmobile.nationbuilder.com/api/v1/lists/1/people?limit=10
        public async Task<NBPerson[]> GetPeopleAsync()
        {
            string path = string.Format("lists/{0}/people?limit=500", this.id);
            var results = await this.Client.GetArrayAsync<NBPerson>(path);

            // Remove duplicates.
            Dictionary<long, NBPerson> dups = new Dictionary<long, NBPerson>();
            foreach (var result in results)
            {
                dups[result.id] = result;
            }

            var uniqueResults = dups.Values.ToArray();

            return uniqueResults;
        }

        public override string ToString()
        {
            return string.Format("{0} ({1} entries)", this.name, this.count);
        }
    }
}
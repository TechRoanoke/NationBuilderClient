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
    // Also a serialization format 
    // http://nationbuilder.com/people_api
    public class NBPerson : NBObject
    {
        // NationBuilder unique ID 
        public long id { get; set; }

        // May be null or missing 
        public string birthdate { get; set; }

        public string GetBirthdate()
        {
            DateTime d;
            if (DateTime.TryParse(this.birthdate, out d))
            {
                return d.ToShortDateString();
            }
            return null;
        }

        public string first_name { get; set; }
        public string last_name { get; set; }

        public string sex { get; set; } // F or M

        public string email { get; set; }

        // These may be missing 
        public string state_file_id { get; set; }

        public string county_file_id {get;set;}

        // Name is the most useful; followup by code, then id. 
        // Beware, NationBuilder sometimes only gives back the precinct_id
        // when you query in /people and you need to do an explicit lookup to get the name. 

        // NB has several ways to refer to a precinct
        // Friendly name. name may be blank. 
        public string precinct_name	{ get ;set ;}

        // Code - may be meaningful and match to the voter database.
        // 53033-King-0262
        public string precinct_code { get; set; }

        // Internal nationbuilder Id. This is useless outside of nationbuilder.
        public string precinct_id { get; set; }

        public string GetPrecinct(IDictionary<long, NBPrecinct> precinctMap)
        {
            if (!string.IsNullOrWhiteSpace(this.precinct_name))
            {
                return this.precinct_name.Trim();
            }
            if (!string.IsNullOrWhiteSpace(this.precinct_code))
            {
                return this.precinct_code.Trim();
            }

            if (!string.IsNullOrWhiteSpace(this.precinct_id))
            {
                long id;
                if (long.TryParse(this.precinct_id, out id))
                {
                    if (precinctMap != null)
                    {
                        NBPrecinct precinct;
                        if (precinctMap.TryGetValue(id, out precinct))
                        {
                            return precinct.GetBestName();
                        }
                    }
                }
                return this.precinct_id;
            }
                               
            return "Unknown Precinct";
        }

        // D, R, other ... 
        public string party { get; set; }


        
        public string phone_normalized  { get; set; }
        public string mobile  { get; set; }
        
        public string phone { get; set; }

        public string GetPhone()
        {
            if (!string.IsNullOrWhiteSpace(this.phone_normalized))
            {
                return this.phone_normalized.Trim();
            }
            if (!string.IsNullOrWhiteSpace(this.phone))
            {
                return this.phone.Trim();
            }

            if (!string.IsNullOrWhiteSpace(this.mobile))
            {
                return this.mobile.Trim();
            }

            return null;
        }


        public class Address
        {
            public string address1 { get; set; }
            public string address2 { get; set; }
            public string address3 { get; set; }
                        
            public string city { get; set; }
            public string state { get; set; }
            public string zip { get; set; }

            public string county { get; set; }

            public string lat { get; set; }
            public string lng { get; set; }
        }

        public Address primary_address { get; set; }

        public void NormalizeAddress()
        {
            if (this.primary_address == null)
            {
                this.primary_address = new Address();
            }
        }


        // Update field 
        // PUT /api/v1/people/:id
        public async Task UpdateAsync()
        {
            await this.Client.UpdatePersonAsync(this.id, this);
        }

        // Lower district (LegDistrict) 
        public string state_lower_district { get; set; }

        public string[] tags { get; set; }
    }
}
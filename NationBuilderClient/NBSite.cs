using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NationBuilder
{
    // Survey pages exist in a site
    // http://nationbuilder.com/sites_api
    // Normally, have 1 site, and the site's slug is the same as the nation's slug
    public class NBSite : NBObject
    {
        public string domain { get; set; }
        public long id { get; set; }
        public string name { get; set; }
        public string slug { get; set; }

        public override string ToString()
        {
            return string.Format("Site: {0},{1},{2}", name, slug, id);
        }
    }
}

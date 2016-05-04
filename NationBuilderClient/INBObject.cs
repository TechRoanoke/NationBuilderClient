using Newtonsoft.Json;

namespace NationBuilder
{
    // Provide a standard way for NB objects (Person, Survey, etc) 
    // to point back to the client (and Auth token) so that they can send
    // messages back to the NationBuilder SaaS.
    interface INBObject
    {
        NBClient Client { get; set; }
    }

    // Convenience base class to provide a client. 
    public class NBObject : INBObject
    {
        [JsonIgnore]
        public NBClient Client { get; set; }
    }
}
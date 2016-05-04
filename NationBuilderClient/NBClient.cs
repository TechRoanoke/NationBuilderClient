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
    public class NBClient
    {

        private const string _endpointFormat = "https://{0}.nationbuilder.com/";
        private string _endpointCore; // "https://{slug}.nationbuilder.com/";
        private string _endpoint; // "https://{slug}.nationbuilder.com/api/v1/";
        private string _accessToken;
        private string _slug;

        private HttpClient _client = new HttpClient();

        public Func<string, Task> OnLogErrorAsync;

        // Slug is used for identifying the NB instance and forming URLs to the API. 
        public string Slug { get { return _slug; } }

        // Get the raw access token. 
        public string AccessToken { get { return _accessToken; } }

        public NBClient(string slug, string accessToken)
        {
            _slug = slug.ToLower(); // $$$ casing is important!
            _endpointCore = string.Format(_endpointFormat, slug);
            _endpoint = _endpointCore + "api/v1/";

            _accessToken = accessToken;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // NB requires an "accept: application/json" else will 406. 
            _client.DefaultRequestHeaders.Add("accept", "application/json");            
        }

        // Log any errors and throw 
        private async Task ThrowOnErrorAsync(HttpMethod method, string url, string jsonRequestBody, HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            // Log it 
            if (OnLogErrorAsync != null)
            {
                // Method, URL,
                // BODY 
                StringBuilder sb = new StringBuilder();
                sb.Append("Request to NB: ");
                sb.AppendLine();

                sb.AppendFormat("{0} {1} HTTP/1.1", method, url);
                sb.AppendLine();

                sb.AppendFormat("Authorization: Bearer {0}", _accessToken);
                sb.AppendLine();

                sb.AppendLine("content-type: application/json");
                sb.AppendLine("accept: application/json");
                sb.AppendLine();
                sb.AppendLine("Response:");


                string json = await response.Content.ReadAsStringAsync();
                sb.AppendLine(json);


                await OnLogErrorAsync(sb.ToString());
            }

            throw new InvalidOperationException("Request failed: " + jsonRequestBody);
        }

        public string ToConnectionString()
        {
            return string.Format("v1;{0};{1}", Slug, AccessToken);
        }
        public static NBClient FromConnectionString(string cx)
        {
            var parts = cx.Split(';');
            string slug = parts[1];
            string accessToken = parts[2];

            return new NBClient(slug, accessToken);
        }

        // Docs: https://github.com/3dna/api_docs/blob/master/doc/tags_api.md#people-endpoint
        // GET /api/v1/tags
        // Beware, tags are case-sensitive. 
        public async Task<NBTag[]> GetAllTags()
        {
            var client = this;

            string path = string.Format("tags?limit=500");
            var results = await client.GetArrayAsync<NBTag>(path);

            // Remove duplicates.
            return results;
        }

        public Task<NBPerson[]> GetPeopleWithTagAsync(NBTag tag)
        {
            return GetPeopleWithTagAsync(tag.name);
        }

        // Beware: tags are case-sensitive.
        // GET /api/v1/tags/:tag/people
        public async Task<NBPerson[]> GetPeopleWithTagAsync(string tag)
        {
            var client = this;

            string path = string.Format("tags/{0}/people?limit=500", tag);
            var results = await client.GetArrayAsync<NBPerson>(path, allow404:true);

            // Remove duplicates.
            Dictionary<long, NBPerson> dups = new Dictionary<long, NBPerson>();
            foreach (var result in results)
            {
                dups[result.id] = result;
            }

            var uniqueResults = dups.Values.ToArray();

            return uniqueResults;
        }

        // Wrapper to GET and PUT an individual person 
        class PersonWrapper : NBObject
        {
            public NBPerson person {get;set;}
        }

        // This endpoint returns the access token's resource owner's representation.
        // GET /api/v1/people/me
        public async Task<NBPerson> GetMeAsync()
        {
            var client = this;

            string path = string.Format("people/me");
            var resultWrapper = await client.GetAsync<PersonWrapper>(path);

            resultWrapper.person.Client = this;
            return resultWrapper.person;
        }

        // Get a person when we know their exact id. 
        public async Task<NBPerson> GetPersonAsync(long id)
        {
            var client = this;

            string path = string.Format("people/{0}", id);
            var resultWrapper = await client.GetAsync<PersonWrapper>(path);

            resultWrapper.person.Client = this;
            return resultWrapper.person;
        }

        // https://github.com/3dna/api_docs/blob/master/doc/people_api.md
        // PUT /api/v1/people/:id
        public async Task<NBPerson> UpdatePersonAsync(long id, NBPerson newFields)
        {
            var client = this;
                        
            newFields.id = id;
            string path = string.Format("people/{0}", id);
            var resultWrapper = await client.PutAsync(path, new PersonWrapper
            {
                 person = newFields
            });

            resultWrapper.person.Client = this;
            return resultWrapper.person;
            
        }
                
        public async Task<NBSite[]> GetSitesAsync()
        {
            // GET https://trcmobile.nationbuilder.com/api/v1/sites/ HTTP/1.1
            var results = await GetArrayAsync<NBSite>("sites");
            return results;    
        }
         
        // Flatten 
        public async Task<NBSurvey[]> GetSurveysAsync()
        {
            var sites = await this.GetSitesAsync();

            List<NBSurvey> l = new List<NBSurvey>();

            foreach (var site in sites)
            {
                // GET https://trcmobile.nationbuilder.com/api/v1/sites/trcmobile/pages/surveys HTTP/1.1
                var results = await GetArrayAsync<NBSurvey>("sites/" + site.slug + "/pages/surveys");

                l.AddRange(results);
            }
            return l.ToArray();            
        }


        class SurveyWrapper : INBObject
        {
            public NBSurvey survey { get; set; }

            public NBClient Client
            {
                get
                {
                    return survey.Client;
                }
                set
                {
                    survey.Client = value;
                }
            }
        }
        
        // Get a specific survey 
        // Key is a combination of id, slug, etc. 
        public async Task<NBSurvey> GetSurveyAsync(string key)
        {
            // GET https://{nation}.nationbuilder.com/api/v1/sites/{site}/pages/surveys/1 
            var path = NBSurvey.PathFromKey(key, _slug);

            var result = await this.GetAsync<SurveyWrapper>(path);

            return result.survey;            
        }
                
        // No endpoint to get a single list
        public async Task<NBList[]> GetListsAsync()
        {
            // GET https://trcmobile.nationbuilder.com/api/v1/lists?limit=10 

            var results = await GetArrayAsync<NBList>("lists");
            return results;
        }

        // Get all precincts. indexed by id. 
        // Generally under 10,000 precincts. 
        public async Task<IDictionary<long, NBPrecinct>> GetPrecinctsAsync()
        {
            // GET https://foobar.nationbuilder.com/api/v1/precincts

            var results = await GetArrayAsync<NBPrecinct>("precincts?limit=500");

            Dictionary<long,NBPrecinct> precincts = new Dictionary<long,NBPrecinct>();
            foreach (var result in results)
            {
                precincts[result.id] = result;
            }

            return precincts;
        }

        class PrecinctWrapper : INBObject
        {
            public NBPrecinct precinct { get; set; }

            public NBClient Client
            {
                get
                {
                    return precinct.Client;
                }
                set
                {
                    precinct.Client = value;
                }
            }
        }

        // Lookup single precinct 
        public async Task<NBPrecinct> GetPrecinctAsync(long id)
        {
            // GET /api/v1/precincts/:id
            string path = "precincts/" + id.ToString();
            var result = await this.GetAsync<PrecinctWrapper>(path);

            return result.precinct;
        }

        static JsonSerializerSettings _settings = new JsonSerializerSettings
        {
             NullValueHandling = NullValueHandling.Ignore
        };

        // PUT returns a filled out version of the request.
        internal async Task<TRequest> PutAsync<TRequest>(string path, TRequest request)
        {
            var url = _endpoint + path;

            string requestJson = JsonConvert.SerializeObject(request, _settings);
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
            var response = await _client.PutAsync(url, content);
            await ThrowOnErrorAsync(HttpMethod.Put, url, requestJson, response);

            TRequest result = await ReadAsJsonAsync<TRequest>(response);
            return result;
        }

        internal async Task<TResponse> PostAsync<TRequest, TResponse>(string path, TRequest request)
        {
            var url = _endpoint + path;

            string requestJson = JsonConvert.SerializeObject(request);
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(url, content);
            await ThrowOnErrorAsync(HttpMethod.Post, url, requestJson, response);

            TResponse result = await ReadAsJsonAsync<TResponse>(response);            
            return result;
        }

        internal async Task<T> GetAsync<T>(string path) where T : INBObject
        {
            var url = _endpoint + path;   
            var response = await _client.GetAsync(url);
            await ThrowOnErrorAsync(HttpMethod.Get, url, null, response);

            var result = await ReadAsJsonAsync<T>(response);
            result.Client = this;
            return result;
        }

        internal async Task<T[]> GetArrayAsync<T>(string path, bool allow404 = false) where T : INBObject
        {
            var url = _endpoint + path;                        

            List<T> list = new List<T>();

            SegmentResult<T> result;

            while (true)
            {
                var response = await _client.GetAsync(url);
                if (allow404)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        break;
                    }
                }
                await ThrowOnErrorAsync(HttpMethod.Get, url, null, response);
                result = await ReadAsJsonAsync<SegmentResult<T>>(response);
                //Console.WriteLine("Got {0}, {1} total.", result.results.Length, list.Count);

                foreach (var x in result.results)
                {
                    x.Client = this;
                    list.Add(x);
                }

                if (result.next == null)
                {
                    break;
                }

                // There are more results.
                // follow next link, relative to _endpointCore
                url = _endpointCore + result.next;
            }

            return list.ToArray();
        }

        static async Task<T> ReadAsJsonAsync<T>(HttpResponseMessage response)
        {
            string json = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException("Request failed: " + json);
            }

            return JsonConvert.DeserializeObject<T>(json);
        }
    }

    public static class NBClientExtensions
    {
        // No endpoint to get a single list. Must get them all. 
        public static async Task<NBList> GetListAsync(this NBClient client, long id)
        {
            var lists = await client.GetListsAsync();
            NBList list = lists.Where(x => x.id == id).First();

            return list;
        }
    }
}

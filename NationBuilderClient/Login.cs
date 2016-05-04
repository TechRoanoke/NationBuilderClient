using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NationBuilder
{
    // Helpers for doing the OAuth login to NationBuilder
    // Oauth hints: http://nationbuilder.com/api_quickstart
    public class NBLogin
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _slug;
        private readonly string _redirectUrl;

        // Public so callers can override it
        public HttpClient Client { get; set; }

        // This is what's registered on the NB App. 
        // Beware: this is case-sensitive. 
        public string RedirectUrl
        {
            get { return _redirectUrl; }
        }

        public NBLogin(
                string clientId,
                string clientSecret,
                string slug,
                string redirectUrl)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _slug = slug;
            _redirectUrl = redirectUrl;

            this.Client = new HttpClient();
        }

        // Request body to token exchange
        class NBLoginRequest
        {
            public string client_id;
            public string client_secret;
            public string redirect_uri;
            public string grant_type = "authorization_code";
            public string code;
        }

        // Response body from token exchange
        class NBLoginResponse
        {
            public string access_token;
            public string token_type;
            public string scope;
        }

        // Get the Uri to begin the OAuth dance.
        public Uri GetNationBuilderLoginUri()
        {
            // See http://weblog.west-wind.com/posts/2009/Feb/05/Html-and-Uri-String-Encoding-without-SystemWeb for tips on avoiding HttpUtiliy in System.Web.
            string loginTemplate = "https://{0}.nationbuilder.com/oauth/authorize?response_type=code&client_id={1}&redirect_uri={2}";
            string login = string.Format(
                loginTemplate, 
                _slug, 
                _clientId,
                System.Uri.EscapeDataString(_redirectUrl));

            return new Uri(login);
        }

        // Called on the redirect URL to exchange the code for the token. 
        public async Task<NBClient> ExchangeAsync(string code)
        {
            var url = "https://" + _slug + ".nationbuilder.com/oauth/token";

            NBLoginRequest body = new NBLoginRequest
            {
                client_id = _clientId,
                client_secret = _clientSecret,
                code = code,
                redirect_uri = _redirectUrl                 
            };

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
            };
            var response = await Client.SendAsync(request);
            string jsonResponse = await response.Content.ReadAsStringAsync();
            NBLoginResponse val = JsonConvert.DeserializeObject<NBLoginResponse>(jsonResponse);

            return new NBClient(_slug, val.access_token);
        }
    }
}
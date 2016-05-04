using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace NationBuilder
{
    // Query all surveys 
    // GET https://{slug_Nation}.nationbuilder.com/api/v1/sites/{slug_site}/pages/surveys HTTP/1.1
    // This is 

    public class NBSurvey : NBObject
    {
        public long id { get; set; } // required
        public string name { get; set; }
        public string slug { get; set; } // optional 

        public string site_slug { get; set; } // Optional 

        public override string ToString()
        {
            return string.Format("Survey: {0} ({1}, {2})", name, slug, id);
        }

        public string ToSerializeKey()
        {
            if (site_slug != null)
            {
                return site_slug + ":" + id.ToString();
            }
            return id.ToString();
        }

        // Get Path from the Key 
        // GET https://{nation}.nationbuilder.com/api/v1/sites/{site}/pages/surveys/{id} 
        public static string PathFromKey(string key, string nationSlug)
        {
            // 012
            // a:b
            string siteSlug;
            string surveyIdStr;

            int idx = key.IndexOf(':');
            if (idx > 0)
            {
                siteSlug = key.Substring(0, idx);
                surveyIdStr = key.Substring(idx + 1);
            }
            else
            {
                siteSlug = nationSlug;
                surveyIdStr = key;
            }
            int surveyId = int.Parse(surveyIdStr);            



            var path = "sites/" + siteSlug + "/pages/surveys/" + surveyId.ToString();
            return path;
        }

        public string status { get; set; }
        public string title { get; set; }

        // Answer for a question 
        // Serialization format. 
        public class NBChoice
        {
            public long id { get; set; }
            public string name { get; set; } // text to display 
            public string[] tags { get; set; } // tags to set when choice is used. 
        }

        // Questions in a survey.
        // Serialization format. 
        public class NBQuestion
        {
            public long id { get; set; }
            public string prompt { get; set; } // Primary question text.             
            string type { get; set; } // multiple

            public NBChoice[] choices { get; set; }

            // 
            public NBChoice LookupChoiceById(long id)
            {
                foreach (var x in choices)
                {
                    if (x.id == id)
                    {
                        return x;
                    }
                }
                string msg = string.Format("Question {0} '{1}' does not have choice id {2}", this.id, this.prompt, id);
                throw new InvalidOperationException(msg);
            }
        }
        public NBQuestion[] questions { get; set; }

        public NBQuestion LookupQuestionById(long id)
        {
            foreach (var x in this.questions)
            {
                if (x.id == id)
                {
                    return x;
                }
            }
            string msg = string.Format("Survey {0} '{1}' does not have question id {2}", 
                this.id, this.name, id);
            throw new InvalidOperationException(msg);
        }

        class SurveyResponseBody
        {
            public Body survey_response { get; set; }
            public class Body
            {
                public long survey_id { get; set; }
                public long person_id { get ;set;}

                public Response[] question_responses { get; set; }

                public class Response
                {
                    public long question_id { get ;set;}
                    public long response { get; set; }
                }
            }
        }

        // questionId --> responseId

        public async Task PostReplyAsync(NBSurveyResponse response)
        {
            await PostReplyAsync(response._personId, response._surveyResponses);
            return;
        }

        public async Task PostReplyAsync(long personId, IDictionary<long, long> surveyResponses)
        {
            var x = new NationBuilder.NBSurvey.SurveyResponseBody.Body.Response[surveyResponses.Count];
            int i =0;
            foreach(var kv in surveyResponses)
            {
                x[i] = new SurveyResponseBody.Body.Response
                {
                     question_id = kv.Key,
                      response = kv.Value
                };
                i++;
            }

            var request = new SurveyResponseBody();
            request.survey_response = new SurveyResponseBody.Body
            {
                 person_id = personId,
                 survey_id = this.id,
                 question_responses = x
            };

            // this will throw on any errors. 
            // POST https://trcmobile.nationbuilder.com/api/v1/survey_responses 
            var response = await this.Client.PostAsync<SurveyResponseBody, JObject>("survey_responses", request);
        }
    }
}
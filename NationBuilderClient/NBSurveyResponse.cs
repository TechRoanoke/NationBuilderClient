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
    // Helper object for creating a response to a survey. 
    public class NBSurveyResponse
    {
        internal readonly long _personId;

        internal IDictionary<long, long> _surveyResponses = new Dictionary<long, long>();

        public NBSurveyResponse(long personId)
        {
            this._personId = personId;
        }

        public void Set(NBSurvey.NBQuestion question, NBSurvey.NBChoice answer)
        {
            _surveyResponses[question.id] = answer.id;
        }
    }
}
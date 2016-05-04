using NationBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            // Enter your slug and access token. 
            string slug = "<your slug here>";
            string accessToken = "<your access token here>";

            NBClient client = new NBClient(slug, accessToken);
            
            // A connection string merges the secrets into a single serializable string for convenience. 
            // You can then re-create a client from just the connection string. 
            string connectionString = client.ToConnectionString();
            // NBClient client = NBClient.FromConnectionString(connectionString);
            
            // Add an error handler to log failed requests. 
            client.OnLogErrorAsync = (info) =>
            {
                Console.WriteLine(info);
                return Task.FromResult(0);
            };

            // Demo functionality 
            ReadTagsAsync(client).Wait();
            PrintSurveysAsync(client).Wait();
            PrintLists(client).Wait();
            PrecinctsAsync(client).Wait();
            DemoAsync(client).Wait();
        }

        private static async Task PrintLists(NBClient client)
        {
            Console.WriteLine("Lists:");
            var lists = await client.GetListsAsync();
            foreach (var list in lists)
            {
                Console.WriteLine("(ListId={0}): {1} ({2} records)", list.id, list.name, list.count);
            }
            Console.WriteLine();
        }

        private static async Task PrecinctsAsync(NBClient client)
        {
            IDictionary<long, NBPrecinct> precincts = await client.GetPrecinctsAsync();
            Console.WriteLine("{0} precincts. Here are the first few:", precincts.Count);
            foreach (var precinct in precincts.Values.Take(10))
            {
                Console.WriteLine(" {0}, {1}, {1}", precinct.id, precinct.code, precinct.name);
            }
            Console.WriteLine();
        }

        // Print all surveys 
        private static async Task PrintSurveysAsync(NBClient client)
        {
            var surveys = await client.GetSurveysAsync();
            foreach (var survey in surveys)
            {
                Console.WriteLine("Survey: {0}", survey.name);
                foreach (var question in survey.questions)
                {
                    Console.WriteLine("  (QuestionId={0}) {1}", question.id, question.prompt);
                    foreach (var answer in question.choices)
                    {
                        Console.WriteLine("     (ChoiceId={0}) {1}", answer.id, answer.name);
                    }
                }
                Console.WriteLine();
            }
        }

        // Post a survey reply back for a given person. 
        // A response can include multiple question/answer pairs. 
        private static async Task PostSurveyReply(NBClient client, NBSurvey survey, long personId, long[] questionId, long[] choiceId)
        {
            NBSurveyResponse response = new NBSurveyResponse(personId);

            for (int i = 0; i < questionId.Length; i++)
            {
                NationBuilder.NBSurvey.NBQuestion question = survey.LookupQuestionById(questionId[i]);
                NationBuilder.NBSurvey.NBChoice choice = question.LookupChoiceById(choiceId[i]);

                response.Set(question, choice);
            }

            await survey.PostReplyAsync(response);
        }

        private static async Task DemoAsync(NBClient client)
        {
            NBPerson person = await client.GetMeAsync();

            Console.WriteLine("Your current token: {0} {1} (id={2})", person.first_name, person.last_name, person.id);
        }

        private static async Task ReadTagsAsync(NBClient client)
        {
            Console.WriteLine("All tags:");
            NBTag[] tags = await client.GetAllTags();
            foreach (var x in tags.Take(10))
            {
                Console.WriteLine(x);
            }

            // Query by tag. 
            var tag = tags[0];
            Console.WriteLine("People with tag '{0}'", tags);
            var people = await client.GetPeopleWithTagAsync(tag);
            foreach (NBPerson person in people.Take(10))
            {
                Console.WriteLine("  {0} {1}", person.first_name, person.last_name);
            }
            Console.WriteLine();
        }
    }
}

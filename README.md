# NationBuilderClient
A C# client for NationBuilder, http://nationbuilder.com/ 
This provides friendly C#  wrappers for the NationBuilder REST API, as defined at http://nationbuilder.com/api_documentation 

It includes handling for people, tags, lists, surveys, and precincts. 
It also includes helpers for doing OAuth login. 

See Samples/Program.cs for example usage. 

To create a client from a slug and access token 
```
  // Enter your slug and access token. 
  string slug = "<your slug here>";
  string accessToken = "<your access token here>";

  NBClient client = new NBClient(slug, accessToken);
```

To print all the lists in the your Nation
```
        private static async Task PrintLists(NBClient client)
        {
            Console.WriteLine("Lists:");
            NBList[] lists = await client.GetListsAsync();
            foreach (NBList list in lists)
            {
                Console.WriteLine("(ListId={0}): {1} ({2} records)", list.id, list.name, list.count);
            }
            Console.WriteLine();
        }
```        

To view the tags and query by tag:
```
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
```

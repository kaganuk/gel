using System;
using System.Threading.Tasks;
using ConsoleArguments;
using Helpers;
using RunFunctions;
using gel;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace RunFunctions
{
    public static class Functions
    {   
        public static void runIssue(Command command){
            var move        = command.args.Find(arg => arg.name == "move");
            
            if (move != null)
            {
                var issueId = move.value;
                if (issueId != null)
                {
                    var transitionUrl     = $"https://{Program.url}/rest/api/2/issue/{issueId.Trim()}/transitions";
                    var statusUrl         = $"https://{Program.url}/rest/api/2/issue/{issueId.Trim()}?fields=status";
                    var loop              = true;
                    while (loop)
                    {
                        JObject json = Program.ProcessRequests(transitionUrl, HttpRequestType.GET).Result;
                        IEnumerable<JToken> transitions     = json.SelectTokens("$.transitions.[*]");
                        Dictionary<Char,Transition> dict    = new Dictionary<Char,Transition>();
                        JObject jsonStatus = Program.ProcessRequests(statusUrl, HttpRequestType.GET).Result;
                        IEnumerable<JToken> status     = jsonStatus.SelectToken("$.fields.status.name");
                        Console.WriteLine(issueId.Trim() + " - " + status.ToString());
                        Console.WriteLine("(0) - Exit");
                        var i = 0;
                        foreach(var item in transitions){
                            i++;
                            dict.Add(
                                Convert.ToChar(i.ToString()),
                                new Transition(
                                    Convert.ToInt32(item.SelectToken("$.id")),
                                    item.SelectToken("$.to.name").ToString()
                                )
                                    
                            );
                            Console.WriteLine("("+i.ToString() + ") - " + item.SelectToken("$.to.name").ToString());
                        }
                        Char choose = Console.ReadKey().KeyChar;
                        if(choose != '0'){
                            if (dict.ContainsKey(choose)){
                                Program.ProcessRequests(transitionUrl, HttpRequestType.POST, 
                                                new JsonContent(
                                                    new { transition = new { id = dict.GetValueOrDefault(choose).id } }
                                                )
                                ).Wait();
                                Console.Clear();
                            } else {
                                Console.WriteLine("\nInvalid number");
                            }
                        } else {
                            loop = false;
                        }
                    }
                }       
            }
        }
        public static void runConfig(Command command){
            var conf = command.args.Find(arg => arg.name == "set");
            if (conf != null)
            {
                var email   = conf.options.Find(opt => opt.name == "--email");
                var token  = conf.options.Find(opt => opt.name == "--token");
                var url     = conf.options.Find(opt => opt.name == "--url");
                if (email != null && token != null && url != null)
                {
                    ShellHelper.Bash("security delete-generic-password -s gel &>-");
                    ShellHelper.Bash($"security add-generic-password -a {email.value} -s gel -p {token.value} -U -j {url.value}>-");
                }
                
            }
        }
    }
}

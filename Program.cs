using RunFunctions;
using Helpers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ConsoleArguments;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace gel
{
    class Program
    {   
        public static String url, email, key;
        const String help = 
        "\n---------------------------------------- \n\n"
        +"GEL - JIRA Client App Help\n"
        + "gel help\n"
        + "gel issue move ZNG-5555\n"
        + "gel config set --email lorem@ipsum.net --token sdfk55sfslslaf44sd --url sample.atlassian.net";
        private static List<Command> commandList =
            new List<Command>(){
                new Command("issue", 
                    new List<Arg>(){
                        new Arg("move", true),
                    }
                ),
                new Command("config", 
                    new List<Arg>(){
                        new Arg("set", false,
                            new List<Option>(){
                                new Option("--email",true),
                                new Option("--token",true),
                                new Option("--url",true)
                            }
                        )
                    }
                )
            };
        static void Main(string[] args)
        {
            var cnslargs = new ConsoleArgs(args, commandList);
            try
            {
                var command = cnslargs.checkCommand();
                if (command.name != "config")
                {
                    setConfigs();
                }
                run(command);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message + "!!!");
                Console.WriteLine(help);
                return;
            }
        }
        private static void run(Command command){
            if (command.name == "issue")
            {
                Functions.runIssue(command);
            } else if (command.name == "config")
            {
                Functions.runConfig(command);
            }
        }
        private static void setConfigs(){
            var consoleOutput   = ShellHelper.Bash("security find-generic-password -s gel");
            if (consoleOutput.Contains("acct"))
            {  
                var start           = consoleOutput.IndexOf("\"acct\"<blob>=\"");
                start               = start + 14;
                var end             = consoleOutput.IndexOf("\"",start);
                var email           = consoleOutput.Substring(start , (end - start));
                var key             = ShellHelper.Bash("security find-generic-password -s gel -w");
            
                start               = consoleOutput.IndexOf("\"icmt\"<blob>=\"");
                start               = start + 14;
                end                 = consoleOutput.IndexOf("\"",start);
                var url             = consoleOutput.Substring(start , (end - start));

                Program.email       = email.Trim();
                Program.key         = key.Trim();
                Program.url         = url.Trim();
            } else {
                throw new Exception("please set your credentials");
            }
        }

        public static async Task ProcessRequests(String url, HttpRequestType requestType, JsonContent Data = null)
        {
            var client = ClientHelper.GetClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            switch (requestType)
            {
                case HttpRequestType.POST:
                {
                    var stringTask  = client.PostAsync(url, Data);
                    var response    = await stringTask;
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("\nProcess succeed");
                    } else {
                        Console.WriteLine("Something went wrong :(\n"+response.ToString());
                    }
                    break;
                }
                case HttpRequestType.GET:
                {
                    var stringTask                      = client.GetStringAsync(url);
                    var response                        = await stringTask;
                    JObject json                        = JObject.Parse(response.ToString());
                    IEnumerable<JToken> transitions     = json.SelectTokens("$.transitions.[*]");
                    Dictionary<Char,Transition> dict    = new Dictionary<Char,Transition>();
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
                            ProcessRequests(url, HttpRequestType.POST, 
                                            new JsonContent(
                                                new { transition = new { id = dict.GetValueOrDefault(choose).id } }
                                            )
                            ).Wait();
                            Console.Clear();
                        } else {
                            Console.WriteLine("\nInvalid number");
                        }
                    }
                    break;
                }
            }
            
        }
    }
}

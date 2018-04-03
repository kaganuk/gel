using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
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
        + "gel issue inprogress ZNG-5555\n"
        + "gel issue waiting ZNG-5555\n"
        + "gel config set --email lorem@ipsum.net --token sdfk55sfslslaf44sd --url sample.atlassian.net"
        ;
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
        enum HttpRequestType { 
            POST,
            GET
        }
        private static async Task<Boolean> ProcessRequests(String url, HttpRequestType requestType, JsonContent Data = null)
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
                    IEnumerable<JToken> token           = json.SelectTokens("$.transitions.[*]");
                    Dictionary<Char,Transition> dict    = new Dictionary<Char,Transition>();
                    Console.WriteLine("Ctrl+C for exit");
                    var i = 0;
                    foreach(var item in token){
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
                        ProcessRequests(url, HttpRequestType.POST, 
                                        new JsonContent(
                                            new { transition = new { id = dict.GetValueOrDefault(choose).id } }
                                        )
                        ).Wait();
                    } else {
                        return true;
                    }
                    break;
                }
            }
        
            return true;
        }
        private static void run(Command command){
            if (command.name == "issue")
            {
                runIssue(command);
            } else if (command.name == "config")
            {
                runConfig(command);
            }
        }
        private static void runIssue(Command command){
            var move        = command.args.Find(arg => arg.name == "move");
            
            if (move != null)
            {
                var issueId = move.value;
                if (issueId != null)
                {
                    while (true)
                    {
                        ProcessRequests($"https://emlakiq.atlassian.net/rest/api/2/issue/{issueId.Trim()}/transitions", HttpRequestType.GET).Wait();    
                    }
                }
            }
        }
        private static void runConfig(Command command){
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
    }
    public static class ClientHelper
    {
        public static HttpClient GetClient()
        {
            var authValue = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Program.email}:{Program.key}")));
            var client = new HttpClient(){
                DefaultRequestHeaders = { Authorization = authValue}
            };
            return client;
        }
    }
    public class JsonContent : StringContent
    {
        public JsonContent(object obj) :
            base(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json")
        { }
    }
    public static class ShellHelper
    {
        public static String Bash(this String cmd, bool showResults = false)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");
            
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = false
                }
            };
            process.Start();
            String result = process.StandardOutput.ReadToEnd();    
            process.WaitForExit();
            return result;
        }
    }
    public class Transition{
        public Int32 id { get; }
        public String name { get; }
        public Transition(Int32 id, String name)
        {
            this.id     = id;
            this.name   = name;
        }  
    }
}

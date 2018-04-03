using System;
using System.Threading.Tasks;
using ConsoleArguments;
using Helpers;
using RunFunctions;
using gel;

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
                    while (true)
                    {
                        Program.ProcessRequests($"https://emlakiq.atlassian.net/rest/api/2/issue/{issueId.Trim()}/transitions", HttpRequestType.GET).Wait();    
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

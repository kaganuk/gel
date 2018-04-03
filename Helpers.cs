using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using gel;

namespace Helpers
{
    enum HttpRequestType { 
        POST,
        GET
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
}

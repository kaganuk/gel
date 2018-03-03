using System;
using System.Collections.Generic;
using ConsoleArguments;

namespace gel
{
    class Program
    {   
        const String help = 
        "\n --------------------------------------- \n\n"
        +"GEL - JIRA Client App Help\n"
        + "gel help\n"
        + "gel issue inprogress ZNG-5555\n"
        + "gel issue waiting ZNG-5555"
        ;
        static void Main(string[] args)
        {
            var cnslargs = new ConsoleArgs(args, setCommands());
            try
            {
                var cmnd = cnslargs.checkCommand();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message + "!!!");
                Console.WriteLine(help);
                return;
            }
        }

        private static List<Command> setCommands(){
            return new List<Command>(){
                new Command("issue", 
                    new List<Arg>(){
                        new Arg(
                            "inprogress",
                            true,
                            new List<Option>(){
                                new Option("-h", false),
                                new Option("-f", true)
                            }
                        ),
                        new Arg("waiting", true)
                    }
                ),
            }; 
        }
    }
}

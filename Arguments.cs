using System;
using System.Collections.Generic;

namespace ConsoleArguments
{
    class ConsoleArgs{
        private string[] allArgs;
        private Queue<String> argsQueue = new Queue<String>();
        private CommandType command;
        private List<ArgType> args = new List<ArgType>();
        private List<String> optionsStr = new List<String>();
        public ConsoleArgs(string[] args)
        {
            allArgs = args;
        }
        
        public void cw(){
            Console.WriteLine("command:" + command.ToString());
            foreach(var item in optionsStr){
                Console.WriteLine(item);
            }
            foreach(var item in args){
                Console.WriteLine(item);
            }
        }
        public void parseArgs(){
            addQueueArgs();
            command = Command.getCommand(argsQueue.Dequeue());

            while (argsQueue.Count > 0)
            {
                var arg = argsQueue.Dequeue();
                if(!(arg.Contains("-"))){
                    args.Add(Args.getArg(arg));
                } else {
                    optionsStr.Add(arg);
                }
            }
        }
        private void addQueueArgs(){
            foreach (var item in allArgs)
            {
                argsQueue.Enqueue(item);
            }
        }
    }

    static class Command {
        private static Dictionary<String,CommandType> CommandList = new Dictionary<String,CommandType>(){
            {"issue" , CommandType.Issue},
            {"auth" , CommandType.Auth},
        };
        public static CommandType getCommand(String command){
            if (CommandList.TryGetValue(command, out CommandType value)){
                return value;
            } else {
                return CommandType.Unknown;
            }
        }
    }

    public enum CommandType {
        Issue,
        Auth,
        Unknown
    }

    static class Args {
        private static Dictionary<String,ArgType> CommandList = new Dictionary<String,ArgType>(){
            {"changeStatus" , ArgType.changeStatus},
            {"get" , ArgType.get},
            {"CommandType.Issue" , ArgType.changeStatus},
            {"move" , ArgType.move},
        };
        public static ArgType getArg(String command){
            if (CommandList.TryGetValue(command, out ArgType value)){
                return value;
            } else {
                return ArgType.Unknown;
            }
        }
    }


    static class CommandArgs {
        private static Dictionary<CommandType,ArgType> CommandList = new Dictionary<CommandType,ArgType>(){
            {CommandType.Issue , ArgType.changeStatus},
            {CommandType.Issue , ArgType.get},
            {CommandType.Issue , ArgType.changeStatus},
            {CommandType.Issue , ArgType.move},
        };
        public static ArgType getArgsForCommand(CommandType command){
            if (CommandList.TryGetValue(command, out ArgType value)){
                return value;
            } else {
                return ArgType.Unknown;
            }
        }
    }

    public enum ArgType {
        changeStatus,
        get,
        move,
        Unknown
    }
}

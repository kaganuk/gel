using System;
using System.Collections.Generic;

namespace ConsoleArguments
{
    class ConsoleArgs{
        private String[] allArgs;
        private Queue<String> argsQueue = new Queue<String>();
        public List<Command> commandList = new List<Command>();
        public Command command;
        public List<Arg> args = new List<Arg>();
        public List<Option> options = new List<Option>();
        public ConsoleArgs(String[] args, List<Command> commands)
        {
            allArgs         = args;
            commandList     = commands;
        }
        
        public Command checkCommand(){
            pushArgsQueue();
            getCommand();
            checkArgs();

            if (args.Count == 0){
                throw new Exception("unknown argument for " + command.name);
            } else {
                command.args = args;
            }

            return command;
        }

        private void checkArgs(){
            if(argsQueue.Count == 0){
                throw new Exception("argument not found");
            }
            var arg = argsQueue.Dequeue();
            foreach (var argItem in command.args)
            {
                if (argItem.name.Equals(arg))
                {   
                    if(argItem.hasValue){
                        if (argsQueue.Count == 0)
                        {
                            throw new Exception(argItem.name + " argument has got to be value");
                        }
                        argItem.value = argsQueue.Dequeue();
                    }
                    if (argItem.options != null)
                    {
                        checkOptions(argItem);
                    }
                    argItem.options = options;
                    this.args.Add(argItem);
                }
            }
        }

        private void checkOptions(Arg argItem){
             while(argsQueue.Count != 0){
                var opt             = argsQueue.Dequeue();
                var optError        = true;
                Option lastOption   = null;
                foreach (var option in argItem.options)
                {
                    if (option.name.Equals(opt))
                    {   
                        if(option.hasValue){
                            if (argsQueue.Count == 0)
                            {
                                throw new Exception(option.name + " option has got to be value");
                            }
                            option.value = argsQueue.Dequeue();
                        }
                        optError        = false;
                        lastOption  = option;
                        this.options.Add(option);
                    }
                }
                if (lastOption != null)
                {
                    argItem.options.Remove(lastOption);   
                }
                if (optError)
                {
                    throw new Exception(opt + " option does not found");
                } 
            }
        }

        public void getCommand(){
            var firstArg = argsQueue.Dequeue();
            foreach (var command in commandList)
            {
                if(command.name.Equals(firstArg)){
                    this.command = command;
                }
            }
            if(command == null){
                throw new Exception("command not found");
            }
        }

        private void pushArgsQueue(){
            foreach (var item in allArgs)
            {
                argsQueue.Enqueue(item);
            }
            if (argsQueue.Count == 0)
            {
                throw new Exception("please check help page");
            }
        }
    }

    class Command {
        public String name;
        public List<Arg> args;
        public Command(String name, List<Arg> args)
        {
            this.name       = name;
            this.args       = args;
        }
    }

    class Arg {
        public String name;
        public Boolean hasValue;
        public String value;
        public List<Option> options = new List<Option>(){};
        public Arg(String name, Boolean hasValue, List<Option> options = null)
        {
            this.name       = name;
            this.hasValue   = hasValue;
            this.options    = options;
        }
    }
    class Option {
        public String name;
        public Boolean hasValue;
        public String value;
        public Option(String name, Boolean hasValue)
        {
            this.name       = name;
            this.hasValue   = hasValue;
        }
    }
}

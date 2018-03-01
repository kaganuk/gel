using System;
using System.Collections.Generic;
using ConsoleArguments;

namespace gel
{
    class Program
    {
        static void Main(string[] args)
        {
            var cns = new ConsoleArgs(args);
            cns.parseArgs();
            cns.cw();
        }
    }
}

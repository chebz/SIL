using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SIL_CommandLine
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            CommandLine commandLine = new CommandLine();
            if (commandLine.start(args))
                return;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SIL_Core;
using System.IO;

namespace SIL_CommandLine
{
    public class CommandLine
    {
        public CommandLine()
        {
            Parser.Initialize();
            Compiler.Initialize();
            Compiler.OnAction += new Compiler.Action(Compiler_OnAction);
        }

        public bool start(string[] arrayOfStringsFromCommandLine)
        {
            //Console.WriteLine(arrayOfStringsFromCommandLine[1]);
            if (arrayOfStringsFromCommandLine.GetLength(0) == 0)
                return false;

            try
            {
                if (!File.Exists(arrayOfStringsFromCommandLine[0]))
                {                    
                Console.WriteLine("File not found");
                    return true;
                }

                SIL_Program program = SIL_IO.Load(arrayOfStringsFromCommandLine[0]);

                Console.WriteLine("Loaded Program: " + arrayOfStringsFromCommandLine[0]);
                // program.Name += "\n\r" + program.Text;
                // program.Text = program.Name;
                // SymbolTable.Initialize();
                program.Parse();
                program.Execute();
            }

            catch (FileNotFoundException ex)
            {
                // Write error.
                Console.WriteLine("File not Found");
                return true;
            }

            catch (Exception e)
            {
                string[] err = e.Message.Split('%');
                for (int i = 0; i < err.Length - 1; ++i)
                {
                    Console.WriteLine(err[i]);
                }
                Console.WriteLine("LineNumber:" + err[err.Length - 1]);
            }

            return true;
        }

        static void Compiler_OnAction(ActionType actionType, object[] args)
        {
            switch (actionType)
            {
                case ActionType.PRINT:
                    Console.Write(args[0]);
                    break;
                case ActionType.PRINTLN:
                    Console.WriteLine(args[0]);
                    break;
                case ActionType.READ:
                    Console.WriteLine(args[1]);
                    string line = Console.ReadLine();
                    int var;
                    while (!int.TryParse(line, out var))
                    {
                        Console.WriteLine("Incorrect input, must be numeric!");
                        line = Console.ReadLine();                        
                    }
                    ((SILVariable)args[0]).Value = (SILInteger)var;
                    break;
                case ActionType.DRAWLINE:
                    Console.WriteLine("DRAWLINE must be run in the windows GUI");
                    break;
            }
        }
    }


}


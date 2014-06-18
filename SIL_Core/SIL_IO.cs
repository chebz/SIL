using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SIL_Core
{
    public static class SIL_IO
    {
        /// <summary>
        /// Save a SIL program to hard drive
        /// </summary>
        /// <param name="path">directory and filename where to save</param>
        /// <param name="program">program to save</param>
        public static void Save(string path, SIL_Program program)
        {
            StreamWriter s = new StreamWriter(File.Create(path));
            s.WriteLine(program.Name);
            s.Write(program.Text);
            s.Close();
        }

        /// <summary>
        /// Loads SIL program from hard drive
        /// </summary>
        /// <param name="path">location of the program</param>
        /// <returns>Creates a program based on loaded data and returns its reference</returns>
        public static SIL_Program Load(string path)
        {
            StreamReader s = new StreamReader(path);
            SIL_Program program = new SIL_Program();
            program.Name = s.ReadLine().Trim();
            program.Text = s.ReadToEnd();;
            s.Close();
            return program;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SIL_Core
{
    public class SIL_Program
    {
        string  text_,
                name_;
        string[] lines_;

        public void Execute()
        {
            Compiler.ProgramName = name_;
            Compiler.Execute();
        }

        public void Parse()
        {
            Compiler.ProgramName = name_;
            Compiler.Compile(text_);
        }  

        public string Text { get { return text_; } set { text_ = value; } }
        public string[] Lines { get { return lines_; } set { lines_ = value; } }
        public string Name { get { return name_; } set { name_ = value; } }
    }
}

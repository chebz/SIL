using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SIL_Core
{
    /// <summary>
    /// I build custom SymbolTable class instead of using a basic generics Dictionary, so that I can throw
    /// custom exceptions and override some methods how I need to.
    /// </summary>
    public static class SymbolTable
    {
        static Dictionary<string, SILInteger> symbols_ = new Dictionary<string, SILInteger>(8192);

        public static void Add(string Name)
        {
            if (symbols_.ContainsKey(Name))
            {
                Replace(Name, null);
                return;
            }
            if (symbols_.Count >= 8192)
                throw new Exception("Symbol table overflow!");
            symbols_.Add(Name, null);
        }

        public static void Replace(string Name, SILInteger Value)
        {
            if (!symbols_.ContainsKey(Name))
                throw new Exception(String.Format("Key \"{0}\" was not found!", Name));
            symbols_[Name] = Value;
        }

        public static SILInteger Get(string Name)
        {
            if (!symbols_.ContainsKey(Name))
                throw new Exception(String.Format("Key \"{0}\" was not found!", Name));
            return symbols_[Name];
        }

        public static void Set(string Name, SILInteger value)
        {
            if (!symbols_.ContainsKey(Name))
                throw new Exception(String.Format("Key \"{0}\" was not found!", Name));
            symbols_[Name] = value;
        }

        public static void Remove(string Name)
        {
            if (!symbols_.ContainsKey(Name))
                throw new Exception(String.Format("Key \"{0}\" was not found!", Name));
            symbols_.Remove(Name);
        }

        public static void Clear()
        {
            symbols_.Clear();
        }

        public static void Clear(string functionspace)
        {
            for(int i =0; i < symbols_.Count; i++)
            {
                if (symbols_.Keys.ElementAt(i).Length > functionspace.Length + 1)
                {
                    if (symbols_.Keys.ElementAt(i).Substring(0, functionspace.Length + 1) == functionspace + ".")
                        symbols_.Remove(symbols_.Keys.ElementAt(i));
                }
            }
        }

        public static int MemUsed
        {
            get { return symbols_.Count; }
        }

        public static int MemFree
        {
            get { return 8192 - symbols_.Count; }
        }
    }
}

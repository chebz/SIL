using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SIL_Core
{
    public class FunctionPrototype
    {
        public ObjectType[] ParameterTypes;
        public ObjectType ReturnType;
        public SIL_Action[] Actions;
        public string[] ParamNames;
    }

    internal static class FunctionTable 
    {
        static Dictionary<string, FunctionPrototype> functions_ = new Dictionary<string, FunctionPrototype>();

        public static FunctionPrototype Get(string name)
        {
            if (!functions_.ContainsKey(name))
                throw new Exception(string.Format("Function \"{0}\" not declared!", name));
            return functions_[name];
        }

        public static void Add(string name, FunctionPrototype prototype)
        {
            if (functions_.ContainsKey(name))
                throw new Exception(string.Format("Function \"{0}\" already declared!", name));
            functions_.Add(name, prototype);
        }

        public static void Clear()
        {
            functions_.Clear();
        }

        public static bool IsDefined(out string functionName)
        {
            foreach (string key in functions_.Keys)
            {
                if (functions_[key].Actions == null)
                {
                    functionName = key;
                    return false;
                }
            }
            functionName = null;
            return true;
        }
    }
}


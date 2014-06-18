using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;


namespace SIL_Core
{
    public enum MathOperator
    {
        Plus,
        Minus,
        Multiply,
        Divide,
        Less,
        More,
        Equals,
        AND,
        OR
    }

    /// <summary>
    /// Instead of using Object_Type enum I could use if (typeof(mySIL_Object) == SILInteger) but
    /// I cant use a switch case in this scenario
    /// </summary>
    public enum ObjectType
    {
        Integer,
        Variable,
        Equation,
        Array, ArrayElement,
        Void,
        Function
    }

    public interface ISILObject
    {
        ObjectType Type { get; }
        ISILObject Value { get; set; }
    }
    
    public class SILVoid : ISILObject
    {
        public ISILObject Value
        {
            get { return null; }
            set { throw new Exception("Void cannot be assigned a value!"); }
        }

        public ObjectType Type { get { return ObjectType.Void; } }
    }

    public class SILFunction : ISILObject
    {
        string name_;
        Queue<SIL_Action> actions_ = new Queue<SIL_Action>();
        ISILObject[] parameters_;
        ISILObject returnValue_;
        ObjectType returnType_;

        public SILFunction(string name, ISILObject[] parameters)
        {
            FunctionPrototype prototype = FunctionTable.Get(name);
            //vlaidate
            if (parameters.Length != prototype.ParameterTypes.Length)
                throw new Exception(string.Format("Function \"{0}\" has incorrect number of parameters!", name));
            //for (int i = 0; i < parameters.Length; i++)
            //{
            //    if (parameters[i].Type != prototype.ParameterTypes[i])
            //        throw new Exception(
            //            string.Format("Function \"{0}\" has incorrect type of parameter #{1}, {2} expected!", 
            //            name, i, Enum.GetName(typeof(ObjectType), prototype.ParameterTypes[i])));
            //}
             
            name_ = name;
            parameters_ = parameters;
        }

        public ISILObject Value
        {
            get
            {
                FunctionPrototype fp = FunctionTable.Get(name_);
                for (int i = 0; i < parameters_.Length; i++)
                {
                    SymbolTable.Add(name_ + "." + fp.ParamNames[i]);
                    SymbolTable.Set(name_ + "." + fp.ParamNames[i], (SILInteger)parameters_[i].Value);
                }

                AddActions(FunctionTable.Get(Name).Actions);
                while (actions_.Count > 0)
                {
                    SIL_Action action = actions_.Dequeue();
                    Compiler.Execute(action);
                    if (action.ActionType == ActionType.RETURN)
                    {
                        returnValue_ = (ISILObject)action.ExecutionArgs[0];
                        EndFunction();
                        break;
                    }
                }
                return returnValue_;
            }
            set { returnValue_ = value; }
        }

        public void AddActions(SIL_Action[] actions)
        {
            foreach (SIL_Action action in actions)
                actions_.Enqueue(action);
        }

        //Dispose local variables
        public void EndFunction()
        {
            SymbolTable.Clear(name_);
        }


        public ObjectType Type { get { return ObjectType.Function; } }

        public string Name { get { return name_; } }
    }

    public class SILArray : ISILObject, IEnumerable<ISILObject>
    {
        private int size_ = 0;
        private string name_;
        // Enumerable classes can return an enumerator
        public IEnumerator<ISILObject> GetEnumerator()
        {
            for(int i = 0; i < size_; i++)
            {
                string elementName = string.Format("{0}[{1}]", name_, i);
                yield return SymbolTable.Get(elementName);
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public SILArray(string name, params ISILObject[] elements)
        {
            if (elements.Length > 8192) throw new Exception("Array is too big, must be 8192 elements or less!");
            // allocate space for the strings
            name_ = name;

            // deep copy
            foreach (ISILObject e in elements)
            {                
                string elementName = string.Format("{0}[{1}]", name_, size_++);
                SymbolTable.Add(elementName);
                SymbolTable.Set(elementName, (SILInteger)e.Value);
            }
        }

        public SILArray(string name, int size)
        {
            if (size > 8192) throw new Exception("Array is too big, must be 8192 elements or less!");
            size_ = size;
            name_ = name;
            ArrayTable.Arrays.Add(name_, this);
        }

        public void Allocate()
        {
            if (SymbolTable.MemFree < size_) throw new Exception(
                string.Format("Insufficient memory to allocate the array \"{0}\"!", name_));

            for (int i = 0; i < size_; i++)
            {
                string elementName = string.Format("{0}[{1}]", name_, i);
                SymbolTable.Add(elementName);
                SymbolTable.Set(elementName, null);
            }            
        }

        public void Clear()
        {
            size_ = 0;
            for (int i = 0; i < size_; i++)
            {
                string elementName = string.Format("{0}[{1}]", name_, i);
                SymbolTable.Remove(elementName);
            }
        }

        public ISILObject Value
        {
            get { return this; }
            set 
            {                
                this.Clear();
                SILArray copyFrom = (SILArray)value;
                foreach (ISILObject e in copyFrom)
                {
                    string elementName = string.Format("{0}[{1}]", name_, size_++);
                    SymbolTable.Add(elementName);
                    SymbolTable.Set(elementName, (SILInteger)e.Value);
                }
            }
        }

        // allow array-like access
        public ISILObject this[int index]
        {
            get
            {
                if (index < 0 || index >= size_)
                {
                    throw new Exception("Index out of bounds!");
                }
                string elementName = string.Format("{0}[{1}]", name_, index);
                return SymbolTable.Get(elementName);
            }
            set
            {
                if (index < 0 || index >= size_)
                {
                    throw new Exception("Index out of bounds!");
                }
                string elementName = string.Format("{0}[{1}]", name_, index);
                SymbolTable.Set(elementName, (SILInteger)value);
            }
        }


        public ObjectType Type { get { return ObjectType.Array; } }

        public int Size { get { return size_; } }
    }

    /// <summary>
    /// my extended integer class
    /// </summary>
    public class SILInteger : ISILObject
    {        
        int value_;

        public ISILObject Value
        {
            get { return this; }
            set
            {
                value_ = (SILInteger)value;
            }
        }

        public static implicit operator int(SILInteger silInt)
        {
            return silInt.value_;
        }

        public static implicit operator SILInteger(int value)
        {
            SILInteger silInt = new SILInteger();
            silInt.value_ = value;
            return silInt;
        }

        public ObjectType Type { get { return ObjectType.Integer; } }
    }

    public class SILVariable : ISILObject
    {
        string name_ = "";

        public SILVariable(string name)
        {
            Name = name;
        }

        public virtual string Name 
        { 
            get { return name_; }
            set
            {
                value = value.Trim();
                ValidateName(value);
                name_ = value;
            }
        }
        public virtual ISILObject Value
        {
            get 
            { 
                return SymbolTable.Get(name_); 
            }
            set
            {
                SymbolTable.Set(name_, (SILInteger)value.Value); 
                //if variable name does not exist in SymbolTable, new entry will be created
                //otherwise old entree's value will be overwrittens
            }
        }

        public static implicit operator int(SILVariable silVar)
        {
            return (SILInteger)silVar.Value;
        }

        /// <summary>
        /// Validates the naming of the variable, if validation fails an exception is thrown
        /// </summary>
        /// <param name="name"></param>
        static void ValidateName(string name)
        {
            //validate for illegal characters
            foreach (char c in name)
            {
                if (!char.IsLetterOrDigit(c) && c != '[' && c != ']' && c != '.')
                    throw new Exception(
                        string.Format("'{0}' is an illegal character in name of variable \"{1}\"",
                        c, name));
            }
            //validate length
            if (name.Length > 20)
                throw new Exception(
                    string.Format("Variable \"{0}\" is must be less then 20 characters long!", name));
            //validate that first character is alphabetic
            if (!char.IsLetter(name[0]))
                throw new Exception(string.Format("Variable \"{0}\" must begin with a letter!", name));
        }

        public virtual ObjectType Type { get { return ObjectType.Variable; } }
    }

    /// <summary>
    /// The miscellaneous structure used to reference arrays
    /// </summary>
    public class SILArrayElement : ISILObject
    {
        ISILObject arrayIndex_;
        string name_;

        public SILArrayElement(string name, ISILObject arrayIndex)
        {
            name_ = name;
            arrayIndex_ = arrayIndex;
        }

        public static SILArrayElement Parse(string fullName, string functionspace)
        {
            fullName = fullName.Trim();
            string[] split = fullName.Split(new char[] { '[' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 2) throw new Exception("Incorrect array declaration!");
            if (!split[1].Contains(']')) 
                throw new Exception("Missing \"]\"!");
            split[1] = split[1].Replace("]", "");

            return new SILArrayElement(functionspace + "." + split[0], 
                Parser.ExpressionParse(functionspace, split[1], new List<ISILObject>()));
        }

        public ISILObject Value
        {
            get
            {
                return ArrayTable.Arrays[name_][(SILInteger)arrayIndex_.Value];
            }
            set
            {
                ArrayTable.Arrays[name_][(SILInteger)arrayIndex_.Value] = (SILInteger)value;
            }
        }

        public ObjectType Type { get { return ObjectType.ArrayElement; } }
    }
    /// <summary>
    /// Recursive data structure
    /// </summary>
    public struct SILEquation : ISILObject
    {
        public ISILObject Argument1 { get; set; }
        public ISILObject Argument2 { get; set; }
        public MathOperator Operation { get; set; }

        public ISILObject Value
        {
            get
            {
                SILInteger val1 = (SILInteger)Argument1.Value;
                SILInteger val2 = (SILInteger)Argument2.Value;

                switch (Operation)
                {
                    case MathOperator.Plus:
                        return (SILInteger)(val1 + val2);
                    case MathOperator.Minus:
                        return (SILInteger)(val1 - val2);
                    case MathOperator.Multiply:
                        return (SILInteger)(val1 * val2);
                    case MathOperator.Divide:
                        return (SILInteger)(val1 / val2);
                    case MathOperator.Less:
                        return (SILInteger)(val1 < val2 ? 1 : 0);
                    case MathOperator.More:
                        return (SILInteger)(val1 > val2 ? 1 : 0);
                    case MathOperator.Equals:
                        return (SILInteger)((int)val1 == (int)val2 ? 1 : 0);
                    case MathOperator.AND:
                        return (SILInteger)(val1 > 0 && val2 > 0 ? 1 : 0);
                    case MathOperator.OR:
                        return (SILInteger)(val1 > 0 || val2 > 0 ? 1 : 0);
                }
                return null;
            }
            set { throw new Exception("SILEquation cannot be assigned to, it is readonly!"); }            
        }

        public ObjectType Type { get { return ObjectType.Equation; } }
    }

    public static class SIL_Math
    {
        static char[] Operators = new char[] { '+', '-', '/', '*', '>', '<', '&', '|', '=' };

        /// <summary>
        /// Checks if the character belongs to mathematical operators list
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsOperator(char c)
        {
            for (int i = 0; i < Operators.Length; i++)
            {
                if (c == Operators[i]) return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if the input string is a mathematical equation
        /// the "-123" would return false however "123-2" would return true
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsEquation(string s)
        {
            s = s.Trim();
            if (s[s.Length - 1] == ']')
            {
                string[] split = s.Split('[');
                if (!IsEquation(split[0]))
                {
                    string innerArgs = split[1].Substring(0, split[1].Length - 1);
                    int bracketCount = 0;
                    foreach (char c in innerArgs)
                    {
                        if (c == '[') bracketCount++;
                        if (c == ']') bracketCount--;
                        if (bracketCount < 0) return true;
                    }
                    return false;
                }
            }
            foreach (char c in Operators)
            {
                if (s.Contains(c))
                    return true;
            }
            return false;
        }
    }
    
}

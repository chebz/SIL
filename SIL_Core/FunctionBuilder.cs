using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SIL_Core
{
    public class FunctionBuilder : ISILObject
    {
        static Dictionary<string, FunctionBuilder> listOfFunctions = new Dictionary<string, FunctionBuilder>();

        List<string> listOfParameters = new List<string>();

        Queue<SIL_Action> listOfActions = new Queue<SIL_Action>();

        private int valueOfReturn;

        public FunctionBuilder(SIL_Action[] sil_Action, string[] listOfParameters)
        {
            foreach (SIL_Action action in sil_Action)
            {
                listOfActions.Enqueue(action);
            }
            this.listOfParameters = listOfParameters.ToList();
        }

        public int getReturnValue()
        {
            return valueOfReturn;
        }

        public static bool functionHasBeenDeclaired(string nameOfFunction)
        {

            nameOfFunction = nameOfFunction.Trim();

            if ( nameOfFunction.Contains('(') )
                nameOfFunction = nameOfFunction.Remove(nameOfFunction.IndexOf('('));

            //validate for illegal characters
            foreach (char c in nameOfFunction)
            {
                if (!char.IsLetterOrDigit(c))
                    throw new Exception(
                        string.Format("'{0}' is an illegal character in name of function \"{1}\"",
                        c, nameOfFunction));
            }
            //validate length
            if (nameOfFunction.Length > 20)
                throw new Exception(
                    string.Format("function \"{0}\" is must be less then 20 characters long!", nameOfFunction));
            //validate that first character is alphabetic
            if (!char.IsLetter(nameOfFunction[0]))
                throw new Exception(string.Format("function \"{0}\" must begin with a letter!", nameOfFunction));

            return listOfFunctions.ContainsKey(nameOfFunction.ToUpper());
        }
        /// <summary>
        /// This method will remove all the function objects that have been created.  Need to be run everytime a new program is run.
        /// </summary>
        public static void resetFunctionBuilder()
        {
            listOfFunctions.Clear();
        }

        /// <summary>
        /// Adds a function to the class Dictionary
        /// </summary>
        /// <param name="Name"></param>
        public static void Add(string Name, FunctionBuilder functionBuilder)
        {
            listOfFunctions.Add(Name, functionBuilder);
        }

        /// <summary>
        /// return a Function object to run a list of SIL_Actions.
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public static FunctionBuilder Get(string nameOfFunction)
        {
            nameOfFunction = nameOfFunction.Trim();

            if (nameOfFunction.Contains('('))
                nameOfFunction = nameOfFunction.Remove(nameOfFunction.IndexOf('('));

            //validate for illegal characters
            foreach (char c in nameOfFunction)
            {
                if (!char.IsLetterOrDigit(c))
                    throw new Exception(
                        string.Format("'{0}' is an illegal character in name of function \"{1}\"",
                        c, nameOfFunction));
            }
            //validate length
            if (nameOfFunction.Length > 20)
                throw new Exception(
                    string.Format("function \"{0}\" is must be less then 20 characters long!", nameOfFunction));
            //validate that first character is alphabetic
            if (!char.IsLetter(nameOfFunction[0]))
                throw new Exception(string.Format("function \"{0}\" must begin with a letter!", nameOfFunction));

            if (!listOfFunctions.ContainsKey(nameOfFunction.ToUpper()))
                throw new Exception(String.Format("Function \"{0}\" was not found!", nameOfFunction));
            return listOfFunctions[nameOfFunction.ToUpper()];
        }

        public int executeFunction(string unparsedArgs, ref object[] parsedArgs)
        {
            setValueOfLocalVariables(unparsedArgs, ref parsedArgs);
            return Compiler.FUNCTION_Execute(listOfActions);
        }

        private void setValueOfLocalVariables(string unparsedArgs, ref object[] parsedArgs)
        {
            // string[] parameters = unparsedArgs.Split(' ');

            string nameOfFunction = unparsedArgs.Substring(unparsedArgs.IndexOf('='));
            string namesOfVariables = nameOfFunction.Substring(unparsedArgs.IndexOf('(') + 1);
            namesOfVariables = namesOfVariables.Trim(')');
            nameOfFunction = nameOfFunction.Remove(unparsedArgs.IndexOf('('));

            string[] namesOfVariablesArray = namesOfVariables.Split(',');

            parsedArgs = new object[namesOfVariablesArray.Length];

            parsedArgs[0] = nameOfFunction;

            for (int i = 1; i < namesOfVariablesArray.Length; ++i)
            {
                parsedArgs[i] = namesOfVariablesArray[i - 1];
            }
        }

        /*
        /// <summary>
        ///  back up function for Function_Parse not to be used.
        /// </summary>
        /// <param name="unparsedArgs"></param>
        /// <param name="parsedArgs"></param>
        /// <param name="lines"></param>
        /// <param name="lineNumber"></param>
        /// <returns></returns>
        private static ActionType FUNCTION_Parse(string unparsedArgs, ref object[] parsedArgs, string[] lines, int lineNumber)
        {
            string[] parameters = unparsedArgs.Split(' ');
            int endLineOfFunction = 0;

            if (parameters.Length < 2) throw new Exception("Function command must be followed by name to call!");

            // check for list of parameters.
            if (!parameters[1].Contains('('))
                throw new Exception("Function command must be formatted correctly, missing (");
            if (!parameters[1].Contains(')'))
                throw new Exception("Function command must be formatted correctly, missing )");

            // check body of function
            if (!(lines[lineNumber].ToUpper() == "BEGIN"))
                throw new Exception("Function command must be followed by BEGIN!");

            for (int i = lineNumber + 2; i < lines.Length; ++i)
            {
                if (lines[i].ToUpper() == "BEGIN")
                    throw new Exception("Function command must be closed by END");

                if (lines[i].ToUpper() == "FUNCTION")
                    throw new Exception("Function command must be closed by END");

                if (lines[i].ToUpper() == "END")
                {
                    endLineOfFunction = i;
                    break;
                }
            }

            if (endLineOfFunction == 0)
            {
                throw new Exception("Function command must be closed by END");
            }

            // declair local variables

            string tempLocalVariables = parameters[1].TrimEnd(')');
            string nameOfFunction = parameters[1].Remove(parameters[1].IndexOf('('));
            tempLocalVariables = tempLocalVariables.Substring(tempLocalVariables.IndexOf('(') + 1);
            string[] localVariables = tempLocalVariables.Split(',');

            SIL_Action[] parsedArgsForFunction = new SIL_Action[endLineOfFunction - 1 - lineNumber + 2];

            int counterForParsedArgs = 0;
            for (int i = lineNumber + 1; i < endLineOfFunction; ++i)
            {
                if (lines[i].ToUpper() == "BEGIN")
                    continue;

                if (lines[i].ToUpper() == "END")
                    break;

                parsedArgs[counterForParsedArgs] = Parse(lines[i], i, lines);
                counterForParsedArgs++;
            }
        }
         * */
        /*
        /// <summary>
        /// backup for Execute Function
        /// </summary>
        /// <param name="actions_"></param>
        /// <returns></returns>
        private static int ExecuteFunction(Queue<SIL_Action> actions_)
        {
            for (int i = 0; i < actions_.Count; ++i)
            {
                if (actions_.ElementAt(i).ActionType == ActionType.FUNCTION)
                    continue;
                SIL_Action action = actions_.ElementAt(i);
                executeSymbols_[action.ActionType](action); //first execute internal logic
                //action.Execute(); //lin action with GUI
            }

            return 0;
        }
         * */

        public ObjectType Type { get { return ObjectType.Function; } }

        SILInteger ISILObject.Value
        {
            get
            {
                SILInteger silInt = new SILInteger();
                silInt.Value = valueOfReturn;
                return silInt;
            }
        }
    }
}

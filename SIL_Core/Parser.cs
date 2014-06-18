using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SIL_Core
{
    public enum ActionType
    {
        None,
        PRINT, PRINTLN, READ,
        LET,
        INTEGER, ARRAY,
        IF, ENDIF,
        WHILE, WEND,
        FOR, NEXT,
        DECLARE, FUNCTION, ENDFUNCTION, CALL, RETURN,
        DRAWLINE
    }

    /// <summary>
    /// Class the performs a single action of code like PRINT, LET, or arithmetic
    /// </summary>
    public class SIL_Action
    {
        object[] parsingArgs_, executionArgs_;
        ActionType actionType_;
        int lineNumber_;
        string functionspace_;

        public delegate void Action(ActionType actionType, object[] args);
        public event Action OnAction;

        public SIL_Action(ActionType actionType, object[] parsingArgs, int lineNumber, string functionspace)
        {
            parsingArgs_ = parsingArgs;
            actionType_ = actionType;
            lineNumber_ = lineNumber;
            functionspace_ = functionspace;
        }

        public void Execute()
        {
            OnAction(actionType_, executionArgs_);
        }

        public object[] ParsingArgs { get { return parsingArgs_; } set { parsingArgs_ = value; } }
        public object[] ExecutionArgs { get { return executionArgs_; } set { executionArgs_ = value; } }
        public ActionType ActionType { get { return actionType_; } }
        public int LineNumber { get { return lineNumber_; } }
        public string Functionspace { get { return functionspace_; } }
    }

    /// <summary>
    /// Class responsible for parsing the code
    /// </summary>
    public static class Parser
    {
        static string[] sourceLines_;
        static Compiler.Action exeAction_;

        delegate ActionType ParseAction(
            string functionspace, 
            string unparsedArgs, 
            ref object[] parsedArgs, 
            ref int lineNumber); //action for parsing
        static Dictionary<string, ParseAction> parserSymbols_;

        public static void Initialize()
        {
            parserSymbols_ = new Dictionary<string, ParseAction>();
            parserSymbols_.Add("PRINT", PRINT_Parse);
            parserSymbols_.Add("READ", READ_Parse);
            parserSymbols_.Add("LET", LET_Parse);
            parserSymbols_.Add("INTEGER", INTEGER_Parse);
            parserSymbols_.Add("ARRAY", ARRAY_Parse);
            parserSymbols_.Add("IF", IF_Parse);
            parserSymbols_.Add("ENDIF", ENDIF_Parse);
            parserSymbols_.Add("WHILE", WHILE_Parse);
            parserSymbols_.Add("WEND", WEND_Parse);
            parserSymbols_.Add("FOR", FOR_Parse);
            parserSymbols_.Add("NEXT", NEXT_Parse);
            parserSymbols_.Add("PRINTLN", PRINTLN_Parse);
            parserSymbols_.Add("DECLARE", DECLARE_Parse);
            parserSymbols_.Add("FUNCTION", FUNCTION_Parse);
            parserSymbols_.Add("ENDFUNCTION", ENDFUNCTION_Parse);
            parserSymbols_.Add("CALL", CALL_Parse);
            parserSymbols_.Add("RETURN", RETURN_Parse);
            parserSymbols_.Add("DRAWLINE", DRAWLINE_Parse);

        }

        /// <summary>
        /// Converts a line of code in SIL Action
        /// </summary>
        /// <param name="line">full unedited line</param>
        /// <returns>new SIL_Action object</returns>
        public static Queue<SIL_Action> Parse(string source, Compiler.Action exeAction)
        {
            sourceLines_ = source.Split('\n');
            exeAction_ = exeAction;
            Queue<SIL_Action> actions = new Queue<SIL_Action>();

            for (int i = 0; i < sourceLines_.Length; i++)
            {
                SIL_Action action = Parser.ParseNext("Main", ref i);
                if (action!= null) actions.Enqueue(action);
            }
            return actions;
        }

        static SIL_Action ParseNext(string functionspace, ref int lineNumber)
        {
            string line = sourceLines_[lineNumber].Trim();
            while ((line == "" || (line.Length >= 2 ? line.Substring(0,2) == "//" : false))
                && lineNumber < sourceLines_.Length)
            {
                lineNumber++;
                line= sourceLines_[lineNumber].Trim();
            }
            if (lineNumber == sourceLines_.Length) return null;

            string[] symbols = line.Split(' ');

            if (!parserSymbols_.ContainsKey(symbols[0]))
                throw new Exception(string.Format("Unidentified command: {0}%{1}", symbols[0], lineNumber));
            object[] paramters = null;

            //We catch internal exception here and format it for future display in GUI
            try
            {

                ActionType actionType = parserSymbols_[symbols[0]](
                    functionspace, line, ref paramters, ref lineNumber);
                SIL_Action action = new SIL_Action(actionType, paramters, lineNumber, functionspace);
                action.OnAction += new SIL_Action.Action(exeAction_);
                return action;
            }
            catch (Exception parseSymbolException)
            {
                throw new Exception(string.Format("{0}%{1}", parseSymbolException.Message, lineNumber));
            }
        }

        /// <summary>
        /// Call's PRINT but uses a differnt ActionTYpe that must be implemented in the UI output windows or console.
        /// </summary>
        /// <param name="unparsedArgs"></param>
        /// <param name="parsedArgs"></param>
        /// <param name="lineNumber"></param>
        /// <returns></returns>
        static ActionType PRINTLN_Parse(string functionspace, string unparsedArgs, ref object[] parsedArgs, ref int lineNumber)
        {
            PRINT_Parse(functionspace, unparsedArgs, ref parsedArgs, ref lineNumber);

            return ActionType.PRINTLN;
        }

        /// <summary>
        /// Parses PRINT command
        /// </summary>
        /// <param name="unparsedArgs">a line of everything including the command symbol</param>
        /// <param name="parsedArgs">ann array of parsed objects which can be strings, variables, or
        /// math operations</param>
        /// <returns>ActionType.PRINT</returns>
        static ActionType PRINT_Parse(string functionspace, string unparsedArgs, ref object[] parsedArgs, ref int lineNumber)
        {            
            string[] parameters = unparsedArgs.Split(' ');

            if (parameters.Length < 2) throw new Exception("PRINT command must be followed by message to display!");
            bool qouteOpen= false, plusSign = false;
            string message = "", oneMessage = "";
            List<object> finalMessages = new List<object>();
            for (int i = 1; i < parameters.Length; i++)
            {
                message += parameters[i] + ((i < parameters.Length - 1) ? " " : "");
            }

            for (int i = 0; i < message.Length; i++)
            {
                //Process quoted messages
                if (message[i] == '"' && !qouteOpen)
                {
                    if (finalMessages.Count > 0 && !plusSign)
                        throw new Exception("+ expected!");
                    qouteOpen = true;
                    plusSign = false;
                    continue;
                }
                if (message[i] == '"' && qouteOpen)
                {
                    qouteOpen = false;
                    finalMessages.Add(oneMessage);
                    oneMessage = "";
                    continue;
                }
                //check that quote is closed before message ends
                if (qouteOpen)
                {
                    if (i == message.Length - 1) throw new Exception("\" Expected!");
                    oneMessage += message[i];
                    continue;
                }

                //process unquoted messages
                if (!qouteOpen)
                {
                    //process numner
                    if (Char.IsNumber(message[i]))
                    {
                        if (finalMessages.Count > 0 && !plusSign)
                            throw new Exception("+ expected!");
                        plusSign = false;
                        while (message[i] != ' ' && message[i] != '+')
                        {
                            if (!Char.IsNumber(message[i]))
                                throw new Exception("Incorrect argument, numeric character is expected!");
                            oneMessage += message[i];
                            i++;
                            if (i == message.Length) break;
                        }
                        finalMessages.Add(oneMessage);
                        continue;
                    }
                    //process variable
                    if (Char.IsLetter(message[i]))
                    {
                        if (finalMessages.Count > 0 && !plusSign)
                            throw new Exception("+ expected!");
                        plusSign = false;
                        string varName = "";
                        while (message[i] != ' ' && message[i] != '+')
                        {
                            varName += message[i];
                            i++;
                            if (i == message.Length) break;
                        }
                        ISILObject variable = Retrieve_SIL_Object(functionspace, varName, null);
                        finalMessages.Add(variable);
                        continue;
                    }
                    if (message[i] == ' ')
                    {
                        continue;
                    }
                    if (message[i] == '+')
                    {
                        plusSign = true;                       
                        continue;
                    }
                    throw new Exception("Unknown symbol!");
                }
            }
            parsedArgs = new object[finalMessages.Count];
            finalMessages.ToArray().CopyTo(parsedArgs, 0);
            return ActionType.PRINT;
        }

        static ActionType READ_Parse(string functionspace, string unparsedArgs, ref object[] parsedArgs, ref int lineNumber)
        {
            string parameters = unparsedArgs.Substring(5, unparsedArgs.Length - 5).Trim();
            string[] split = parameters.Split(new char[] { ',' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 2) throw new Exception("Syntax error!");
            split[1] = split[1].Trim();
            if (split[1][0] != '\"' || split[1][split[1].Length - 1] != '\"')
                throw new Exception("Missing \"!");
            split[1] = split[1].Substring(1, split[1].Length - 2);
            ISILObject variable = Retrieve_SIL_Object(functionspace, split[0], null);
            parsedArgs = new object[] { variable, split[1] };
            return ActionType.READ;
        }

        static ActionType DECLARE_Parse(string functionspace, string unparsedArgs, ref object[] parsedArgs, ref int lineNumber)
        {
            //DECLARE FUNCTION testFunction(INTEGER x, ARRAY arr) AS INTEGER
            string parameters = unparsedArgs.Substring(8, unparsedArgs.Length - 8);
            if (parameters.Substring(0, 9) != "FUNCTION ") throw new Exception("FUNCTION keyword expected!");
            parameters = parameters.Substring(9, parameters.Length - 9);
            string functionName = "";
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i] != '(') functionName += parameters[i];
                else
                {
                    parameters = parameters.Substring(i + 1, parameters.Length - i - 1);
                    break;
                }
                if (i == parameters.Length - 1) throw new Exception("\"(\" expected!");
            }
            if (!parameters.Contains(')')) throw new Exception("\")\" expected!");
            string[] split = parameters.Split(new char[]{')'}, StringSplitOptions.RemoveEmptyEntries);
            if (split[split.Length - 1].Length < 4) throw new Exception("Syntax error!");
            if (split[split.Length - 1].Substring(0, 4) != " AS ") throw new Exception("AS expected!");
            //Get return type
            string returnType_s = split[split.Length - 1].Substring(4, split[split.Length - 1].Length - 4);
            if (returnType_s.Length == 0) throw new Exception("Missing type! Must be integer, array, or void!");
            ObjectType returnType = ObjectType.Void;
            switch (returnType_s)
            {
                case "INTEGER":
                    returnType = ObjectType.Integer;
                    break;
                case "ARRAY":
                    returnType = ObjectType.Array;
                    break;
                case "VOID":
                    returnType = ObjectType.Void;
                    break;
                default:
                    throw new Exception(
                        string.Format("Incorrect return type \"{0}\"! Must be integer, array, or void!",
                        split[1]));
            }
            //Get Parameter Types      
            ObjectType[] parameterTypes = new ObjectType[]{};
            if (split.Length == 2)
            {
                string[] parameterTypes_s = split[0].Split(',');
                parameterTypes = new ObjectType[parameterTypes_s.Length];
                for (int i = 0; i < parameterTypes_s.Length; i++)
                {
                    parameterTypes_s[i] = parameterTypes_s[i].Trim();
                    switch (parameterTypes_s[i])
                    {
                        case "INTEGER":
                            parameterTypes[i] = ObjectType.Integer;
                            break;
                        case "ARRAY":
                            parameterTypes[i] = ObjectType.Array;
                            break;
                        case "VOID":
                            parameterTypes[i] = ObjectType.Void;
                            break;
                        default:
                            throw new Exception(
                                string.Format("Incorrect parameter type \"{0}\"! Must be integer, array, or void!",
                                parameterTypes_s[1]));
                    }
                }
            }

            FunctionPrototype prototype = new FunctionPrototype();
            prototype.Actions = null;
            prototype.ParameterTypes = parameterTypes;
            prototype.ReturnType = returnType;
            FunctionTable.Add(functionName, prototype);

            return ActionType.DECLARE;
        }

        static ActionType FUNCTION_Parse(string functionspace, string unparsedArgs, ref object[] parsedArgs, ref int lineNumber)
        {
            Queue<SIL_Action> actions = new Queue<SIL_Action>();

            //FUNCTION MyFunction(INTEGER x, ARRAY y)
            string parameters = unparsedArgs.Substring(9, unparsedArgs.Length - 9);
            if (!parameters.Contains("(")) throw new Exception("\"(\" Expected!");
            string[] split = parameters.Split(new char[] { '(' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length != 2) throw new SyntaxErrorException();
            string functionName = split[0];
            FunctionPrototype prototype = FunctionTable.Get(functionName);

            //Parse parameters & their types
            split[1] = split[1].Trim();
            if (split[1][split[1].Length-1] != ')') throw new Exception("\")\" Expected!");
            split[1] = split[1].Substring(0, split[1].Length - 1);
            string[] split2 = split[1].Split(',');
            if (split2.Length == 1 && split2[0].Trim() == "") split2 = new string[] { };
            if (split2.Length != prototype.ParameterTypes.Length)
                throw new Exception("Incorrect number of arguments!");
            List<string> paramNames = new List<string>();
            for (int i = 0; i < split2.Length; i++)
            {
                split2[i] = split2[i].Trim();
                if (split2[i] == "") 
                    throw new Exception(string.Format("Missing parameter type and name for argument #{0}!", i));
                if (!split2[i].Contains(" ")) 
                    throw new Exception(string.Format("Missing parameter type or name for argument #{0}!", i));
                string[] split3 = split2[i].Split(' ');
                if (split3.Length != 2) throw new SyntaxErrorException();
                string parameterType_s = split3[0].Trim();
                string parameterName = split3[1].Trim();
                paramNames.Add(parameterName);
                ObjectType parameterType;
                switch (parameterType_s)
                {
                    case "INTEGER":
                        parameterType = ObjectType.Integer;
                        break;
                    case "ARRAY":
                        parameterType = ObjectType.Array;
                        break;
                    default:
                        throw new Exception(
                            string.Format("\"{0}\" is not a supported parameter type, only INTEGER and ARRAY are supported!",
                            parameterType_s));
                }
                if (parameterType != prototype.ParameterTypes[i]) throw new Exception(
                    string.Format("Incorrect type of parameter #{0}, {1} expexted!",
                    i, Enum.GetName(typeof(ObjectType), prototype.ParameterTypes[i])));
            }

            SIL_Action action;
            do
            {
                lineNumber++;
                if (lineNumber >= sourceLines_.Length)
                    throw new Exception("ENDFUNCTION expected!");
                action = ParseNext(functionName, ref lineNumber);
                if (action.ActionType == ActionType.FUNCTION)
                    throw new Exception("ENDFUNCTION expected!");
                if (action != null) actions.Enqueue(action);
            } while (action.ActionType != ActionType.ENDFUNCTION);

            prototype.Actions = actions.ToArray();
            prototype.ParamNames = paramNames.ToArray();

            return ActionType.FUNCTION;
        }

        static ActionType ENDFUNCTION_Parse(string functionspace, string unparsedArgs, ref object[] parsedArgs, ref int lineNumber)
        {
            return ActionType.ENDFUNCTION;
        }

        static ActionType CALL_Parse(string functionspace, string unparsedArgs, ref object[] parsedArgs, ref int lineNumber)
        {
            string parameters = unparsedArgs.Substring(5, unparsedArgs.Length - 5);

            if (!parameters.Contains("(")) throw new Exception("\"(\" Expected!");
            string[] split = parameters.Split(new char[] { '(' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length != 2) throw new SyntaxErrorException();
            string functionName = split[0];
            FunctionPrototype prototype = FunctionTable.Get(functionName);

            //Parse parameters & their types
            split[1] = split[1].Trim();
            if (split[1][split[1].Length-1] != ')') throw new Exception("\")\" Expected!");
            split[1] = split[1].Substring(0, split[1].Length - 1);
            string[] split2 = split[1].Split(',');
            if (split2.Length == 1 && split2[0].Trim() == "") split2 = new string[] { };
            if (split2.Length != prototype.ParameterTypes.Length)
                throw new Exception("Incorrect number of arguments!");
            List<ISILObject> functionParameters = new List<ISILObject>();
            for (int i = 0; i < split2.Length; i++)
            {
                string parameter_s = split2[i].Trim();
                if (parameter_s == "")
                    throw new Exception(string.Format("Missing parameter type and name for argument #{0}!", i));
                ISILObject parameter;
                if (SIL_Math.IsEquation(parameter_s))
                    parameter = ExpressionParse(functionspace, parameter_s, new List<ISILObject>());
                else parameter = Retrieve_SIL_Object(functionspace, parameter_s, null);                 

                if ((prototype.ParameterTypes[i] == ObjectType.Array && parameter.Type != ObjectType.Array) ||
                        (prototype.ParameterTypes[i] == ObjectType.Integer && parameter.Type == ObjectType.Array))
                    throw new Exception(
                        string.Format("Incorrect type of parameter #{0}, {1} expexted!",
                        i, Enum.GetName(typeof(ObjectType), prototype.ParameterTypes[i])));
                functionParameters.Add(parameter);
            }

            SILFunction function = new SILFunction(functionName, functionParameters.ToArray());
            
            parsedArgs = new object[] { function };
            return ActionType.CALL;
        }

        static ActionType RETURN_Parse(string functionspace, string unparsedArgs, ref object[] parsedArgs, ref int lineNumber)
        {
            string parameters = unparsedArgs.Substring(7, unparsedArgs.Length - 7);
            ISILObject returnValue = ExpressionParse(functionspace, parameters, new List<ISILObject>());
            parsedArgs = new object[] { returnValue };
            return ActionType.RETURN;
        }

        static ActionType INTEGER_Parse(string functionspace, string unparsedArgs, ref object[] parsedArgs, ref int lineNumber)
        {
            string parameters = unparsedArgs.Substring(8, unparsedArgs.Length - 8);

            parameters = parameters.Trim();
            foreach (char c in parameters)
            {
                if (!char.IsLetterOrDigit(c) && c != '[' && c != ']')
                    throw new Exception(
                            string.Format("'{0}' is an illegal character in name of variable \"{1}\"",
                            c, parameters));
            }
            parsedArgs = new object[] { functionspace + "." + parameters };

            return ActionType.INTEGER;
        }

        static ActionType ARRAY_Parse(string functionspace, string unparsedArgs, ref object[] parsedArgs, ref int lineNumber)
        {
            string parameters = unparsedArgs.Substring(6, unparsedArgs.Length - 6);

            parameters = parameters.Trim();
            string[] split = parameters.Split(new char[]{'['}, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 2) throw new Exception("Incorrect array declaration!");
            string arrayName = split[0];
            if (!split[1].Contains(']')) throw new Exception("Missing \"]\"!");
            split[1] = split[1].Replace("]", "");
            int arraySize = 0;
            if (!int.TryParse(split[1], out arraySize)) throw new Exception("Incorrect array size specified!");

            SILArray array = new SILArray(functionspace + "." + arrayName, arraySize);
            parsedArgs = new object[] { array };

            return ActionType.ARRAY;
        }

        /// <summary>
        /// Parses LET function
        /// </summary>
        /// <param name="unparsedArgs"></param>
        /// <param name="parsedArgs"></param>
        /// <returns></returns>
        static ActionType LET_Parse(string functionspace, string unparsedArgs, ref object[] parsedArgs, ref int lineNumber)
        {
            //remove LET word
            string parameters = unparsedArgs.Substring(4, unparsedArgs.Length - 4);

            if (parameters.Length < 2) throw new Exception("LET command must be followed by assignment operation!");

            string[] args = parameters.Split('=');
            if (args.Length != 2) throw new Exception("Syntax error!");

            ISILObject varOrArrElement = Retrieve_SIL_Object(functionspace, args[0], null);
            ISILObject var = ExpressionParse(functionspace, args[1], new List<ISILObject>());

            parsedArgs = new object[] { varOrArrElement, var };

            return ActionType.LET;
        }

        #region Expression Parsing
        public static ISILObject ExpressionParse(string functionspace, string unparsedArgs, List<ISILObject> equations)
        {
            unparsedArgs = unparsedArgs.Trim();

            //Check if it is not an equation but a variable, otherwise recurse until it is a single object
            if (!SIL_Math.IsEquation(unparsedArgs) && !unparsedArgs.Contains('['))
            {
                return Retrieve_SIL_Object(functionspace, unparsedArgs, equations);         
            }

            int leftIndex = 0,
                rightIndex = 0;
            string deepestEquation;

            //parse arrays first
            if (unparsedArgs.Contains('['))
            {
                deepestEquation = FindDeepest(unparsedArgs, '[', ']', ref rightIndex, ref leftIndex);

                ISILObject arrIndex = ProcessEquation(functionspace, deepestEquation, equations);
                string arrName = "";
                leftIndex--;
                while (leftIndex > 0 ? 
                    (!SIL_Math.IsOperator(unparsedArgs[leftIndex - 1]) && 
                        unparsedArgs[leftIndex - 1] != '[' && 
                        unparsedArgs[leftIndex - 1] != '(') : false)
                {
                    leftIndex--;
                    arrName += unparsedArgs[leftIndex];
                }
                arrName = Reverse(arrName).Trim();
                int leftLength = leftIndex;
                string leftPart = leftLength > 0 ? unparsedArgs.Substring(0, leftLength) : "";
                rightIndex = rightIndex == unparsedArgs.Length ? rightIndex : rightIndex + 1;
                int rightLength = unparsedArgs.Length - rightIndex;
                string rightPart = rightLength > 0 ? unparsedArgs.Substring(rightIndex, rightLength) : "";

                equations.Add(new SILArrayElement(functionspace + "." + arrName, arrIndex));
                unparsedArgs = leftPart +
                    "{" + (equations.Count - 1).ToString() + "}" +
                    rightPart;

                return ExpressionParse(functionspace, unparsedArgs, equations);
            }
            deepestEquation = FindDeepest(unparsedArgs, '(', ')', ref rightIndex, ref leftIndex);

            if (leftIndex > 1)
            {
                if (char.IsLetterOrDigit(unparsedArgs[leftIndex - 2]))
                {
                    string functionName = "";
                    leftIndex--;
                    while (leftIndex > 0 ?
                        (!SIL_Math.IsOperator(unparsedArgs[leftIndex - 1]) &&
                            unparsedArgs[leftIndex - 1] != '[' &&
                            unparsedArgs[leftIndex - 1] != '(') : false)
                    {
                        leftIndex--;
                        functionName += unparsedArgs[leftIndex];
                    }
                    functionName = Reverse(functionName).Trim();

                    int leftLength = leftIndex;
                    string leftPart = leftLength > 0 ? unparsedArgs.Substring(0, leftLength) : "";
                    rightIndex = rightIndex == unparsedArgs.Length ? rightIndex : rightIndex + 1;
                    int rightLength = unparsedArgs.Length - rightIndex;
                    string rightPart = rightLength > 0 ? unparsedArgs.Substring(rightIndex, rightLength) : "";
                    
                    string function_s = unparsedArgs.Substring(leftLength, rightIndex - leftLength).Trim();
                    SILFunction function;
                    object[] functionArgs = null;
                    int linenumber = 0;
                    CALL_Parse(functionspace, "CALL " + function_s, ref functionArgs, ref linenumber);
                    function = (SILFunction)functionArgs[0];

                    equations.Add(function);
                    unparsedArgs = leftPart +
                        "{" + (equations.Count - 1).ToString() + "}" +
                        rightPart;

                    return ExpressionParse(functionspace, unparsedArgs, equations);
                }
            }

            if (!SIL_Math.IsEquation(deepestEquation))
            {
                unparsedArgs = unparsedArgs.Replace("(" + deepestEquation + ")", deepestEquation.Trim());
            }
            else
            {
                equations.Add((SILEquation)ProcessEquation(functionspace, deepestEquation, equations));
                int leftLength = leftIndex > 0 ? leftIndex - 1 : 0;
                string leftPart = leftLength > 0 ? unparsedArgs.Substring(0, leftLength) : "";
                rightIndex = rightIndex == unparsedArgs.Length ? rightIndex : rightIndex + 1;
                int rightLength = unparsedArgs.Length - rightIndex;
                string rightPart = rightLength > 0 ? unparsedArgs.Substring(rightIndex, rightLength) : "";
                unparsedArgs = leftPart +
                    "{" + (equations.Count - 1).ToString() + "}" +
                    rightPart;                
            }
            return ExpressionParse(functionspace, unparsedArgs, equations);
        }

        static string FindDeepest(string unparsedArgs, char bracket1, char bracket2, ref int rightIndex, ref int leftIndex)
        {
            // find the deepest bracketed equation
            // we get through the entire equation character by character
            // if we encounter '(' we add to bracketCount
            // ')' we subtract from bracketCount
            // if bracketCount is greater then last maxBracketCount we overwrite it
            // and store starting position of the new deepest bracketed equation
            int bracketCount = 0,
                maxBracketCount = 0;

            for (int i = 0; i < unparsedArgs.Length; i++)
            {
                if (unparsedArgs[i] == bracket1)
                {
                    bracketCount++;
                    if (bracketCount > maxBracketCount)
                    {
                        maxBracketCount = bracketCount;
                        leftIndex = i + 1;
                    }
                }
                if (unparsedArgs[i] == bracket2)
                {
                    bracketCount--;

                    if (bracketCount < 0) // make sure the brackets are placed correctly
                        throw new Exception("'(' Expected!");
                }
            }

            //once we iterated through entire equation we make sure the final bracketCount is 0,
            //i.e. there are equal number of '(' and ')'
            //we already checked for closing bracket ')' inside the loop, so at this point only
            //extra '(' can occur - so we throw the following exception
            if (bracketCount != 0) throw new Exception("')' Expected!");

            //If maxBracketCount > 0 then there are brackets used in the equation, and we need to find the
            //closing bracket for the deepest equation to separate it from the rest of the equation
            if (maxBracketCount > 0)
            {
                for (int i = leftIndex; i < unparsedArgs.Length; i++)
                {
                    if (unparsedArgs[i] == bracket2)
                    {
                        rightIndex = i;
                        break; //closing position is already found, no need to iterate through the rest
                    }
                }
            }
            else rightIndex = unparsedArgs.Length;

            //retrieve the deepest bracketed part of subequation and convert it into an equation class
            return unparsedArgs.Substring(leftIndex, rightIndex - leftIndex);
        }

        struct OperatorPosition
        {
            public int position;
            public char c;
        }
        /// <summary>
        /// Converts the input equation string into a SIL_Equation recursive datastructure
        /// </summary>
        /// <param name="equation">basic input eqaution, must NOT contain any brackets!</param>
        /// <returns>Converted equation class which can be calculated</returns>
        static ISILObject ProcessEquation(string functionspace, string equationString, List<ISILObject> equations)
        {
            //always returns equation
            if (!SIL_Math.IsEquation(equationString))
            {
                return Retrieve_SIL_Object(functionspace, equationString, equations);
            }

            char[] separators = { '/', '*', '+', '-', '&', '|', '<', '>', '=' };

            string leftVar = "",
                    rightVar = "",
                    equationSubstring = "";
            int leftIndex = 0,
                rightIndex = 0;
            SILEquation equation = new SILEquation();

            equationString = equationString.Trim();
            //multiplication and division needs to be taken care of first!


            List<OperatorPosition> firstPos = new List<OperatorPosition>();

            for (int i = 0; i < separators.Length; i++)
            {
                int pos = equationString.IndexOf(separators[i]);
                if (pos >= 0)
                {
                    OperatorPosition op = new OperatorPosition();
                    op.position = pos;
                    op.c = separators[i];
                    firstPos.Add(op);
                }
                if (i == 1 && firstPos.Count > 0) break;
            }
            firstPos.Sort(SortFirstOperators);

            equation.Operation = ConvertToOperator(firstPos[0].c);
            //now find the two terms this operation applies to and build new SIL_Equation class
            //find left variable                
            for (int i = firstPos[0].position - 1; i >= 0; i--)
            {
                leftIndex = i;
                if (SIL_Math.IsOperator(equationString[i]))
                {
                    if (i == 1) leftIndex--; //allow negative numbers
                    break;
                }
                leftVar += equationString[i];
            }
            leftVar = leftVar.Trim();
            if (leftVar.Length == 0) leftVar = "0";
            else leftVar = Reverse(leftVar); //reverse the string b/c I read it from right to left
            equation.Argument1 = Retrieve_SIL_Object(functionspace, leftVar, equations);

            //find right variable   
            for (int i = firstPos[0].position + 1; i < equationString.Length; i++)
            {
                if (SIL_Math.IsOperator(equationString[i])) break;
                rightIndex = i;
                rightVar += equationString[i];
            }
            rightIndex++;
            rightVar = rightVar.Trim();
            if (rightVar.Length == 0) rightVar = "0";

            equation.Argument2 = Retrieve_SIL_Object(functionspace, rightVar, equations);

            equationSubstring = equationString.Substring(leftIndex, rightIndex - leftIndex);

            string leftPart = leftIndex > 0 ? equationString.Substring(0, leftIndex + 1) : "";
            int rightLength = equationString.Length - rightIndex;
            string rightPart = rightLength > 0 ? equationString.Substring(rightIndex, rightLength) : "";
            equationString = leftPart +
                "{" + equations.Count.ToString() + "}" +
                rightPart;
            equations.Add(equation);

            return ProcessEquation(functionspace, equationString, equations);
        }

        private static int SortFirstOperators(OperatorPosition x, OperatorPosition y)
        {
            if (x.position == y.position) return 0;
            if (x.position > y.position) return 1;
            return -1;            
        }

        static MathOperator ConvertToOperator(char c)
        {
            if (c == '+') return MathOperator.Plus;
            if (c == '-') return MathOperator.Minus;
            if (c == '*') return MathOperator.Multiply;
            if (c == '/') return MathOperator.Divide;
            if (c == '&') return MathOperator.AND;
            if (c == '|') return MathOperator.OR;
            if (c == '<') return MathOperator.Less;
            if (c == '>') return MathOperator.More;
            return MathOperator.Equals;
        }
        //Might be a good idea to move the below functions somewhere else?

        /// <summary>
        /// This function parses a variable into a correct SIL_Object class
        /// {number} = equation pulled from equations array
        /// xyz = user variable
        /// 123 = system variable
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="equations"></param>
        /// <returns></returns>
        static ISILObject Retrieve_SIL_Object(string functionspace, string variable, List<ISILObject> equations)
        {
            variable = variable.Trim();
            //is it equation?
            if (variable.Contains('{'))
            {
                //remove the brackets
                variable = variable.Replace("{", "");
                variable = variable.Replace("}", "");
                int equationIndex = int.Parse(variable);
                return equations[equationIndex];
            }
            //is it a number?
            int iVal = 0;
            if (int.TryParse(variable, out iVal))
            {
                SILInteger silInt = new SILInteger();
                silInt = iVal;
                return silInt;
            }
            if (variable.Contains("[]"))
            {
                return ArrayTable.Arrays[functionspace + "." + variable.Substring(0, variable.Length - 2)];
            }
            if (variable.Contains('['))
            {
                return SILArrayElement.Parse(variable, functionspace);
            }
            if (variable.Contains('('))
            {
                SILFunction function;
                object[] functionArgs = null;
                int linenumber = 0;
                CALL_Parse(functionspace, "CALL " + variable, ref functionArgs, ref linenumber);
                function = (SILFunction)functionArgs[0];
                return function;
            }
            //is it a variable?
            SILVariable silVar = new SILVariable(functionspace + "." + variable); //naming validation is inside
            //if the variable is incorrectly named validation will throw an exception
            return silVar;
        }

        /// <summary>
        /// Reverses a string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        static string Reverse(string input)
        {
            string output = "";
            for (int i = input.Length - 1; i >=0; i--)
            {
                output += input[i];
            }
            return output;
        }
        #endregion Expression Parsing

        static ActionType IF_Parse(string functionspace, string unparsedArgs, ref object[] parsedArgs, ref int lineNumber)
        {
            //remove the IF word
            string parameters = unparsedArgs.Substring(3, unparsedArgs.Length - 3);

            //validate that expression ends in THEN
            string THENstring = unparsedArgs.Substring(unparsedArgs.Length - 5, 5);
            if (THENstring != " THEN") throw new Exception("Missing THEN keyword!");

            //get the conditional expression
            parameters = parameters.Substring(0, parameters.Length - 5);
            //parse this stuff into expression
            ISILObject var = ExpressionParse(functionspace, parameters, new List<ISILObject>());
            SIL_Action action;
            Queue<SIL_Action> actions_ = new Queue<SIL_Action>();
            do
            {
                lineNumber++;
                if (lineNumber >= sourceLines_.Length)
                    throw new Exception("ENDIF expected!");
                action = ParseNext(functionspace, ref lineNumber);
                if (action != null) actions_.Enqueue(action);
            } while (action.ActionType != ActionType.ENDIF);
            parsedArgs = new object[2];
            parsedArgs[0] = var;
            parsedArgs[1] = actions_;

            return ActionType.IF;
        }

        static ActionType ENDIF_Parse(string functionspace, string unparsedArgs, ref object[] parsedArgs, ref int lineNumber)
        {
            return ActionType.ENDIF;
        }

        static ActionType WHILE_Parse(string functionspace, string unparsedArgs, ref object[] parsedArgs, ref int lineNumber)
        {
            //remove the WHILE word
            string parameters = unparsedArgs.Substring(6, unparsedArgs.Length - 6);

            ISILObject var = ExpressionParse(functionspace, parameters, new List<ISILObject>());

            SIL_Action action;
            Queue<SIL_Action> actions_ = new Queue<SIL_Action>();
            do
            {
                lineNumber++;
                if (lineNumber >= sourceLines_.Length)
                    throw new Exception("WEND expected!");
                action = ParseNext(functionspace, ref lineNumber);
                if (action != null) actions_.Enqueue(action);
            } while (action.ActionType != ActionType.WEND);
            parsedArgs = new object[2];
            parsedArgs[0] = var;
            parsedArgs[1] = actions_;

            return ActionType.WHILE;
        }

        static ActionType WEND_Parse(string functionspace, string unparsedArgs, ref object[] parsedArgs, ref int lineNumber)
        {
            return ActionType.WEND;
        }

        /// <summary>
        /// FOR is basically 2 actions: LET and WHILE combined with incrementing equation
        /// </summary>
        /// <param name="unparsedArgs"></param>
        /// <param name="parsedArgs"></param>
        /// <param name="lineNumber"></param>
        /// <returns></returns>
        static ActionType FOR_Parse(string functionspace, string unparsedArgs, ref object[] parsedArgs, ref int lineNumber)
        {
            //FOR i = 3, i < 10, i = i + 1
            //remove the FOR word
            string parameters = unparsedArgs.Substring(4, unparsedArgs.Length - 4);
            //validate and decompose the string
            string[] split = parameters.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 3) throw new Exception("Incorrect syntax! Use \"FOR variable, condition, increment\"");
            
            parsedArgs = new object[3];
            Queue<SIL_Action> actions_ = new Queue<SIL_Action>();

            //Create LET statement
            object[] parsedArgsLET = null;
            parserSymbols_["LET"](
                functionspace, 
                "LET " + split[0], //we have to add "LET " string in front for parser to recognize this command as LET
                ref parsedArgsLET, 
                ref lineNumber);
            SIL_Action LETaction = new SIL_Action(ActionType.LET, parsedArgsLET, lineNumber, functionspace);
            LETaction.OnAction += new SIL_Action.Action(exeAction_);
            parsedArgs[0] = LETaction;

            //Parse condition
            try
            {
                SILEquation condition = (SILEquation)ExpressionParse(functionspace, split[1], new List<ISILObject>());
                parsedArgs[1] = condition;
            }
            catch (Exception conidtionEx)
            {
                throw conidtionEx;
                throw new Exception("Condition must be an equation");
            }           

            //Parse increment
            object[] parsedArgsINCREMENT = null;
            parserSymbols_["LET"](
                functionspace, 
                "LET " + split[2],
                ref parsedArgsINCREMENT, 
                ref lineNumber);
            SIL_Action INCREMENTaction = new SIL_Action(ActionType.LET, parsedArgsINCREMENT, lineNumber, functionspace);
            INCREMENTaction.OnAction += new SIL_Action.Action(exeAction_);
            
            //Parse everythig inside FOR block
            SIL_Action action;
            do
            {
                lineNumber++;
                if (lineNumber >= sourceLines_.Length)
                    throw new Exception("NEXT expected!");
                action = ParseNext(functionspace, ref lineNumber);
                if (action != null) actions_.Enqueue(action);
            } while (action.ActionType != ActionType.NEXT);
            //Add increment action last
            actions_.Enqueue(INCREMENTaction);

            parsedArgs[2] = actions_;

            return ActionType.FOR;
        }

        static ActionType NEXT_Parse(string functionspace, string unparsedArgs, ref object[] parsedArgs, ref int lineNumber)
        {
            return ActionType.NEXT;
        }

        static ActionType DRAWLINE_Parse(string functionspace, string unparsedArgs, ref object[] parsedArgs, ref int lineNumber)
        {
            unparsedArgs = unparsedArgs.Trim();

            parsedArgs = new object[5];

            if (unparsedArgs.Length < 9)
                throw new Exception("Incorrect syntax! Parameters for drawing line is missing");

            // remove DRAWLINE and (
            string parameters = unparsedArgs.Substring(8 + 2, unparsedArgs.Length - 8 - 2);

            parameters = parameters.TrimEnd(')');

            string[] parametersArray = parameters.Split(',');
            if (!(parametersArray.GetLength(0) == 5))
                throw new Exception("Incorrect syntax! Parameters for drawing line is missing");

            int tempInt;

            if (int.TryParse((string)parametersArray[0], out tempInt))
                throw new Exception("First parameters must be a correct color");
            else
                parsedArgs[0] = parametersArray[0];

            for (int i = 1 ; i < parametersArray.Length; ++i)
            {
                if (!int.TryParse((string)parametersArray[i], out tempInt))
                    throw new Exception("2 to 5 parameters must be a number");
                else
                    parsedArgs[i] = tempInt;
            }

            return ActionType.DRAWLINE;
        }

    }
}

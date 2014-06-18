using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SIL_Core
{
    /// <summary>
    /// This class exhibits the main functionality of our SIL project
    /// When designing it I followed the few SILple assumptions:
    /// 1) The WHOLE program has to be parsed before it is executed. That way
    /// it will execute faster after it is parsed. Parsing also generates bunch garbage, execution
    /// on other hand should generate as least garbage as possible. The only source of garbage in
    /// the execution is the unboxing costs of function parameters (I am using object[] args).
    /// This could be fixed in the future by using generics, YAY with C# its easy to do! :)
    /// 2) If the code parsing fails program does not execute at all. It will only notify the line #
    /// of the syntax error. I explained why on the forum.
    /// 3) The SIL Compiler is an event oriented class. Which means that whichever GUI you decide to
    /// implement, all you would have to do is to add full event handling for it. Event handler has
    /// to be of the type: void Action(ActionType actionType, object[] parameters). With the big switch 
    /// statement inside the GUI or using a delegate collection like I did in SymbolTable, link each 
    /// event type to a specific response inside your GUI
    /// </summary>
    public static class Compiler
    {
        public static string ProgramName { get; set; }
        static Queue<SIL_Action> actions_ = new Queue<SIL_Action>();
        public delegate void Action(ActionType actionType, object[] args);
        public static event Action OnAction;

        public delegate void ExecuteAction(SIL_Action action); //action for executing
        static Dictionary<ActionType, ExecuteAction> executeSymbols_;

        public static void Initialize()
        {
            executeSymbols_ = new Dictionary<ActionType, ExecuteAction>();
            executeSymbols_.Add(ActionType.PRINT, PRINT_Execute);
            executeSymbols_.Add(ActionType.READ, READ_Execute);
            executeSymbols_.Add(ActionType.LET, LET_Execute);
            executeSymbols_.Add(ActionType.INTEGER, INTEGER_Execute);
            executeSymbols_.Add(ActionType.IF, IF_Execute);
            executeSymbols_.Add(ActionType.ENDIF, ENDIF_Execute);
            executeSymbols_.Add(ActionType.WHILE, WHILE_Execute);
            executeSymbols_.Add(ActionType.WEND, WEND_Execute);
            executeSymbols_.Add(ActionType.FOR, FOR_Execute);
            executeSymbols_.Add(ActionType.NEXT, NEXT_Execute);
            executeSymbols_.Add(ActionType.ARRAY, ARRAY_Execute);
            executeSymbols_.Add(ActionType.PRINTLN, PRINT_Execute);
            executeSymbols_.Add(ActionType.DECLARE, DECLARE_Execute);
            executeSymbols_.Add(ActionType.FUNCTION, FUNCTION_Execute);
            executeSymbols_.Add(ActionType.ENDFUNCTION, ENDFUNCTION_Execute);
            executeSymbols_.Add(ActionType.RETURN, RETURN_Execute);
            executeSymbols_.Add(ActionType.CALL, CALL_Execute);
            executeSymbols_.Add(ActionType.DRAWLINE, DRAWLINE_Execute);
        }

        /// <summary>
        /// Compile the whole program. Compile does not execute the program, it only parses
        /// program text and stores it into SILple commands fastly accessible at execution time. 
        /// It has to be called prior to execution.
        /// </summary>
        /// <param name="text">The full program text</param>
        public static void Compile(string text)
        {
            FunctionTable.Clear();
            FunctionPrototype prototype = new FunctionPrototype();
            prototype.Actions = null;
            prototype.ParameterTypes = new ObjectType[] { };
            prototype.ReturnType = ObjectType.Void;
            FunctionTable.Add("Main", prototype);

            SymbolTable.Clear();
            ArrayTable.Arrays.Clear();    

            actions_ = Parser.Parse(text, action_OnAction);
            //verify that all functions were defined
            string functionName;
            if (!FunctionTable.IsDefined(out functionName))
                throw new Exception(string.Format("Function \"{0}\" is not defined!", functionName));
        }

        static void action_OnAction(ActionType actionType, object[] args)
        {
            OnAction(actionType, args);
        }

        public static void Execute()
        {
            SILFunction mainFunction = new SILFunction("Main", new ISILObject[]{});
            SIL_Action action = new SIL_Action(ActionType.CALL, new object[] { mainFunction }, 0, "Main");
            executeSymbols_[action.ActionType](action); //first execute internal logic               
        }

        internal static void Execute(SIL_Action action)
        {
            executeSymbols_[action.ActionType](action);
            action.Execute();
        }

        static void PRINT_Execute(SIL_Action action)
        {
            //if there is arithmetic involved it needs to be executed here

            //At the moment no variables or arithmetic exists so we just build a single string to print
            string textToPrint = "";
            foreach (object arg in action.ParsingArgs)
            {
                if (arg.GetType() == typeof(string))
                    textToPrint += (string)arg;
                if (arg is ISILObject)
                {
                    ISILObject silObj = (ISILObject)arg;
                    int n = (SILInteger)silObj.Value;
                    textToPrint += n.ToString();
                }
            }
            action.ExecutionArgs = new string[] { textToPrint };
        }

        static void READ_Execute(SIL_Action action)
        {
            action.ExecutionArgs = action.ParsingArgs;
        }

        static void LET_Execute(SIL_Action action)
        {
            ISILObject varOrArrElement = (ISILObject)action.ParsingArgs[0];
            varOrArrElement.Value = ((ISILObject)action.ParsingArgs[1]).Value; 
        }

        static void INTEGER_Execute(SIL_Action action)
        {
            string silVar = (string)action.ParsingArgs[0];
            SymbolTable.Add(silVar);
        }

        static void ARRAY_Execute(SIL_Action action)
        {
            SILArray array = (SILArray)action.ParsingArgs[0];
            array.Allocate();
        }

        static void DECLARE_Execute(SIL_Action action)
        {
            //---
        }

        static void FUNCTION_Execute(SIL_Action action)
        {
            //---
        }

        static void ENDFUNCTION_Execute(SIL_Action action)
        {
            
        }

        static void CALL_Execute(SIL_Action action)
        {
            SILFunction function = (SILFunction)action.ParsingArgs[0];            
            ISILObject value = function.Value;
            action.ExecutionArgs = new object[] { value };
        }

        static void RETURN_Execute(SIL_Action action)
        {
            ISILObject returnValue = (ISILObject)action.ParsingArgs[0];
            action.ExecutionArgs = new object[] { returnValue.Value };
        }

        static void IF_Execute(SIL_Action action)
        {
            if ((int)((SILInteger)((ISILObject)action.ParsingArgs[0]).Value) > 0)
            {
                Queue<SIL_Action> nestedActions = (Queue<SIL_Action>)action.ParsingArgs[1];
                while (nestedActions.Count > 0)
                {
                    SIL_Action nestedAction = nestedActions.Dequeue();
                    executeSymbols_[nestedAction.ActionType](nestedAction); //first execute internal logic
                    nestedAction.Execute(); //lin action with GUI
                }
            }
            
        }

        static void ENDIF_Execute(SIL_Action action)
        {
            //---
        }

        static void WHILE_Execute(SIL_Action action)
        {
            while ((int)((SILInteger)((ISILObject)action.ParsingArgs[0]).Value) > 0)
            {
                Queue<SIL_Action> nestedActions = new Queue<SIL_Action>((Queue<SIL_Action>)action.ParsingArgs[1]);
                while (nestedActions.Count > 0)
                {
                    SIL_Action nestedAction = nestedActions.Dequeue();
                    executeSymbols_[nestedAction.ActionType](nestedAction); //first execute internal logic
                    nestedAction.Execute(); //lin action with GUI
                }
            }

        }

        static void WEND_Execute(SIL_Action action)
        {
            //---
        }

        static void FOR_Execute(SIL_Action action)
        {
            //execute first LET statement once
            SIL_Action LETAction = (SIL_Action)action.ParsingArgs[0];
            executeSymbols_[ActionType.LET](LETAction); //first execute internal logic
            LETAction.Execute();

            while ((int)((SILInteger)((ISILObject)action.ParsingArgs[1]).Value) > 0)
            {
                Queue<SIL_Action> nestedActions = new Queue<SIL_Action>((Queue<SIL_Action>)action.ParsingArgs[2]);
                while (nestedActions.Count > 0)
                {
                    SIL_Action nestedAction = nestedActions.Dequeue();
                    executeSymbols_[nestedAction.ActionType](nestedAction); //first execute internal logic
                    nestedAction.Execute(); //lin action with GUI
                }
            }
        }

        static void NEXT_Execute(SIL_Action action)
        {
            //---
        }

        static void DRAWLINE_Execute(SIL_Action action)
        {
            action.ExecutionArgs = action.ParsingArgs;
        }
    }
}

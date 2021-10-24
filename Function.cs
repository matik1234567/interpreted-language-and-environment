using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading;

namespace Interpreter
{
    class Function:InterFunctions
    {
        public string FunctionName;
        public string OriginalFuncName;
        public Variable to_return = null;
        public Dictionary<string, Variable> LocalVariables = new Dictionary<string, Variable>();
  
        private string[] argumnt_list;
        
        public Function(string FunctionName_, int codeLine_)
        {
            OriginalFuncName = FunctionName_;
            codeLine = codeLine_;
            Parse(FunctionName_);
            try
            {
                InterFunctions.AddToGlobalFunctions(this.FunctionName, this.RunFunction);
            }
            catch(InterException e) { throw e; }
            
        }

        public Function(Function ob)
        {
            this.FunctionName = ob.FunctionName;
            this.OriginalFuncName = ob.OriginalFuncName;        
            this.LocalVariables = ob.LocalVariables;
            this.argumnt_list = ob.argumnt_list;
            this.child_list = ob.child_list;
        }

        private void Parse(string function_line)
        {
            string[] parts = function_line.Split('(');
            FunctionName = "main";
            if (function_line != "#main():")
            {
                FunctionName = parts[0].Substring(4, parts[0].Length - 4);
            }
           // FunctionName = Validation.FunctionName(function_line);
            string temp = Regex.Replace(parts[1], @"[\(\)\:]+", "");
            if (temp=="")
            {
                return;
            }
            argumnt_list = temp.Split(',');
            
            foreach(var v in argumnt_list)
            {
                LocalVariables.Add(v, null);
            }

        }
        public Variable FindInLocalVariables(string name_)
        {
            if (LocalVariables.ContainsKey(name_))
            {
                return LocalVariables[name_];
            }
            return null;
        }

      
        public static Variable FindInLocalVariables(string name_, Dictionary<string, Variable>SomeLocalDict)
        {
            if (SomeLocalDict.ContainsKey(name_))
            {
                return SomeLocalDict[name_];
            }
            return null;
        }
        public void InitializeNextCall(InterFunctions starter_)//kolejne elementy Interfunction
        {
            child_list.Add(starter_);
        }
        
        private Variable RunPriv(List<Variable> args)
        { 
            bool returnd = false;
            DebugStep();
            if (argumnt_list != null && (argumnt_list.Length != args.Count))
            {
                throw new InterException("4A","Invalid argument number passed to function: " + FunctionName);
            }
            List<string> added_temp = new List<string>();
            if (argumnt_list != null)
            {
                for (int a = 0; a < args.Count; a++)
                {   
                    InterStack.CurrentFunction.LocalVariables[argumnt_list[a]] = args[a];
                }
            }

            try
            {
                bool skip = false;
                foreach (var st in child_list)
                {
                    if (skip == true)
                    {
                        if (st is ElifStatement || st is ElseStatemenmt)
                        {
                            continue;
                        }
                        else
                        {
                            skip = false;
                        }
                    }

                    StatementFlag statementFlag = StatementFlag.None;
                   // DebugStep();

                    st.Action(ref returnd, ref statementFlag);

                    if (returnd == true) { break; }
                    if (statementFlag == StatementFlag.SkipBlock)
                    {
                        skip = true;
                    }
                }

            }
            catch (InterException e)
            {
                InterStack.RemoveStackSpecific(this);
                InterStack.CurrentFunction.LocalVariables.Clear();
                throw e;
            }
            
            returnd = false;
            InterStack.CurrentFunction.LocalVariables.Clear();
            InterStack.RemoveStackSpecific(this);
            return to_return;
        }
        public Variable RunFunction(List<Variable> args)
        {
            Function newcall = new Function(this);
            try
            {
                InterStack.OnStack(newcall);
            }
            catch(InterException e) { throw e; }
            return newcall.RunPriv(args);

        }

        public void RunFunctionMain()//no args function = main
        {
            DebugStep();
            try
            {
                bool skip = false;
                bool returned = false;
                foreach (var st in child_list)
                {
                    try
                    {
                        if (skip == true)
                        {
                            if (st is ElifStatement || st is ElseStatemenmt)
                            {
                                continue;
                            }
                            else
                            {
                                skip = false;
                            }
                        }

                        StatementFlag statementFlag = StatementFlag.None;
                       

                        st.Action(ref returned, ref statementFlag);

                        if (returned == true) { break; }
                        if (statementFlag == StatementFlag.SkipBlock)
                        {
                            skip = true;
                        }
                    }
                    catch(InterException e) { throw e; }
                    
                }
            }
            catch (InterException e)
            {
                throw e;
            }
        }

    }
}

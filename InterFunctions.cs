using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Interpreter
{
    class InterFunctions
    {
        protected int codeLine;
        public enum StatementFlag { Next, SkipBlock, None };
        public static Dictionary<string, Func<List<Variable>, Variable>> functions = new Dictionary<string, Func<List<Variable>, Variable>>();

        public InterFunctions() { }
        public List<InterFunctions> child_list = new List<InterFunctions>();

        public virtual void Action(ref bool returned, ref StatementFlag statementFlag) { }

        public static void AddToGlobalFunctions(string func_name, Func<List<Variable>, Variable> func)
        {
            if (functions.ContainsKey(func_name))
            {
                throw new InterException("1F", "Function name: " + func_name + " already exist");
            }
            functions.Add(func_name, func);
        }

        public virtual void Clear()
        {
            foreach (var a in child_list)
            {
                a.Clear();
            }
            child_list.Clear();
        }

        public static void GlobalFunctionInitialize()
        {
            functions.Add("print", SystemFunctions.Print_);
            functions.Add("UpdateIndex", SystemFunctions.UpdateIndex_);
            functions.Add("AtIndex", SystemFunctions.AtIndex_);
            functions.Add("Length", SystemFunctions.Length_);
            functions.Add("Append", SystemFunctions.Append_);
            functions.Add("Type", SystemFunctions.Type_);
            functions.Add("Sqrt", SystemFunctions.Sqrt_);
            functions.Add("Pow", SystemFunctions.Pow_);
            functions.Add("input", SystemFunctions.Input_);
            functions.Add("RemoveIndex", SystemFunctions.RemoveInd_);
            functions.Add("IndexOf", SystemFunctions.IndexOf_);
        }

        protected static async void TreeViewRefreash(Variable arg, string arg_name)
        {
            await Task.Run(() => RuntimeParse.RefreashTreeView(InterStack.CurrentFunction.FunctionName, arg_name, arg.value));
        }

        protected void DebugStep()//wait, jesli suspend to wylaczyc tryb debugowania
        {
            Interpreter.DebugSessionCurrentLine = codeLine;
            if (Interpreter.DebugSession == false)
            {
                return;
            }
            
            while (Interpreter.NextStepSignal == false)
            {
                Thread.Sleep(50);
                if (Interpreter.DebugSession == false)
                {
                    return;
                }
            }
            Interpreter.NextStepSignal = false;
        }
    }
    class ForLoop : InterFunctions
    {
        private Variable var;
        private DynamicField start_;
        private DynamicField end_;
        private DynamicField value_up_;

     
        public ForLoop(string command, int codeLine_)
        {
            codeLine = codeLine_;
            Parse(command);
        }

        private void Parse(string command)
        {
            int ind_b = command.IndexOf('(');
            command = command.Substring(ind_b + 1, command.Length - ind_b - 3);
            string[] args = command.Split(',');

            int v;
            int.TryParse(args[1].ToString(), out v);
            var = new Int(args[0], v);
            
            start_ = new DynamicField(args[1]);
            end_ = new DynamicField(args[2]);
           
            if (args.Length == 3)
            {
                value_up_=new DynamicField("1");
            }
            else if(args.Length == 4)
            {
               
                value_up_= new DynamicField(args[3]);
            }
            else
            {
                throw new InterException("1A","Invalid argument list in ForLoop: "+command);
            }
          
        }
        public override void Action(ref bool returned,  ref StatementFlag statementFlag_)
        {
            if (returned == true)
            {
                return;
            }
            DebugStep();

            if (!InterStack.CurrentFunction.LocalVariables.ContainsKey(var.name))
            {
                InterStack.CurrentFunction.LocalVariables.Add(var.name, var);
            }
            else
            {
                var = InterStack.CurrentFunction.LocalVariables[var.name];
            }
            Variable value_up_temp = value_up_.FieldAction();
            if((int)value_up_temp.value>=0)
            {
                for (var.value = start_.FieldAction().value; var.value < end_.FieldAction().value; var.value = var.value + value_up_temp.value)
                {
                    DebugStep();
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
                        if (Interpreter.DebugSession)
                        {
                            TreeViewRefreash(var, var.name);
                        }
                        st.Action(ref returned, ref statementFlag);
                        if (statementFlag == StatementFlag.SkipBlock)
                        {
                            skip = true;
                        }
                    }
                }
            }
            else
            {
                for (var.value = start_.FieldAction().value; var.value > end_.FieldAction().value; var.value = var.value + value_up_temp.value)
                {
                    DebugStep();
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
                        if (Interpreter.DebugSession)
                        {
                            TreeViewRefreash(var, var.name);
                        }
                        st.Action(ref returned, ref statementFlag);
                        if (statementFlag == StatementFlag.SkipBlock)
                        {
                            skip = true;
                        }
                    }
                }
            }
        
        }

        public override void Clear()
        {
            foreach (var a in child_list)
            {
                a.Clear();
            }
            child_list.Clear();
            if (start_ != null){ start_.Clear();}
            if (end_ != null) { end_.Clear(); }
            if (value_up_ != null) { value_up_.Clear(); }
        }
    }

    class IfStatement : InterFunctions
    {
        protected int mode; //Operands.value
        private static Dictionary<string, int> Operands = new Dictionary<string, int>()
        {
            {"==", 0},
            {"<>", -1},
            {"<", 1 },
            {">", 2 },
            {"<=", 3 },
            {">=", 4 },
        };
        protected DynamicField arg1;
        protected DynamicField arg2;
        public IfStatement() { }
        public IfStatement(string command, int codeLine_)//if(x==2) int, string, float
        {
            codeLine = codeLine_;
            Parse(command);    
        }
        private bool OperatorValue()
        {
            try
            {
                switch (mode)
                {
                    case 0: return arg1.FieldAction().value == arg2.FieldAction().value;
                    case -1: return arg1.FieldAction().value != arg2.FieldAction().value;
                    case 1: return arg1.FieldAction().value < arg2.FieldAction().value;
                    case 2: return arg1.FieldAction().value > arg2.FieldAction().value;
                    case 3: return arg1.FieldAction().value <= arg2.FieldAction().value;
                    case 4: return arg1.FieldAction().value >= arg2.FieldAction().value;
                    default: throw new InterException("1B", "invalid logic operator");
                }
            }
            catch(InterException e)
            {
                throw e;
            }
            catch(Exception e)
            {
                throw new InterException("1B", e.Message);
            }
        }

        protected void Parse(string command, int statementLength = 3)//if:3, elif:5
        {
            string sub = command.Substring(statementLength, command.Length - (statementLength + 2));

            MatchCollection matches = Regex.Matches(sub, @"(==)|(<>)|(<=)|(>=)|(<)|(>)");
            if (matches.Count != 1)
            {
                throw new InterException("1C", "Undefined operator: " + sub);
            }
            string oper = matches[0].ToString();

            this.mode = Operands[oper];
            string[] args = Regex.Split(sub, oper);
            this.arg1 = new DynamicField(args[0]);
            this.arg2 = new DynamicField(args[1]);  
        }
        public override void Action(ref bool returned, ref StatementFlag statementFlag)
        {
            
            if (returned == true)
            {
                return;
            }
            DebugStep();
            if (OperatorValue())
            {
                statementFlag = StatementFlag.SkipBlock;
                foreach (var e in child_list)//if true iterate
                {
                    StatementFlag newFlag = StatementFlag.None;

                    e.Action(ref returned, ref newFlag);
                    //DebugStep();
                    if (newFlag == StatementFlag.SkipBlock) { break; }
                }
            }
            else
            {
                statementFlag = StatementFlag.Next;
            }
        }

        public override void Clear()
        {
            foreach (var a in child_list)
            {
                a.Clear();
            }
            child_list.Clear();
            if (arg1 != null)
            {
                arg1.Clear();
            }
            if (arg2 != null)
            {
                arg2.Clear();
            }
        }
    }

    class ElifStatement : IfStatement
    {
        public ElifStatement(string command, int codeLine_)
        {
            codeLine = codeLine_;
            Parse(command, 5);
        }
    }

    class ElseStatemenmt : InterFunctions
    {
        public ElseStatemenmt( int codeLine_) {
            codeLine = codeLine_;
        }

        public override void Action(ref bool returned, ref StatementFlag statementFlag)
        {
            DebugStep();
            foreach (var e in child_list)
            {
                DebugStep();
                e.Action(ref returned, ref statementFlag);
            }
        }
    }
    class Method : InterFunctions//metoda to dynamic field
    {
        private DynamicField method;
        public Method(string to_parse, int codeLine_)
        {
            codeLine = codeLine_;
            method = new DynamicField(to_parse);
        }
       
        public override void Action(ref bool returned, ref StatementFlag statementFlag)
        {
            if (returned == true)
            {
                return;
            }

            DebugStep();
            method.FieldAction(); 

            foreach (var e in child_list)
            {
                DebugStep();
                e.Action(ref returned, ref statementFlag);
            }
        }
        public override void Clear()
        {
            foreach (var a in child_list)
            {
                a.Clear();
            }
            child_list.Clear();
            if (method != null) { method.Clear(); };
        }
    }


    class VariableCreate : InterFunctions
    {
        private Variable var;
        private DynamicField arg2 = null;
        private string name;
        private string list_args = "";
        public VariableCreate(string command, int codeLine_)//expected $zmienna=2
        {
            codeLine = codeLine_;
            Parse(command);    
        }
        private void Parse(string command)
        {
            string[] args = command.Split('=');
            string name = args[0].Substring(1, args[0].Length - 1);
            if (Validation.IfVariableNameIsValid(name) == false)
            {
                throw new InterException("1D","Invalid variable name: " + name);
            }
            this.name = name;
            if (Validation.IsTable(args[1]) == true)//lista
            {
                list_args = args[1];
            }
            else
            {
                arg2 = new DynamicField(args[1]);     
            }    
        }

        public override void Action( ref bool returned, ref StatementFlag statementFlag)
        {
            if (returned == true)
            {
                return;
            }
            DebugStep();

            if (list_args != "")
            {
                var = new LinkedList(this.name, list_args);
            }
            if (!InterStack.CurrentFunction.LocalVariables.ContainsKey(name))
            {
                InterStack.CurrentFunction.LocalVariables.Add(name, var);
            }
          
            
            if (arg2 != null)//zwykla zmienna
            {
                try
                {
                    Variable act = Variable.Copy(arg2.FieldAction());
                    act.name = this.name;
                    InterStack.CurrentFunction.LocalVariables[this.name] =act;
                    TreeViewRefreash(act, act.name);
                }
                catch(InterException e) { throw e; }
            }
            foreach (var e in child_list)
            {
              
                e.Action( ref returned, ref statementFlag);
                DebugStep();
            }
        }

        public override void Clear()
        {
            foreach (var a in child_list)
            {
                a.Clear();
            }
            child_list.Clear();
            if (arg2 != null)
            {
                arg2.Clear();
            }
        }

    }

    class VariableOperation : InterFunctions
    {  
        private DynamicField arg_second;
        private string name;
        private List<DynamicField> list_index = null; //0->new value, 1,2...n indexes
        public VariableOperation(string command, int codeLine_)
        {
            codeLine = codeLine_;
            try
            {
                Parse(command);
            }
            catch(InterException e) { throw e; }
        }

        private void Parse(string command)
        {
            string[] args = command.Split('=');
            if (args.Length != 2)
            {
                throw new InterException("1E","Invalid variable assignment: " + command);
            }

            this.name = args[0];
            if (name.IndexOf('(') != -1)//element listy
            {
                list_index = new List<DynamicField>();
                int ind = name.IndexOf('(');
                string list = name.Substring(0, ind);
                string element = name.Substring(ind+1, name.Length - ind -2);
                this.name = list;

                string[] args_indexes = element.Split(',');
                foreach(string arg in args_indexes)
                {
                    list_index.Add(new DynamicField(arg));
                }
            }
            arg_second = new DynamicField(args[1]); 
        }

      
        public override void Action(ref bool returned, ref StatementFlag statementFlag)
        {
            if (returned == true)
            {
                return;
            }

            DebugStep();
            if (list_index != null)//assignment to Table
            {
                List<Variable> argss = new List<Variable>();
               
                for(int i = 0; i < list_index.Count; i++)
                {
                        argss.Add(list_index[i].FieldAction());
                }
                InterStack.CurrentFunction.LocalVariables[name].UpdateNestedValue(argss, arg_second.FieldAction());
            }
            else
            {
                Variable arg1 = InterStack.CurrentFunction.FindInLocalVariables(name);
                Variable temp = arg_second.FieldAction();
                if(temp == null)
                {
                    arg1.value = null;
                }
                else
                {
                    arg1.value = Variable.ConvertAssign(arg1,temp.value);
                }
                

                if (Interpreter.DebugSession)
                {
                    TreeViewRefreash(arg1, name);
                }
            }

            foreach (var e in child_list)
            {
                DebugStep();
                e.Action( ref returned, ref statementFlag); 
            }
        }
        public override void Clear()
        {
            foreach (var a in child_list)
            {
                a.Clear();
            }
            child_list.Clear();
            if (arg_second != null)
            {
                arg_second.Clear();
            }
            if (list_index != null)
            {
                list_index.Clear();
            } 
        }
    }
    class ReturnStatement : InterFunctions
    {
        private DynamicField field = null;
        public ReturnStatement(string arg_, int codeLine_)//expected @5, @zmienna, @
        {
            codeLine = codeLine_;
            arg_ = Regex.Replace(arg_, @"[\@']+", "");
            if (arg_ != "")
            {
                field = new DynamicField(arg_);
            }
        }
       
        public override void Action(ref bool returned, ref StatementFlag statementFlag)
        {
            returned = true;
            Interpreter.DebugSessionCurrentLine = codeLine;
            if (field == null)
            {
                DebugStep();
                InterStack.CurrentFunction.to_return = null;
                return;
            }
            DebugStep();
            InterStack.CurrentFunction.to_return = field.FieldAction();
        
        }
        public override void Clear()
        {
            foreach (var a in child_list)
            {
                a.Clear();
            }
            child_list.Clear();
            if (field != null)
            {
                field.Clear();
            }
        }
    }

   
}

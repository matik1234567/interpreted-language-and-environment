using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Interpreter
{
    class SystemFunctions
    {
        public SystemFunctions() { }

        public static Variable IndexOf_(List<Variable> args)//0->string, 1->element string
        {
            if (args[0] is String && args[1] is String)
            {
                if(!(args[0].value is string))
                {
                    throw new InterException("TypeError", args[0].name);
                }
                if (!(args[1].value is string))
                {
                    throw new InterException("TypeError", args[1].name);
                }
                return new Int("", args[0].value.IndexOf(args[1].value));
              
            }
            else
            {
                throw new InterException("Method IndexOf can be used only for String");
            }
            return null;
        }
        public static Variable RemoveInd_(List<Variable> args)//0->table, 1->index to del
        {
            if (args[0] is LinkedList)
            {
                if (!(args[1].value is int))
                {
                    throw new InterException("TypeError", args[1].name);
                }
                if (!(args[0].value is List<Variable>))
                {
                    throw new InterException("TypeError", args[0].name);
                }
                args[0].RemoveIndex(args[1]);
            }
            else
            {
                throw new InterException("Method RemoveIndex can be used only for Array");
            }
            return null;
        }
        public static Variable Append_(List<Variable> args)//0->table, 1->element to add
        {
            if(args[0] is LinkedList)
            {
                if (!(args[0].value is List<Variable>))
                {
                    throw new InterException("TypeError", args[0].name);
                }
                args[0].Append(args[1]);
            }
            else
            {
                throw new InterException("Method Append can be used only for Array");
            }
            return null;
        }
        public static Variable Print_(List<Variable> args)
        {
            
            foreach (var e in args)
            {
                if(e is LinkedList)
                {
                    throw new InterException("Method print can not be used for Array");
                }
                if (e == null || e.value == null)
                {

                    Interpreter.ConsoleTextBox.Invoke((MethodInvoker)delegate { Interpreter.ConsoleTextBox.AppendText("NULL"); });
                }
                else if (Convert.ToString(e.value) == @"\n")
                {
                
                    Interpreter.ConsoleTextBox.Invoke((MethodInvoker)delegate { Interpreter.ConsoleTextBox.AppendText("\r\n"); });
                }
                else if (e != null)
                {
                    Interpreter.ConsoleTextBox.Invoke((MethodInvoker)delegate { Interpreter.ConsoleTextBox.AppendText(Convert.ToString(e.value)); });
                 
                }
               
                Interpreter.ConsoleTextBox.Invoke((MethodInvoker)delegate { Interpreter.ConsoleTextBox.ScrollToCaret(); });
               
            }
 
            return null;
        }

        public static Variable UpdateIndex_(List<Variable> args)//UpdateList(list, index, new value)
        {
            if(args[0] is LinkedList|| args[0] is String)
            {
                if (!(args[0].value is List<Variable>)&& !(args[0].value is string))
                {
                    throw new InterException("TypeError", args[0].name);
                }
               
                try
                {
                    args[0].UpdateValue(args[1], args[2]);
                }
                catch(InterException e) { throw e; }
            }
            else
            {
                throw new InterException("Method UpdateIndex can be used only for Array and String");
            }
            return null;
       
        }

        public static Variable AtIndex_(List<Variable> args)//args[0] is String/LinkedList, arg[1] index
        {
            if (args[0] is String || args[0] is LinkedList)
            {
                if (!(args[0].value is List<Variable>) && !(args[0].value is string))
                {
                    throw new InterException("TypeError", args[0].name);
                }
                List<Variable> temp = new List<Variable>();
                temp.Add(args[1]);
                try
                {
                    return args[0].ReturnAtIndex(temp);
                }
                catch(InterException e) { throw e; }

            }
            else
            {
                throw new InterException("Method AtIndex can be used only for Array and String");
            }
            
        }

        public static Variable Length_(List<Variable> args)//args[0] is object
        {
            if(args[0] is String || args[0] is LinkedList)
            {
                if (!(args[0].value is List<Variable>) && !(args[0].value is string))   
                {
                    throw new InterException("TypeError", args[0].name);
                }
                return args[0].Length();
            }
            else
            {
                throw new InterException("Method Length can be used only for Array and String");
            }
        }

        public static Variable Type_(List<Variable> args)//args[0] is object
        {
            if (args.Count != 1)
            {
                throw new InterException("Invalid argument passed to method Type");
            }
            Variable n = new String("", args[0].VarType());
            return n;
          
        }

        public static Variable Sqrt_(List<Variable> args)//args[0] is object
        {
            if (args[0] is Int || args[0] is Double)
            {
                if (!(args[0].value is int) && !(args[0].value is double))
                {
                    throw new InterException("TypeError", args[0].name);
                }
                return new Double("", Math.Sqrt(args[0].value));
               
            }
            throw new InterException("Invalid argument passed to method Sqrt");
        }

        public static Variable Pow_(List<Variable> args)//args[0] is object
        {
            if ((args[0] is Int || args[0] is Double)&& (args[1] is Int || args[1] is Double))
            {
                if ((!(args[0].value is int) && !(args[0].value is double))||(!(args[1].value is int) && !(args[1].value is double)))
                {
                    throw new InterException("TypeError", args[0].name);
                }
                return new Double("", Math.Pow(args[0].value, args[1].value));

            }
            throw new InterException("Invalid argument passed to method Pow");
        }

        public static Variable Input_(List<Variable> args)//args[0] is object
        {
            while (Interpreter.ConsoleInput == false)
            {
                continue;
            }
            Interpreter.ConsoleInput = false;
            return Variable.ReturnType(Interpreter.ConsoleLine);
            
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace Interpreter
{
    
    class Interpreter
    {
        private MainFunction mainFunction = new MainFunction();
        private Validation validation = new Validation();
        public Interpreter(RichTextBox console) {
            ConsoleTextBox = console;
        }
        
        private bool is_main = false;
        public static bool DebugSession = false;
        public static int DebugSessionCurrentLine = 0;
        public static bool NextStepSignal = false;
        public static RichTextBox ConsoleTextBox;
        public static string ConsoleLine = "";
        public static bool ConsoleInput = false;
        public static bool InterpreterRunning = false;
        private InterFunctions RecognizeInterCreateObject(string command, int codeLine)//dodawanie do listy obiektow i utworzenie odpowiedniego
        {
            try
            {
                InterFunctions interf;
                if (command.IndexOf("for(") == 0)
                {
                    interf = new ForLoop(command, codeLine);
                }
                else if (command.IndexOf("if(") == 0)
                {
                    interf = new IfStatement(command, codeLine);
                }
                else if (command.IndexOf("elif(") == 0)
                {
                    interf = new ElifStatement(command, codeLine);
                }
                else if (command.IndexOf("else") == 0)
                {
                    interf = new ElseStatemenmt(codeLine);
                }
                else if (command[0] == '$')
                {
                    interf = new VariableCreate(command, codeLine);
                }
                else if (command.IndexOf('=') != -1)
                {
                    interf = new VariableOperation(command, codeLine);
                }
                else if (command[0] == '@')
                {
                    interf = new ReturnStatement(command, codeLine);
                }
                else
                {
                    interf = new Method(command, codeLine);
                }
                return interf;
            }
            catch(InterException e)
            {
                throw e;
            }
        }
        
        private int TabNumber(ref string command)
        {
            string command_cpy = command;
            command = Regex.Replace(command, @"\t", "");
            return Validation.TabNumber(command_cpy);
        }

        private int CreateFunctionTree(string[]command_hie, int start)
        {
            Function function = new Function(command_hie[start], start);
    
            if (command_hie[start].IndexOf("main(") != -1)
            {
                is_main = true;
            }
            List<InterFunctions> stack = new List<InterFunctions>();
            start++;
            int i;
            int prev_tab_nums = 1;
            for (i = start; i < command_hie.Length; i++)
            {
                if (Validation.IsWhiteChars(command_hie[i])) { continue; }
                if (command_hie[i][0] == '#'){ break; }

                string comand_cpy = command_hie[i];
                int current_tab_nums = TabNumber(ref comand_cpy);
                InterFunctions n;
                
                try
                {
                    n = RecognizeInterCreateObject(comand_cpy, i);
                }
                catch (InterException e){ throw e; }

                if (current_tab_nums==1)
                {
                    stack.Clear();
                    stack.Add(n);
                    function.InitializeNextCall(n);
                }
                else if (prev_tab_nums == current_tab_nums)
                {
                    if (stack.Count > 0)
                    {
                        stack.RemoveAt(stack.Count - 1);
                    }
                    stack[stack.Count - 1].child_list.Add(n);
                    stack.Add(n);    
                }
                else if (current_tab_nums > prev_tab_nums)
                {
                    stack[stack.Count - 1].child_list.Add(n);
                    stack.Add(n);
                }
                else //current_tab_nums < prev_tab_nums
                {
                    int dif = prev_tab_nums - current_tab_nums+1;
                    while (dif > 0)
                    {
                        dif--;
                        stack.RemoveAt(stack.Count - 1);
                    }
                    stack[stack.Count - 1].child_list.Add(n);
                    stack.Add(n);
                }
                prev_tab_nums = current_tab_nums;
                
            }
            if (is_main == true)
            {
                mainFunction.InitializeCallTree(function);
            }
            else
            {
                mainFunction.AddFunction(function);
            }
      
            stack.Clear();
            return i - 1;

        }
        private void EnvironmentSeparateFunctions(string console)
        {
            
            string[] command_hie = console.Split('\n');
            
            for(int i = 0; i < command_hie.Length; i++)
            {
                if(command_hie[i] == "" || Validation.IsOnlyTabs(command_hie[i]))
                {
                    continue;
                }
                if (command_hie[i][0] == '#')
                {
                    try
                    {
                        i = CreateFunctionTree(command_hie, i);
                    }
                    catch (InterException e)
                    {
                        throw e;
                    }
                }
            }
            if (is_main == false)
            {
                throw new InterException("main() function is not initialized");
            }
        }


        private void ReturnExitError()
        {
            Interpreter.ConsoleTextBox.Invoke((MethodInvoker)delegate { ConsoleTextBox.AppendText("\r\n"); });
            Interpreter.ConsoleTextBox.Invoke((MethodInvoker)delegate { ConsoleTextBox.AppendText("Exit code: -1"); });
            InterFunctions.functions.Clear();
            Interpreter.DebugSession = false;
            mainFunction.Clear();
            Interpreter.InterpreterRunning = false;
        }
        public void InterpretDebug(string console)
        {
            ConsoleTextBox.Invoke((MethodInvoker)delegate { ConsoleTextBox.Clear(); });
            InterFunctions.GlobalFunctionInitialize();
            console = Validation.RemoveSpaces(console);
            if (validation.Validate(console))//sa bledy
            {
                ReturnExitError();
                return;
            }
            
            try
            {
                is_main = false;
                EnvironmentSeparateFunctions(console);
            }
            catch (InterException)
            {
                ReturnExitError();
                return;
            }
            InterStack stack = new InterStack();
            try
            {
                DebugSession = true;
                mainFunction.Start();
                mainFunction.Clear();
            }
            catch(InterException)
            {

                ReturnExitError();
                return;
            }
            ReturnExitSuccess();
        }

        private void ReturnExitSuccess()
        {
            Interpreter.ConsoleTextBox.Invoke((MethodInvoker)delegate { ConsoleTextBox.AppendText("\r\n"); });
            Interpreter.ConsoleTextBox.Invoke((MethodInvoker)delegate { ConsoleTextBox.AppendText("Exit code: 0"); });
            InterFunctions.functions.Clear();
            Interpreter.DebugSession = false;
            mainFunction.Clear();
            Interpreter.InterpreterRunning = false;
        }
        public void Interpret(string console)
        {
           
            ConsoleTextBox.Invoke((MethodInvoker)delegate { ConsoleTextBox.Clear(); });
            InterFunctions.GlobalFunctionInitialize();
            console = Validation.RemoveSpaces(console);
            if (validation.Validate(console))//sa bledy
            {
                ReturnExitError();
                return;
            }
          
            try
            {
                is_main = false;
                EnvironmentSeparateFunctions(console);
            }
            catch(InterException)
            {
                ReturnExitError();
                return;
            }
            InterStack stack = new InterStack();
            try
            {
                System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
                timer.Start();
                mainFunction.Start();
                mainFunction.Clear();
                timer.Stop();
                ConsoleTextBox.Invoke((MethodInvoker)delegate { Interpreter.ConsoleTextBox.AppendText(Environment.NewLine + "Execute time: " +Convert.ToString(timer.Elapsed.TotalSeconds) + " s."+ Environment.NewLine); });
              
            }
            catch (InterException){
                ReturnExitError();
                return;
            }
            ReturnExitSuccess();
        }
    }
}

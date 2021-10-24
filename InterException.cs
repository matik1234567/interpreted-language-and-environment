using System;
using System.Windows.Forms;

namespace Interpreter
{
    class InterException:Exception
    {
 
        public InterException()
        {
        }

        public InterException(string message)
            : base(message)
        {
            Interpreter.ConsoleTextBox.Invoke((MethodInvoker)delegate { Interpreter.ConsoleTextBox.AppendText("(" + Convert.ToString(Interpreter.DebugSessionCurrentLine + 1) +") "+ message + Environment.NewLine); });
          
        }

        public InterException(string errorCode, string message)
           : base(message)
        {
            Interpreter.ConsoleTextBox.Invoke((MethodInvoker)delegate { Interpreter.ConsoleTextBox.AppendText("("+Convert.ToString(Interpreter.DebugSessionCurrentLine+1) +") InterpreterError code:" + errorCode + " ; " + message + Environment.NewLine); });
           
        }


    }
}

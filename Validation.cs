using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Interpreter
{
    class Validation//sluzy do walidacji kodu
    {
        private List<string> error_messages = new List<string>();

        private static Regex ifvalid = new Regex(@"^[\t]*if\([^\!\@\^\#\$=<>]+(==|<=|>=|<>|>|<)[^\!\@\^\#\$=<>]+\):$");
        private static Regex ifvalid_first = new Regex(@"^[\t]*if\(");
        private static Regex forvalid = new Regex(@"^[\t]*for\([a-zA-Z][a-zA-Z | 0-9]*,[^,]+,[^,]+([,][^,]+)?\):$");
        private static Regex forvalid_first = new Regex(@"^[\t]*for\(");
        private static Regex ifcarcreatevalid = new Regex(@"^[\t]*\$[a-zA-Z][^\(\)\$\&\*\-\/\+\@\#]*=[^=\&\@\$\#]*$");
        private static Regex ifcarcreatevalid_first = new Regex(@"^[\t]*\$");
        private static Regex ifvaropevalid = new Regex(@"^[\t]*[a-zA-Z][^\$\&\@\#]*=[^=\&\@\$\#]*$");
        private static Regex ifreturnstatement = new Regex(@"^[\t]*\@[^\@]*$");
        private static Regex ifdefinedfunction_first = new Regex(@"^[\t]*\#[^\#]*def\($");
        private static Regex ifdefinedfunction = new Regex(@"^[\t]*\#[^\#]*def\(.*\):$");
        private static Regex ifvariablenamevalid = new Regex(@"^[a-zA-Z | 0-9 _]*$");
        private static Regex isDigit = new Regex(@"^[0-9 | \-]+$");
        private static Regex isFunctionCall = new Regex(@"^([a-zA-Z]|[0-9]|[_])+\(.*\)$");//dla rownan
        private static Regex isBracketCall = new Regex(@"^\(([a-zA-Z]|[0-9]|[_\+\-\/\*\%\(\)])+\)$");//dla rownan
        private static Regex isTable = new Regex(@"^\([^\/\*\%]*\)$");
        private static Regex tabCount = new Regex(@"[\t]");
        private static Regex tabCountEnd = new Regex(@"[\t]$");
        private static Regex iswhitechars = new Regex(@"^(( )|(\t))*$");
        private static Regex isTabOnly = new Regex(@"^[\t]+$");
        private static Regex functionName = new Regex(@"(?<=#def )[^\(]+");

        public Validation() {}

        public static string FunctionName(string line)
        {
            MatchCollection match = functionName.Matches(line);
            if (match.Count == 0) { return line; }
            return match[0].ToString();
        }
        public static bool  IsDigit(string line)
        {
            return isDigit.IsMatch(line);
        }
        public static bool IsFunctionCall(string line)
        {
            return isFunctionCall.IsMatch(line);
        }

        public static bool IsBracketCall(string line)
        {
            return isBracketCall.IsMatch(line);
        }

        public static bool IsTable(string line)
        {
            return isTable.IsMatch(line);
        }
        public static bool IsOperator(char line)
        {
            switch (line)
            {
                case '+': return true;
                case '-': return true;
                case '*': return true;
                case '/': return true;
                case '%': return true;
                default: return false;
            }
        }
        public static int TabNumber(string command)
        {
            return tabCount.Matches(command).Count - tabCountEnd.Matches(command).Count;
        }

        public static bool IfVariableNameIsValid(string line)
        {
            return ifvariablenamevalid.IsMatch(line);
        }

        public static bool IsWhiteChars(string line)
        { 
            return iswhitechars.IsMatch(line);
        }
    
        public static bool IsOnlyTabs(string line)
        {
            return isTabOnly.IsMatch(line);    
        }
        public static bool ForIsValid(string line)
        {
            return forvalid.IsMatch(line);
         
        }
        public static bool IfIsValid(string line)
        {

            return ifvalid.IsMatch(line);
           
        }
        public static bool VariableCreateIsValid(string line)
        {
            
            return ifcarcreatevalid.IsMatch(line);
           
        }

        public static bool VariableOperationIsValid(string line)
        {
            return ifvaropevalid.IsMatch(line);
          
        }
        private bool CommandLineValid(string command)
        {
            if (forvalid_first.IsMatch(command))
            {
                return ForIsValid(command);
            }
            else if (ifvalid_first.IsMatch(command))
            {
                return IfIsValid(command);
            }
            else if (ifcarcreatevalid_first.IsMatch(command))
            {
                return VariableCreateIsValid(command);
            }
            else if (command.IndexOf('=') != -1)
            {
                return VariableOperationIsValid(command);
            }
            else if (ifreturnstatement.IsMatch(command))
            {
                return true;
            }
            else if (ifdefinedfunction_first.IsMatch(command))
            {
                return ifdefinedfunction.IsMatch(command);
            }
            else
            {
                return true;
            }

        }
        private void PrintErrorList()
        {
            foreach(var a in error_messages)
            {
                Interpreter.ConsoleTextBox.Invoke((MethodInvoker)delegate { Interpreter.ConsoleTextBox.AppendText(a + Environment.NewLine); });
            }
            error_messages.Clear();
        }
        public bool Validate(string source_code)
        {
            string[] code_lines = source_code.Split('\n');
            bool error = false;
            for(int i = 0; i < code_lines.Length; i++)
            {
                if (code_lines[i] == "" || Validation.IsOnlyTabs(code_lines[i]))
                {
                    continue;
                }
                if (CommandLineValid(code_lines[i]) == false)
                {
                    string t = code_lines[i].Replace("\t", "");
                    error_messages.Add("(" + (i+1).ToString() + ") " + "SytaxError: " + t);
                    error = true;
                }
            }
            PrintErrorList();
            return error;
        }

        public static string RemoveSpaces(string command)//usuwa spacje chyba ze jest w 'nazwa nazwa' to nie
        {

            int com_len = command.Length;
            bool isc = false;
            string ret = "";
            for (int i = 0; i < com_len; i++)
            {
                if (command[i] == '\'')
                {
                    if (isc == false)
                    {
                        isc = true;
                    }
                    else
                    {
                        isc = false;
                    }
                }
                if (isc == true)
                {
                    ret += command[i];
                    continue;
                }
                if (command[i] != ' ')
                {
                    ret += command[i];
                }
            }
            return ret;
        }

    }
}

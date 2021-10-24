using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Threading;


namespace Interpreter
{
    struct MatchColor
    {
        public Regex pattern;
        public Color color;
        public int patternLength;
        public MatchColor(Regex pattern_, Color color_, int patternLength_=1)
        {
            pattern = pattern_;
            color = color_;
            patternLength = patternLength_;
        }
    }
    class RuntimeParse
    {
        private static Regex isFor = new Regex(@"^[\t]+for\(.+$");
        private static Regex isIf = new Regex(@"^[\t]+if\(.+$");
        private static Regex isElif = new Regex(@"^[\t]+elif\(.+$");
        private static Regex isElse = new Regex(@"^[\t]+else\:$");
        private static Regex isReturn = new Regex(@"^[\t]+@.*$");
        private static Regex isVariableCreate = new Regex(@"^[\t]+\$.+$");
        private static Regex function = new Regex(@"^#.+$");
        private static Regex functionDef= new Regex(@"^#def .+$");
        private static Regex tabcount = new Regex(@"[\t]");

        private static Regex ifcarcreatevalid = new Regex(@"^[\t]+\$[a-zA-Z][^\(\)\$\&\*\-\/\+\@\#\=]*=[^=\&\@\$\#]+$");//z validation
        private static Regex ifdefinedfunction = new Regex(@"^[\t]*\#def.+\(.*\):$");//z validation

        private List<MatchColor> RegexList = new List<MatchColor> ();
        private Dictionary<string, HashSet<string>> VistitedNodes = new Dictionary<string, HashSet<string>>();//key-function names, value-locally created variables

        private static RichTextBox richTextBox = null;
        private static TreeView treeView = null;

        public RuntimeParse(RichTextBox richTextBox_,TreeView treeView_)
        {
            richTextBox = richTextBox_;
            treeView = treeView_;
            RegexList.Add(new MatchColor(isFor, Color.FromArgb(255, 204, 120, 50),3));
            RegexList.Add(new MatchColor(isIf, Color.FromArgb(255, 204, 120, 50),2));
            RegexList.Add(new MatchColor(isElif, Color.FromArgb(255, 204, 120, 50), 4));
            RegexList.Add(new MatchColor(isElse, Color.FromArgb(255, 204, 120, 50), 4));
            RegexList.Add(new MatchColor(isReturn, Color.FromArgb(255, 204, 120, 50)));
            RegexList.Add(new MatchColor(function, Color.FromArgb(255, 225, 197, 92), 1));
            RegexList.Add(new MatchColor(functionDef, Color.FromArgb(255, 225, 197, 92), 4));
            RegexList.Add(new MatchColor(isVariableCreate, Color.FromArgb(255, 126, 87, 194), 1));

        }

        private int TabNumber( string command)
        {
           return tabcount.Matches(command).Count;
        }

        public static int RefreashTreeView(string funcOriginalName, string variableName, object newValue_)
        {
           
            string newValue = Convert.ToString(newValue_);
            bool function_contain = false;
            treeView.Invoke((MethodInvoker)delegate { function_contain = treeView.Nodes.ContainsKey(funcOriginalName); });
            if (function_contain == true)
            {
                bool var_contain = false;
                treeView.Invoke((MethodInvoker)delegate { var_contain = treeView.Nodes[funcOriginalName].Nodes.ContainsKey(variableName); });
                if(var_contain == true)
                {
                    treeView.Invoke((MethodInvoker)delegate { treeView.BeginUpdate(); });
                    treeView.Invoke((MethodInvoker)delegate { treeView.Nodes[funcOriginalName].Nodes[variableName].Text = variableName + "  [ " + newValue + " ]"; });
                    treeView.Invoke((MethodInvoker)delegate { treeView.EndUpdate(); });
                    treeView.Invoke((MethodInvoker)delegate { treeView.Refresh(); });

                }
                else
                {
                    treeView.Invoke((MethodInvoker)delegate { treeView.BeginUpdate(); });
                    treeView.Invoke((MethodInvoker)delegate { treeView.Nodes[funcOriginalName].Nodes.Add(variableName, variableName + "  [ " + newValue + " ]"); });
                    treeView.Invoke((MethodInvoker)delegate { treeView.EndUpdate(); });
                    treeView.Invoke((MethodInvoker)delegate { treeView.Nodes[funcOriginalName].Nodes[variableName].ImageKey = "var"; });
                    treeView.Invoke((MethodInvoker)delegate { treeView.Refresh(); });
                }
            }
            return 0;
        }

      
        private void AddVariableToFunctionTreeView(string functionName, string variableName)
        {
            variableName = variableName.Substring(1, variableName.Length - 1);
            variableName = Validation.RemoveSpaces(variableName);
            string[] args = variableName.Split('=');
            string func_name = functionName;
            if (functionName != "#main():")
            {
                func_name = Validation.FunctionName(functionName);
            }
            else
            {
                func_name = "main";
            }
            if (treeView.Nodes.ContainsKey(func_name))
            {
                string newText;
                args[0] = args[0].Replace("$", "");
                if (Validation.IsTable(args[1]) == true)
                {
                    newText = args[0] + "  [  Array  ]";
                }
                else
                {
                    newText = args[0] + "  [ " + args[1] + " ]";
                }
               
                if (treeView.Nodes[func_name].Nodes.ContainsKey(args[0]))
                {
  
                    if (treeView.Nodes[func_name].Nodes[args[0]].Text == newText)
                    {
                        VistitedNodes[func_name].Add(args[0]);
                        return;
                    }
                    treeView.BeginUpdate();
                    treeView.Nodes[func_name].Nodes[args[0]].Text = newText;
                    treeView.Nodes[func_name].Nodes[args[0]].ImageKey = "var";
                    treeView.EndUpdate();
                    VistitedNodes[func_name].Add(args[0]);

                }
                else
                {
                    treeView.BeginUpdate();
                    treeView.Nodes[func_name].Nodes.Add(args[0], newText);
                    treeView.Nodes[func_name].Nodes[treeView.Nodes[func_name].Nodes.Count-1].ImageKey = "var";
                    treeView.EndUpdate();
                    VistitedNodes[func_name].Add(args[0]);
                }
                
            }
            VistitedNodes[func_name].Add(args[0]);
        }
      
        private void UpdateTreeView(string functionName, string variableCreate)
        {
            if (functionName == "") { return; }
            bool is_main = false;
            if (variableCreate == "")//dodajemy funkcje
            {
               
                string func_name = functionName;
                if (functionName != "#main():")
                {
                    func_name = Validation.FunctionName(functionName);
                }
                else
                {
                    func_name = "main";
                    is_main = true;

                }
                if (treeView.Nodes.ContainsKey(func_name))
                {
                    if (VistitedNodes.ContainsKey(func_name)) { return; }
                    VistitedNodes.Add(func_name, new HashSet<string>());
                    return;
                }

                treeView.BeginUpdate();
                treeView.Nodes.Add(func_name, func_name);//bylo jako 2 arg functionName
                treeView.EndUpdate();
                if(is_main == false)
                {
                    treeView.Nodes[func_name].ImageKey = "func";
                }
                else
                {
                    treeView.Nodes[func_name].ImageKey = "main";
                }
                VistitedNodes.Add(func_name, new HashSet<string>());


            }
            else//funkcja moze juz istniec wiec sprawdzic czy istnieje
            {
                AddVariableToFunctionTreeView(functionName, variableCreate);
            }
        }
       
       
        public void Remove(TreeNode tn)
        {
            if (tn.Checked)
            {
                //tv1.Nodes.Remove(tn);
                if (tn.Parent != null)
                    tn.Parent.Nodes.Remove(tn);
                else
                    treeView.Nodes.Remove(tn);
            }
            else if (tn.Nodes.Count > 0)
            {
                foreach (TreeNode tn1 in tn.Nodes)
                {
                    Remove(tn1);
                }
            }
        }
        private void DeleteTreeViewUnused()
        {
            for (int i = 0; i < treeView.Nodes.Count; i++)
            {
                if (VistitedNodes.ContainsKey(treeView.Nodes[i].Name) == false)//jesli usunieto funckje
                {
                    treeView.BeginUpdate();
                    treeView.Nodes.RemoveAt(i);
                    treeView.EndUpdate();
                }
                else//jesli dalej jest funckcja sprawdz czy zmienne rowniez
                {
                    for(int j = 0; j < treeView.Nodes[i].Nodes.Count; j++)
                    {
                        if (VistitedNodes[treeView.Nodes[i].Name].Contains(treeView.Nodes[i].Nodes[j].Name))
                        {
                            continue;
                        }
                        else
                        {
                            treeView.BeginUpdate();
                            treeView.Nodes[i].Nodes.RemoveAt(j);
                            treeView.EndUpdate();
                        }
                    }
                }
            } 
        }
        public void RefreashTreeView()
        {
            
            if (richTextBox.Lines.Count() == 0) { return; }
            string functionname_cpy = "";

            for(int i = 0; i < richTextBox.Lines.Count(); i++)
            {
                string currentlinetext = richTextBox.Lines[i];
                if (ifdefinedfunction.IsMatch(currentlinetext)==true||currentlinetext=="#main():")
                {
                    functionname_cpy = currentlinetext;
                    UpdateTreeView(functionname_cpy, "");
                }
                else if (ifcarcreatevalid.IsMatch(currentlinetext)==true)
                {
                    
                    UpdateTreeView(functionname_cpy, currentlinetext);
                }
            }
            DeleteTreeViewUnused();
            VistitedNodes.Clear();
          
        }

        public void RunForAll()
        {
            if (Interpreter.DebugSession == true)
            {
                return;
            }
            int lineCount = 0;
            treeView.Invoke((MethodInvoker)delegate { lineCount = richTextBox.Lines.Count(); });
            int prevLinesLength = 0;
            for(int i = 0; i < lineCount; i++)
            {
                treeView.Invoke((MethodInvoker)delegate { prevLinesLength += richTextBox.Lines[i].Length; });
                treeView.Invoke((MethodInvoker)delegate { richTextBox.Select(prevLinesLength, 0); });

                Run();

            }
        }
        public void Run()
        {
            if (Interpreter.DebugSession == true)
            {
                return;
            }
            int lineindex =0;//aktualna linia
            treeView.Invoke((MethodInvoker)delegate { lineindex = richTextBox.GetLineFromCharIndex(richTextBox.SelectionStart); });
            int firstcharindex = 0;//aktualny znak
            treeView.Invoke((MethodInvoker)delegate { firstcharindex = richTextBox.GetFirstCharIndexOfCurrentLine(); });
            int count = 0;
            treeView.Invoke((MethodInvoker)delegate { count = richTextBox.Lines.Count(); });
            if (lineindex >= count || lineindex<0)
            {
                return;
            }
            string currentlinetext = "";
            treeView.Invoke((MethodInvoker)delegate { currentlinetext = richTextBox.Lines[lineindex]; });
            if (currentlinetext=="" || Validation.IsOnlyTabs(currentlinetext))
            {
                return;
            }
            
            int tab_num = TabNumber(currentlinetext);
            foreach (var r in RegexList)
            {
                if (r.pattern.IsMatch(currentlinetext))
                {

                    treeView.Invoke((MethodInvoker)delegate { richTextBox.Select(firstcharindex, currentlinetext.Length); });
                    treeView.Invoke((MethodInvoker)delegate { richTextBox.SelectionColor = richTextBox.ForeColor; });
                    treeView.Invoke((MethodInvoker)delegate { richTextBox.Select(firstcharindex + tab_num, r.patternLength); });
                    treeView.Invoke((MethodInvoker)delegate { richTextBox.SelectionColor = r.color; });
                    treeView.Invoke((MethodInvoker)delegate { richTextBox.Select(firstcharindex + currentlinetext.Length, 0); });
                  
                }
               
            }
            RefreashTreeView();
        }
    }
}

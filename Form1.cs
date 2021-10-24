using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;


namespace Interpreter
{
    public partial class Form1 : Form
    {
        private string sourceCodeDirectory = "";
        private RuntimeParse runtimeParse = null;
        private int TabKeyNum = 1;
        private delegate void SafeCallDelegate(string text);
        private Interpreter interpreter;
        private Color DebugColor = Color.FromArgb(255, 118, 44, 44);
        private Keys PrevKey;
        public Form1()
        {
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            
            InitializeComponent();
            InitializeMethodsTree();
            richTextBox1.Text = "\n#main():\n\t";
            richTextBox1.Select(9, 0);
            runtimeParse = new RuntimeParse(this.richTextBox1, this.treeView1);
            interpreter = new Interpreter(richTextBox2);
            ImageList imgl = new ImageList();
            imgl.Images.Add("func", Properties.Resources.function);
            imgl.Images.Add("var", Properties.Resources.variable);
            imgl.Images.Add("main", Properties.Resources.function_main);
            treeView1.Nodes.Add("main", "main");
            treeView1.ImageList = imgl;
            treeView1.Nodes[0].ImageKey = "main";

            new ToolTip().SetToolTip(this.button1, "Execute");
            new ToolTip().SetToolTip(this.button4, "Debug");
            new ToolTip().SetToolTip(this.button2, "Exit Debug");
            new ToolTip().SetToolTip(this.button3, "Next Step");
            
        }

        private void TreeView1_BeforeSelect(Object sender, TreeViewCancelEventArgs e)
        {
            e.Cancel = true;
        }

        private void TreeView2_BeforeSelect(Object sender, TreeViewCancelEventArgs e)
        {
            e.Cancel = true;
        }

        public int getWidth()
        {
            int w = 25;   
            int line = richTextBox1.Lines.Length;

            if (line <= 99)
            {
                w = 20 + (int)richTextBox1.Font.Size;
            }
            else if (line <= 999)
            {
                w = 30 + (int)richTextBox1.Font.Size;
            }
            else
            {
                w = 50 + (int)richTextBox1.Font.Size;
            }

            return w;
        }
        private void InitializeMethodsTree()
        {
            
            ImageList imgl = new ImageList();
            imgl.Images.Add("type", Properties.Resources.type);
            imgl.Images.Add("func", Properties.Resources.function);
            treeView2.ImageList = imgl;
           
            treeView2.BeginUpdate();
            treeView2.Nodes.Add("Int"); //0
            treeView2.Nodes[0].ImageKey = "type";
            treeView2.Nodes.Add("Double"); //1
            
            treeView2.Nodes.Add("String"); //2
            treeView2.Nodes.Add("Bool"); //3
            treeView2.Nodes.Add("Array"); //4
            treeView2.Nodes.Add("print((Variable)[inf])");
            treeView2.Nodes[5].ImageKey = "func";
            treeView2.Nodes.Add("input() @Variable");
            treeView2.Nodes[6].ImageKey = "func";
            treeView2.Nodes.Add("Type(Variable) @String");
            treeView2.Nodes[7].ImageKey = "func";

            treeView2.Nodes[0].Nodes.Add("Sqrt(Int) @Double");
            treeView2.Nodes[0].Nodes.Add("Pow(Int, Int/Double) @Double");

            treeView2.Nodes[1].Nodes.Add("Sqrt(Double) @Double");
            treeView2.Nodes[1].Nodes.Add("Pow(Double, Int/Double) @Double");

            treeView2.Nodes[3].Nodes.Add("True @Bool");
            treeView2.Nodes[3].Nodes.Add("False @Bool");

            treeView2.Nodes[2].Nodes.Add("UpdateIndex(String, Int, String)");
            treeView2.Nodes[2].Nodes.Add("Length(String) @Int");
            treeView2.Nodes[2].Nodes.Add("AtIndex(String, Int) @String");
            treeView2.Nodes[2].Nodes.Add("IndexOf(String, String) @Int");

            treeView2.Nodes[4].Nodes.Add("UpdateIndex(Array, Int, Variable)");
            treeView2.Nodes[4].Nodes.Add("Length(Array) @Int");
            treeView2.Nodes[4].Nodes.Add("Append(Array, Variable)");
            treeView2.Nodes[4].Nodes.Add("AtIndex(Array, Int) @Variable");
            treeView2.Nodes[4].Nodes.Add("RemoveIndex(Array, Int)");

            for(int i = 0; i < treeView2.Nodes.Count; i++)
            {
                for(int j = 0; j < treeView2.Nodes[i].Nodes.Count; j++)
                {
                    treeView2.Nodes[i].Nodes[j].ImageKey = "func";
                }
            }
           
            treeView2.EndUpdate();

        }
       
        public void AddLineNumbers()
        {
            
            Point pt = new Point(0, 0);   
            int First_Index = richTextBox1.GetCharIndexFromPosition(pt);
            int First_Line = richTextBox1.GetLineFromCharIndex(First_Index);     
            pt.X = ClientRectangle.Width;
            pt.Y = ClientRectangle.Height;   
            int Last_Index = richTextBox1.GetCharIndexFromPosition(pt);
            int Last_Line = richTextBox1.GetLineFromCharIndex(Last_Index);
            LineNumberTextBox.SelectionAlignment = HorizontalAlignment.Center;
            LineNumberTextBox.Text = "";
            LineNumberTextBox.Width = getWidth();  
            for (int i = First_Line; i <= Last_Line + 2; i++)
            {
                LineNumberTextBox.Text += i + 1 + "\n";
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LineNumberTextBox.Font = richTextBox1.Font;
            richTextBox1.Select();
            AddLineNumbers();
        }

        private void richTextBox1_SelectionChanged(object sender, EventArgs e)
        {
            Point pt = richTextBox1.GetPositionFromCharIndex(richTextBox1.SelectionStart);
            if (pt.X == 1)
            {
                AddLineNumbers();
            }
        }

        private void richTextBox1_VScroll(object sender, EventArgs e)
        {
            LineNumberTextBox.Text = "";
            AddLineNumbers();
            LineNumberTextBox.Invalidate();
        }


        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (richTextBox1.Text == "")
            {
                AddLineNumbers();
            }
            int row = richTextBox1.GetLineFromCharIndex(richTextBox1.SelectionStart);
            int col = richTextBox1.SelectionStart - (richTextBox1.GetFirstCharIndexFromLine(1 + richTextBox1.GetLineFromCharIndex(richTextBox1.SelectionStart) - 1));
           if (richTextBox1.Lines.Count()==0)
           {
               return;
           }
            if (col != richTextBox1.Lines[row].Length)
            {
                return;
            }

            if (runtimeParse != null)
            {
                runtimeParse.Run();
          
            }
        }

        private void richTextBox1_FontChanged(object sender, EventArgs e)
        {
            LineNumberTextBox.Font = richTextBox1.Font;
            richTextBox1.Select();
            AddLineNumbers();
        }

        private void LineNumberTextBox_MouseDown(object sender, MouseEventArgs e)
        {
            richTextBox1.Select();
            LineNumberTextBox.DeselectAll();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            AddLineNumbers();
        }

        private void RunInmterpreter()
        {
            if(Interpreter.InterpreterRunning == true)
            {
                return;
            }
            runtimeParse.RunForAll();
            string t = richTextBox1.Text;
            Interpreter.InterpreterRunning = true;
            Task.Run(() => interpreter.Interpret(t));
            
        }
        private void button1_Click(object sender, EventArgs e)
        {
            RunInmterpreter();
        }
        void changeLine(RichTextBox RTB, int line, string text)
        {
            int s1 = RTB.GetFirstCharIndexFromLine(line);
            int s2 = line < RTB.Lines.Count() - 1 ?
                      RTB.GetFirstCharIndexFromLine(line + 1) - 1 :
                      RTB.Text.Length;
            RTB.Select(s1, s2 - s1);
            RTB.SelectedText = text;
        }
       
        private string MultiplyTab()
        {
            string t ="";
            for(int i = 0;i< TabKeyNum; i++)
            {
                t += "\t";
            }
            return t;
        }
        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
           
            if (e.KeyCode == Keys.F5)
            {
                RunInmterpreter();
            }
            else if (e.KeyCode == Keys.F6)
            {
                RunInterpreterDebug();
            }
            else if (e.KeyCode == Keys.F7)
            {
                Interpreter.NextStepSignal = true;
            }
            else if (e.KeyCode == Keys.F9)
            {
                Interpreter.DebugSession = false;
            }
            else if (e.KeyCode == Keys.Return)
            {
 
                
                if (PrevKey == (Keys.Shift|Keys.Oem1) || PrevKey ==Keys.Oem1)
                {
                    TabKeyNum += 1;
                    int cursorPosition = richTextBox1.SelectionStart;
                    int lineIndex = richTextBox1.GetLineFromCharIndex(cursorPosition);
                    string lineText = richTextBox1.Lines[lineIndex];
                    changeLine(richTextBox1, lineIndex, lineText + MultiplyTab());
                    
                }
                else
                {
           
                    int cursorPosition = richTextBox1.SelectionStart;
                    int lineIndex = richTextBox1.GetLineFromCharIndex(cursorPosition);
                    string lineText = richTextBox1.Lines[lineIndex];
                    changeLine(richTextBox1, lineIndex, lineText + "\t");
                    TabKeyNum = 1;
                }
                
            }
           
            else if (e.KeyData == (Keys.Shift | Keys.D9))
            {
                int cursorPosition = richTextBox1.SelectionStart;
                int lineIndex = richTextBox1.GetLineFromCharIndex(cursorPosition);
                string lineText = richTextBox1.Lines[lineIndex];
                changeLine(richTextBox1, lineIndex, lineText + ")");
                richTextBox1.Select(cursorPosition, 0);
            }
            else if(e.KeyCode == Keys.Back)
            {
                TabKeyNum =1;
            }
            PrevKey = e.KeyData;
        }
    
        private void Console_OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                Interpreter.ConsoleLine = richTextBox2.Lines[richTextBox2.Lines.Count()-2];
                Interpreter.ConsoleInput = true;
            }
        }

        private void RunSlider()
        {
            int prevLine = 0;
            int firstcharindex = 0;
            Regex tabcount = new Regex(@"[\t]");
            int count_of_tabs = 0;
            int prev_count_of_tabs = 0;
            while (Interpreter.DebugSession == true)
            {
                
                int c = Interpreter.DebugSessionCurrentLine;
                richTextBox1.Invoke((MethodInvoker)delegate { count_of_tabs = tabcount.Matches(richTextBox1.Lines[c]).Count; });
                if (c == prevLine)
                {
                    continue;
                }
                else
                {
                    richTextBox1.Invoke((MethodInvoker)delegate { firstcharindex = richTextBox1.GetFirstCharIndexFromLine(prevLine); });
                    richTextBox1.Invoke((MethodInvoker)delegate { richTextBox1.Select(firstcharindex + prev_count_of_tabs, richTextBox1.Lines[prevLine].Length- prev_count_of_tabs); });
                    richTextBox1.Invoke((MethodInvoker)delegate { richTextBox1.SelectionBackColor = richTextBox1.BackColor; });
                    prevLine = c;
                    prev_count_of_tabs = count_of_tabs;
                }
                
                richTextBox1.Invoke((MethodInvoker)delegate { firstcharindex = richTextBox1.GetFirstCharIndexFromLine(c); });
                richTextBox1.Invoke((MethodInvoker)delegate { richTextBox1.Select(firstcharindex + count_of_tabs, richTextBox1.Lines[c].Length - count_of_tabs); });
                richTextBox1.Invoke((MethodInvoker)delegate { richTextBox1.SelectionBackColor = DebugColor; });
                
            }
            richTextBox1.Invoke((MethodInvoker)delegate { firstcharindex = richTextBox1.GetFirstCharIndexFromLine(prevLine); });
            richTextBox1.Invoke((MethodInvoker)delegate { richTextBox1.Select(firstcharindex+ prev_count_of_tabs, richTextBox1.Lines[prevLine].Length- prev_count_of_tabs); });
            richTextBox1.Invoke((MethodInvoker)delegate { richTextBox1.SelectionBackColor = richTextBox1.BackColor; });
        }
      
        private void RunInterpreterDebug()
        {
            if (Interpreter.InterpreterRunning == true)
            {
                return;
            }
            runtimeParse.RunForAll();
            Interpreter.DebugSession = true;
            string t = richTextBox1.Text;
            Interpreter.InterpreterRunning = true;
            Task.Run(() => RunSlider());
            Task.Run(() => interpreter.InterpretDebug(t));


        }
        private void button4_Click(object sender, EventArgs e)
        {
                RunInterpreterDebug();  
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var fileContent = string.Empty;
            var filePath = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "txt files (*.txt)|*.txt";
                openFileDialog.FilterIndex = 2;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    filePath = openFileDialog.FileName;
                    var fileStream = openFileDialog.OpenFile();

                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        fileContent = reader.ReadToEnd();
                    }
                }
            }
            sourceCodeDirectory = filePath;
            richTextBox1.Text = fileContent;
            string filename = filePath.Substring(filePath.LastIndexOf("\\") + 1, filePath.Length - filePath.LastIndexOf("\\") - 1);
            richTextBox1.Select(richTextBox1.Text.Length, 0);

            label1.Text = filename;
            runtimeParse.RunForAll();//mozna jako watek zrobic
            AddLineNumbers();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sourceCodeDirectory == "")//tworzenie nowego pliku
            {

                SaveFileDialog saveFileDialog1 = new SaveFileDialog();

                saveFileDialog1.Filter = "txt files (*.txt)|*.txt";
                saveFileDialog1.FilterIndex = 2;

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string filePath = saveFileDialog1.FileName;
                    var myStream = saveFileDialog1.OpenFile();
                    using (StreamWriter writer = new StreamWriter(myStream))
                    {
                        writer.Write(richTextBox1.Text);
                    }
                    sourceCodeDirectory = filePath;
                    myStream.Close();
                }
            }
            else//zapisanie do sciezki istniejacej
            {
                using (StreamWriter writer = new StreamWriter(sourceCodeDirectory))
                {
                    writer.Write(richTextBox1.Text);
                }
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "txt files (*.txt)|*.txt";
            saveFileDialog1.FilterIndex = 2;
            string filePath;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                filePath = saveFileDialog1.FileName;
                var myStream = saveFileDialog1.OpenFile();
                using (StreamWriter writer = new StreamWriter(myStream))
                {
                    writer.Write(richTextBox1.Text);
                }
                sourceCodeDirectory = filePath;
                myStream.Close();
                string filename = filePath.Substring(filePath.LastIndexOf("\\") + 1, filePath.Length - filePath.LastIndexOf("\\") - 1);
                label1.Text = filename;
            }

        }

        private void newFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sourceCodeDirectory = "";
            label1.Text = "Unknown";
            richTextBox1.Clear();
            richTextBox1.Text = "\n#main():\n\t";
            richTextBox1.Select(9, 0);
            runtimeParse.Run();
        }

        private void releaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RunInmterpreter();
        }

        private void debugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RunInterpreterDebug();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Interpreter.NextStepSignal = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Interpreter.DebugSession = false;
        }

        private void nextStepF7ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Interpreter.NextStepSignal = true;
        }

        private void exitDebugF9ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Interpreter.DebugSession = false;
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            runtimeParse.Run();
        }
    }
}


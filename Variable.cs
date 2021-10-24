using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;
namespace Interpreter
{
    class Variable
    {
        public string name;
        public dynamic value;
        public Variable() { }
        public static Variable FindInGlobalVariables(string name_)
        {
            if (name_ == null)
            {
                return null;
            }
            if (InterStack.CurrentFunction.LocalVariables == null)
            {
                return null;
            }
            if (InterStack.CurrentFunction.LocalVariables.ContainsKey(name_))
            {
                return InterStack.CurrentFunction.LocalVariables[name_];
            }
            return null;
        }

        public static dynamic ConvertAssign(Variable arg, dynamic val)
        {
            if(arg is Int)
            {
                return Convert.ToInt32(val);
            }
            else if(arg is Double)
            {
                return Convert.ToDouble(val);
            }
            else if(arg is String)
            {
                return Convert.ToString(val);
            }
            else if(arg is Bool)
            {
                return Convert.ToBoolean(val);
            }
            else if(arg is LinkedList)
            {
                return val;
            }
            return null;
        }
        public static double Calculate(string expression)
        {
            try
            {
                DataTable table = new DataTable();
                table.Columns.Add("expression", typeof(string), expression);
                DataRow row = table.NewRow();
                table.Rows.Add(row);
                return double.Parse((string)row["expression"]);
            }
            catch(Exception e)
            {
                throw new InterException(e.Message);
            }  
        }

        public static Variable Copy(Variable org)
        {
            if(org is Int)
            {
                return new Int(org.name,(int)org.value);
            }
            else if(org is Double)
            {
                return new Double(org.name,(double)org.value);
            }
            else if (org is Bool)
            {
                return new Bool(org.name, Convert.ToBoolean(org.value));
            }
            else if (org is String)
            {
                return new String(org.name, Convert.ToString(org.value));
            }
            else if (org is LinkedList)
            {
                Variable l = new LinkedList();
                foreach (var a in org.value)
                {
                    l.value.Add(Copy(a));
                }
                return l;
            }
            return new Variable();
           // throw new InterException("2A", "Variable type error"); 
        }
       
        public static Variable ReturnType(string element)
        {
            if (element == null)
            {
                throw new InterException("2B", "NULL element was passed");
            }
            Variable r;
           
            if (element == "True")
            {
                r = new Bool("", true);
            }
            else if (element == "False")
            {
                r = new Bool("", false);
            }
            else if (Regex.IsMatch(element, @"^\'.*\'$"))
            {
                r = new String("", element);
            }
            else if (Regex.IsMatch(element, @"^(\-|\+)?[0-9]+\.[0-9]+$"))
            {
                double w;
                try
                {
                    w = Calculate(element);
                }
                catch (InterException e) { throw e; }
                r = new Double("", w);

            }
            else if(Regex.IsMatch(element, @"^(\-|\+)?[0-9]+$"))
            {
                double w;
                try
                {
                    w = Calculate(element);
                }
                catch(InterException e) { throw e; }
                try
                {
                    r = new Int("", Convert.ToInt32(w));
                }
                catch(Exception e)
                {
                    throw new InterException(e.Message);
                }
            }
            else if(Regex.IsMatch(element, @"^\(.*\)$"))//table
            {
                r = new LinkedList("", element);
            }
            else
            {
                r = null;
            }
            return r;
        }

        public virtual Variable Length() { return null;  }

        public virtual void UpdateValue(Variable index, Variable value) {}

        public virtual Variable ReturnAtIndex(List<Variable> arg){ return null; }

        public virtual void RemoveIndex(Variable arg) {}

        public static Variable operator+(Variable a, Variable b)
        {
            if(a is Double || b is Double || a is Int || b is Int)
            {
                return new Double("", (double)a.value + (double)b.value);
            }
            else if(a is String && b is String)
            {
                return new String("", a.value + b.value);
            }
            else
            {
                throw new InterException("2C","Cant use operator + for variable type: " + a.GetType().Name + " & " + b.GetType().Name);
            }
        }
        public static Variable operator -(Variable a, Variable b)
        {
            if (a is Double || b is Double || a is Int || b is Int)
            {
                return new Double("", (double)a.value - (double)b.value);
            }
            else
            {
                throw new InterException("2D", "Cant use operator - for variable type: " + a.GetType().Name + " & " + b.GetType().Name);
            }
        }
        public static Variable operator *(Variable a, Variable b)
        {
            if (a is Double || b is Double || a is Int || b is Int)
            {
                return new Double("", (double)a.value * (double)b.value);
            }
            else
            {
                throw new InterException("2E", "Cant use operator * for variable type: " + a.GetType().Name + " & " + b.GetType().Name);
            }
        }
        public static Variable operator /(Variable a, Variable b)
        {
            if (a is Double || b is Double || a is Int || b is Int)
            {
                return new Double("", (double)a.value / (double)b.value);
            }
           
            else
            {
                throw new InterException("2F", "Cant use operator / for variable type: " + a.GetType().Name + " & " + b.GetType().Name);
            }
        }
        public static Variable operator %(Variable a, Variable b)
        {
            if (a is Double || b is Double || a is Int || b is Int)
            {
                return new Double("", (double)a.value % (double)b.value);
            }
            else
            {
                throw new InterException("2G", "Cant use operator % for variable type: " + a.GetType().Name + " & " + b.GetType().Name);
            }
        }
        
        public virtual void Append(Variable toAppend) {}

        public virtual string VarType()
        {
            return "NULL";
        }

        public virtual void UpdateNestedValue(List<Variable> indexes, Variable new_value)  { }
    }
    class Int : Variable
    {
        public Int(string name_, int value_)
        {
            name = name_;
            value = value_;
        }

        public override Variable Length() {
            return new Int("", 0);
        }

        public override string ToString()
        {
            return Convert.ToString(value);
        }

        public override string VarType()
        {
            return "Int";
        }
    }

    class String : Variable
    {
        public String(string name_, string value_)
        {
            value_ = Regex.Replace(value_, @"[\']+", "");
            name = name_;
            value = value_;
        }
        public override Variable Length()
        {
            return new Int("", value.Length);
        }
      
        public override string ToString()
        {
            return value;
        }

        public override Variable ReturnAtIndex(List<Variable> arg)//arg[0].value ->int index
        {
            return new String("", value[arg[0].value].ToString());
        }

        public override string VarType()
        {
            return "String";
        }
        public override void UpdateValue(Variable index, Variable value)
        {
            if(value.value.Length!=1)
            {
                throw new InterException("2H","UpdateValue: argument 3 invalid length <> 1");
            }
          
            if ((int)index.value >= this.value.Length)
            {
                throw new InterException("2I", "Index: " + Convert.ToString( index.value )+ " out of range for: " + name);
            }
            char[] array_rep = value.value.ToCharArray();
            char[] array = this.value.ToCharArray();
            array[(int)index.value] = array_rep[0];
            this.value = new string(array);
        }
    }

    class Double : Variable
    {

        public Double(string name_, double value_)
        {
            name = name_;
            value = value_;
        }
     
      
        public override string ToString()
        {
            return Convert.ToString(value);
        }
        public override Variable Length()
        {
            return new Int("", 0);
        }

        public override string VarType()
        {
            return "Double";
        }

    }
    class Bool : Variable
    {
        public Bool(string name_, bool value_)
        {
            name = name_;
            value = value_;
        }
    
        public override string ToString()
        {
            return Convert.ToString(value);
        }

        public override Variable Length()
        {
            return new Int("", 0);
        }

        public override string VarType()
        {
            return "Bool";
        }
    }
    class LinkedList : Variable
    {
        public LinkedList() { 
            this.value = new List<Variable>();
        }
        private List<string> ParseArgs(string table_args)
        {
            List<string> args = new List<string>();
            int bracket = 0;
            int length = table_args.Length;
            string temp = "";
            for (int i = 0; i < length; i++)
            {
                if (table_args[i] == '(') { bracket++; }
                else if (table_args[i] == ')') { bracket--; }
                if (bracket == 0 && table_args[i] == ',')
                {
                    args.Add(temp);
                    temp = "";
                    continue;
                }
                temp += table_args[i];
            }
            if (temp != "")
            {
                args.Add(temp);
            }
            return args;
        }

        public LinkedList(string name_, string elements_to_parse)//expected (4,True,-2)
        {
            elements_to_parse = elements_to_parse.Substring(1, elements_to_parse.Length - 2);
            List<string> args = ParseArgs(elements_to_parse);
            value = new List<Variable>();
            foreach (string e in args)
            {
                Variable v;
                try
                {
                     v = Variable.ReturnType(e);
                }
                catch(InterException ex) { throw ex; }
                value.Add(v);
            }
            name = name_;
        }
        public override void UpdateValue(Variable index, Variable value)
        {

            if ((int)index.value >= this.value.Count)
            {
                throw new InterException("2J", "Index: " + Convert.ToString(index.value) + " out of range for: " + name);
            }
            this.value[(int)index.value].value = value.value;
        }
        
        public override void UpdateNestedValue(List<Variable>indexes, Variable new_value)
        {
            if (indexes.Count >= 1)
            {
                
                Variable temp = this;
                foreach(var ind in indexes)
                {
                 
                    temp = temp.value[(int)ind.value];

                }
                temp.value = new_value.value;        
            }
            else
            {
                throw new InterException("2K","Invalid Table indexes");
            }
        }

        public override string ToString()
        {
            return Convert.ToString(value);
        }

        public override Variable ReturnAtIndex(List<Variable> arg)//arg[0].value ->int index,arg[n] dla zagniezczonych tablic
        {
           
            if ((int)arg[0].value >= value.Count)
            {
                throw new InterException("2L","Index: " + Convert.ToString(arg[0].value) + " out of range for: " + name);
            }
            if (arg.Count == 1)
            {
                return value[(int)arg[0].value];
            }
            else if(arg.Count > 1)
            {
                LinkedList table = new LinkedList();
                table.value = this.value;
                foreach (var a in arg)
                {
                    if(table.value[(int)a.value] is LinkedList)
                    {
                        table = (LinkedList)table.value[(int)a.value];
                    }
                    else
                    {
                        return table.value[(int)a.value];
                    }
                   
                }
                return table;
            }
            else
            {
                throw new InterException("2M","Invalid Table idexes");
            } 
        }

        public override Variable Length()
        {
            return new Int("", value.Count);
        }

        public override void Append(Variable toAppend)
        {
            
            this.value.Add(Copy(toAppend));
        }

        public override string VarType()
        {
            return "Array";
        }

        public override void RemoveIndex(Variable arg)//arg[0] index
        {
            if ((int)arg.value >= this.value.Count)
            {
                throw new InterException("2N","Index: " + Convert.ToString(arg.value) + " out of range for: " + name);
            }
            this.value.RemoveAt((int)arg.value);
        }
    }
}

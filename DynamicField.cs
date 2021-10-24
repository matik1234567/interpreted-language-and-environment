using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Interpreter
{
    class Field
    {
        public List<DynamicField> FuncArgs = new List<DynamicField>();
        public static VariableField zero = new VariableField(new Int("", 0));
        public Field() { }

        public void Clear()
        {
            foreach(var d in FuncArgs)
            {
                d.Clear();
            }
          
            FuncArgs.Clear();
        }
      
        public virtual Variable ActionfieldElements()
        {
            throw new InterException("3A","");
        }

        public virtual Variable OperatorAction(Variable arg1, Variable arg2)
        {
            throw new InterException("3B", "");
        }

        public static Field ChooseOperatorPiority(char operator_)
        {
            string expr = operator_.ToString();
            if (Regex.Matches(expr, @"[+\-]").Count != 0)
            {
                return new OperatorPior2(operator_);
            }
            else
            {
                return new OperatorPior1(operator_);
            }
        }
        public static Field FieldRecognize(string command)
        {
            if (Validation.IsFunctionCall(command))
            {
                return new FunctionField(command);
            }
            else if (Validation.IsBracketCall(command))
            {
                return new BracketField(command);
            }
            else
            {
                return new VariableField(command);
            }
        }
    }


    class FunctionField : Field
    {
        private string func_name;
        public FunctionField(string field_)
        {
            int ind = field_.IndexOf('(');
            if(ind == -1)
            {
                throw new InterException("3C", "Invalid function syntax");

            }
            func_name = field_.Substring(0, ind);
            try
            {
                FuncArg(this, field_);
            }
            catch(InterException e) { throw e; }
        }
        private void FuncArg(Field field_, string first_field)
        {
            int n = 0;
            int ind = first_field.IndexOf('(');
            first_field = first_field.Substring(ind + 1, first_field.Length - ind - 2);
            if (first_field == "")
            {
                return;
            }
            int len = first_field.Length;
            string temp = "";
            bool protect = false;
            for (int i = 0; i < len; i++)
            {
                if (first_field[i] == '(') { n++; }
                else if (first_field[i] == ')') { n--; }
                if (first_field[i] == '\'' && protect == false) { protect = true; }
                else if (first_field[i] == '\'' && protect == true) { protect = false; }
                if (first_field[i] == ',' && n == 0 && protect == false)
                {
                    try
                    {
                        field_.FuncArgs.Add(new DynamicField(temp));
                    }
                    catch (InterException e) { throw e; }
                    temp = "";
                    continue;
                }
                temp += first_field[i];
            }
            try
            {
                field_.FuncArgs.Add(new DynamicField(temp));
            }
            catch (InterException e) { throw e; }
        }
        public override Variable ActionfieldElements()
        {
            
            List<Variable> t = new List<Variable>();
            foreach(var q in FuncArgs)
            {
                try
                {
                    t.Add(q.FieldAction());
                }
                catch(InterException e) { throw e; }
            }
            if (InterStack.CurrentFunction.LocalVariables != null && InterStack.CurrentFunction.LocalVariables.ContainsKey(func_name))
            {
                if (InterStack.CurrentFunction.LocalVariables[func_name] is LinkedList)
                {
                    return InterStack.CurrentFunction.LocalVariables[func_name].ReturnAtIndex(t);
                }
            }
            else if (!InterFunctions.functions.ContainsKey(func_name))
            {
                throw new InterException("3F", "Can not find function: " + func_name);
            }
            Variable r = InterFunctions.functions[func_name](t);
        
            return r;

        }
    }
    class VariableField : Field
    {
        private Variable variable=null;
        private string variable_name="";
        private bool isConstant = false;
        public VariableField(string field_)
        {
            variable_name = field_;          
        }

        public VariableField(Variable field_)
        {  
            variable = field_;
            isConstant = true;
        }

        public override Variable ActionfieldElements()
        {
            if (isConstant==true)
            {
                return variable;
            }
            else
            {
                Variable n = Variable.FindInGlobalVariables(variable_name);
                if (n == null)
                {
                    n = Variable.ReturnType(variable_name);
                }
                if (n == null)
                {
                    throw new InterException("3D", "Undefined variablie: " + variable_name);
                }
                return n;
            }    
        }
    }

    class BracketField : Field
    {
        public BracketField(string field_)//wywolanie expected:(-2+4+fun())
        {
            try
            {
                field_ = BracketCut(field_);
            }
            catch(InterException e) { throw e; }
            try
            {
                FuncArgs.Add(new DynamicField(field_));
            }
            catch(InterException e) { throw e; }
        }
        private string BracketCut(string command)
        {
            int br1 = Regex.Matches(command, @"[\(]").Count;
            int br2 = Regex.Matches(command, @"[\)]").Count;
            if (br1 == 1 && br2 == 1)
            {
                return command.Substring(1, command.Length-2);
            }
            else if (br1 == br2)
            {
                return command;
            }
            else
            {
                throw new InterException("3E","Brackets syntax error: "+ command);
            }

        }
        public override Variable ActionfieldElements()
        {
            try
            {
                return FuncArgs[0].FieldAction();
            }
            catch(InterException e) { throw e; }
        }
    }

    class OperatorPior1 : Field
    {
        private Func<Variable, Variable, Variable> function = null;
        private static Dictionary<char, Func<Variable, Variable, Variable>> operators = new Dictionary<char, Func<Variable, Variable, Variable>>
        {
            {'*', Multiplication }, {'/', Division }, {'%', Modulo },
        };
        public OperatorPior1(char field_)
        {
            
            function = operators[field_];   
        }
        
        public static Variable Multiplication(Variable arg1, Variable arg2)
        {
            try { return arg1 * arg2; }catch(InterException e) { throw e; }
        }
        public static Variable Division(Variable arg1, Variable arg2)
        {
            try { return arg1 / arg2; } catch (InterException e) { throw e; }
        }
        public static Variable Modulo(Variable arg1, Variable arg2)
        {
            try { return arg1 % arg2; } catch (InterException e) { throw e; }
        }
        public override Variable ActionfieldElements()
        {
            return null;
        }
        public override Variable OperatorAction(Variable arg1, Variable arg2)
        {
            try { return function(arg1, arg2); }catch(InterException e) { throw e; }
            
        }
    }
    class OperatorPior2: Field
    {
        private Func<Variable, Variable, Variable> function = null;
        private static Dictionary<char, Func<Variable, Variable, Variable>> operators = new Dictionary<char, Func<Variable, Variable, Variable>>
        {
            { '+', Addition }, {'-', Substraction }, 
        };
        public OperatorPior2(char field_)
        {

            function = operators[field_];
        }
        public static Variable Addition(Variable arg1, Variable arg2)
        {
            try { return arg1 + arg2; } catch (InterException e) { throw e; }
        }
        public static Variable Substraction(Variable arg1, Variable arg2)
        {
            try { return arg1 - arg2; } catch (InterException e) { throw e; }
        }
       
        public override Variable ActionfieldElements()
        {
            return null;
        }
        public override Variable OperatorAction(Variable arg1, Variable arg2)
        {
            try { return function(arg1, arg2); } catch (InterException e) { throw e; }
        }
    }

    class DynamicField
    {
        private List<Field> Mainelements = new List<Field>();

        public void Clear()
        {
            foreach(var f in Mainelements)
            {
                f.Clear();
            }
            Mainelements.Clear();
        }
        public DynamicField(string field_)
        {
         
            try
            {
                DynamicFieldSeparate(field_);
            }
            catch(InterException e)
            {
                throw e;
            }
        }
       
        private void DynamicFieldSeparate(string field_)
        {
            int bracket = 0;
            int command_length = field_.Length;
            string current = "";
            for (int i = 0; i < command_length; i++)
            {
                if (field_[i] == '(') { bracket++; }
                else if (field_[i] == ')') { bracket--; }

                if (bracket == 0 && Validation.IsOperator(field_[i]) == true)
                {
                    if (current != "")
                    {
                        this.Mainelements.Add(Field.FieldRecognize(current));//dodanie field
                    }
                    
                    this.Mainelements.Add(Field.ChooseOperatorPiority(field_[i]));//dodanie operatora
                    current = "";
                    continue;
                }
                current += field_[i];
            }
            if (current != "")
            {
                this.Mainelements.Add(Field.FieldRecognize(current));
            } 
        }
       

        public Variable FieldAction()//degradacja (..) oraz func(..) do zmiennych nastepnie *,/,%,+,-
        {
            if (this.Mainelements.Count == 1)
            {
                try
                {
                    return this.Mainelements[0].ActionfieldElements();
                }
                catch(InterException e) { throw e; }
            }
            List<Field> NewMainelemets = new List<Field>();
            for(int i = 0; i < this.Mainelements.Count; i++)
            {
                if(this.Mainelements[i] is FunctionField || this.Mainelements[i] is BracketField)
                {
                    try
                    {
                        NewMainelemets.Add(new VariableField(this.Mainelements[i].ActionfieldElements()));
                    }
                    catch(InterException e) { throw e; }      
                }
                else
                {
                    NewMainelemets.Add(this.Mainelements[i]);
                }
            }
            for(int j = 0; j < NewMainelemets.Count; j++)
            {
                if(NewMainelemets[j] is OperatorPior1)
                {
                    try
                    {
                        NewMainelemets[j + 1] = new VariableField(NewMainelemets[j].OperatorAction(NewMainelemets[j - 1].ActionfieldElements(),
                            NewMainelemets[j + 1].ActionfieldElements()));

                    }
                    catch(InterException e) { throw e; }
                    NewMainelemets.RemoveAt(j);
                    NewMainelemets.RemoveAt(j-1);
                    j = 0;
                }
            }
            for (int j = 0; j < NewMainelemets.Count; j++)
            {
                if (NewMainelemets[j] is OperatorPior2)
                {
                    if (j == 0)//operator - moze byc na poczatku(-3+4) wiec operacja 0-3
                    {
                        try
                        {
                            NewMainelemets[j + 1] = new VariableField(NewMainelemets[j].OperatorAction(Field.zero.ActionfieldElements(), 
                                NewMainelemets[j + 1].ActionfieldElements()));

                        }
                        catch(InterException e) { throw e; }
                        NewMainelemets.RemoveAt(j);
                        j = 0;
                    }
                    else
                    {
                        try
                        {
                            NewMainelemets[j + 1] = new VariableField(NewMainelemets[j].OperatorAction(NewMainelemets[j - 1].ActionfieldElements(),
                                NewMainelemets[j + 1].ActionfieldElements()));

                        }
                        catch(InterException e) { throw e; }
                        NewMainelemets.RemoveAt(j);
                        NewMainelemets.RemoveAt(j - 1);
                        j = 0;
                    }
                }
            }
            try
            {
                return NewMainelemets[0].ActionfieldElements();
            }
            catch(InterException e) { throw e; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Interpreter
{
    class MainFunction
    {
       
        public static Dictionary<string, Function> InitializedFunctions = new Dictionary<string, Function>();
        private Dictionary<string, Variable> LocalVariables = new Dictionary<string, Variable>();
        Function mainFunc = null;
        public MainFunction() {}

        public void AddFunction(Function funcion_)
        {
            InitializedFunctions.Add(funcion_.FunctionName, funcion_);
        }

        public void InitializeCallTree(Function starter_)
        {
            mainFunc = starter_;
        }

        public void InitializeVariables(Dictionary<string, Variable> LocalVariables_)
        {
            LocalVariables = LocalVariables_;

        }
        public void Start()
        {
            try
            {
                InterStack.OnStack(mainFunc);
                mainFunc.RunFunctionMain();
            }
            catch(InterException e)
            {
                throw e;
            }
        }
        public void Clear()
        {
            foreach(var func in InitializedFunctions)
            {
                func.Value.Clear();
            }
            InitializedFunctions.Clear();
        }
    }
}

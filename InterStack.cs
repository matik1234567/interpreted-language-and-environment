using System;
using System.Collections.Generic;
using System.Text;

namespace Interpreter
{
    class InterStack
    {
        private static List<Function> StackFunctions = new List<Function>();
        public static Function CurrentFunction=null;
        private const int StackMaxSize = 40;

        public InterStack()
        {
            StackFunctions.Clear();
        }

        public static void OnStack(Function func)
        {
            StackFunctions.Add(func);
            CurrentFunction = func;
            if (StackMaxSize < StackFunctions.Count)
            {
               
                throw new InterException("Stack overflow");
            }
        }

        public static void RemoveStackLast()
        {
            StackFunctions.Remove(StackFunctions[StackFunctions.Count - 1]);
            CurrentFunction = StackFunctions[StackFunctions.Count - 1];
        }

        public static void RemoveStackSpecific(Function func)
        {
            foreach(var a in StackFunctions)
            {
                if (a == func)
                {
                    StackFunctions.Remove(a);
                    CurrentFunction = StackFunctions[StackFunctions.Count - 1];
                    return;

                }
            }
            throw new InterException("Stack overflow");
        }

        public void Clear()
        {
            CurrentFunction.Clear();
            StackFunctions.Clear();
        }
    }
}

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// The environment in which the program executes
public class Program
{
    public class Frame // A call-stack frame.
    {
        Dictionary<string, object> variables = new Dictionary<string,object>();
        object retVal;

        public void SetVar(string key, object value)
        {
            variables[key] = value;
        }

        public void SetRetVal(object retVal)
        {
            this.retVal = retVal;
        }

        public object GetRetVal() {return retVal;}

        public object GetVar(string key)
        {
            if(!variables.ContainsKey(key)) {
                Debug.LogError("Attempt to access undeclared var '" + key + "'");
                GameObject.FindObjectOfType<GUIIDE>().WriteLine("<color=red>Variable " + key + " does not exist in this context</color>");
                return null;
            }
            return variables[key];
        }
    }

    Dictionary<string, Func<object[], object>> functions;
    Dictionary<string, ASTFunction> userFunctions = new Dictionary<string,ASTFunction>();
    Stack<Frame> frames = new Stack<Frame>();
    bool retFlag = false;

    public Program()
    {
        // Builtin functions
        functions = new Dictionary<string,Func<object[],object>>()
        {
            {"print", delegate(object[] args)
            {
                //Debug.Log("print: '" + args[0] + "'");
                GUIIDE.Ide.WriteLine(args[0].ToString());
                return null;
            }},
            {"tuple", delegate(object[] args)
            {
                object[] tuple = new object[args.Length];
                args.CopyTo(tuple, 0);

                return tuple;
            }},
            {"list", delegate(object[] args)
            {
                return new List<object>(args);
            }},
            {"length", delegate(object[] args)
            {
                if(args[0] is string) {
                    return ((string)args[0]).Length;
                }

                ICollection col = args[0] as ICollection;

                return col.Count;
            }},
            {"at", delegate(object[] args)
            {
                ICollection col = args[0] as ICollection;
                int index = (int)args[1];

                if(args[0] is string) {
                    string str = (string)args[0];

                    if(index >= str.Length) {
                        GUIIDE.Ide.WriteLine("<color=red>Index " + index + " is out of range</color>");
                    }

                    return str[index];
                }

                if(index >= col.Count) {
                    GUIIDE.Ide.WriteLine("<color=red>Index " + index + " is out of range</color>");
                    return null;
                }

                if(col is object[]) {
                    object[] array = (object[])col;
                    return array[index];
                }

                if(col is List<object>) {
                    List<object> array = (List<object>)col;
                    return array[index];
                }

                return null;
            }},
            {"setAt", delegate(object[] args)
            {
                ICollection col = args[0] as ICollection;
                int index = (int)args[1];
                object val = args[2];

                if(col is object[]) {
                    object[] array = (object[])col;
                    array[index] = val;
                }

                if(col is List<object>) {
                    List<object> array = (List<object>)col;
                    array[index] = val;
                }

                return null;
            }},
            {"rand", delegate(object[] args)
            {
                int min = (int)args[0];
                int max = (int)args[1];

                return UnityEngine.Random.Range(min, max);
            }},
            {"typeof", delegate(object[] args)
            {
                object obj = args[0];

                if(obj is int) {
                    return "int";
                } else if(obj is float) {
                    return "float";
                } else if(obj is string) {
                    return "string";
                } else if(obj is object[]) {
                    return "tuple";
                } else if(obj is List<object>) {
                    return "list";
                } else if(obj is bool) {
                    return "bool";
                }
                return "null";
            }},
        };
        
        frames.Push(new Frame());
    }

    
    void EecursivePrint(int count)
    {
	    if(count < 0) {
		    return;
        }
        GUIIDE.Ide.WriteLine("" + count);
	    EecursivePrint(count - 1);
    }

    public bool DoReturn {get{return retFlag;}}

    public void Return()
    {
        retFlag = true;
    }

    public void SetRetVal(object retVal)
    {
        frames.Peek().SetRetVal(retVal);
    }

    public object GetRetVal() {return frames.Peek().GetRetVal();}

    public void PushFrame()
    {
        frames.Push(new Frame());
    }

    public void PopFrame()
    {
        frames.Pop();
        retFlag = false;
    }

    public void RegisterFuncion(ASTFunction function)
    {
        userFunctions.Add(function.Name, function);
    }

    public object Call(string func, params object[] args)
    {
        if(functions.ContainsKey(func)) {
            return functions[func](args);
        }

        if(!userFunctions.ContainsKey(func)) {
            GameObject.FindObjectOfType<GUIIDE>().WriteLine("<color=red>Function " + func + " does not exist</color>");
        }

        return userFunctions[func].Call(this, args);
    }

    public void SetVar(string key, object value)
    {
        frames.Peek().SetVar(key, value);
    }

    public object GetVar(string key)
    {
        return frames.Peek().GetVar(key);
    }
}

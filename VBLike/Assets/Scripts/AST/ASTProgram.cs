using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

// Base class for all Abstract Syntax Tree Nodes
public class ASTNode
{
    protected const string TAB = "    ";

    // Logs the node, for debug purposes
    public virtual void Log(StringBuilder logger, string space)
    {
        logger.AppendLine(space + GetType().Name);
    }
}

public class ASTFunction : ASTNode
{
    public string Name {get; private set;}
    List<string> args = new List<string>();
    ASTStatements statements = new ASTStatements();

    public ASTFunction(string name)
    {
        Name = name;
    }

    public void AddArg(string arg)
    {
        args.Add(arg);
    }

    public void AddStatement(ASTStatement statement)
    {
        statements.AddStatement(statement);
    }

    public override void Log(StringBuilder logger, string space)
    {
        logger.AppendLine(space + GetType().Name + ": " + Name);

        foreach(var arg in args) {
            logger.AppendLine(space + "  " + arg);
        }

        statements.Log(logger, TAB + space);
    }

    public object Call(Program program, object[] args)
    {
        program.PushFrame();

        for(int i = 0; i < args.Length; i++) {
            program.SetVar(this.args[i], args[i]);
        }

        statements.Eval(program);

        object retVal = program.GetRetVal();

        program.PopFrame();

        return retVal;
    }
}

public class ASTProgram : ASTNode
{
    public ASTStatements Statements {get; private set;}
    List<ASTFunction> functions = new List<ASTFunction>();

    public ASTProgram()
    {
        Statements = new ASTStatements();
    }

    public void AddFunction(ASTFunction function)
    {
        functions.Add(function);
    }

    public override void Log(StringBuilder logger, string space)
    {
        base.Log(logger, space);
        Statements.Log(logger, TAB + space);

        foreach(var func in functions) {
            func.Log(logger, TAB + space);
        }
    }

    // Eval executes the program
    public void Eval(Program program)
    {
        float startTime = Time.realtimeSinceStartup;
        foreach(var func in functions) {
            program.RegisterFuncion(func);
        }

        Statements.Eval(program);

        float delta = Time.realtimeSinceStartup - startTime;

        Debug.Log("Took " + (delta / 1000f) + " to evaluate");

        GameObject.FindObjectOfType<GUIIDE>().WriteLine("<b>Took " + (delta / 1000f) + "s to run</b>");
    }
}

public class ASTStatements : ASTNode
{
    List<ASTStatement> statements = new List<ASTStatement>();

    public void AddStatement(ASTStatement statement)
    {
        statements.Add(statement);
    }

    public override void Log(StringBuilder logger, string space)
    {
        base.Log(logger, space);
        foreach(var stmnt in statements) {
            stmnt.Log(logger, TAB + space);
        }
    }

    public void Eval(Program program)
    {
        foreach(var stmnt in statements) {
            stmnt.Eval(program);
            if(program.DoReturn) {
                break;
            }
        }
    }
}


using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class ASTExpression : ASTNode
{
    public virtual object Eval(Program program)
    {
        return null;
    }
}

public class ASTFuncCall : ASTExpression
{
    string funcName;
    List<ASTExpression> expressions = new List<ASTExpression>();

    public ASTFuncCall(string funcName)
    {
        this.funcName = funcName;
    }

    public void AddArg(ASTExpression expression)
    {
        expressions.Add(expression);
    }

    public override void Log(StringBuilder logger, string space)
    {
        logger.AppendLine(space + GetType().Name + ": " + funcName);

        foreach(var expr in expressions) {
            expr.Log(logger, TAB + space);
        }
    }

    public override object Eval(Program program)
    {
        object[] args = new object[expressions.Count];

        for(int i = 0; i < expressions.Count; i++) {
            args[i] = expressions[i].Eval(program);
        }

        return program.Call(funcName, args);
    }
}

public class ASTLiteral : ASTExpression
{
    object value;

    public ASTLiteral(object value)
    {
        this.value = value;
    }

    public override void Log(StringBuilder logger, string space)
    {
        logger.AppendLine(space + GetType().Name + ": " + value);
    }

    public override object Eval(Program program)
    {
        return value;
    }
}

public class ASTVariable : ASTExpression
{
    string varName;

    public ASTVariable(string varName)
    {
        this.varName = varName;
    }

    public override void Log(StringBuilder logger, string space)
    {
        logger.AppendLine(space + GetType().Name + ": " + varName);
    }

    public override object Eval(Program program)
    {
        return program.GetVar(varName);
    }
}

public class ASTIndexer : ASTExpression
{
    ASTExpression baseExpr;
    ASTExpression indexExpr;

    public ASTIndexer(ASTExpression baseExpr, ASTExpression indexExpr)
    {
        this.baseExpr = baseExpr;
        this.indexExpr = indexExpr;
    }

    public override void Log(StringBuilder logger, string space)
    {
        logger.AppendLine(space + GetType().Name + ": ");
        baseExpr.Log(logger, TAB + space);
        indexExpr.Log(logger, TAB + space);
    }

    public override object Eval(Program program)
    {
        object baseObj = baseExpr.Eval(program);
        object index = indexExpr.Eval(program);

        if(baseObj is object[]) {
            object[] array = (object[])baseObj;
            return array[(int)index];
        } else if(baseObj is List<object>) {
            List<object> array = (List<object>)baseObj;
            return array[(int)index];
        }
        return null;
    }
}

public class ASTOperator : ASTExpression
{
    string op;
    ASTExpression left;
    ASTExpression right;

    public ASTOperator(string op, ASTExpression left, ASTExpression right)
    {
        this.op = op;
        this.left = left;
        this.right = right;
    }

    public override void Log(StringBuilder logger, string space)
    {
        logger.AppendLine(space + GetType().Name + ": " + op);
        left.Log(logger, TAB + space);
        right.Log(logger, TAB + space);
    }

    public override object Eval(Program program)
    {
        object lhs = left.Eval(program);
        object rhs = right.Eval(program);

        if(lhs.GetType() == typeof(string) || rhs.GetType() == typeof(string)) {
            return EvalString(lhs.ToString(), rhs.ToString());
        }

        if(lhs.GetType() == typeof(int)) {
            return EvalInt((int)lhs, (int)rhs);
        }

        if(lhs.GetType() == typeof(bool)) {
            return EvalBool((bool)lhs, (bool)rhs);
        }

        return null;
    }

    object EvalInt(int lhs, int rhs)
    {
        switch(op) {
            case "+":
                return lhs + rhs;
            case "-":
                return lhs - rhs;
            case "*":
                return lhs * rhs;
            case "/":
                return lhs / rhs;

            case "=":
                return lhs == rhs;
            case "!=":
                return lhs != rhs;
            case "<":
                return lhs < rhs;
            case ">":
                return lhs > rhs;
        }
        return -1;
    }

    object EvalString(string lhs, string rhs)
    {
        switch(op) {
            case "+":
                return lhs + rhs;
        }
        return "";
    }

    object EvalBool(bool lhs, bool rhs)
    {
        switch(op) {
            case "=":
                return lhs == rhs;
            case "!=":
                return lhs != rhs;
            case "&":
                return lhs && rhs;
            case "|":
                return lhs || rhs;
        }
        return false;
    }
}


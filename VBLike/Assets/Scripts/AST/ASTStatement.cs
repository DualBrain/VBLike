using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class ASTStatement : ASTNode
{
    public virtual void Eval(Program program)
    {

    }
}

public class ASTReturn : ASTStatement
{
    ASTExpression expression;

    public ASTReturn(ASTExpression expression)
    {
        this.expression = expression;
    }

    public override void Log(StringBuilder logger, string space)
    {
        base.Log(logger, space);
        expression.Log(logger, TAB + space);
    }

    public override void Eval(Program program)
    {
        object retVal = expression.Eval(program);
        program.SetRetVal(retVal);
        program.Return();
    }
}

public class ASTSet : ASTStatement
{
    string varName;
    ASTExpression expression;
    List<ASTExpression> indexers = new List<ASTExpression>();

    public ASTSet(string varName, ASTExpression expression, List<ASTExpression> indexers)
    {
        this.varName = varName;
        this.expression = expression;
        this.indexers = indexers;
    }

    public override void Log(StringBuilder logger, string space)
    {
        logger.AppendLine(space + GetType().Name + ": " + varName);
        expression.Log(logger, TAB + space);

        foreach(var indexer in indexers) {
            indexer.Log(logger, TAB + space);
        }
    }

    public override void Eval(Program program)
    {
        object value = expression.Eval(program);

        if(indexers.Count == 0) {
            program.SetVar(varName, value);
        } else {
            object cur = program.GetVar(varName);

            for(int i = 0; i < indexers.Count; i++) {
                int index = (int)indexers[i].Eval(program);

                if(cur is object[]) {
                    object[] array = (object[])cur;

                    if(i == indexers.Count - 1) {
                        array[index] = value;
                    } else {
                        cur = array[index];
                    }
                } else if(cur is List<object>) {
                    List<object> array = (List<object>)cur;

                    if(i == indexers.Count - 1) {
                        array[index] = value;
                    } else {
                        cur = array[index];
                    }
                }
            }
        }

        //Debug.Log("Setting var '" + varName + "' to " + value);
    }
}

public class ASTIf : ASTStatement
{
    ASTExpression check;
    ASTStatements statements;
    ASTStatements elseStatements;
    List<ASTIf> elseifs = new List<ASTIf>();

    public ASTIf(ASTExpression check, ASTStatements statements)
    {
        this.check = check;
        this.statements = statements;
    }

    public void SetElse(ASTStatements statements)
    {
        elseStatements = statements;
    }
    
    public void SetElseIf(ASTIf ifStmnt)
    {
        elseifs.Add(ifStmnt);
    }

    public override void Log(StringBuilder logger, string space)
    {
        base.Log(logger, space);
        check.Log(logger, TAB + space);
        statements.Log(logger, TAB + space);

        foreach(var stmnt in elseifs) {
            stmnt.Log(logger, TAB + space);
        }

        if(elseStatements != null) {
            elseStatements.Log(logger, TAB + space);
        }
    }

    public override void Eval(Program program)
    {
        bool b = (bool)check.Eval(program);
        if(b) {
            statements.Eval(program);
        } else {
            foreach(var stmnt in elseifs) {
                if((bool)stmnt.check.Eval(program)) {
                    stmnt.statements.Eval(program);
                    return;
                }
            }

            if(elseStatements != null) {
                elseStatements.Eval(program);
            }
        }
    }
}

public class ASTWhile : ASTStatement
{
    ASTExpression check;
    ASTStatements statements;

    public ASTWhile(ASTExpression check, ASTStatements statements)
    {
        this.check = check;
        this.statements = statements;
    }

    public override void Log(StringBuilder logger, string space)
    {
        base.Log(logger, space);
        check.Log(logger, TAB + space);
        statements.Log(logger, TAB + space);
    }

    public override void Eval(Program program)
    {
        while((bool)check.Eval(program)) {
            statements.Eval(program);
        }
    }
}

public class ASTInlineCall : ASTStatement
{
    ASTFuncCall funcCall;

    public ASTInlineCall(ASTFuncCall funcCall)
    {
        this.funcCall = funcCall;
    }

    public override void Log(StringBuilder logger, string space)
    {
        base.Log(logger, space);

        if(funcCall != null) {
            funcCall.Log(logger, TAB + space);
        }
    }

    public override void Eval(Program program)
    {
        funcCall.Eval(program);
    }
}

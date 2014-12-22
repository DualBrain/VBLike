using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System;

// Uses tokens to turn the source into an Abstract Syntax Tree
// An Abstract Syntax Tree is a tree data structure representing a script
public class Parser
{
    Lexer lexer;
    ASTProgram program = new ASTProgram();

    public ASTProgram ProgramNode {get{return program;}}

    public Parser(string source)
    {
        lexer = new Lexer(source);
        ParseProgram();

        StringBuilder logger = new StringBuilder("Parser\n");

        program.Log(logger, "");

        Debug.Log(logger.ToString());
    }

    void ParseProgram()
    {
        while(lexer.IsReading) {
            if(lexer.PeekNext() == TokenType.Def) { // Function definition
                lexer.NextToken(); // Eat the def keyword

                Token funcNameToken = MatchNext(TokenType.Identifier); // Function name

                MatchNext(TokenType.BracketOpen);

                ASTFunction function = new ASTFunction(funcNameToken.Source);
                program.AddFunction(function);

                // Parameters
                while(true) {
                    if(lexer.PeekNext() == TokenType.BracketClose) {
                        lexer.NextToken();
                        break;
                    }

                    Token argToken = MatchNext(TokenType.Identifier);
                    function.AddArg(argToken.Source);

                    if(lexer.PeekNext() == TokenType.BracketClose) {
                        lexer.NextToken();
                        break;
                    }

                    MatchNext(TokenType.Comma);
                }

                MatchNext(TokenType.Do);

                // Statements
                while(lexer.PeekNext() != TokenType.End) {
                    function.AddStatement(ParseStatement());
                }

                lexer.NextToken();
            } else {
                program.Statements.AddStatement(ParseStatement());
            }
        }
    }

    ASTStatement ParseStatement()
    {
        if(lexer.PeekNext() == TokenType.Identifier) { // Function call
            ASTFuncCall funcCall = ParseExpression() as ASTFuncCall;
            return new ASTInlineCall(funcCall);
        }

        Token first = lexer.NextToken();

        switch(first.Type) {
            case TokenType.Set:
                {
                    Token varNameToken = MatchNext(TokenType.Identifier);
                    List<ASTExpression> indexers = new List<ASTExpression>();

                    while(lexer.PeekNext() == TokenType.SquareBraceOpen) {
                        MatchNext(TokenType.SquareBraceOpen);

                        indexers.Add(ParseExpression());

                        MatchNext(TokenType.SquareBraceClose);
                    }

                    MatchNext(TokenType.To);

                    ASTExpression expression = ParseExpression();

                    return new ASTSet(varNameToken.Source, expression, indexers);
                }
            case TokenType.If:
                {
                    ASTExpression check = ParseExpression();
                    ASTStatements statements = new ASTStatements();
                    ASTIf ifStmnt = new ASTIf(check, statements);

                    MatchNext(TokenType.Do);

                    while(lexer.PeekNext() != TokenType.End) {
                        if(lexer.PeekNext() == TokenType.Else) {
                            lexer.NextToken();

                            statements = new ASTStatements();
                            ifStmnt.SetElse(statements);

                            MatchNext(TokenType.Do);
                        } else if(lexer.PeekNext() == TokenType.Elif) {
                            lexer.NextToken();

                            statements = new ASTStatements();
                            ASTIf elseifStmnt = new ASTIf(ParseExpression(), statements);

                            ifStmnt.SetElseIf(elseifStmnt);

                            MatchNext(TokenType.Do);
                        }
                        statements.AddStatement(ParseStatement());
                    }

                    lexer.NextToken();

                    return ifStmnt;
                }
            case TokenType.While:
                {
                    ASTExpression check = ParseExpression();
                    ASTStatements statements = new ASTStatements();
                    ASTWhile whileStmnt = new ASTWhile(check, statements);

                    MatchNext(TokenType.Do);

                    while(lexer.PeekNext() != TokenType.End) {
                        statements.AddStatement(ParseStatement());
                    }

                    lexer.NextToken();

                    return whileStmnt;
                }
            case TokenType.Return:
                {
                    return new ASTReturn(ParseExpression());
                }
            default:
                throw new Exception("Unexpected token " + first);
        }

        return null;
    }

    ASTExpression ParseExpression()
    {
        ASTExpression firstExpr = ParseExpressionImpl();

        if(lexer.IsReading) {
            if(lexer.PeekNext() == TokenType.Operator) { // Operators
                Token opToken = lexer.NextToken();

                ASTExpression rightSide = ParseExpression();

                ASTOperator opNode = new ASTOperator(opToken.Source, firstExpr, rightSide);

                return opNode;
            } else if(lexer.PeekNext() == TokenType.SquareBraceOpen) { // Array access
                ASTIndexer upper = null;

                while(lexer.PeekNext() == TokenType.SquareBraceOpen) {
                    MatchNext(TokenType.SquareBraceOpen);

                    ASTExpression index = ParseExpression();

                    upper = new ASTIndexer(firstExpr, index);
                    firstExpr = upper;

                    MatchNext(TokenType.SquareBraceClose);
                }

                return upper;
            }
        }
        return firstExpr;
    }

    ASTExpression ParseExpressionImpl()
    {
        Token first = lexer.NextToken();

        switch(first.Type) {
            case TokenType.True:
                return new ASTLiteral(true);
            case TokenType.False:
                return new ASTLiteral(false);
            case TokenType.Number:
                if(first.Source.Contains(".")) {
                    return new ASTLiteral(float.Parse(first.Source));
                }
                try {
                    int i = int.Parse(first.Source);
                } catch(System.Exception e) {
                    Debug.LogError("Integer Invalid: " + first.Source + " len: " + first.Source.Length);
                }
                return new ASTLiteral(int.Parse(first.Source));
            case TokenType.String:
                return new ASTLiteral(first.Source);
            case TokenType.Identifier:
                {
                    if(lexer.PeekNext() == TokenType.BracketOpen) {
                        ASTFuncCall funcCall = new ASTFuncCall(first.Source);

                        lexer.NextToken(); // Eat open bracket

                        while(true) {
                            if(lexer.PeekNext() == TokenType.BracketClose) {
                                lexer.NextToken();
                                break;
                            }

                            ASTExpression arg = ParseExpression();
                            funcCall.AddArg(arg);

                            if(lexer.PeekNext() == TokenType.BracketClose) {
                                lexer.NextToken();
                                break;
                            }

                            MatchNext(TokenType.Comma);
                        }

                        return funcCall;
                    }
                    return new ASTVariable(first.Source);
                }
            default:
                throw new Exception("Unexpected token " + first);
        }
    }

    Token MatchNext(TokenType type)
    {
        Token token = lexer.NextToken();

        if(token.Type != type) {
            throw new Exception("Expected token " + type + " but got " + token);
        }

        return token;
    }
}

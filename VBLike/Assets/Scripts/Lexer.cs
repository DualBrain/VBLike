using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public enum TokenType
{
    None,

    Identifier,
    Number,
    String,

    True,
    False,

    BracketOpen,
    BracketClose,

    SquareBraceOpen,
    SquareBraceClose,

    Comma,

    Set,
    To,

    If,
    Else,
    Elif,
    While,
    Do,
    End,
    Return,

    Operator,

    Def,
};

public class Token
{
    public TokenType Type {get; private set;}
    public string Source {get; private set;}
    public int LineNumber {get; private set;}
    public int ColumnNumber {get; private set;}

    public override string ToString()
    {
        return string.Format("({0}: '{1}') [{2}, {3}]", Type, Source, LineNumber, ColumnNumber);
    }

    public Token(TokenType type, string source, int line, int column)
    {
        Type = type;
        Source = source;
        LineNumber = line;
        ColumnNumber = column;
    }
}

// Breaks up the source into more managable tokens
// A token is a single code element such as a open-parenthesis or a string
public class Lexer
{
    string source;
    int index;

    int line;
    int column;

    const string LETTERS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    const string WORD_BREAK = " \r\n\t()[]+-*/<>&|,";
    const string NUMBERS = "0123456789";
    static readonly List<string> OPERATORS = new List<string>()
    {
        "+", "-", "*", "/",
        "<", ">", "<=", ">=",
        "=", "!="
    };

    public bool IsReading {get{return index < source.Length;}}

    char CurChar
    {
        get
        {
            if(index >= source.Length) {
                throw new System.Exception("Index " + index + " out of range at [" + line + ", " + column + "]");
            }
            return source[index];
        }
    }

    public Lexer(string source)
    {
        Debug.Log("Source\n" + source);
        this.source = StripComments(source);
        Debug.Log("No Comments\n" + this.source);
    }

    string StripComments(string source)
    {
        string newSource = "";

        for(int i = 0; i < source.Length; i++) {
            if(source[i] == '\'') {
                for(; i < source.Length && source[i] != '\n'; i++) {
                }
            } else {
                newSource += source[i];
            }
        }

        return newSource;
    }

    public Token NextToken()
    {
        EatWhitespace();

        TokenType type = NextTokenType();
        string source = ParseByToken(type);
        Token token = new Token(type, source, line, column);

        EatWhitespace();

        return token;
    }

    string ParseByToken(TokenType type)
    {
        switch(type) {
            case TokenType.String:
                {
                    string str = "";

                    Consume(); // Eat open quote

                    while(IsReading && CurChar != '"') {
                        str += CurChar;
                        Consume();
                    }
                    
                    Consume();

                    return str;
                }
            case TokenType.Number:
            case TokenType.Identifier:
                {
                    string id = "";

                    while(IsReading && !WORD_BREAK.Contains(CurChar.ToString())) {
                        id += CurChar;
                        Consume();
                    }

                    return id;
                }
            case TokenType.Comma:
            case TokenType.SquareBraceOpen:
            case TokenType.SquareBraceClose:
            case TokenType.BracketOpen:
            case TokenType.BracketClose:
                {
                    string op = CurChar.ToString();
                    Consume();
                    return op;
                }
            case TokenType.Operator:
                {
                    foreach(var op in OPERATORS) {
                        if(MatchWord(op)) {
                            Consume(op.Length);
                            return op;
                        }
                    }
                    return "NO OP";
                }
            default:
                Consume(type.ToString().Length);
                return type.ToString();
        }
    }

    public TokenType PeekNext()
    {
        EatWhitespace();
        return NextTokenType();
    }

    TokenType NextTokenType()
    {
        if(MatchWord("set")) {
            return TokenType.Set;
        }

        if(MatchWord("to")) {
            return TokenType.To;
        }

        if(MatchWord("if")) {
            return TokenType.If;
        }

        if(MatchWord("end")) {
            return TokenType.End;
        }

        if(MatchWord("do")) {
            return TokenType.Do;
        }

        if(MatchWord("true")) {
            return TokenType.True;
        }

        if(MatchWord("false")) {
            return TokenType.False;
        }

        if(MatchWord("while")) {
            return TokenType.While;
        }

        if(MatchWord("else")) {
            return TokenType.Else;
        }

        if(MatchWord("elif")) {
            return TokenType.Elif;
        }

        if(MatchWord("def")) {
            return TokenType.Def;
        }

        if(MatchWord("return")) {
            return TokenType.Return;
        }

        if(LETTERS.Contains(CurChar.ToString())) {
            return TokenType.Identifier;
        }

        if(NUMBERS.Contains(CurChar.ToString())) {
            return TokenType.Number;
        }

        switch(CurChar) {
            case ',':
                return TokenType.Comma;
            case '(':
                return TokenType.BracketOpen;
            case ')':
                 return TokenType.BracketClose;
            case '[':
                return TokenType.SquareBraceOpen;
            case ']':
                return TokenType.SquareBraceClose;
            case '"':
                return TokenType.String;
        }

        foreach(var op in OPERATORS) {
            if(MatchWord(op)) {
                return TokenType.Operator;
            }
        }

        return TokenType.None;
    }

    bool MatchWord(string word)
    {
        if(IsPeekString(word)) {
            return true;
        }
        return false;
    }

    bool IsPeekString(string word)
    {
        if(index + word.Length > source.Length) {
            return false;
        }

        for(int i = 0; i < word.Length; i++) {
            if(word[i] != source[i + index]) {
                return false;
            }
        }

        if(index + word.Length < source.Length) {
            if(LETTERS.Contains(source[index + word.Length].ToString())) {
                return false;
            }
        }

        return true;
    }

    void EatWhitespace()
    {
        while(IsReading && (CurChar == ' ' || CurChar == '\t' || CurChar == '\r' || CurChar == '\n')) {
            Consume();
        }
    }

    void Consume()
    {
        column++;

        if(CurChar == '\n') {
            line++;
            column = 0;
        }
        
        index++;
    }

    void Consume(int length)
    {
        for(int i = 0; i < length; i++) {
            Consume();
        }
    }
}

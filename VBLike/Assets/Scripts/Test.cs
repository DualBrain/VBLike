using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;

// Used to test stuff
public class Test : MonoBehaviour
{
    [SerializeField] TextAsset file;

    void Awake()
    {
        Lexer lexer = new Lexer(file.text);
        string log = "Tokens\n";
        
        while(lexer.IsReading) {
            Token token = lexer.NextToken();
            log += string.Format("{0}:{1}: [{2},{3}]", token.Type, token.Source, token.LineNumber, token.ColumnNumber) + "\n";
        }
        
        Debug.Log(log);
        
        //Parser parser = new Parser(file.text);
        //ASTProgram programRoot = parser.ProgramNode;
        //
        //Program program = new Program();
        //
        //programRoot.Eval(program);
    }
}

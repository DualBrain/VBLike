using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GUIIDE : MonoBehaviour
{
    [SerializeField] TextAsset file;
    [SerializeField] InputField sourceEditor;
    [SerializeField] Text console;
    [SerializeField] Scrollbar scroller;

    public static GUIIDE Ide {get; private set;}

    public void Run()
    {
        console.text = "";

        WriteLine("<b>Parsing...</b>");

        float startTime = Time.realtimeSinceStartup;

        Parser parser = new Parser(sourceEditor.text);
        ASTProgram programRoot = parser.ProgramNode;
        
        float delta = Time.realtimeSinceStartup - startTime;

        WriteLine("<b>Took " + (delta / 1000f) + "s to parse</b>");

        Program program = new Program();
        
        WriteLine("<b>Running...</b>");

        programRoot.Eval(program);
    }

    public void Clear()
    {
        console.text = "";
    }

    public void WriteLine(string text)
    {
        console.text += text + "\n";
    }

    void Awake()
    {
        Ide = this;
        sourceEditor.text = file.text.Replace("\r", "");
        StartCoroutine(ResetScroll());
    }

    IEnumerator ResetScroll()
    {
        yield return new WaitForSeconds(0.1f);
        scroller.value = 1f;
    }
}

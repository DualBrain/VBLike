using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

[System.Serializable]
public class SourceEditor : InputField
{
    [SerializeField] LayoutElement layoutElement;
    [SerializeField] Scrollbar scroller;
    [SerializeField] Text displayText;
    Regex colorTags = new Regex("<[^>]*>");
    Regex strings = new Regex("\"[^\"]*\"");
    Regex numbers = new Regex(@"[-+]?[0-9]*\.?[0-9]+");
    Regex keyWords = new Regex(@"def\s|elif\s|else\s|if\s|return\s|try\s|while\s|true\s|false\s|set\s|end\s|do\s|to\s|while\s");
    //[SerializeField] InputField inputField;
    bool call = true;

    public void OnClickedMe()
    {
        if(!isFocused) {
            OnFocus();
            PointerEventData p = new PointerEventData(EventSystem.current);

            int index = this.GetCharacterIndexFromPosition(p.position);

            m_DrawStart = index;
            m_DrawEnd = index;

            this.OnPointerDown(p);
        }

        StartCoroutine(BreakFocusSelectAll(new BaseEventData(EventSystem.current)));
    }

    IEnumerator BreakFocusSelectAll(BaseEventData data)
    {
        yield return new WaitForSeconds(0.1f);

        
    }
    
    public void Highlight(string text)
    {
        //InputField inf = this;
        //
        //inf.text = colorTags.Replace(inf.text, @"");
        //inf.text = keyWords.Replace(inf.text, @"<color=blue>$&</color>");
        ////inf.text = strings.Replace(inf.text, @"<color=red>$&</color>");
        //inf.MoveTextEnd(false);
        string dText = this.text;
        dText = dText.Replace("<", "<\r").Replace("</", "<\t/");
        dText = keyWords.Replace(dText, @"<color=blue>$&</color>");
        dText = numbers.Replace(dText, @"<color=red>$&</color>");
        displayText.text = strings.Replace(dText, @"<color=red>$&</color>");
    }

    private void RemoveTags(string text)
    {
        //InputField inf = this;
        //inf.text = colorTags.Replace(inf.text, @"");
        //inf.MoveTextEnd(false);

        int pos = m_CaretPosition;
        int selPos = m_CaretSelectPosition;
        
        DeactivateInputField();
        ActivateInputField();
        
        m_CaretPosition = Mathf.Min(pos, selPos, this.text.Length - 1);
        m_CaretSelectPosition = Mathf.Min(pos, selPos, this.text.Length - 1);
    }

    protected override void Start()
    {
        //inputField.onValueChange.AddListener(new UnityEngine.Events.UnityAction<string>(ResizeInput));
        //onEndEdit.AddListener(new UnityEngine.Events.UnityAction<string>(Highlight));
        onValueChange.AddListener(new UnityEngine.Events.UnityAction<string>(RemoveTags));

        Highlight("");
    }

    void ResizeInput(string text)
    {
        //Debug.Log("some kind of resizing horror");
        InputField inputField = this;
        var fullText = inputField.text;
        Vector2 extents = inputField.textComponent.rectTransform.rect.size;
        var settings = inputField.textComponent.GetGenerationSettings(extents);
        settings.generateOutOfBounds = false;
        var prefheight = new TextGenerator().GetPreferredHeight(fullText, settings) + 10;

        //if(prefheight > inputField.textComponent.rectTransform.rect.height - 10)
        //{
            RectTransform parent = GetComponent<RectTransform>().parent as RectTransform;
            GetComponent<RectTransform>().sizeDelta = new Vector2(GetComponent<RectTransform>().sizeDelta.x, Mathf.Max(prefheight, parent.sizeDelta.y));
            displayText.rectTransform.sizeDelta = textComponent.rectTransform.sizeDelta;
        //}
    }

    void Update()
    {
        if(Application.isPlaying) {
            ResizeInput("");
            Highlight("");
            UpdateLabel();
            //GetComponent<RectTransform>().sizeDelta = inputField.textComponent.rectTransform.sizeDelta;
            //GetComponent<RectTransform>().anchoredPosition = inputField.textComponent.rectTransform.anchoredPosition;
        }

        float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
        scroller.value += scrollDelta * Time.deltaTime * 50f * scroller.size;

        //OnFocus();
        //
        //const int MAX_LINES = 10;
        //int[] offsets = GetLineOffsets();
        //int offset = Mathf.Min((int)(scroller.value * (float)offsets.Length), Mathf.Max(0, offsets.Length - MAX_LINES));
        //int displayLines = Mathf.Min(offsets.Length, MAX_LINES);
        //
        ////Debug.Log("start: " + offset + "\n end: " + (offset + displayLines) + "\n lineCount: " + offsets.Length);
        //
        //int start = offset;
        //int end = Mathf.Min(offset + displayLines, offsets.Length - 1);
        //
        //m_DrawStart = offsets[start];
        //m_DrawEnd = offsets[end];

        ////textComponent.text = text.Substring(m_DrawStart, m_DrawEnd - m_DrawStart);
        //Rebuild(CanvasUpdate.Layout);
        //Rebuild(CanvasUpdate.LatePreRender);
    }

    //int[] GetLineOffsets()
    //{
    //    List<int> offsets = new List<int>();
    //    string text = textComponent.text;
    //    int length = text.Length;
    //
    //    offsets.Add(0);
    //    for(int i = 1; i < length; i++) {
    //        if(text[i] == '\n') {
    //            offsets.Add(i + 1);
    //        }
    //    }
    //
    //    return offsets.ToArray();
    //}
}

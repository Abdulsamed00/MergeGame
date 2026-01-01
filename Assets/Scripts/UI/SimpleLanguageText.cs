using UnityEngine;
using UnityEngine.UI;

public class SimpleLanguageText : MonoBehaviour
{
    [TextArea] public string turkish;
    [TextArea] public string english;

    Text text;

    void Awake()
    {
        text = GetComponent<Text>();
    }

    void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (text == null) return;

        if (LanguageData.CurrentLanguage == Language.Turkish)
            text.text = turkish;
        else
            text.text = english;
    }
}
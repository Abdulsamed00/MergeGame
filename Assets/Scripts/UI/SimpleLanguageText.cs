using UnityEngine;
using UnityEngine.UI;

public class SimpleLanguageText : MonoBehaviour
{
    [TextArea] public string turkish;
    [TextArea] public string english;

    Text text;

    void OnEnable()
    {
        text = GetComponent<Text>();
        Refresh();
    }

    public void Refresh()
    {
        if (LanguageData.CurrentLanguage == Language.Turkish)
            text.text = turkish;
        else
            text.text = english;
    }
}
using UnityEngine;
using UnityEngine.UI;

public class SimpleLanguageText : MonoBehaviour
{
    [TextArea] public string turkish;
    [TextArea] public string english;

    private Text text;

    void Awake()
    {
        text = GetComponent<Text>();

        if (text == null)
        {
            Debug.LogError(gameObject.name + " üzerinde Text component yok!");
        }
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
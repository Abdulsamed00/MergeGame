using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSound : MonoBehaviour
{
    private Button myButton;

    void Start()
    {
        myButton = GetComponent<Button>();
        
        myButton.onClick.AddListener(() => 
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayClick();
            }
        });
    }
}
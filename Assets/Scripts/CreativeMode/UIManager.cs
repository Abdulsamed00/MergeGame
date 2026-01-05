using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public List<ObjeVerisi> tumObjeler;
    public ObjectButton buttonPrefab;
    public Transform contentParent;

    void Start()
    {
        foreach (var obje in tumObjeler)
        {
            var btn = Instantiate(buttonPrefab, contentParent);
            btn.Setup(obje);
        }
    }
}
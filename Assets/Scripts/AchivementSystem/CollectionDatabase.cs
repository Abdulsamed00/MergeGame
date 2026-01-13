using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CollectionDB", menuName = "Oyun/Collection Database")]
public class CollectionDatabase : ScriptableObject
{
    public List<ObjeVerisi> tumObjelerSorted;
}

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_Screen", menuName = "Scriptable Objects/SO_Screen")]
public class SO_Screen : ScriptableObject
{
    public GameObject prefab;
    public List<GameObject> prefabsScreen = new List<GameObject>();
}

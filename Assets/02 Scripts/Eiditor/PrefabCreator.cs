using UnityEngine;
using static Won.Constants;

public class PrefabCreator : MonoBehaviour
{
    public GameObject Prefab;
    public int Count = BoardSize * BoardSize;

    [ContextMenu("Create prefabList")]
    public void GenerateBoard()
    {
        string name = Prefab.name;

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < Count; i++)
        {
            GameObject go = Instantiate(Prefab, transform);
            go.name = $"{name} {i}"; 
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractMapGenerator : MonoBehaviour
{
    [SerializeField]
    protected GameObject roomGameObject, mapHolder;
    [SerializeField]
    [Range(1000000, 9999999)]
    protected int seed = 5034143;//size emptyRoom 

    public void GenerateDungeon()
    {
        CreateNewMap();
    }

    public void GenerateRandDungeon()
    {
        seed = Random.Range(1000000, 9999999);
        CreateNewMap();
    }

    protected abstract void CreateNewMap();
}

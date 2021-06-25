using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractMapGenerator : MonoBehaviour
{
    [SerializeField]
    protected GameObject roomGameObject, mapHolder;

    public static int seed = GameManager.gameSeed;//size emptyRoom 

    public void GenerateDungeon()
    {
        CreateNewMap();
    }

    public void GenerateRandDungeon()
    {
        seed = GameManager.gameSeed;
        CreateNewMap();
    }

    protected abstract void CreateNewMap();
}

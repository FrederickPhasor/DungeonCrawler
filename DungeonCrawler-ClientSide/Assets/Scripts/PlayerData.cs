using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  class PlayerData : MonoBehaviour
{
    public static PlayerData pData;
    public string playerName;

     void Awake()
    {
        if (pData != null)
            GameObject.Destroy(pData);
        else
            pData = this;
        DontDestroyOnLoad(this);
    }
}

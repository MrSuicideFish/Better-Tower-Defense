using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionManager : MonoBehaviour
{
    private static ConstructionManager _instance;
    private static ConstructionManager GetInstance()
    {
        if (_instance == null)
        {
            GameObject instObj = new GameObject("_CONSTRUCTION_MANAGER");
            _instance = instObj.AddComponent<ConstructionManager>();
        }
        return _instance;
    }

    public static void BeginConstructionMode()
    {
        GameGridManager.Instance.gameObject.SetActive(true);
    }

    public static void EndConstructionMode()
    {
        GameGridManager.Instance.gameObject.SetActive(false);
    }
}

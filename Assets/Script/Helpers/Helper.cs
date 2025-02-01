using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helper
{
    public static Elem GetRandElemInList<Elem>(List<Elem> list)
    {
        int len = list.Count;
        int randVal = UnityEngine.Random.Range(0, len);

        return list[randVal];
    }

    public static void ChangeRendererState(GameObject gm, bool enable)
    {
        foreach (var item in gm.GetComponentsInChildren<MeshRenderer>())
        {
            item.enabled = enable;
        }
    }
}

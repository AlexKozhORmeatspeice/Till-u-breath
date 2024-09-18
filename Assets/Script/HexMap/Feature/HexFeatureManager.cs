using System;
using System.Collections;
using System.Collections.Generic;
using Script;
using UnityEngine;


public class HexFeatureManager : MonoBehaviour
{
    private static HexFeatureCollection featureCollection;
    private Transform container;

    public void Clear()
    {
        if (container)
        {
            Destroy(container.gameObject);
        }

        container = new GameObject("Features Container").transform;
        container.SetParent(transform, false);
    }

    public void Apply()
    {

    }

    public void AddFeature(HexCell cell, Vector3 pos)
    {
        HexHash hash = HexMetrics.SampleHashGrid(pos);
        Transform prefab = PickPrefab(cell.FeatureLevel, hash.a, hash.d);
        if (!prefab)
            return;

        Transform instance = Instantiate(prefab);
        pos.y += instance.localScale.y * 0.5f;
        instance.localPosition = HexMetrics.Perturb(pos);

        instance.localRotation = Quaternion.Euler(0f, 360f * hash.c, 0f);

        instance.SetParent(container, false);
    }

    Transform PickPrefab(int level, float hash, float choice)
    {
        if (level > 0)
        {
            float[] thresholds = HexMetrics.GetFeatureThreshold(level - 1);
            for (int i = 0; i < thresholds.Length; i++)
            {
                if (hash < thresholds[i])
                {
                    return featureCollection.Pick(choice);
                }
            }
        }

        return null;
    }

    public static void SetFeature(HexFeatureCollection collection)
    {
        featureCollection = collection;
    }

}

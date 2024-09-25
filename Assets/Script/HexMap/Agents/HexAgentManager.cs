using UnityEngine;

public class HexAgentManager : MonoBehaviour
{
    public void Apply()
    {
        
    }

    public void Clear()
    {
        
    }
    
    public void AddFeature(HexCell cell, Vector3 pos)
    {
        /*Transform prefab = PickPrefab(cell.FeatureLevel, hash.a, hash.d);
        if (!prefab)
            return;
        
        Transform instance = Instantiate(prefab);
        pos.y += instance.localScale.y * 0.5f;
        instance.localPosition = HexMetrics.Perturb(pos);

        instance.localRotation = Quaternion.Euler(0f, 360f * hash.c, 0f);

        instance.SetParent(container, false);*/
    }
}

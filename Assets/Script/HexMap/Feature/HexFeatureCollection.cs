using UnityEngine;


[System.Serializable] 
public struct HexFeatureCollection 
{
    public string name;

    public Transform[] prefabs;

    public Transform Pick(float choice)
    {
        return prefabs[(int)(choice * prefabs.Length)];
    }

    public int Length { 
        get
        {
            return prefabs.Length; 
        } 
    }
}

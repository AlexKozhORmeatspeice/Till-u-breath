using Script.Agents.AgentsList.Supplies;
using UnityEngine;

[System.Serializable] 
public struct HexFeatureCollection
{
    public bool isUnique;
    public Places name;

    public Transform[] prefabs;

    public Transform Pick(float choice)
    {
        if (isUnique)
            return prefabs[0];
        
        return prefabs[(int)(choice * prefabs.Length)];
    }

    public static bool operator ==(HexFeatureCollection collection1, HexFeatureCollection collection2)
    {
        return collection1.name == collection2.name;
    }
    
    public static bool operator !=(HexFeatureCollection collection1, HexFeatureCollection collection2)
    {
        return collection1.name != collection2.name;
    }

    public int Length { 
        get
        {
            if (isUnique)
                return prefabs.Length < 1 ? 0 : 1;
            
            return prefabs.Length; 
        } 
    }
}

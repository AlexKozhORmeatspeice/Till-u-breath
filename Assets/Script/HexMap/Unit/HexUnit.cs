using UnityEngine;

[System.Serializable]
public struct HexUnit
{
    public UnitName name;
    public bool isUnique;
    public GameObject prefab;
}

public enum UnitName
{
    Boar,
    Worker,
    Doctor,
    Hero
}

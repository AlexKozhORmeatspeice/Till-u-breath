using Script.Agents.AgentsList.Supplies;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CellTypesBitmask
{
    private Int64 bitmask;

    public CellTypesBitmask(List<HexCellType> _cellTypes)
    {
        bitmask = 0;
        foreach(var name in _cellTypes)
        {
            bitmask |= (Int64)(1 << (int)name);
        }
    }

    public CellTypesBitmask(Int64 _bitmask)
    {
        bitmask = _bitmask;
    }

    public bool HasCellType(HexCellType name)
    {
        Int64 checkBitmask = (Int64)(1 << (int)name);

        return (bitmask & checkBitmask) != 0;
    }

    public void AddCellType(HexCellType name)
    {
        Int64 nameBitmask = (Int64)(1 << (int)name);
        bitmask |= nameBitmask;
    }

    public CellTypesBitmask Inverse()
    {
        Int64 invBitmask = ~bitmask;

        return new CellTypesBitmask(invBitmask);
    }
}

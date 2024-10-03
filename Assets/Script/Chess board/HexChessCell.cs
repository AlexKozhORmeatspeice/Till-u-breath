using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Script;
using UnityEngine;

public class HexChessCell : MonoBehaviour
{
    public int Elevation
    {
        get { return elevation; }
        set
        {
            if (elevation == value)
                return;

            elevation = value;

            Vector3 pos = transform.localPosition;
            pos.y = value * (HexMetrics.elevationStep);
            pos.y += (HexMetrics.SampleNoise(pos).y * 2f - 1f) * HexMetrics.elevationPerturbStrength;
            transform.localPosition = pos;

            Refresh();
        }
    }

    public Vector3 Position
    {
        get { return transform.localPosition; }
    }

    public Color Color
    {
        get { return HexMetrics.colors[terrainTypeInd]; }
    }

    public int TerrainTypeInd
    {
        get
        {
            return terrainTypeInd;
        }
        set
        {
            if (terrainTypeInd != value)
            {
                terrainTypeInd = value;
                Refresh();
            }
        }
    }
    
    public HexChessCell GetNeighbor(HexDirection dir)
    {
        return neighbors[(int)dir];
    }

    public void SetNeighbor(HexDirection direction, HexChessCell cell)
    {
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
    }

    public HexEdgeType GetEdgeType(HexDirection dir)
    {
        return HexMetrics.GetEdgeType(elevation, neighbors[(int)dir].elevation);
    }
    public HexEdgeType GetEdgeType(HexChessCell otherCell)
    {
        return HexMetrics.GetEdgeType(elevation, otherCell.elevation);
    }

    private void Refresh()
    {
        if (chunk)
        {
            chunk.Refresh();

            for (int i = 0; i < neighbors.Length; i++)
            {
                HexChessCell neighbor = neighbors[i];
                if (neighbor != null && neighbor.chunk != chunk)
                {
                    neighbor.chunk.Refresh();
                }
            }
        }       
    }

    private void RefreshSelfOnly()
    {
        if (chunk)
        {
            chunk.Refresh();
        }
    }
    public int GetElevationDifference(HexDirection dir)
    {
        int d = Mathf.Abs(elevation - GetNeighbor(dir).elevation);
        return d;
    }

    public void Save(BinaryWriter writer)
    {
        writer.Write((byte)terrainTypeInd);
        writer.Write((byte)elevation);
    }
    public void Load(BinaryReader reader)
    {
        terrainTypeInd = reader.ReadByte();
        Elevation = reader.ReadByte();
    }
    
    [SerializeField] private HexChessCell[] neighbors;
    private int elevation = int.MinValue;
    public HexGridChessChunk chunk;

    public HexCoordinates coordinates;
    
    private int terrainTypeInd;
}

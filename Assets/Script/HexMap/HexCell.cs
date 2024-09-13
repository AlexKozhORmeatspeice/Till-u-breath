using System;
using System.Collections;
using System.Collections.Generic;
using Script;
using UnityEngine;

public class HexCell : MonoBehaviour
{
    [SerializeField] private HexCell[] neighbors;
    private int elevation = int.MinValue;
    
    public RectTransform uiRect;
    public HexGridChunk chunk;

    public HexCoordinates coordinates;

    private Color color;

    private bool hasIncomingRiver, hasOutgoingRiver;
    private HexDirection incomingRiver, outgoingRiver;

    public bool HasIncomingRiver => hasIncomingRiver;
    public bool HasOutgoingRiver => hasOutgoingRiver;
    public HexDirection IncomingRiver => incomingRiver;
    public HexDirection OutgoingRiver => outgoingRiver;

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

            Vector3 uiPos = uiRect.localPosition;
            uiPos.z = -pos.y;
            uiRect.localPosition = uiPos;
            
            if(hasOutgoingRiver && elevation < GetNeighbor(outgoingRiver).Elevation)
                RemoveOutgoingRiver();
            
            if(hasIncomingRiver && elevation > GetNeighbor(incomingRiver).Elevation)
                RemoveIncomingRiver();
            
            Refresh();
        }
    }

    public Vector3 Position
    {
        get { return transform.localPosition; }
    }

    public Color Color
    {
        get { return color; }
        set
        {
            if (color == value)
                return;
            color = value;
            Refresh();
        }
    }

    public float StreamBedY
    {
        get
        {
            return (elevation + HexMetrics.streamBedElevationOffset) * HexMetrics.elevationStep;
        }
    }
    
    public float RiverSurfaceY
    {
        get
        {
            return (elevation + HexMetrics.riverSurfaceElevationOffset) * HexMetrics.elevationStep;
        }
    }

    public bool HasRiver
    {
        get { return (hasIncomingRiver || hasOutgoingRiver); }
    }

    public bool HasRiverBeginOrEnd
    {
        get { return (hasIncomingRiver != hasOutgoingRiver); }
    }

    public bool HasRiverThroughEdge(HexDirection dir)
    {
        return
            ((hasIncomingRiver && incomingRiver == dir) ||
            (hasOutgoingRiver && outgoingRiver == dir));
    }

    public HexCell GetNeighbor(HexDirection dir)
    {
        return neighbors[(int)dir];
    }

    public void SetNeighbor(HexDirection direction, HexCell cell)
    {
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
    }

    public HexEdgeType GetEdgeType(HexDirection dir)
    {
        return HexMetrics.GetEdgeType(elevation, neighbors[(int)dir].elevation);
    }
    public HexEdgeType GetEdgeType(HexCell otherCell)
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
                HexCell neighbor = neighbors[i];
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

    public void SetOutgoingRiver(HexDirection dir)
    {
        if (hasOutgoingRiver && outgoingRiver == dir)
            return;
        
        HexCell neighbor = GetNeighbor(dir);
        if(!neighbor || elevation < neighbor.elevation)
            return;
        
        RemoveOutgoingRiver();
        if (hasIncomingRiver && incomingRiver == dir)
        {
            RemoveIncomingRiver();
        }

        hasOutgoingRiver = true;
        outgoingRiver = dir;
        RefreshSelfOnly();

        neighbor.RemoveIncomingRiver();
        neighbor.hasIncomingRiver = true;
        neighbor.incomingRiver = dir.Opposite();
        neighbor.RefreshSelfOnly();
    }

    public void RemoveOutgoingRiver()
    {
        if (!hasOutgoingRiver)
            return;
        
        hasOutgoingRiver = false;
        RefreshSelfOnly();

        HexCell neighbor = GetNeighbor(outgoingRiver);
        neighbor.hasIncomingRiver = false;
        neighbor.RefreshSelfOnly();
    }
    public void RemoveIncomingRiver()
    {
        if (!hasIncomingRiver)
            return;
        
        hasIncomingRiver = false;
        RefreshSelfOnly();

        HexCell neighbor = GetNeighbor(outgoingRiver);
        neighbor.hasOutgoingRiver = false;
        neighbor.RefreshSelfOnly();
    }

    public void RemoveRiver()
    {
        RemoveOutgoingRiver();
        RemoveIncomingRiver();
    }
}

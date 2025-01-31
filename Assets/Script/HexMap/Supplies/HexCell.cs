using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Script;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class HexCell : MonoBehaviour
{
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

            for (int i = 0; i < roads.Length; i++)
            {
                if (roads[i] && GetElevationDifference((HexDirection)i) > 1)
                {
                    SetRoad(i, false);
                }
            }
            
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
            return (elevation + HexMetrics.waterElevationOffset) * HexMetrics.elevationStep;
        }
    }
    
    public float WaterSurfaceY
    {
        get
        {
            return (waterLevel + HexMetrics.waterElevationOffset) * HexMetrics.elevationStep;
        }
    }

    public int FeatureLevel
    {
        get { return featureLevel; }
        set
        {
            if (FeatureLevel != value)
            {
                featureLevel = value;
                RefreshSelfOnly();
            }
        }
    }
    public int FeatureCollectionInd
    {
        get { return featureCollectionInd; }
        set
        {
            if (featureCollectionInd != value)
            {
                featureCollectionInd = value;
                RefreshSelfOnly();
            }
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
    public bool HasRoad
    {
        get
        {
            for (int i = 0; i < roads.Length; i++)
            {
                if (HasRoadThroughEdge((HexDirection)i))
                {
                    return true;
                }
            }

            return false;
        }
        
    }

    public HexDirection RiverBeginOrEndDirection
    {
        get
        {
            return hasIncomingRiver ? incomingRiver : outgoingRiver;
        }
    }

    public int WaterLevel
    {
        get
        {
            return waterLevel;
        }
        set
        {
            if (waterLevel == value)
                return;
            
            waterLevel = value;
            Refresh();
        }
    }

    public bool IsUnderwater
    {
        get
        {
            return waterLevel > elevation;
        }
    }

    public IAgent Unit
    {
        get => unit;
        set => unit = value;
    }

    public Item Item
    {
        get => item;
        set => item = value;
    }

    public int Distance
    {
        get
        {
            return distance;
        }
        set
        {
            distance = value;
            UpdateDistanceLabel();
        }
    }
    
    public HexCellType CellType
    {
        get
        {
            return (HexCellType)terrainTypeInd;
        }
    }

    public bool HasRoadThroughEdge(HexDirection dir)
    {
        return roads[(int)dir];
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

            if (Unit != null)
            {
                Unit.ValidateLocation();
            }
        }       
    }

    private void RefreshSelfOnly()
    {
        if (chunk)
        {
            chunk.Refresh();
            if (Unit != null)
            {
                Unit.ValidateLocation();
            }
        }
    }

    public void SetOutgoingRiver(HexDirection dir)
    {
        if (hasOutgoingRiver && outgoingRiver == dir)
            return;
        
        HexCell neighbor = GetNeighbor(dir);
        if(!neighbor || Mathf.Abs(elevation - neighbor.elevation) > 1.2f)
            return;
        
        RemoveOutgoingRiver();
        if (hasIncomingRiver && incomingRiver == dir)
        {
            RemoveIncomingRiver();
        }

        hasOutgoingRiver = true;
        outgoingRiver = dir;
        

        neighbor.RemoveIncomingRiver();
        neighbor.hasIncomingRiver = true;
        neighbor.incomingRiver = dir.Opposite();
        
        SetRoad((int)dir, false);
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

    public void AddRoad(HexDirection dir)
    {
        int i = (int)dir;
        if(!roads[i] && !HasRiverThroughEdge(dir) && GetElevationDifference(dir) <= 1)
            SetRoad(i, true);
    }
    
    public void RemoveRoads()
    {
        for (int i = 0; i < neighbors.Length; i++)
        {
            if (roads[i])
            {
                SetRoad(i, false);
            }
        }
    }

    private void SetRoad(int i, bool state)
    {
        roads[i] = state;
        neighbors[i].roads[(int)((HexDirection)i).Opposite()] = state;
        neighbors[i].RefreshSelfOnly();
        RefreshSelfOnly();
    }

    public int GetElevationDifference(HexDirection dir)
    {
        int d = Mathf.Abs(elevation - GetNeighbor(dir).elevation);
        return d;
    }

    void UpdateDistanceLabel()
    {
        TMP_Text lable = uiRect.GetComponent<TMP_Text>();
        lable.text = distance == int.MaxValue ? "" : distance.ToString();
    }

    public void DisableOutline()
    {
        Image outline = uiRect.GetChild(0).GetComponent<Image>();
        outline.enabled = false;
    }

    public void EnableOutline(Color color, bool halfAlpha = false)
    {
        Image outline = uiRect.GetChild(0).GetComponent<Image>();
        if(!halfAlpha)
        {
            outline.color = color;
        }
        else
        {
            Color halfAlphaColor = new Color(color.r, color.g, color.b, 0.5f);
            outline.color = halfAlphaColor;
        }
        
        outline.enabled = true;
    }
    
    public HexCell PathFrom { get; set; }
    public int SearchHeuristic { get; set; }
    public int SearchPriority
    {
        get
        {
            return distance + SearchHeuristic;
        }
    }

    public HexCell NextWithSamePriority { get; set; }

    public void Save(BinaryWriter writer)
    {
        writer.Write((byte)terrainTypeInd);
        writer.Write((byte)elevation);
        writer.Write((byte)waterLevel);
        writer.Write((byte)featureLevel);
        writer.Write((byte)featureCollectionInd);
        writer.Write((byte)unitTypeInd);

        if (hasIncomingRiver)
        {
            writer.Write((byte)(incomingRiver + 128));
        }
        else
        {
            writer.Write((byte)0);
        }
        
        if (HasOutgoingRiver)
        {
            writer.Write((byte)(outgoingRiver + 128));
        }
        else
        {
            writer.Write((byte)0);
        }

        int roadFlags = 0;
        for (int i = 0; i < roads.Length; i++)
        {
            if (roads[i])
            {
                roadFlags |= 1 << i;
            }
        }
        writer.Write((byte)roadFlags);
    }
    
    public void Load(BinaryReader reader)
    {
        terrainTypeInd          = reader.ReadByte();
        Elevation               = reader.ReadByte();
        WaterLevel              = reader.ReadByte();
        FeatureLevel            = reader.ReadByte();
        FeatureCollectionInd    = reader.ReadByte();
        unitTypeInd             = reader.ReadByte();

        byte riverData = reader.ReadByte();
        if (riverData >= 128)
        {
            hasIncomingRiver = true;
            incomingRiver = (HexDirection)(riverData - 128);
        }
        else
        {
            hasIncomingRiver = false;
        }
        
        riverData = reader.ReadByte();
        if (riverData >= 128)
        {
            hasOutgoingRiver = true;
            outgoingRiver = (HexDirection)(riverData - 128);
        }
        else
        {
            hasOutgoingRiver = false;
        }

        int roadFlags = reader.ReadByte();
        for (int i = 0; i < roads.Length; i++)
        {
            roads[i] = (roadFlags & (1 << i)) != 0;
        }
    }
    
    [SerializeField] private HexCell[] neighbors;
    [SerializeField] private bool[] roads;
    private int elevation = int.MinValue;

    private int distance;
    
    private int waterLevel;
    private int featureLevel;

    private IAgent unit;
    private Item item;

    public RectTransform uiRect;
    public HexGridChunk chunk;

    public HexCoordinates coordinates;
    
    private int terrainTypeInd;
    private int featureCollectionInd;

    private bool hasIncomingRiver, hasOutgoingRiver;
    private HexDirection incomingRiver, outgoingRiver;

    private int unitTypeInd //some magic to save objects on map
    {
        get
        {
            int ind = -1;

            if (unit == null)
                return ind;

            return HexUnitManager.GetAgentIndex(unit);
        }
        set
        {
            if (value == 255) //when convert -1 to byte gets 255 as a result
            {
                if(unit != null)
                {
                    unit.Die();
                    unit = null;
                }
                return;
            }

            IAgent agent = HexUnitManager.CreateUnit(this, value);
            unit = agent;
        }
    }
}

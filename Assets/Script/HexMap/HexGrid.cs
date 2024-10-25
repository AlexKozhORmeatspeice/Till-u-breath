using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using Script;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour
{
    [SerializeField] private HexFeatureCollection[] featureCollections;
    [SerializeField] private HexCell cellPrefab;
    [SerializeField] private TMP_Text cellLabelPrefab;

    private int cellCountX, cellCountZ;
    public int chunkCountX = 4, chunkCountZ = 3;
    
    private HexCell[] cells;
    public HexCell[] Cells => cells;
    
    public Texture2D noiseSource;
    
    public HexGridChunk chunkPrefab;
    private HexGridChunk[] chunks;

    public int seed;

    void OnEnable()
    {
        if (!HexMetrics.noiseSource)
        {
            HexMetrics.noiseSource = noiseSource;
            HexMetrics.InitializeHashGrid(seed);
            HexMetrics.featureCollections = featureCollections;
        }
        
    }
    private void Awake()
    {
        HexMetrics.noiseSource = noiseSource;
        HexMetrics.InitializeHashGrid(seed);
        HexMetrics.featureCollections = featureCollections;
        
        cellCountX = chunkCountX * HexMetrics.chunkSizeX;
        cellCountZ = chunkCountZ * HexMetrics.chunkSizeZ;

        CreateChunks();
        CreateCells();
        HexPathfinding.Cells = cells;
        InputManager.SetGrid(this);
    }

    void CreateChunks()
    {
        chunks = new HexGridChunk[chunkCountX * chunkCountZ];

        int i = 0;
        for (int z = 0; z < chunkCountZ; z++)
        {
            for (int x = 0; x < chunkCountX; x++)
            {
                HexGridChunk chunk = chunks[i++] = Instantiate(chunkPrefab);
                chunk.transform.SetParent(transform);
            }
        }
    }
    
    
    private void CreateCells()
    {
        cells = new HexCell[cellCountX * cellCountZ];

        int i = 0;
        for (int z = 0; z < cellCountZ; z++)
        {
            for (int x = 0; x < cellCountX; x++)
            {
                CreateCell(x, z, i++);
            }
        }
    }

    public HexCell GetCell(Vector3 pos)
    {
        pos = transform.InverseTransformPoint(pos);
        HexCoordinates coordinates = HexCoordinates.FromPosition(pos);
        
        int ind = coordinates.X + coordinates.Z * cellCountX + coordinates.Z / 2;
        return cells[ind];
    }
    
    public HexCell GetCell(HexCoordinates coordinates)
    {
        int z = coordinates.Z;

        if (z < 0 || z >= cellCountZ)
        {
            return null;
        }
        
        int x = coordinates.X + coordinates.Z / 2;
        if (x < 0 || x >= cellCountX)
        {
            return null;
        }
        return cells[x + z * cellCountX];
    }

    private void CreateCell(int x, int z, int i)
    {
        Vector3 pos;
        pos.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
        pos.y = 0.0f;
        pos.z = z * (HexMetrics.outerRadius * 1.5f);
        
        HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
        
        cell.transform.localPosition = pos;
        cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);

        if (x > 0)
        {
            cell.SetNeighbor(HexDirection.W, cells[i-1]);
        }

        if (z > 0)
        {
            if ((z & 1) == 0)
            {
                cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX]);
                if (x > 0)
                {
                    cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX - 1]);
                }
            }
            else
            {
                cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX]);
                if (x < cellCountX - 1)
                {
                    cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX + 1]);
                }
            }
        }
        
        TMP_Text label = Instantiate<TMP_Text>(cellLabelPrefab);
        label.rectTransform.anchoredPosition = new Vector2(pos.x, pos.z);

        cell.uiRect = label.rectTransform;
        cell.DisableOutline();
        
        cell.Elevation = 0;
        
        AddCellToChunk(x, z, cell);
    }

    private void AddCellToChunk(int x, int z, HexCell cell)
    {
        int chunkX = x / HexMetrics.chunkSizeX;
        int chunkZ = z / HexMetrics.chunkSizeZ;
        HexGridChunk chunk = chunks[chunkX + chunkZ * chunkCountX];
        
        int localX = x - chunkX * HexMetrics.chunkSizeX;
        int localZ = z - chunkZ * HexMetrics.chunkSizeZ;
        
        chunk.AddCell(localX + localZ * HexMetrics.chunkSizeX, cell);
    }

    public void ShowUI(bool visible)
    {
        foreach (HexGridChunk chunk in chunks)
        {
            chunk.ShowUI(visible);
        }
    }
    
    public void Save(BinaryWriter writer)
    {
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].Save(writer);
        }
    }
    
    public void Load(BinaryReader reader)
    {
        StopAllCoroutines();
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].Load(reader);
        }
        for (int i = 0; i < chunks.Length; i++)
        {
            chunks[i].Refresh();
        }
    }
}
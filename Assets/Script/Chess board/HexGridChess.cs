using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using Script;
using TMPro;
using TMPro.Examples;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HexGridChess : MonoBehaviour
{
    [SerializeField] private HexChessCell cellPrefab;

    private int cellCountX, cellCountZ;
    public int chunkCountX = 4, chunkCountZ = 3;
    
    
    private HexChessCell[] cells;

    public Texture2D noiseSource;
    
    public HexGridChessChunk chunkPrefab;
    private HexGridChessChunk[] chunks;

    public int seed;

    void OnEnable()
    {
        if (!HexMetrics.noiseSource)
        {
            HexMetrics.noiseSource = noiseSource;
            HexMetrics.InitializeHashGrid(seed);
        }
        
    }
    private void Awake()
    {
        HexMetrics.noiseSource = noiseSource;
        HexMetrics.InitializeHashGrid(seed);
        
        cellCountX = chunkCountX * HexMetrics.chunkSizeX;
        cellCountZ = chunkCountZ * HexMetrics.chunkSizeZ;

        CreateChunks();
        CreateCells();
    }

    void CreateChunks()
    {
        chunks = new HexGridChessChunk[chunkCountX * chunkCountZ];

        int i = 0;
        for (int z = 0; z < chunkCountZ; z++)
        {
            for (int x = 0; x < chunkCountX; x++)
            {
                HexGridChessChunk chunk = chunks[i++] = Instantiate(chunkPrefab);
                chunk.transform.SetParent(transform);
            }
        }
    }
    
    
    private void CreateCells()
    {
        cells = new HexChessCell[cellCountX * cellCountZ];

        int i = 0;
        for (int z = 0; z < cellCountZ; z++)
        {
            for (int x = 0; x < cellCountX; x++)
            {
                CreateCell(x, z, i++);
            }
        }
    }

    public HexChessCell GetCell(Vector3 pos)
    {
        pos = transform.InverseTransformPoint(pos);
        HexCoordinates coordinates = HexCoordinates.FromPosition(pos);
        
        int ind = coordinates.X + coordinates.Z * cellCountX + coordinates.Z / 2;
        return cells[ind];
    }
    
    public HexChessCell GetCell(HexCoordinates coordinates)
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

        HexChessCell cell = cells[i] = Instantiate<HexChessCell>(cellPrefab);
        
        cell.transform.localPosition = pos;
        cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);

        if (x > 0)
        {
            UnityEngine.Debug.Log(i);
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
        
        cell.Elevation = 0;
        
        AddCellToChunk(x, z, cell);
    }

    private void AddCellToChunk(int x, int z, HexChessCell cell)
    {
        int chunkX = x / HexMetrics.chunkSizeX;
        int chunkZ = z / HexMetrics.chunkSizeZ;
        HexGridChessChunk chunk = chunks[chunkX + chunkZ * chunkCountX];
        
        int localX = x - chunkX * HexMetrics.chunkSizeX;
        int localZ = z - chunkZ * HexMetrics.chunkSizeZ;
        
        chunk.AddCell(localX + localZ * HexMetrics.chunkSizeX, cell);
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
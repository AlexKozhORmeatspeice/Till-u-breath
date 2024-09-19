using System;
using System.Collections;
using System.Collections.Generic;
using Script;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.IO;

public class HexMapEditor : MonoBehaviour
{
    private void Awake()
    {
        dataPath = Application.persistentDataPath;
        isApplyElevation = true;
        
    }

    private void Start()
    {
        dropdownFeature.onValueChanged.AddListener(FeaturesIsChanged);
        
        dropdownFeature.options.Clear();
        foreach (HexFeatureCollection feature in HexMetrics.featureCollections)
        {
            dropdownFeature.options.Add(new TMPro.TMP_Dropdown.OptionData() { text = feature.name });
        }
        FeaturesIsChanged(0);
        
        if (File.Exists(mapPath))
        {
            Load();
        }
    }

    private void Update()
    {
        if (Input.GetMouseButton(0) &&
            !EventSystem.current.IsPointerOverGameObject())
        {
            HandleInput();
        }
        else
        {
            prevCell = null;
        }
    }

    void HandleInput()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit))
        {
            HexCell currentCell = grid.GetCell(hit.point);
            if (prevCell && prevCell != currentCell)
            {
                ValidateDrag(currentCell);
            }
            else
            {
                isDrag = false;
            }
            EditCells(currentCell);
            prevCell = currentCell;
        }
        else
        {
            prevCell = null;
        }
    }

    void EditCells(HexCell center)
    {
        int centerX = center.coordinates.X;
        int centerZ = center.coordinates.Z;
        
        for (int r = 0, z = centerZ - brushSize; z <= centerZ; z++, r++) {
            for (int x = centerX - r; x <= centerX + brushSize; x++) {
                EditCell(grid.GetCell(new HexCoordinates(x, z)));
            }
        }
        
        for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++) {
            for (int x = centerX - brushSize; x <= centerX + r; x++) {
                EditCell(grid.GetCell(new HexCoordinates(x, z)));
            }
        }
    }
    
    void EditCell(HexCell cell)
    {
        if (!cell)
            return;

        if (isApplyTerrainType)
            cell.TerrainTypeInd = activeTerrainTypeIndex;
        
        if(isApplyElevation)
            cell.Elevation = activeElevation;

        if (isApplyWaterLevel)
            cell.WaterLevel = activeWaterLevel;

        if (isApplyFeature)
        {
            cell.FeatureCollectionInd = activeFeatureColectionIndex;
            cell.FeatureLevel = featureLevel;
        }

        if (riverMode == OptionalToggle.No)
        {
            cell.RemoveRiver();
        }

        if (roadMode == OptionalToggle.No)
        {
            cell.RemoveRoads();
        }
        
        if (isDrag)
        {
            HexCell otherCell = cell.GetNeighbor(dragDir.Opposite());
            if (otherCell)
            {
                if (riverMode == OptionalToggle.Yes)
                {
                    otherCell.SetOutgoingRiver(dragDir);
                }

                if (roadMode == OptionalToggle.Yes)
                {
                    otherCell.AddRoad(dragDir);
                }
            }
        }
            
    }
    
    public void SetElevationLevel(float elevation)
    {
        activeElevation = (int)elevation;
    }

    public void SetApplyElevation(bool toggle)
    {
        isApplyElevation = toggle;
    }

    public void SetBrushSize(float val)
    {
        brushSize = (int)val;
    }

    public void ShowUI(bool visible)
    {
        grid.ShowUI(visible);
    }

    public void SetRiverMode(int mode)
    {
        riverMode = (OptionalToggle)mode;
    }
    
    public void SetRoadMode(int mode)
    {
        roadMode = (OptionalToggle)mode;
    }
    
    public void SetApplyWaterLevel(bool toggle)
    {
        isApplyWaterLevel = toggle;
    }

    public void SetWaterLevel(float level)
    {
        activeWaterLevel = (int)level;
    }
    
    public void SetApplyFeatureLevel(bool toggle)
    {
        isApplyFeature = toggle;
    }

    public void SetFeatureLevel(float level)
    {
        featureLevel = (int)level;
    }

    public void SetTerrainTypeIndex(int index)
    {
        activeTerrainTypeIndex = index;
    }
    
    public void SetApplyTerrainType(bool toggle)
    {
        isApplyTerrainType = toggle;
    }

    void ValidateDrag(HexCell currentCell)
    {
        for (dragDir = HexDirection.NE; dragDir < HexDirection.NW; dragDir++)
        {
            if (prevCell.GetNeighbor(dragDir) == currentCell)
            {
                isDrag = true;
                return;
            }

            isDrag = false;
        }
    }
    
    private void FeaturesIsChanged(int i)
    {
        activeFeatureColectionIndex = i;
        
        sliderFeature.maxValue = HexMetrics.featureCollections[activeFeatureColectionIndex].Length;
    }

    public void Save()
    {
        string path = mapPath;
        Debug.Log(path);
        using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create)))
        {
            writer.Write(0);
            grid.Save(writer);
        }
        
    }
    
    public void Load()
    {
        string path = mapPath;
        using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
        {
            int header = reader.ReadInt32();
            if (header != 0)
            {
                Debug.Log("Unknown format for map");
            }
            else
            {
                grid.Load(reader);
            }
        }
    }

    [SerializeField] private HexGrid grid;
    [SerializeField] private Transform chunkPrefab;
    [SerializeField] private TMPro.TMP_Dropdown dropdownFeature;
    [SerializeField] private Slider sliderFeature;
    [SerializeField] private Texture2DArray texArray;

    private bool isApplyTerrainType = true;
    private int activeTerrainTypeIndex;
    
    private int activeFeatureColectionIndex;
    private bool isApplyFeature = true;
    private int featureLevel;

    private int activeElevation, activeWaterLevel;
    private bool isApplyElevation = true;
    private bool isApplyWaterLevel = true;
    
    private int brushSize;

    private OptionalToggle riverMode, roadMode;

    private bool isDrag;
    private HexDirection dragDir;
    private HexCell prevCell;

    private HexFeatureManager featureManager;
    private string dataPath;
    private const string mapPath = "Assets/Script/HexMap/MapEditor/SaveFiles/test.map";
}

enum OptionalToggle
{
    None, Yes, No
}

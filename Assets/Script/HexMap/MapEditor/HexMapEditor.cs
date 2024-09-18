using System;
using System.Collections;
using System.Collections.Generic;
using Script;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HexMapEditor : MonoBehaviour
{
    private void Awake()
    {
        isApplyElevation = true;
        SelectColor(0);
        
        dropdownFeature.onValueChanged.AddListener(FeaturesIsChanged);
        
        dropdownFeature.options.Clear();
        foreach (HexFeatureCollection feature in featureCollections)
        {
            dropdownFeature.options.Add(new TMPro.TMP_Dropdown.OptionData() { text = feature.name });
        }
        FeaturesIsChanged(0);
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
        
        if(isApplyColor)
            cell.Color = activeColor;
        
        if(isApplyElevation)
            cell.Elevation = activeElevation;

        if (isApplyWaterLevel)
            cell.WaterLevel = activeWaterLevel;

        if (isApplyFeature)
        {
            cell.FeatureCollection = activeFeatureCollection;
            cell.UrbanLevel = featureLevel;
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
    
    public void SelectColor(int ind)
    {
        isApplyColor = ind >= 0;
        if(isApplyColor)
            activeColor = colors[ind];
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
    
    public void SetApplyUrbanLevel(bool toggle)
    {
        isApplyFeature = toggle;
    }

    public void SetUrbanLevel(float level)
    {
        featureLevel = (int)level;
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
        activeFeatureCollection = featureCollections[i];
        sliderFeature.maxValue = activeFeatureCollection.Length;
    }
    
    [SerializeField] private HexFeatureCollection[] featureCollections;

    [SerializeField] private Color[] colors;
    [SerializeField] private HexGrid grid;
    [SerializeField] private Transform chunkPrefab;
    [SerializeField] private TMPro.TMP_Dropdown dropdownFeature;
    [SerializeField] private Slider sliderFeature;

    private bool isApplyFeature = true;
    private int featureLevel;
    private HexFeatureCollection activeFeatureCollection;
    
    private Color activeColor;
    
    private int activeElevation, activeWaterLevel;
    private int brushSize;

    private bool isApplyColor = true;
    private bool isApplyElevation = true;
    private bool isApplyWaterLevel = true;

    private OptionalToggle riverMode, roadMode;

    private bool isDrag;
    private HexDirection dragDir;
    private HexCell prevCell;
    
    
    
    public HexFeatureCollection[] FeatureCollections
    {
        get => featureCollections;
    }
    
    private HexFeatureManager featureManager;
}

enum OptionalToggle
{
    None, Yes, No
}

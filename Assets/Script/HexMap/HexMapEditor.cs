using System;
using System.Collections;
using System.Collections.Generic;
using Script;
using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour
{
    private void Awake()
    {
        isApplyElevation = true;
        SelectColor(0);
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

        if (riverMode == OptionalToggle.No)
        {
            cell.RemoveRiver();
        }
        else if (isDrag && riverMode == OptionalToggle.Yes)
        {
            HexCell otherCell = cell.GetNeighbor(dragDir.Opposite());
            if (otherCell)
            {
                otherCell.SetOutgoingRiver(dragDir);   
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
    
    [SerializeField] private Color[] colors;
    [SerializeField] private HexGrid grid;

    private Color activeColor;
    
    private int activeElevation;
    private int brushSize;

    private bool isApplyColor;
    private bool isApplyElevation;

    private OptionalToggle riverMode;

    private bool isDrag;
    private HexDirection dragDir;
    private HexCell prevCell;
}

enum OptionalToggle
{
    None, Yes, No
}

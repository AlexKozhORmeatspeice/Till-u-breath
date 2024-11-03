using UnityEngine;

public static class InputManager
{
    private static HexGrid grid;
    
    public static HexCell GetCellUnderCursor()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit))
        {
            return grid.GetCell(hit.point);
        }

        return null;
    }

    public static void SetGrid(HexGrid newGrid)
    {
        grid = newGrid;
    }
}

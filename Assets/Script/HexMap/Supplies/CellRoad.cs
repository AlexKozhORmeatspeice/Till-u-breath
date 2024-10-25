using System.Collections.Generic;
using System.Diagnostics;

public class CellRoad
{
    public CellRoad()
    {
        list = new List<HexCell>();
        length = 0;
        count = 0;
    }

    private List<HexCell> list;
    private int length;
    private int count;
    public int Count => count;
    public int Length => length;

    public void Push(HexCell cell)
    {
        if(count == 0)
            length = cell.Distance;

        if (count < list.Count)
        {
            list[count - 1] = cell;
        }            
        else
        {
            list.Add(cell);
        }

        count++;
    }

    public HexCell Pop()
    {
        if (count - 1 < 0)
            return null;
        
        HexCell cell = list[count - 1];
        count--;
        length -= cell.Distance;
        
        return cell;
    }
     

    public void Clear()
    {
        count = 0;
        length = 0;
        list.Clear();
    }
}
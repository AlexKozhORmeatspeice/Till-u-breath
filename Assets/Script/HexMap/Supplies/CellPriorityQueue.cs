using System.Collections.Generic;
using UnityEngine.Rendering;

public class CellPriorityQueue
{
     private List<HexCell> list = new List<HexCell>();
     private int count = 0;
     private int length = 0;
     private int minimum = int.MaxValue;

     public int Count => count;
     public int Length => length;

     public void Enqueue(HexCell cell)
     {
          count++;
          int priority = cell.SearchPriority;
          if (priority < minimum)
          {
               minimum = priority;
          }
          
          while (priority >= list.Count)
          {
               list.Add(null);
          }
          cell.NextWithSamePriority = list[priority];
          list[priority] = cell;
          length += cell.Distance;
     }

     public HexCell Dequeue()
     {
          count--;
          for (; minimum < list.Count; minimum++)
          {
               HexCell cell = list[minimum];
               if (cell != null)
               {
                    list[minimum] = cell.NextWithSamePriority;
                    length -= cell.Distance;
                    return cell;
               }
          }
          return null;
     }

     public void Change(HexCell cell, int oldPriority)
     {
          HexCell current = list[oldPriority];
          HexCell next = current.NextWithSamePriority;
          if (current == cell)
          {
               list[oldPriority] = next;
          }
          else
          {
               while (next != cell)
               {
                    current = next;
                    if (current.NextWithSamePriority != null)
                    {
                         next = current.NextWithSamePriority;
                    }
                    else
                    {
                         break;
                    }
               }
          }
          
          Enqueue(cell);
          count--;
     }

     public void Clear()
     {
          list.Clear();
          count = 0;
          minimum = int.MaxValue;
     }
}



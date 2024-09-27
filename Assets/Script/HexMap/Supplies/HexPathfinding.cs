using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Script;

public static class HexPathfinding
{
    private static HexCell[] cells;

    public static HexCell[] Cells
    {
        private get => cells;
        set => cells = value;
    }
    private static CellPriorityQueue priorityQueue;
    
    public static void FindPath(HexCell fromCell, HexCell toCell)
    {
        if(fromCell == null || toCell == null)
            return;
        
        if (priorityQueue == null)
        {
            priorityQueue = new CellPriorityQueue();
        }
        else
        {
            priorityQueue.Clear();
        }
        
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].Distance = int.MaxValue;
            cells[i].DisableOutline();
        }
        fromCell.EnableOutline(Color.blue);
        toCell.EnableOutline(Color.red);
        
        
        fromCell.Distance = 0;
        priorityQueue.Enqueue(fromCell);
        while (priorityQueue.Count > 0)
        {
            HexCell current = priorityQueue.Dequeue();

            if (current == toCell)
            {
                current = current.PathFrom;
                while (current != fromCell)
                {
                    current.EnableOutline(Color.white);
                    current = current.PathFrom;
                }
                break;
            }
            
            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                HexCell neighbor = current.GetNeighbor(d);
                if (neighbor == null)
                    continue;
                HexEdgeType curEdgeType = current.GetEdgeType(neighbor);
                if(curEdgeType == HexEdgeType.Cliff)
                    continue;

                int distance = current.Distance;
                if (current.IsUnderwater) {
                    distance += SurfaceTime.water;
                }
                else if(current.HasRoadThroughEdge(d)) 
                {
                    distance += SurfaceTime.road;
                }
                else if(curEdgeType == HexEdgeType.Flat)
                {
                    distance += SurfaceTime.ground;
                }
                else if (curEdgeType == HexEdgeType.Slope)
                {
                    distance += SurfaceTime.mountains;
                }

                if (neighbor.Distance == int.MaxValue)
                {
                    neighbor.Distance = distance;
                    neighbor.PathFrom = current;
                    neighbor.SearchHeuristic = neighbor.coordinates.DistanceTo(toCell.coordinates);
                    priorityQueue.Enqueue(neighbor);
                }
                else if(distance < neighbor.Distance)
                {
                    int oldPrior = neighbor.SearchPriority;
                    neighbor.Distance = distance;
                    neighbor.PathFrom = current;
                    priorityQueue.Change(neighbor, oldPrior);
                }
            }
        }
    }
}

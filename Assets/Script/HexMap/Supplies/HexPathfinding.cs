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
    
    public static CellRoad FindPath(HexCell fromCell, HexCell toCell)
    {
        if(fromCell == null || toCell == null)
            return null;
        
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
        }


        fromCell.Distance = 0;
        priorityQueue.Enqueue(fromCell);
        while (priorityQueue.Count > 0)
        {
            HexCell current = priorityQueue.Dequeue();
            
            //final road creation
            if (current == toCell)
            {
                CellRoad road = new CellRoad();
                
                road.Push(current);
                current = current.PathFrom;
                
                while (current != fromCell)
                {
                    road.Push(current);
                    
                    current = current.PathFrom;
                }

                return road;
            }
            //
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
        
        return null;
    }

    public static int GetTimeDist(HexCell fromCell, HexCell toCell)
    {
        if(fromCell == null || toCell == null)
            return -1;
        
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
        }
        
        fromCell.Distance = 0;
        priorityQueue.Enqueue(fromCell);
        while (priorityQueue.Count > 0)
        {
            HexCell current = priorityQueue.Dequeue();
            
            //final road creation
            if (current == toCell)
            {
                int dist = 0;
                
                return current.Distance;
            }
            //
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
        
        return -1;
    }
}

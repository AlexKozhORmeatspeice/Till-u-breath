using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Script;
using System.Linq;

public static class HexMath
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

    public static bool CanMove(HexCell fromCell, HexCell toCell, int lastTimeMove, int speed = 1)
    {
        int timeDist = HexMath.GetTimeDist(fromCell, toCell);
        return TimeManager.NowTime - lastTimeMove >= timeDist / speed;
    }

    public static int Distance(HexCell fromCell, HexCell toCell)
    {
        Vector3 coords = new Vector3(fromCell.coordinates.X - toCell.coordinates.X,
                                     fromCell.coordinates.Y - toCell.coordinates.Y,
                                     fromCell.coordinates.Z - toCell.coordinates.Z);

        return (int)(Mathf.Abs(coords.x) + Mathf.Abs(coords.y) + Mathf.Abs(coords.z)) / 2;
    }

    public static List<IAgent> CheckAgentsInRadius(int radius, HexCell startCell)
    {
        if (radius == 0 || startCell == null)
            return null;

        List<IAgent> agents = new List<IAgent>(); 

        Dictionary<HexCell, bool> wasHere = new Dictionary<HexCell, bool>();
        Stack<HexCell> stack = new Stack<HexCell>();
        stack.Push(startCell);

        //DFS
        while (stack.Count() != 0)
        {
            HexCell nowCell = stack.Pop();
            if (nowCell.Unit != null)
            {
                agents.Add(nowCell.Unit);
            }

            for (HexDirection dir = HexDirection.NE; dir != HexDirection.NW; dir++)
            {
                HexCell newCell = nowCell.GetNeighbor(dir);
                if (newCell != null && Distance(startCell, newCell) <= radius && !wasHere[newCell])
                {
                    stack.Push(newCell);
                    wasHere[newCell] = true;   
                }
            }
        }

        return agents;
    }

    public static IAgent CheckAgentInRadius(int radius, HexCell startCell, IAgent exceptAgent = null)
    {
        if (radius == 0 || startCell == null)
            return null;

        Dictionary<HexCell, bool> wasHere = new Dictionary<HexCell, bool>();
        Stack<HexCell> stack = new Stack<HexCell>();
        stack.Push(startCell);

        //DFS
        while (stack.Count() != 0)
        {
            HexCell nowCell = stack.Pop();
            if (nowCell.Unit != null && nowCell.Unit != exceptAgent)
            {
                return nowCell.Unit;
            }

            for (HexDirection dir = HexDirection.NE; dir != HexDirection.NW; dir++)
            {
                HexCell newCell = nowCell.GetNeighbor(dir);
                if (newCell != null && Distance(startCell, newCell) <= radius && !wasHere[newCell])
                {
                    stack.Push(newCell);
                    wasHere[newCell] = true;
                }
            }
        }

        return null;
    }

    public static bool IsAgentInRadius(int radius, HexCell startCell, IAgent agent)
    {
        if (radius == 0 || startCell == null)
            return false;

        Dictionary<HexCell, bool> wasHere = new Dictionary<HexCell, bool>();
        Stack<HexCell> stack = new Stack<HexCell>();
        stack.Push(startCell);

        //DFS
        while (stack.Count() != 0)
        {
            HexCell nowCell = stack.Pop();
            if (nowCell.Unit == agent)
            {
                return true;
            }

            for (HexDirection dir = HexDirection.NE; dir != HexDirection.NW; dir++)
            {
                HexCell newCell = nowCell.GetNeighbor(dir);
                if (newCell != null && Distance(startCell, newCell) <= radius && !wasHere[newCell])
                {
                    stack.Push(newCell);
                    wasHere[newCell] = true;
                }
            }
        }

        return false;
    }
}

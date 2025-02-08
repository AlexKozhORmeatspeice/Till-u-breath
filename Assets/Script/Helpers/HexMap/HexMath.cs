using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Script;
using System.Linq;
using static UnityEngine.RuleTile.TilingRuleOutput;

public static class HexMath
{
    private static HexCell[] cells;
    private static Dictionary<HexDirection, Vector3> world_dirByHex_dir = new Dictionary<HexDirection, Vector3>()
    {
        {HexDirection.NE, new Vector3( Mathf.Sqrt(2) / 2, 0,  Mathf.Sqrt(2) / 2)},
        {HexDirection.E,  new Vector3( 1,                 0,  0)},
        {HexDirection.SE, new Vector3( Mathf.Sqrt(2) / 2, 0, -Mathf.Sqrt(2) / 2)},
        {HexDirection.SW, new Vector3(-Mathf.Sqrt(2) / 2, 0, -Mathf.Sqrt(2) / 2)},
        {HexDirection.W,  new Vector3(-1,                 0,  0)},
        {HexDirection.NW, new Vector3(-Mathf.Sqrt(2) / 2, 0,  Mathf.Sqrt(2) / 2)},
    };

    public static HexCell[] Cells
    {
        private get => cells;
        set => cells = value;
    }
    private static CellPriorityQueue priorityQueue;
    
    public static CellRoad FindPath(HexCell fromCell, HexCell toCell, CellTypesBitmask excepctCells = null)
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
                if(curEdgeType == HexEdgeType.Cliff 
                   || (excepctCells != null && excepctCells.HasCellType(neighbor.CellType))
                   || neighbor.Unit != null)
                {
                    continue;
                }

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

    public static CellRoad FindPath(HexCell fromCell, Vector3 dir, CellTypesBitmask excepctCells = null)
    {
        if (fromCell == null || dir == Vector3.zero)
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
            float angle = Vector3.Dot((current.Position -  fromCell.Position).normalized, dir.normalized);
            if (current != fromCell && angle > 0.0f && angle <= Mathf.PI / 4.0f)
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
                if (curEdgeType == HexEdgeType.Cliff
                   || (excepctCells != null && excepctCells.HasCellType(neighbor.CellType))
                   || neighbor.Unit != null)
                {
                    continue;
                }

                int distance = current.Distance;
                if (current.IsUnderwater)
                {
                    distance += SurfaceTime.water;
                }
                else if (current.HasRoadThroughEdge(d))
                {
                    distance += SurfaceTime.road;
                }
                else if (curEdgeType == HexEdgeType.Flat)
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

                    float neighborAngle = Mathf.Clamp(Vector3.Dot((neighbor.Position - fromCell.Position).normalized, dir.normalized), 0.0f, 1.0f);
                    neighbor.SearchHeuristic = (int)(neighborAngle * Mathf.Rad2Deg);
                    
                    priorityQueue.Enqueue(neighbor);
                }
                else if (distance < neighbor.Distance)
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

    public static bool CanMove(HexCell fromCell, HexCell toCell, int lastTimeMove)
    {
        if (Distance(fromCell, toCell) > 1 || FindPath(fromCell, toCell) == null)
            return false;

        int timeDist = HexMath.GetTimeDist(fromCell, toCell);
        return TimeManager.NowTime - lastTimeMove >= timeDist;
    }

    public static int Distance(HexCell fromCell, HexCell toCell)
    {
        if(fromCell == null || toCell == null) return int.MaxValue;

        Vector3 coords = new Vector3(fromCell.coordinates.X - toCell.coordinates.X,
                                     fromCell.coordinates.Y - toCell.coordinates.Y,
                                     fromCell.coordinates.Z - toCell.coordinates.Z);

        return (int)(Mathf.Abs(coords.x) + Mathf.Abs(coords.y) + Mathf.Abs(coords.z)) / 2;
    }

    public static List<IAgent> FindAgentsInRadius(HexCell startCell, int radius, AgentsBitmask exceptAgents = null)
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
                if (exceptAgents == null)
                {
                    agents.Add(nowCell.Unit);
                }
                else if (!exceptAgents.HasName(nowCell.Unit.GetAgentName()))
                {
                    agents.Add(nowCell.Unit);
                }
            }

            for (HexDirection dir = HexDirection.NE; dir != HexDirection.NW; dir++)
            {
                HexCell newCell = nowCell.GetNeighbor(dir);
                if (newCell != null && Distance(startCell, newCell) <= radius && (!wasHere.ContainsKey(newCell) || !wasHere[newCell]))
                {
                    stack.Push(newCell);
                    wasHere[newCell] = true;   
                }
            }
        }

        return agents;
    }

    public static IAgent FindAgentInRadius(HexCell startCell, int radius, AgentsBitmask exceptAgents = null)
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

            if (nowCell.Unit != null)
            {
                if(exceptAgents == null)
                {
                    return nowCell.Unit;
                }
                else if(!exceptAgents.HasName(nowCell.Unit.GetAgentName()))
                {
                    return nowCell.Unit;
                }
            }

            for (HexDirection dir = HexDirection.NE; dir != HexDirection.NW; dir++)
            {
                HexCell newCell = nowCell.GetNeighbor(dir);
                if (newCell != null && Distance(startCell, newCell) <= radius && (!wasHere.ContainsKey(newCell) || !wasHere[newCell]))
                {
                    stack.Push(newCell);
                    wasHere[newCell] = true;
                }
            }
        }

        return null;
    }

    public static bool IsAgentInRadius(HexCell startCell, int radius, AgentName nameAgent)
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
            if (nowCell.Unit.GetAgentName() == nameAgent)
            {
                return true;
            }

            for (HexDirection dir = HexDirection.NE; dir != HexDirection.NW; dir++)
            {
                HexCell newCell = nowCell.GetNeighbor(dir);
                if (newCell != null && Distance(startCell, newCell) <= radius && (!wasHere.ContainsKey(newCell) || !wasHere[newCell]))
                {
                    stack.Push(newCell);
                    wasHere[newCell] = true;
                }
            }
        }

        return false;
    }

    public static Food FindFoodInRadius(HexCell startCell, int radius)
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

            Food food = nowCell.Item as Food;
            if (food != null && food.gameObject.activeSelf)
            {
                return food;
            }

            for (HexDirection dir = HexDirection.NE; dir != HexDirection.NW; dir++)
            {
                HexCell newCell = nowCell.GetNeighbor(dir);
                if (newCell != null && Distance(startCell, newCell) <= radius && (!wasHere.ContainsKey(newCell) || !wasHere[newCell]))
                {
                    stack.Push(newCell);
                    wasHere[newCell] = true;
                }
            }
        }

        return null;
    }

    public static HexDirection Hex_dirByWorld_dir(Vector3 world_dir)
    {
        float minAngle = float.MaxValue;
        Vector3 normWorldDir = world_dir.normalized;
        
        HexDirection resHexDir = HexDirection.NE;
        for(HexDirection dir = HexDirection.NE; dir != HexDirection.NW; dir++) 
        {
            Vector3 hexVec = world_dirByHex_dir[dir];
            float angle = Vector3.Dot(hexVec, normWorldDir);

            /*it might be worse it to check if it's minimum between just neighbors
            but it's still O(1), so i don't think it's necessary*/
            if (angle < minAngle)
            {
                resHexDir = dir;
                minAngle = angle;
            }
        }

        return resHexDir;
    }
    public static Vector3 World_dirByHex_dir(HexDirection hex_dir)
    {
        return world_dirByHex_dir[hex_dir];
    }
}

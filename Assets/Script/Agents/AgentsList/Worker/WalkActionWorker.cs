using Script;
using Script.Agents.AgentsList.Supplies;
using UnityEngine;

public class WalkActionWorker : BaseAction<WorkerAgent.WorkerActions>
{
    private WorkerAgent workerAgent;
    private CellRoad roadToWork;

    private int lastTimeMove;
    private HexCell endCell;
    private HexCell startCell;

    public override void Start()
    {
        lastTimeMove = TimeManager.NowTime;
        workerAgent = agent.GetComponent<WorkerAgent>();
        
        int min = int.MaxValue;
        HexCell startCell = agent.NowAgentState.onCell;
        
        
        if (endCell == null)
        {
            //find path to work
            foreach (HexCell cell in workerAgent.HexGrid.Cells)
            {
                if (workerAgent.WorkPlace.Equals((Places)cell.FeatureCollectionInd))
                {
                    CellRoad road = HexPathfinding.FindPath(startCell, cell);
                
                    if (road != null && road.Length < min)
                    {
                        Debug.Log(road.Length);
                        endCell = cell;
                        min = road.Length;
                    }
                }
            }
        }
        
    }

    public override AgentState<WorkerAgent.WorkerActions> Update()
    {
        HexCell nowCell = agent.NowAgentState.onCell;
        roadToWork = HexPathfinding.FindPath(nowCell, endCell); 
        
        //баля ну типо я пока в рот ебал как не просчитывать каждый ход путь
        
        if (roadToWork == null)
        {
            Debug.Log(agent.GetGameObject().name + " can't find a place to work");
            return new WorkerState(nowCell, WorkerAgent.WorkerActions.walk, lastTimeMove);
        }
        
        WorkerState newState = new WorkerState(nowCell, WorkerAgent.WorkerActions.walk, lastTimeMove);
        
        HexCell newCell = roadToWork.Pop();
        
        int timeDist = HexPathfinding.GetTimeDist(nowCell, newCell);
        if (TimeManager.NowTime - lastTimeMove >= timeDist)
        {
            newState.onCell = newCell;
            newState.lastMoveTime = TimeManager.NowTime;
        }

        return newState;
    }

    public override void Exit()
    {
        startCell = endCell;
    }

    public override WorkerAgent.WorkerActions GetNextState()
    {
        if (endCell == agent.NowAgentState.onCell)
        {
            return WorkerAgent.WorkerActions.work;
        }
        
        return WorkerAgent.WorkerActions.walk;
    }

    public WalkActionWorker(WorkerAgent.WorkerActions key, Agent<WorkerAgent.WorkerActions> nowAgent) : base(key, nowAgent)
    {
    }
}
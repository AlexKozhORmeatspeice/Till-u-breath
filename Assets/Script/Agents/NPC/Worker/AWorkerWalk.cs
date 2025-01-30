using Script;
using Script.Agents.AgentsList.Supplies;
using UnityEngine;

public class AWorkerWalk : BaseAction<WorkerAgent.WorkerActions>
{
    private WorkerAgent workerAgent;
    private CellRoad roadToWork;

    private HexCell endCell;
    private HexCell startCell;

    public override void Start()
    {
        workerAgent = agent.GetComponent<WorkerAgent>();
        workerAgent.lastTimeMove = TimeManager.NowTime;
        
        int min = int.MaxValue;
        HexCell startCell = agent.NowAgentState.onCell;

        if (endCell == null)
        {
            //find path to work
            foreach (HexCell cell in workerAgent.HexGrid.Cells)
            {
                if (workerAgent.WorkPlace.Equals((Places)cell.FeatureCollectionInd))
                {
                    CellRoad road = HexMath.FindPath(startCell, cell);
                
                    if (road != null && road.Length < min)
                    {
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
        roadToWork = HexMath.FindPath(nowCell, endCell); 
        
        //баля ну типо я пока в рот ебал как не просчитывать каждый ход путь
        
        if (roadToWork == null)
        {
            return new WorkerState(nowCell, WorkerAgent.WorkerActions.walk, workerAgent.lastTimeMove);
        }
        
        WorkerState newState = new WorkerState(nowCell, WorkerAgent.WorkerActions.walk, workerAgent.lastTimeMove);
        
        HexCell newCell = roadToWork.Pop();
        
        newState.onCell = newCell;

        return newState;
    }

    public override void Exit()
    {
        startCell = endCell;
    }

    public override WorkerAgent.WorkerActions GetNextAction()
    {
        if (endCell == agent.NowAgentState.onCell)
        {
            return WorkerAgent.WorkerActions.work;
        }
        
        return WorkerAgent.WorkerActions.walk;
    }

    public override void OnFrameUpdate()
    {
        //
    }

    public override WorkerAgent.WorkerActions GetNextActionOnFrameUpdate()
    {
        return WorkerAgent.WorkerActions.walk;
    }

    public AWorkerWalk(WorkerAgent.WorkerActions key, Agent<WorkerAgent.WorkerActions> nowAgent) : base(key, nowAgent)
    {
    }
}
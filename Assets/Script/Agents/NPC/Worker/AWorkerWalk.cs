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
        HexCell startCell = agent.nowAgentState.onCell;

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

    public override void Update()
    {
        HexCell nowCell = agent.nowAgentState.onCell;
        roadToWork = HexMath.FindPath(nowCell, endCell); 
        
        if (roadToWork == null)
        {
            return;
        }
        
        HexCell newCell = roadToWork.Pop();
        
        agent.nowAgentState.onCell = newCell;
    }

    public override void Exit()
    {
        startCell = endCell;
    }

    public override WorkerAgent.WorkerActions GetNextAction()
    {
        if (endCell == agent.nowAgentState.onCell)
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
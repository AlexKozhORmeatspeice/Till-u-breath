using Script;
using UnityEngine;

public class WalkActionWorker : BaseAction<WorkerAgent.WorkerState>
{

    public override void Start()
    {
        Debug.Log("I want to fuck communists");
    }

    public override AgentState Update()
    {
        HexCell nowCell = agent.NowAgentState.onCell;
        HexCell newCell = nowCell.GetNeighbor(HexDirection.SW);

        AgentState newState = new AgentState(newCell);
        return newState;
    }

    public override void Exit()
    {
        throw new System.NotImplementedException();
    }

    public override WorkerAgent.WorkerState GetNextState()
    {
        return WorkerAgent.WorkerState.walk;
    }

    public WalkActionWorker(WorkerAgent.WorkerState key, Agent<WorkerAgent.WorkerState> nowAgent) : base(key, nowAgent)
    {
    }
}
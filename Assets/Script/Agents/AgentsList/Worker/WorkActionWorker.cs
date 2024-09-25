using UnityEngine;

public class WorkActionWorker : BaseAction<WorkerAgent.WorkerState>
{
    public override void Start()
    {
        Debug.Log("StartWork");
    }

    public override AgentState Update()
    {
        throw new System.NotImplementedException();
    }

    public override void Exit()
    {
        throw new System.NotImplementedException();
    }

    public override WorkerAgent.WorkerState GetNextState()
    {
        throw new System.NotImplementedException();
    }

    public WorkActionWorker(WorkerAgent.WorkerState key, Agent<WorkerAgent.WorkerState> nowAgent) : base(key, nowAgent)
    {
        
    }
}

using UnityEngine;

public class WorkerAgent : Agent<WorkerAgent.WorkerState>
{
    public enum WorkerState
    {
        walk,
        work
    }
    
    protected override void Start()
    {
        actionStates[WorkerState.walk] = new WalkActionWorker(WorkerState.walk, this);
        actionStates[WorkerState.work] = new WorkActionWorker(WorkerState.work, this);
        
        nowAction = actionStates[WorkerState.walk];

        base.Start();
    }
}

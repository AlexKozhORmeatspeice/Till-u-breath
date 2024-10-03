using Script.Agents.AgentsList.Supplies;
using UnityEngine;

public class WorkerAgent : Agent<WorkerAgent.WorkerActions>
{
    public enum WorkerActions
    {
        walk,
        work
    }

    [SerializeField] private int timeToWork;
    [SerializeField] private Places workPlace;

    public Places WorkPlace => workPlace;
    public int TimeToWork => timeToWork;

    protected override void Start()
    {
        actionStates[WorkerActions.walk] = new WalkActionWorker(WorkerActions.walk, this);
        actionStates[WorkerActions.work] = new WorkActionWorker(WorkerActions.work, this);
        
        nowAction = actionStates[WorkerActions.walk];
        
        base.Start(); //always the last
    }
}

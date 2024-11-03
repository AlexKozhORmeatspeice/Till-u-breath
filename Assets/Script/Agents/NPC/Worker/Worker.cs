using Script.Agents.AgentsList.Supplies;
using UnityEngine;
using static Hero;

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

    public int lastTimeMove;

    protected override void Start()
    {
        actionStates[WorkerActions.walk] = new AWorkerWalk(WorkerActions.walk, this);
        actionStates[WorkerActions.work] = new AWorkWorker(WorkerActions.work, this);
        
        nowAction = actionStates[WorkerActions.walk];

        SetState(new WorkerState(NowAgentState.onCell, WorkerActions.walk, TimeManager.NowTime));

        base.Start(); //always the last
    }

    protected override void ChangeState(AgentState<WorkerActions> state)
    {
        base.ChangeState(state);

        if (state == null)
        {
            lastTimeMove = TimeManager.NowTime;
        }
        else
        {
            WorkerState workerSt = state as WorkerState;
            lastTimeMove = workerSt.lastMoveTime;
        }
    }
}

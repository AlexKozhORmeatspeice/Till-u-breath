using UnityEngine;

public class WorkActionWorker : BaseAction<WorkerAgent.WorkerActions>
{
    private WorkerState workerState;
    private WorkerAgent worker;
    private int startOfWork;
    
    public override void Start()
    {
        startOfWork = TimeManager.NowTime;
        worker = agent.GetComponent<WorkerAgent>();
    }

    public override AgentState<WorkerAgent.WorkerActions> Update()
    {
        workerState = (WorkerState)agent.GetComponent<WorkerAgent>().NowAgentState;
        Debug.Log((TimeManager.NowTime - startOfWork).ToString() + " and " + worker.TimeToWork.ToString());
        
        return new WorkerState(workerState.onCell, WorkerAgent.WorkerActions.work, workerState.lastMoveTime);
    }

    public override void Exit()
    {
        //
    }

    public override WorkerAgent.WorkerActions GetNextState()
    {
        /*if (TimeManager.NowTime - startOfWork >= worker.TimeToWork)
        {
            return WorkerAgent.WorkerActions.walk;
        }*/

        return WorkerAgent.WorkerActions.work;
    }

    public WorkActionWorker(WorkerAgent.WorkerActions key, Agent<WorkerAgent.WorkerActions> nowAgent) : base(key, nowAgent)
    {
        
    }
}

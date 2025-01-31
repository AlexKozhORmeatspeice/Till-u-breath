using UnityEngine;

public class AWorkWorker : BaseAction<WorkerAgent.WorkerActions>
{
    private WorkerState workerState;
    private WorkerAgent worker;
    private int startOfWork;
    
    public override void Start()
    {
        startOfWork = TimeManager.NowTime;
        worker = agent.GetComponent<WorkerAgent>();
        workerState = (WorkerState)worker.nowAgentState;
    }

    public override void Update()
    {
        //
    }

    public override void Exit()
    {
        //
    }

    public override WorkerAgent.WorkerActions GetNextAction()
    {
        /*if (TimeManager.NowTime - startOfWork >= worker.TimeToWork)
        {
            return WorkerAgent.WorkerActions.walk;
        }*/

        return WorkerAgent.WorkerActions.work;
    }

    public override void OnFrameUpdate()
    {
        //
    }

    public override WorkerAgent.WorkerActions GetNextActionOnFrameUpdate()
    {
        return WorkerAgent.WorkerActions.work;
    }

    public AWorkWorker(WorkerAgent.WorkerActions key, Agent<WorkerAgent.WorkerActions> nowAgent) : base(key, nowAgent)
    {
        
    }
}

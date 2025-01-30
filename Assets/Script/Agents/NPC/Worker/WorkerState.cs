public class WorkerState : AgentState<WorkerAgent.WorkerActions>
{
    public WorkerState(HexCell cell, WorkerAgent.WorkerActions state, int _lastMoveTime) : base(cell, state)
    {
        lastMoveTime = _lastMoveTime;
    }
}
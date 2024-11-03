class AWalkDoctor : BaseAction<Doctor.DoctorActions>
{
    DoctorState state;
    Doctor doctor;

    public override void Start()
    {
        state = agent.NowAgentState as DoctorState;
        doctor = agent as Doctor;

        doctor.MenuOrder.gameObject.SetActive(false);
    }

    public override void OnFrameUpdate()
    {
        throw new System.NotImplementedException();
    }

    public override AgentState<Doctor.DoctorActions> Update()
    {
        throw new System.NotImplementedException();
    }

    public override void Exit()
    {
        throw new System.NotImplementedException();
    }

    public override Doctor.DoctorActions GetNextAction()
    {
        return Doctor.DoctorActions.walk;
    }

    public override Doctor.DoctorActions GetNextActionOnFrameUpdate()
    {
        return Doctor.DoctorActions.walk;
    }

    public AWalkDoctor(Doctor.DoctorActions key, Agent<Doctor.DoctorActions> nowAgent) : base(key, nowAgent)
    {
    }
}

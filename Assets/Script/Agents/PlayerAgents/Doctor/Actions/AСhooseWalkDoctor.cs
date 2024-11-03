using UnityEngine;

internal class AСhooseWalkDoctor : BaseAction<Doctor.DoctorActions>
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
        Debug.Log("choose walk");
    }

    public override AgentState<Doctor.DoctorActions> Update()
    {
        return state;
    }

    public override void Exit()
    {
        //
    }

    public override Doctor.DoctorActions GetNextAction()
    {
        return Doctor.DoctorActions.chooseWalk;
    }

    public override Doctor.DoctorActions GetNextActionOnFrameUpdate()
    {
        return Doctor.DoctorActions.chooseWalk;
    }
    public AСhooseWalkDoctor(Doctor.DoctorActions key, Agent<Doctor.DoctorActions> nowAgent) : base(key, nowAgent)
    {
    }
}


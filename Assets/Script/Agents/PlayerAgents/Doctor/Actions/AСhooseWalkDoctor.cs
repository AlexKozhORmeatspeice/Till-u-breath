using UnityEngine;

internal class AСhooseWalkDoctor : BaseAction<Doctor.DoctorActions>
{
    Doctor doctor;
    public override void Start()
    {
        doctor = agent as Doctor;

        doctor.MenuOrder.gameObject.SetActive(false);
    }

    public override void OnFrameUpdate()
    {
        Debug.Log("choose walk");
    }

    public override void Update()
    {
        //
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


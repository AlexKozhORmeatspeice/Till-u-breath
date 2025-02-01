using UnityEngine;

class AChooseOrderDoctor : BaseAction<Doctor.DoctorActions>
{
    Doctor doctor;
    OrderMenu menu;

    private float timeBeforeStartChoose = 0.1f;
    private float startChooseTime;

    private bool canChoose;
    public override void Start()
    {
        canChoose = false;
        startChooseTime = Time.time;

        doctor = agent as Doctor;
        menu = doctor.MenuOrder;

        menu.gameObject.SetActive(true);
    }

    public override void OnFrameUpdate()
    {
        canChoose = Time.time - startChooseTime > timeBeforeStartChoose;
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
        return Doctor.DoctorActions.chooseOrder;
    }

    public override Doctor.DoctorActions GetNextActionOnFrameUpdate()
    {
        bool gotLeftMouse = Input.GetMouseButtonDown(0);
        bool gotRightMouse = Input.GetMouseButtonDown(1);
        if(gotRightMouse || Input.GetKeyDown(KeyCode.Space))
        {
            return Doctor.DoctorActions.followHero;
        }


        if (canChoose && gotLeftMouse && menu.CurrentOrder.order == Orders.Move)
        {
            return Doctor.DoctorActions.chooseWalk;
        }

        return Doctor.DoctorActions.chooseOrder;
    }

    public AChooseOrderDoctor(Doctor.DoctorActions key, Agent<Doctor.DoctorActions> nowAgent) : base(key, nowAgent)
    {
    }
}

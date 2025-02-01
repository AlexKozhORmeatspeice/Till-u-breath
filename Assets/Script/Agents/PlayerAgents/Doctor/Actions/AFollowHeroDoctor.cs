using UnityEngine.EventSystems;
using UnityEngine;
public class AFollowHeroDoctor : BaseAction<Doctor.DoctorActions>
{
    Doctor doctor;
    public override void Start()
    {
        doctor = agent as Doctor;

        doctor.MenuOrder.gameObject.SetActive(false);
    }

    public override void OnFrameUpdate()
    {
        //
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
        return Doctor.DoctorActions.followHero;
    }

    public override Doctor.DoctorActions GetNextActionOnFrameUpdate()
    {
        bool isOverUI = EventSystem.current.IsPointerOverGameObject();

        if (!isOverUI && Input.GetMouseButtonDown(0))
        {
            HexCell cell = InputManager.GetCellUnderCursor();

            if (cell != null && agent.nowAgentState.onCell != null && cell == agent.nowAgentState.onCell) //choose hero
            {
                cell.EnableOutline(doctor.StartColor);
                return Doctor.DoctorActions.chooseOrder;
            }
        }

        return Doctor.DoctorActions.followHero;
    }

    public AFollowHeroDoctor(Doctor.DoctorActions key, Agent<Doctor.DoctorActions> nowAgent) : base(key, nowAgent)
    {
    }
}

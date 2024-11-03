using UnityEngine.EventSystems;
using UnityEngine;
public class AFollowHeroDoctor : BaseAction<Doctor.DoctorActions>
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
        //
    }

    public override AgentState<Doctor.DoctorActions> Update()
    {
        return agent.NowAgentState;
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

            if (cell != null && cell == state.onCell) //choose hero
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

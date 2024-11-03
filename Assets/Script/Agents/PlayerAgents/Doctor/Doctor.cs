using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Hero;

public class Doctor : Agent<Doctor.DoctorActions>
{
    public enum DoctorActions
    {
        followHero,
        chooseOrder,
        chooseWalk,
        walk
    }
    [SerializeField] private OrderMenu menu;
    public OrderMenu MenuOrder => menu;

    [SerializeField] private Color startColor;
    [SerializeField] private Color moveColor;
    [SerializeField] private Color endColor;
    public Color StartColor => startColor;
    public Color MoveColor => moveColor;
    public Color EndColor => endColor;

    public int lastTimeMove;
    public Hero hero;
    protected override void Start()
    {
        actionStates[DoctorActions.followHero] = new AFollowHeroDoctor(DoctorActions.followHero, this);
        actionStates[DoctorActions.chooseOrder] = new AChooseOrderDoctor(DoctorActions.chooseOrder, this);
        actionStates[DoctorActions.chooseWalk] = new AÑhooseWalkDoctor(DoctorActions.followHero, this);
        actionStates[DoctorActions.walk] = new AWalkDoctor(DoctorActions.followHero, this);

        nowAction = actionStates[DoctorActions.followHero];
        SetState(new DoctorState(NowAgentState.onCell, DoctorActions.followHero, TimeManager.NowTime));

        base.Start(); //always the last
    }

    protected override void ChangeState(AgentState<DoctorActions> state)
    {
        base.ChangeState(state);

        if (state == null)
        {
            lastTimeMove = TimeManager.NowTime;
        }
        else
        {
            DoctorState doctorSt = state as DoctorState;
            lastTimeMove = doctorSt.lastMoveTime;
        }
    }
}

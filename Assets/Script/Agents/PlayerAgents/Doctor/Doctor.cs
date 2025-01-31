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
    [Header("UISettings")]
    [SerializeField] private Color startColor;
    [SerializeField] private Color moveColor;
    [SerializeField] private Color endColor;

    [Header("Prefabs")]
    [SerializeField] private OrderMenu menu;
    public OrderMenu MenuOrder => menu;
    public Color StartColor => startColor;
    public Color MoveColor => moveColor;
    public Color EndColor => endColor;

    [Header("Script vars")]
    public int lastTimeMove;
    public Hero hero;
    protected override void AgentStart()
    {
        actionStates[DoctorActions.followHero] = new AFollowHeroDoctor(DoctorActions.followHero, this);
        actionStates[DoctorActions.chooseOrder] = new AChooseOrderDoctor(DoctorActions.chooseOrder, this);
        actionStates[DoctorActions.chooseWalk] = new AÑhooseWalkDoctor(DoctorActions.followHero, this);
        actionStates[DoctorActions.walk] = new AWalkDoctor(DoctorActions.followHero, this);

        nowAgentState.actionState = DoctorActions.followHero;
    }
}

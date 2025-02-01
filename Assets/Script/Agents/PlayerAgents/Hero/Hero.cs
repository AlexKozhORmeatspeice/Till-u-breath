using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : Agent<Hero.HeroActions>
{
    public enum HeroActions
    {
        inaction,
        chooseOrder,
        chooseAttack,
        attack,
        chooseWalk,
        walk
    }

    private Weapon nowWeapon;

    [NonSerialized] public HexCell moveEndCell;
    [NonSerialized] public int lastTimeMove;

    [Header("UI")]
    [SerializeField] private Color startColor;
    [SerializeField] private Color moveColor;
    [SerializeField] private Color endColor;
    public Color StartColor => startColor;
    public Color MoveColor => moveColor;
    public Color EndColor => endColor;

    protected override void AgentStart()
    {
        actionStates[HeroActions.inaction] = new AInactionHero(HeroActions.inaction, this);
        actionStates[HeroActions.chooseWalk] = new AChooseWalkHero(HeroActions.chooseWalk, this);
        actionStates[HeroActions.walk] = new AWalkHero(HeroActions.walk, this);
        
        nowAgentState.actionState = HeroActions.inaction;
    }
}

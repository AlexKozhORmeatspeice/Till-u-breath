using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : Agent<Hero.HeroActions>
{
    public enum HeroActions
    {
        inaction,
        chooseWalk,
        walk
    }

    [SerializeField] private Color startColor;
    [SerializeField] private Color moveColor;
    [SerializeField] private Color endColor;
    public Color StartColor => startColor;
    public Color MoveColor => moveColor;
    public Color EndColor => endColor;

    public HexCell moveEndCell;
    public int lastTimeMove;

    protected override void Start()
    {
        actionStates[HeroActions.inaction] = new AInactionHero(HeroActions.inaction, this);
        actionStates[HeroActions.chooseWalk] = new AChooseWalkHero(HeroActions.chooseWalk, this);
        actionStates[HeroActions.walk] = new AWalkHero(HeroActions.walk, this);
        
        nowAction = actionStates[HeroActions.inaction];
        SetState(new HeroState(NowAgentState.onCell, HeroActions.inaction, TimeManager.NowTime));

        base.Start(); //always the last
    }

    protected override void ChangeState(AgentState<HeroActions> state)
    {
        base.ChangeState(state);
        
        if(state == null)
        {
            lastTimeMove = TimeManager.NowTime;
        }
        else
        {
            HeroState heroSt = state as HeroState;
            lastTimeMove = heroSt.lastMoveTime;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroState : AgentState<Hero.HeroActions>
{
    public HeroState(HexCell cell, Hero.HeroActions state, int _lastMoveTime) : base(cell, state)
    {
        lastMoveTime = _lastMoveTime;
    }
}

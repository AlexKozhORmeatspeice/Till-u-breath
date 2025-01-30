using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boar : Agent<Boar.BoarActions>
{
    public enum BoarActions
    {
        move,
        searchFood,
        runFromHero,
        attackHero
    }

    [SerializeField] private HexCellType cellMoveType;
    public HexCellType CellMoveType => cellMoveType;
    [SerializeField] private int speed = 2;
    public int Speed => speed;

    protected override void Start()
    {
        actionStates[BoarActions.move] = new AWalkBoar(BoarActions.move, this);
        /*actionStates[BoarAction.searchFood] = new AChooseWalkHero(HeroActions.chooseWalk, this);
        actionStates[BoarAction.runFromHero] = new AWalkHero(HeroActions.walk, this);
        actionStates[BoarAction.attackHero] = new AWalkHero(HeroActions.walk, this);*/

        nowAction = actionStates[BoarActions.move];
        //SetState(new AgentState<BoarActions>(NowAgentState.onCell, BoarActions.move));

        base.Start(); //always the last
    }

  protected override void ChangeState(AgentState<BoarActions> state)
    {
        base.ChangeState(state);
    }
}

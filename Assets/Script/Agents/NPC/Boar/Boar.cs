using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Boar;

public class Boar : Agent<Boar.BoarActions>
{
    public enum BoarActions
    {
        move,
        searchFood,
        runFromHero,
        attackHero,
        walkForFood,
        digForFood
    }

    [SerializeField] private HexCellType cellMoveType;
    [SerializeField] private int searchFoodRadius = 12;
    [SerializeField] private int searchEnemyRadius = 10;
    public HexCellType CellMoveType => cellMoveType;
    public int SearchFoodRadius => searchFoodRadius;
    public int SearchEnemyRadius => searchEnemyRadius;

    [NonSerialized] public Food searchFood;

    protected override void AgentStart()
    {
        actionStates[BoarActions.move] = new AWalkBoar(BoarActions.move, this);
        actionStates[BoarActions.searchFood] = new AFindFoodBoar(BoarActions.searchFood, this);
        actionStates[BoarActions.walkForFood] = new AWalkForFoodBoar(BoarActions.walkForFood, this);
        actionStates[BoarActions.digForFood] = new ADigForFoodBoar(BoarActions.digForFood, this);

        nowAgentState.actionState = BoarActions.move;
    }

    protected override void ChangeState(AgentState<BoarActions> state)
    {
        base.ChangeState(state);
  }
}

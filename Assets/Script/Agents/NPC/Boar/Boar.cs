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
        walk,
        findFood,
        runFromHero,
        attackHero,
        walkForFood,
        digForFood
    }
    [Header("Vars Boar")]
    [SerializeField] private HexCellType cellMoveType;
    [SerializeField] private int searchFoodRadius = 12;
    [SerializeField] private int searchEnemyRadius = 10;
    [Header("Randomness")]
    [SerializeField][Range(0.0f, 1.0f)] private float chanceToFindFood = 0.3f;

    public HexCellType CellMoveType => cellMoveType;
    public int SearchFoodRadius => searchFoodRadius;
    public int SearchEnemyRadius => searchEnemyRadius;
    public float ChanceToFindFood => chanceToFindFood;

    [NonSerialized] public Food searchFood;

    protected override void AgentStart()
    {
        actionStates[BoarActions.walk] = new AWalkBoar(BoarActions.walk, this);
        actionStates[BoarActions.findFood] = new AFindFoodBoar(BoarActions.findFood, this);
        actionStates[BoarActions.walkForFood] = new AWalkForFoodBoar(BoarActions.walkForFood, this);
        actionStates[BoarActions.digForFood] = new ADigForFoodBoar(BoarActions.digForFood, this);

        nowAgentState.actionState = BoarActions.walk;

        searchFood = null;
    }

    protected override void ChangeState(AgentState<BoarActions> state)
    {
        base.ChangeState(state);
  }
}

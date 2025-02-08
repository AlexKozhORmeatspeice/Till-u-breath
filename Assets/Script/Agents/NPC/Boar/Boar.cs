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
        runFromAgents,
        attackAgent,
        walkForFood,
        digForFood
    }

    [Header("Vars Boar")]
    [SerializeField] private List<HexCellType> cellToMoveTypes;
    [SerializeField] private List<AgentName> enemyList;
    [SerializeField] private int searchFoodRadius = 12;
    [SerializeField] private int searchEnemyRadius = 10;
    [Header("Randomness")]
    [SerializeField][Range(0.0f, 1.0f)] private float chanceToFindFood = 0.3f;

    private VisibleAgents seeAgents;
    private CellTypesBitmask cellsToMoveBitmask;
    
    [NonSerialized] public Food searchFood;
    [NonSerialized] public IAgent attackAgent;
    
    public int SearchFoodRadius => searchFoodRadius;
    public int SearchEnemyRadius => searchEnemyRadius;
    public float ChanceToFindFood => chanceToFindFood;
    public VisibleAgents SeeAgents => seeAgents;
    public CellTypesBitmask CellsToMoveBitmask => cellsToMoveBitmask;

    protected override void AgentStart()
    {
        actionStates[BoarActions.walk] = new AWalkBoar(BoarActions.walk, this);
        actionStates[BoarActions.findFood] = new AFindFoodBoar(BoarActions.findFood, this);
        actionStates[BoarActions.walkForFood] = new AWalkForFoodBoar(BoarActions.walkForFood, this);
        actionStates[BoarActions.digForFood] = new ADigForFoodBoar(BoarActions.digForFood, this);
        actionStates[BoarActions.runFromAgents] = new ARunFromAgentsBoar(Boar.BoarActions.runFromAgents, this);

        nowAgentState.actionState = BoarActions.walk;

        searchFood = null;

        seeAgents = new VisibleAgents(this, new AgentsBitmask(enemyList), searchEnemyRadius);
        cellsToMoveBitmask = new CellTypesBitmask(cellToMoveTypes);
    }

    protected override void ChangeState(AgentState<BoarActions> state)
    {
        base.ChangeState(state);
  }
}

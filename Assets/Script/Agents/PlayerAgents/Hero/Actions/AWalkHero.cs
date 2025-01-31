using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.EventSystems;
public class AWalkHero : BaseAction<Hero.HeroActions>
{
    private Hero hero;
    private CellRoad road;

    public override void Start()
    {
        hero = agent.GetComponent<Hero>();

        road = HexMath.FindPath(agent.nowAgentState.onCell, hero.moveEndCell);
    }

    public override void Update()
    {
        HexCell nowCell = agent.nowAgentState.onCell;

        road.DisableRoadColor();
        road = HexMath.FindPath(nowCell, hero.moveEndCell);

        if (road == null)
        {
            return;
        }

        HexCell newCell = road.Pop();
        agent.nowAgentState.onCell = newCell;

        road.EnableRoadColor(hero.MoveColor, true);
    }

    public override void Exit()
    {
        //
    }

    public override Hero.HeroActions GetNextAction()
    {
        if (hero.moveEndCell == null || agent.nowAgentState.onCell == hero.moveEndCell)
        {
            road = HexMath.FindPath(agent.nowAgentState.onCell, hero.moveEndCell);
            road.DisableRoadColor();

            return Hero.HeroActions.inaction;
        }
            

        return Hero.HeroActions.walk;
    }

    public override void OnFrameUpdate()
    {
        //
    }

    public override Hero.HeroActions GetNextActionOnFrameUpdate()
    {
        bool isOverUI = EventSystem.current.IsPointerOverGameObject();

        if (!isOverUI && Input.GetMouseButtonDown(0))
        {
            HexCell cell = InputManager.GetCellUnderCursor();

            if (cell != null && cell == agent.nowAgentState.onCell)
            {
                road = HexMath.FindPath(agent.nowAgentState.onCell, hero.moveEndCell);
                road.DisableRoadColor();

                return Hero.HeroActions.inaction;
            }
        }

        return Hero.HeroActions.walk;
    }

    public AWalkHero(Hero.HeroActions key, Agent<Hero.HeroActions> nowAgent) : base(key, nowAgent)
    {
    }
}


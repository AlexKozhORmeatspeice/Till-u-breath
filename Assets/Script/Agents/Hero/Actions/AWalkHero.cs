using UnityEngine;
using UnityEngine.EventSystems;
public class AWalkHero : BaseAction<Hero.HeroActions>
{
    private Hero hero;
    private CellRoad road;

    public override void Start()
    {
        hero = agent.GetComponent<Hero>();
        hero.lastTimeMove = TimeManager.NowTime;
    }

    public override AgentState<Hero.HeroActions> Update()
    {
        HexCell nowCell = agent.NowAgentState.onCell;
        road = HexPathfinding.FindPath(nowCell, hero.moveEndCell);

        if (road == null)
        {
            return agent.NowAgentState;
        }

        HeroState newState = new HeroState(nowCell, Hero.HeroActions.walk, hero.lastTimeMove);

        HexCell newCell = road.Pop();
        int timeDist = HexPathfinding.GetTimeDist(nowCell, newCell);
        if (TimeManager.NowTime - hero.lastTimeMove >= timeDist)
        {
            newState.onCell = newCell;
            newState.lastMoveTime = TimeManager.NowTime;
        }

        return newState;
    }

    public override void Exit()
    {
        //
    }

    public override Hero.HeroActions GetNextAction()
    {
        if (hero.moveEndCell == null || agent.NowAgentState.onCell == hero.moveEndCell)
            return Hero.HeroActions.inaction;

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

            if (cell != null && cell == agent.NowAgentState.onCell) //choose hero
            {
                cell.EnableOutline(hero.StartColor);
                return Hero.HeroActions.chooseWalk;
            }
        }

        return Hero.HeroActions.walk;
    }

    public AWalkHero(Hero.HeroActions key, Agent<Hero.HeroActions> nowAgent) : base(key, nowAgent)
    {
    }
}


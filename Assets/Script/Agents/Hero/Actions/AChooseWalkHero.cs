using Script;
using Script.Agents.AgentsList.Supplies;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class AChooseWalkHero : BaseAction<Hero.HeroActions>
{
    private CellRoad nowRoad;
    private Hero hero;
    private AgentState<Hero.HeroActions> state;

    public override void Start()
    {
        hero = agent.GetComponent<Hero>();
        state = agent.NowAgentState;

        hero.moveEndCell = null;
    }
    public override void OnFrameUpdate()
    {
        ChooseRoad();
    }

    public override AgentState<Hero.HeroActions> Update()
    {
        return agent.NowAgentState;
    }

    public override void Exit()
    {
        //
    }

    public override Hero.HeroActions GetNextAction()
    {
        return Hero.HeroActions.chooseWalk;
    }
    public override Hero.HeroActions GetNextActionOnFrameUpdate()
    {
        bool isOverUI = EventSystem.current.IsPointerOverGameObject();
        if (!isOverUI && Input.GetMouseButtonDown(0))
        {
            HexCell cell = InputManager.GetCellUnderCursor();

            if (cell != null && cell == state.onCell) //choose hero
            {
                DisableRoad();
                return Hero.HeroActions.inaction;
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            DisableRoad();
            return Hero.HeroActions.inaction;
        }

        if (Input.GetKey(KeyCode.Space) && nowRoad != null)
        {
            state.onCell.DisableOutline();
            DisableRoadColor();

            return Hero.HeroActions.walk;
        }
        else if(Input.GetKey(KeyCode.Space))
        {
            DisableRoad();
            return Hero.HeroActions.inaction;
        }

        return Hero.HeroActions.chooseWalk;
    }

    private void ChooseRoad()
    {
        bool isOverUI = EventSystem.current.IsPointerOverGameObject();

        //enable road
        if (!isOverUI && Input.GetMouseButtonDown(0))
        {
            HexCell cell = InputManager.GetCellUnderCursor();

            if (cell != null && cell != state.onCell) //enable new road
            {
                if (hero.moveEndCell != null)
                {
                    DisableRoadColor();
                }

                CreateRoad(state.onCell, cell);
            }
        }
    }

    private void CreateRoad(HexCell fromCell, HexCell toCell)
    {
        hero.moveEndCell = toCell;
        nowRoad = HexPathfinding.FindPath(state.onCell, hero.moveEndCell);

        EnableRoadColor(hero.MoveColor);
        fromCell.EnableOutline(hero.StartColor);
        toCell.EnableOutline(hero.EndColor);
    }

    private void DisableRoad()
    {
        agent.NowAgentState.onCell.DisableOutline();

        if (nowRoad != null)
            DisableRoadColor();

        nowRoad = null;
        hero.moveEndCell = null;
    }

    private void EnableRoadColor(Color color)
    {
        if (nowRoad == null)
            return;

        CellRoad sameRoad = new CellRoad();
        int c = nowRoad.Count;
        for (int i = 0; i < c; i++)
        {
            HexCell cell = nowRoad.Pop();
            cell.EnableOutline(color);
            sameRoad.Push(cell);
        }

        nowRoad = sameRoad;
    }

    private void DisableRoadColor()
    {
        if (hero.moveEndCell != null)
            hero.moveEndCell.DisableOutline();
        if (nowRoad == null)
            return;

        CellRoad sameRoad = new CellRoad();
        int c = nowRoad.Count;
        for (int i = 0; i < c; i++)
        {
            HexCell cell = nowRoad.Pop();
            cell.DisableOutline();
            sameRoad.Push(cell);
        }

        nowRoad = sameRoad;
    }

    public AChooseWalkHero(Hero.HeroActions key, Agent<Hero.HeroActions> nowAgent) : base(key, nowAgent)
    {
    }
}
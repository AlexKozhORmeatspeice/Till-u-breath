using Script;
using Script.Agents.AgentsList.Supplies;
using System.Collections;
using Unity.VisualScripting;
using Unity.VisualScripting.ReorderableList;
using UnityEngine;
using UnityEngine.EventSystems;

public class AChooseWalkHero : BaseAction<Hero.HeroActions>
{
    private CellRoad nowRoad;
    private Hero hero;
    private AgentState<Hero.HeroActions> state;

    private bool clicked;
    private bool isDoubleClicked;
    private float lastClickTime;
    private float timeBetweenClicks = 0.2f;
    public override void Start()
    {
        hero = agent.GetComponent<Hero>();
        state = agent.nowAgentState;

        hero.moveEndCell = null;
        nowRoad = null;

        clicked = false;
        isDoubleClicked = false;
        lastClickTime = Time.time;
    }
    public override void OnFrameUpdate()
    {
        ChooseRoad();

        //check double click
        if (clicked && !isDoubleClicked)
        {
            if (Input.GetMouseButtonDown(0))
            {
                isDoubleClicked = true;
            }
        }

        if (Input.GetMouseButtonDown(0) && !isDoubleClicked)
        {
            lastClickTime = Time.time;
        }

        clicked = Time.time - lastClickTime < timeBetweenClicks;
        
        if(!clicked)
        {
            isDoubleClicked = false;
        }
    }

    public override void Update()
    {
        //    
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

            if (cell != null && cell.Unit != null) //choose hero
            {
                DisableRoad();
                return Hero.HeroActions.inaction;
            }

            if(nowRoad != null && cell == hero.moveEndCell && isDoubleClicked)
            {
                state.onCell.DisableOutline();
                hero.moveEndCell.DisableOutline();

                nowRoad.EnableRoadColor(hero.MoveColor, true);

                return Hero.HeroActions.walk;
            }
        }

        if (Input.GetMouseButtonDown(1)) //press on right mouse
        {
            DisableRoad();
            return Hero.HeroActions.inaction;
        }


        if(Input.GetKey(KeyCode.Space))
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
                CreateRoad(state.onCell, cell);
            }
        }
    }

    private void CreateRoad(HexCell fromCell, HexCell toCell, bool halfAlpha = false)
    {
        DisableRoad();

        hero.moveEndCell = toCell;
        nowRoad = HexMath.FindPath(state.onCell, hero.moveEndCell);

        if(nowRoad != null)
        {
            nowRoad.EnableRoadColor(hero.MoveColor, halfAlpha);
            toCell.EnableOutline(hero.EndColor, halfAlpha);
        }
        else
        {
            hero.moveEndCell = null;
        }
        
        fromCell.EnableOutline(hero.StartColor, halfAlpha);
    }

    private void DisableRoad()
    {
        agent.nowAgentState.onCell.DisableOutline();

        if (hero.moveEndCell != null)
        {
            hero.moveEndCell.DisableOutline();
        }
            
        if (nowRoad != null)
        {
            nowRoad.DisableRoadColor();
        }
           

        nowRoad = null;
        hero.moveEndCell = null;
    }

    public AChooseWalkHero(Hero.HeroActions key, Agent<Hero.HeroActions> nowAgent) : base(key, nowAgent)
    {
    }
}
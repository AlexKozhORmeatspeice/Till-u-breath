using Script;
using Script.Agents.AgentsList.Supplies;
using UnityEngine;
using UnityEngine.EventSystems;

public class AInactionHero : BaseAction<Hero.HeroActions>
{
    private Hero hero;
    private AgentState<Hero.HeroActions> state;

    public override void Start()
    {
        hero = agent.GetComponent<Hero>();
        state = hero.nowAgentState;
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
        return Hero.HeroActions.inaction;
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

            if (cell != null && cell == state.onCell) //choose hero
            {
                cell.EnableOutline(hero.StartColor);
                return Hero.HeroActions.chooseWalk;
            }
        }

        return Hero.HeroActions.inaction;
    }
    public AInactionHero(Hero.HeroActions key, Agent<Hero.HeroActions> nowAgent) : base(key, nowAgent)
    {
    }

}

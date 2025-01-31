using Script;
using UnityEngine;

class AWalkBoar : BaseAction<Boar.BoarActions>
{
    Boar boar;
    public override void Start()
    {
        boar = agent.gameObject.GetComponent<Boar>();
    }

    public override void Update()
    {
        HexDirection dir = (HexDirection)(Random.Range(0, 6));
        HexCell nowCell  = agent.nowAgentState.onCell;
        HexCell moveCell = agent.nowAgentState.onCell.GetNeighbor(dir);
        
        if(moveCell != null && moveCell.CellType == boar.CellMoveType)
        {
            agent.nowAgentState.onCell = moveCell;
        }
    }

    public override Boar.BoarActions GetNextAction()
    {
        if(agent.nowAgentState.Energy <= 50 || agent.nowAgentState.HP < 100)
        {
            return Boar.BoarActions.searchFood;
        }

        return agent.nowAgentState.actionState;
    }

    public override Boar.BoarActions GetNextActionOnFrameUpdate()
    {
        return agent.nowAgentState.actionState;
    }

    public override void OnFrameUpdate()
    {
        //
    }

    public override void Exit()
    {
        //
    }

    public AWalkBoar(Boar.BoarActions key, Agent<Boar.BoarActions> nowAgent) : base(key, nowAgent)
    {
    }
}


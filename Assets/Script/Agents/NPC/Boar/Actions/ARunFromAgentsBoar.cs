using Script;
using UnityEngine;

class ARunFromAgentsBoar : BaseAction<Boar.BoarActions>
{
    Boar boar;
    CellRoad road;
    public override void Start()
    {
        road = null;
        boar = agent.gameObject.GetComponent<Boar>();
    }

    public override void Update()
    {
        Vector3 runawayDir = boar.SeeAgents.GetRunawayDir();
        if (runawayDir != Vector3.zero)
        {
            HexCell nowCell = agent.nowAgentState.onCell;

            road = HexMath.FindPath(nowCell, runawayDir, boar.CellsToMoveBitmask.Inverse());

            if (road == null)
            {
                Debug.Log(1);
                return;
            }

            HexCell newCell = road.Pop();
            agent.nowAgentState.onCell = newCell;
        }
    }

    public override Boar.BoarActions GetNextAction()
    {
        if(!boar.SeeAgents.IsSeeAgents())
        {
            return Boar.BoarActions.walk;
        }

        return Boar.BoarActions.runFromAgents;
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

    private HexCell GetValidMoveCell(HexDirection dir)
    {
        HexCell moveCell = agent.nowAgentState.onCell.GetNeighbor(dir);
        /*HexCell moveCellNext = agent.nowAgentState.onCell.GetNeighbor(dir.Next());
        HexCell moveCellPrev = agent.nowAgentState.onCell.GetNeighbor(dir.Previous());

        if (moveCell == null || moveCell.CellType != boar.CellMoveType)
        {
            if(moveCellNext != null && moveCellNext.CellType == boar.CellMoveType)
            {
                return moveCellNext;
            }

            if(moveCellPrev != null && moveCellPrev.CellType == boar.CellMoveType)
            {
                return moveCellPrev;
            }

            return null;
        }
*/
        return moveCell;
    }
    public ARunFromAgentsBoar(Boar.BoarActions key, Agent<Boar.BoarActions> nowAgent) : base(key, nowAgent)
    {
    }
}


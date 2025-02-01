using Script;
using UnityEngine;

class ADigForFoodBoar : BaseAction<Boar.BoarActions>
{
    Boar boar;
    bool eatenFood;
    public override void Start()
    {
        eatenFood = false;
        boar = agent.GetComponent<Boar>();
        Debug.Log("Digging");
    }

    public override void Update()
    {
        boar.searchFood = GetFoodByDigging();

        if(boar.searchFood == null)
        {
            NextCell();
        }
        else
        {
            eatenFood = true;
            boar.searchFood.Use(agent);
            boar.searchFood = null;
        }
    }

    public override Boar.BoarActions GetNextAction()
    {
        if(eatenFood)
        {
            return Boar.BoarActions.findFood;
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

    private Food GetFoodByDigging()
    {
        float chance = Random.Range(0.0f, 1.0f);
        if(chance > boar.ChanceToFindFood)
        {
            Food food1 = Pooler.Instance.SpawnPoolObject(Helper.GetRandElemInList(Pooler.foodObjNames),
                                                        Vector3.zero, 
                                                        Quaternion.identity)
                                       .GetComponent<Food>();

            Food food2 = Pooler.Instance.SpawnPoolObject(Helper.GetRandElemInList(Pooler.foodObjNames),
                                                        Vector3.zero,
                                                        Quaternion.identity)
                                       .GetComponent<Food>();
            
            food2.Drop(agent.nowAgentState.onCell);

            return food1;
        }

        return null;
    }

    private void NextCell()
    {
        HexDirection dir = (HexDirection)(Random.Range(0, 6));
        HexCell nowCell = agent.nowAgentState.onCell;
        HexCell moveCell = agent.nowAgentState.onCell.GetNeighbor(dir);

        if (moveCell != null && moveCell.CellType == boar.CellMoveType)
        {
            agent.nowAgentState.onCell = moveCell;
        }
    }

    public ADigForFoodBoar(Boar.BoarActions key, Agent<Boar.BoarActions> nowAgent) : base(key, nowAgent)
    {
    }
}


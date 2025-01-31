using Script;
using UnityEngine;

class ADigForFoodBoar : BaseAction<Boar.BoarActions>
{
    Boar boar;
    bool getFood;
    public override void Start()
    {
        getFood = false;
        Debug.Log("Digging");
    }

    public override void Update()
    {
        getFood = GetFoodByDigging();

        if(!getFood)
        {
            NextCell();
        }
        else
        {

        }
    }

    public override Boar.BoarActions GetNextAction()
    {
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

    private bool GetFoodByDigging()
    {

        return true;
    }

    private void NextCell()
    {

    }

    public ADigForFoodBoar(Boar.BoarActions key, Agent<Boar.BoarActions> nowAgent) : base(key, nowAgent)
    {
    }
}


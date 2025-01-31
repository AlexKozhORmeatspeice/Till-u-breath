using Script;
using UnityEngine;

class AWalkForFoodBoar : BaseAction<Boar.BoarActions>
{
    Boar boar;

    public override void Start()
    {
        boar = agent.gameObject.GetComponent<Boar>();

        Debug.Log("Walking for food");
    }

    public override void Update()
    {
        //
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

    public AWalkForFoodBoar(Boar.BoarActions key, Agent<Boar.BoarActions> nowAgent) : base(key, nowAgent)
    {
    }
}


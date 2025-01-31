using System;
using UnityEngine;

public abstract class BaseAction<AgentAction> where AgentAction : Enum
{
      protected Agent<AgentAction> agent;
      public BaseAction(AgentAction key, Agent<AgentAction> nowAgent)
      {
            StateKey = key;
            agent = nowAgent;
      }

      public AgentAction StateKey
      {
            get;
            private set;
      }
      
      protected bool isActive;
      
      public abstract void Start(); 
      public abstract void Update(); //to work with Agent use: agent.nowAgentState

      public abstract void Exit();

      public abstract AgentAction GetNextAction();

      public abstract AgentAction GetNextActionOnFrameUpdate();

      public abstract void OnFrameUpdate();
}

using System;
using UnityEngine;

public abstract class BaseAction<AState> where AState : Enum
{
      protected Agent<AState> agent;
      public BaseAction(AState key, Agent<AState> nowAgent)
      {
            StateKey = key;
            agent = nowAgent;
      }

      public AState StateKey
      {
            get;
            private set;
      }
      
      protected bool isActive;
      
      public abstract void Start(); 
      public abstract AgentState<AState> Update();

      public abstract void Exit();

      public abstract AState GetNextState();
}

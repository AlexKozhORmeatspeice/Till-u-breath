using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoctorState : AgentState<Doctor.DoctorActions>
{
    public DoctorState(HexCell cell, Doctor.DoctorActions state, int _lastMoveTime) : base(cell, state)
    {
        lastMoveTime = _lastMoveTime;
    }
}

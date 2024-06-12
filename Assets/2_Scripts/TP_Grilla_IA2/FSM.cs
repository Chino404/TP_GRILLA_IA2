using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM <T>
{
    private State _actualState;
    private Dictionary<T, State> _states = new Dictionary<T, State>();

    public T ActualState { get; private set; }

    public void CreateState(T elem, State state) => _states[elem] = state;
  
    public void ChangeState(T elem)
    {
        if (!_states.ContainsKey(elem)) return;

        _actualState?.Exit();
        _actualState = _states[elem];
        ActualState = elem;
        _actualState.Enter();

    }

    public void Update() => _actualState?.Update();
    public void LateUpdate() => _actualState?.LateUpdate();
    public void FixedUpdate() => _actualState?.FixedUpdate();
}

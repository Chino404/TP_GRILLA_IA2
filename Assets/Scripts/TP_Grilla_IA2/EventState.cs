using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EventState : State
{
    public override void Enter() => OnEnter();

    public override void Update() => OnUpdate();

    public override void LateUpdate() => OnLateUpdate();

    public override void FixedUpdate() => OnFixedUpdate();

    public override void Exit() => OnExit();


    public Action OnEnter = delegate { };
    public Action OnUpdate = delegate { };
    public Action OnLateUpdate = delegate { };
    public Action OnFixedUpdate = delegate { };
    public Action OnExit = delegate { };
}

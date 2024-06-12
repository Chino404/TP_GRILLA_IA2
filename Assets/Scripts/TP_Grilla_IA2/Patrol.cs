using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public class Patrol :  IState
//{
//    FSM _fsm;
//    Hunter _hunter;
//    Transform[] _wayPoints;
//    float _counter;
    

//    public Patrol(FSM fsm, Transform[] waypoints, Hunter hunter)
//    {
//        _fsm = fsm;
//        _wayPoints = waypoints;
//        _hunter = hunter;
//    } 

//    public void OnEnter()
//    {
//        _counter = _hunter.counter;
//        Debug.Log("enter patrol");
//    }

//    public void OnUpdate()
//    {
//        AddForce(Seek(_wayPoints[_actualIndex].position));

//        if (Vector3.Distance(_hunter.gameObject.transform.position, _wayPoints[_actualIndex].position) <= 0.3f)
//        {
//            _actualIndex++;
//            if (_actualIndex >= _wayPoints.Length)
//                _actualIndex = 0;
//        }

        
//        _hunter.gameObject.transform.position += _hunter._velocity * Time.deltaTime;
//        _hunter.gameObject.transform.forward = _hunter._velocity;
//        _counter -= Time.deltaTime;

//        if (_counter <= 0)
//            _fsm.ChangeState("Idle");

//        foreach (var item in GameManager.Instance.boids)
//        {
//            if (Vector3.Distance(item.transform.position , _hunter.transform.position) <= _hunter.viewRadius)
//                _fsm.ChangeState("Chase");
//        }

//    }

//    public void OnExit()
//    {
//        Debug.Log("exit patrol");

//    }

    
//}

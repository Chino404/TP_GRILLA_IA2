using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hunter : MonoBehaviour
{
    private enum States { Idle, Patrol, Chase }
    private FSM<States> _fsm;
    public Hunter hunter;

    private Vector3 _velocity; //Lo hice publico para que pueda modificarlo en el Patrol y poder pedirlo en el Boid

    public float viewRadius;

    public float maxVelocity;
    public float maxForce;

    public float counterIdle;
    public float counter;

    [SerializeField]private Transform[] _wayPoints;
    public Boid[] target;

    private int _actualIndex;


    private void Awake()
    {
        SetUpFSM();
    }

    void SetUpFSM()
    {
        var idle = new EventState();
        idle.OnEnter = () => counter = counterIdle;
        idle.OnUpdate = () =>
        {
              counter -= Time.deltaTime;
              if (counter <= 0)
              {
                  _fsm.ChangeState(States.Patrol);
              }
        };

        var patrol = new EventState();
        patrol.OnUpdate = () =>
        {
              AddForce(Seek(_wayPoints[_actualIndex].position));

              if (Vector3.Distance(transform.position, _wayPoints[_actualIndex].position) <= 0.3f)
              {
                  _actualIndex++;
                  if (_actualIndex >= _wayPoints.Length)
                      _actualIndex = 0;
              }

              transform.position += _velocity * Time.deltaTime;
              transform.forward = _velocity;
              counter -= Time.deltaTime;

              if (counter <= 0)
                  _fsm.ChangeState(States.Idle);

              foreach (var item in GameManager.Instance.boids)
              {
                  if (Vector3.Distance(item.transform.position, transform.position) <= viewRadius)
                      _fsm.ChangeState(States.Chase);
              }
        };




        _fsm = new FSM<States>();
        _fsm.CreateState(States.Idle, idle);
        _fsm.CreateState(States.Patrol, patrol);
        _fsm.ChangeState(States.Idle);
    }

    private void Update()
    {
        _fsm.Update();
    }

    Vector3 Seek(Vector3 target)
    {
        var desired = target - transform.position;
        desired.Normalize();
        desired *= maxVelocity;

        var steering = desired - _velocity;
        steering = Vector3.ClampMagnitude(steering, maxForce);

        return steering;
    }

    public void AddForce(Vector3 dir)
    {
        _velocity += dir;

        _velocity = Vector3.ClampMagnitude(_velocity, maxVelocity);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);
    }
}

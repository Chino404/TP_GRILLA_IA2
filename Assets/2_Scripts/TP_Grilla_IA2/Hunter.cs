using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hunter : MonoBehaviour
{
    private enum States { Idle, Patrol, Chase }
    private FSM<States> _fsm;
    public Hunter hunter;
    public Bomb bombita;
    [HideInInspector] public Vector3 velocity; //Lo hice publico para que pueda modificarlo en el Patrol y poder pedirlo en el Boid

    public float viewRadius;

    public float maxVelocity;
    public float maxForce;

    //remplassar el esto por mana
    public float counterIdle;
    public float counterPatrolChase;
    //public float counterChase;
    public float counter;

    [SerializeField] private Transform[] _wayPoints;


    public int _actualIndex;

    private float _closestDistance = Mathf.Infinity;
    public Boid _currentTarget;

    private void Awake()
    {
        if (!GameManager.Instance.hunter) GameManager.Instance.hunter = this;
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
                counter = counterPatrolChase;
            }
        };

        var patrol = new EventState();
        patrol.OnEnter = () => /*counter=counterPatrolChase*/ Debug.Log("entre a patrol");
        patrol.OnUpdate = () =>
        {
            AddForce(Seek(_wayPoints[_actualIndex].position));

            if (Vector3.Distance(transform.position, _wayPoints[_actualIndex].position) <= 0.3f)
            {
                _actualIndex++;
                if (_actualIndex >= _wayPoints.Length)
                    _actualIndex = 0;
            }

            transform.position += velocity * Time.deltaTime;
            transform.forward = velocity;
            counter -= Time.deltaTime;

            if (counter <= 0)
                _fsm.ChangeState(States.Idle);
            var entities = bombita.Query().ToFList();

            foreach (var item in entities)
            {
                if (Vector3.Distance(item.transform.position, transform.position) <= bombita.radius)
                    _fsm.ChangeState(States.Chase);
            }
        };

        var chase = new EventState();
        chase.OnEnter = () =>
        {
            Debug.Log("entro a chase");
            //    counter = counterChase;
        };

        chase.OnUpdate = () =>
        {
            var entities = bombita.Query().ToFList();
            foreach (Boid target in entities)
            {
                float _distance = Vector3.Distance(transform.position, target.transform.position);

                if (_distance < _closestDistance)
                {
                    _closestDistance = _distance;
                    _currentTarget = target;
                }
            }

            if (_currentTarget != null)
            {
                AddForce(Pursuit(_currentTarget.transform.position + _currentTarget.Velocity));

                transform.position += velocity * Time.deltaTime;
                gameObject.transform.forward = velocity;
                counter -= Time.deltaTime;

                if (Vector3.Distance(transform.position, _currentTarget.transform.position) < bombita.radius)
                {
                    StartCoroutine(ExplodeAfterDelay());

                }
            }

            else { _fsm.ChangeState(States.Patrol); }
            if (counter <= 0) _fsm.ChangeState(States.Idle);
            if (_currentTarget != null) if (Vector3.Distance(transform.position, _currentTarget.transform.position) > bombita.radius) { _fsm.ChangeState(States.Patrol); }
        };

        _fsm = new FSM<States>();
        _fsm.CreateState(States.Idle, idle);
        _fsm.CreateState(States.Patrol, patrol);
        _fsm.CreateState(States.Chase, chase);
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

        var steering = desired - velocity;
        steering = Vector3.ClampMagnitude(steering, maxForce);

        return steering;
    }

    Vector3 Pursuit(Vector3 target)
    {
        return Seek(target);
    }

    public void AddForce(Vector3 dir)
    {
        velocity += dir;

        velocity = Vector3.ClampMagnitude(velocity, maxVelocity);
    }


    private IEnumerator ExplodeAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);
        if (bombita != null)
        {
            bombita.Explode(); // Llama al método Explode de la bomba existente
        }
        else
        {
            Debug.LogError("No bomb reference set on the Hunter.");
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);
    }
}

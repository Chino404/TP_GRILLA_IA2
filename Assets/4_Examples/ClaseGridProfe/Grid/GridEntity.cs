using System;
using UnityEngine;

//[ExecuteInEditMode]
public class GridEntity : MonoBehaviour
{
	public event Action<GridEntity> OnMove = delegate {};
	public event Action<GridEntity> OnDestroyEvent = delegate {};
    protected Vector3 _velocity;

    public Vector3 Velocity
    {
        get { return _velocity; }
    }

    public bool onGrid;
    Renderer _rend;

    private void Awake()
    {
        _rend = GetComponent<Renderer>();
    }

   public virtual  void Update() {
        if (onGrid)
            _rend.material.color = Color.red;
        else
            _rend.material.color = Color.blue;

		//Optimization: Hacer esto solo cuando realmente se mueve y no en el update
		//transform.position += velocity * Time.deltaTime;
	    OnMove(this);
	}

    public void OnDestroy()
    {
        SpatialGrid.Instance.boidsList.Remove(this);
        OnDestroyEvent(this);
        Debug.Log("Me destrui");

    }


}

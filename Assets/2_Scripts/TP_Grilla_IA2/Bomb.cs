using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    //PARA BUSCAR LOS AGENTES EN LA GRILLA, NECESITO UNA FUNCION QUE SEA "Query"

    [SerializeField] private SpatialGrid _targetGrid;
    [SerializeField] private float _radius;
    [SerializeField] private float _timer;

    private void Start()
    {
        _targetGrid = SpatialGrid.Instance;
    }



    public void Explode()
    {
        var entities = Query().ToFList(); 
        if (entities != null)
            DestroyGridEntities(entities);
    }

    void DestroyGridEntities(IEnumerable<GridEntity> entities)
    {
       foreach (var item in entities)
        {
            //item.OnDestroy();
            Destroy(item.gameObject); //Destruyo el GameObject
        }
  
    }

    public IEnumerable<GridEntity> Query()
    {

        //creo una "caja" con las dimensiones deseadas, y luego filtro segun distancia para formar el c�rculo
        return _targetGrid.Query(
            transform.position + new Vector3(-_radius, 0, -_radius),
            transform.position + new Vector3(_radius, 0, _radius),
            x => {
                var position2d = x - transform.position;
                position2d.y = 0;
                return position2d.sqrMagnitude < _radius * _radius;
            });
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _radius);
    }
}

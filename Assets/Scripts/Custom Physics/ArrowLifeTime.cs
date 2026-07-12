using CustomPhysics;
using System.Collections;
using UnityEngine;

public class ArrowLifeTime : MonoBehaviour
{
    [SerializeField] private GameObject particle;

    PhysicsObject arrowPhys;

    private bool canSpawn = true;
    private void Start()
    {
        arrowPhys = GetComponent<PhysicsObject>();
    }
    private void Update()
    {
        if (arrowPhys.hasCollided && arrowPhys.interactedObject != null && canSpawn)
        {
            Debug.Log(arrowPhys);
            StartCoroutine(SpawnParticle());
        }
    }

    private IEnumerator SpawnParticle()
    {
        canSpawn = false;
        Instantiate(particle, transform.position, Quaternion.identity).transform.localEulerAngles = new Vector3(0,0, arrowPhys.rotation);
        yield return new WaitForSeconds(0.1f);
        canSpawn = true;
    }

}

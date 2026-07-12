using CustomPhysics;
using System.Collections;
using UnityEngine;

public class ArrowLifeTime : MonoBehaviour
{
    private void Update()
    {

    }

    void OnBecameVisible()
    {
        StopAllCoroutines();
    }

    IEnumerator OnBecameInvisible()
    {
        yield return new WaitForSeconds(2);
        GetComponent<PhysicsObject>().DestroyObject();
    }
}

using CustomPhysics;
using System.Collections;
using UnityEngine;

public class ArrowLifeTime : MonoBehaviour
{

    void OnBecameVisible()
    {
        StopAllCoroutines();
    }

    IEnumerator OnBecameInvisible()
    {
        yield return new WaitForSeconds(2);
        GetComponent<PhysicsObject>().DestroyObject();
    }

    private void OnDestroy()
    {
        GetComponent<SpriteRenderer>().enabled = false;
    }
}

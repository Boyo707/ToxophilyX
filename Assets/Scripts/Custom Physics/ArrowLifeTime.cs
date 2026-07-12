using CustomPhysics;
using System.Collections;
using UnityEngine;

public class ArrowLifeTime : MonoBehaviour
{

    private void OnDestroy()
    {
        GetComponent<SpriteRenderer>().enabled = false;
    }
}

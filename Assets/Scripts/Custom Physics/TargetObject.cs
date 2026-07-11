using UnityEngine;
using CustomPhysics;


public class TargetObject : MonoBehaviour
{
    PhysicsObject objPhysics;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        objPhysics = GetComponent<PhysicsObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (objPhysics.hasTriggered)
        {
            Debug.Log("HIT!");
        }
    }
}

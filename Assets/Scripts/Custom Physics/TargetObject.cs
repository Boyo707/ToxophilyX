using UnityEngine;
using CustomPhysics;


public class TargetObject : MonoBehaviour
{
    PhysicsObject objPhysics;

    private bool isHit = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        objPhysics = GetComponent<PhysicsObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (objPhysics.hasTriggered && !isHit)
        {
            isHit = true;
            objPhysics.interactedObject.SetVelocity(Vector3.zero);
        }
    }
}

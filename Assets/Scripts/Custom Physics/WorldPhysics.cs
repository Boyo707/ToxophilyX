using System.Collections.Generic;
using UnityEngine;

public class WorldPhysics : MonoBehaviour
{
    [SerializeField] private float gravitationalForce = 9.81f;
    [SerializeField] private float gravityScalar = 200;
    [SerializeField] private float velocityScalar = 1;

    public static WorldPhysics instance;

    public List<PhysicsObject> physObjs = new();

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (gravityScalar == 0)
        {
            Debug.LogError("Cant have the gravityScalar at 0");
        }
        if(velocityScalar == 0)
        {
            Debug.LogError("Cant have the velocityScalar at 0");
        }
    }
    private void FixedUpdate()
    {        
        for (int i = 0; i < physObjs.Count; i++)
        {
            PhysicsObject currentObj = physObjs[i];

            currentObj.Position[0] += currentObj.Velocity[0] / velocityScalar;
            currentObj.Position[1] += currentObj.Velocity[1] / velocityScalar;
            currentObj.Velocity[1] += currentObj.Mass * -gravitationalForce / gravityScalar * currentObj.GravityMultiplier;

            if (physObjs[i].hasCollided)
            {
            }

            for (int j = 0; j < physObjs.Count; j++)
            {
                PhysicsObject otherObj = physObjs[j];
                if (currentObj == otherObj || currentObj.vecVelocity() == Vector2.zero) continue;

                if(DotProductLineSphere(currentObj, otherObj) < 0)
                {
                    currentObj.Velocity[1] *= -0.95f;
                }
                
            }

            currentObj.ApplyPhysics();
        }
    }

    private float DotProductLineSphere(PhysicsObject currentObj, PhysicsObject otherObj)
    {
        Vector3 displacement = currentObj.vecPosition() - otherObj.vecPosition();
        Vector3 projection = Vector3.Project(displacement, otherObj.GetLineNormal());
        return Vector3.Dot(otherObj.GetLineNormal(), projection);
    }

    private void IdentifyCollision(PhysicsObject currentObj, PhysicsObject otherObj)
    {
        //identify collision
        switch (currentObj.ColliderShape)
        {
            case ColliderType.Circle:
                break;
            case ColliderType.Line:
                break;
            case ColliderType.Square:
                break;
        }
    }

    public void AssignPhysicsObject(PhysicsObject physObj)
    {
        physObjs.Add(physObj);
    }
}

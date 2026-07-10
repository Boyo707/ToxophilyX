using System.Collections.Generic;
using UnityEngine;

namespace CustomPhysics
{
    public class WorldPhysics : MonoBehaviour
    {
        [SerializeField] private float gravitationalForce = 9.81f;
        [SerializeField] private float gravityScalar = 200;
        [SerializeField] private float velocityScalar = 1;
        [SerializeField] private float collisionSkin = 0.001f;

        public static WorldPhysics instance;

        public List<PhysicsObject> physObjs = new();

        private void Awake()
        {
            if (instance != null)
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
            if (velocityScalar == 0)
            {
                Debug.LogError("Cant have the velocityScalar at 0");
            }
        }
        private void FixedUpdate()
        {
            for (int i = 0; i < physObjs.Count; i++)
            {
                PhysicsObject currentObj = physObjs[i];

                float newGravity = gravitationalForce;
                //currentObj.Velocity[1] += currentObj.Mass * -gravitationalForce / gravityScalar;
                if (currentObj.Mass == 0) continue;
                currentObj.Velocity[1] += -(newGravity * Time.fixedDeltaTime);

                currentObj.Position[0] += currentObj.Velocity[0] * Time.fixedDeltaTime;
                currentObj.Position[1] += currentObj.Velocity[1] * Time.fixedDeltaTime;

                for (int j = 0; j < physObjs.Count; j++)
                {
                    PhysicsObject otherObj = physObjs[j];
                    if (currentObj == otherObj || currentObj.vecVelocity() == Vector2.zero) continue;

                    if (DotProductLineSphere(currentObj, otherObj) < 0)
                    {
                        NewVelocity(currentObj, otherObj);
                    }

                }

                currentObj.ApplyPhysics();
            }
        }

        private float DotProductLineSphere(PhysicsObject currentObj, PhysicsObject otherObj)
        {
            Vector3 displacement = currentObj.vecPosition() - otherObj.vecPosition();
            Vector3 projection = Vector3.Project(displacement, otherObj.GetLineNormal());
            return Vector3.Dot(displacement, otherObj.GetLineNormal()) - currentObj.Radius;
        }

        private void NewVelocity(PhysicsObject currentObj, PhysicsObject otherObj)
        {
            Vector3 thisVelocity = currentObj.vecVelocity();
            Vector3 normal = otherObj.GetLineNormal();
            float normalVelocityDot = Vector2.Dot(thisVelocity, normal);
            float magnitude = thisVelocity.magnitude;

            Debug.Log(magnitude);

            //add a check if its going down hill 
            if (magnitude < 0.66f)
            {
                //start resting
                float dot = DotProductLineSphere(currentObj, otherObj);
                Vector2 diff = normal * -dot;

                currentObj.Position[0] += diff.x;
                currentObj.Position[1] += diff.y;

                // Stop the bounce.
                currentObj.Velocity[0] = 0;
                currentObj.Velocity[1] = 0;
            }
            else
            {
                //bounce

                //place above collision line to prevent clipping
                float projDot = DotProductLineSphere(currentObj, otherObj);

                Vector3 diff = otherObj.GetLineNormal() * (-projDot + collisionSkin);

                Debug.Log(diff.y);

                currentObj.Position[0] += diff.x;
                currentObj.Position[1] += diff.y;


                //bounce code
                Vector3 normalVelocity = normalVelocityDot * normal;

                Vector3 tangentVelocity = thisVelocity - normalVelocity;

                normalVelocity = -normalVelocity * currentObj.Restitution;

                tangentVelocity *= (1f - currentObj.Friction);

                Vector3 reflectedVelocity = normalVelocity + tangentVelocity;

                Debug.Log("Starting velocity: " + thisVelocity + " reflected: " + reflectedVelocity);
                currentObj.Velocity[0] = reflectedVelocity.x;
                currentObj.Velocity[1] = reflectedVelocity.y;
            }
            
            
        }
        public void AssignPhysicsObject(PhysicsObject physObj)
        {
            physObjs.Add(physObj);
        }
    }
}
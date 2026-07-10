using System.Collections.Generic;
using UnityEngine;

namespace CustomPhysics
{
    public class WorldPhysics : MonoBehaviour
    {
        [SerializeField] private float gravitationalForce = 9.81f;
        [SerializeField] private Vector3 worldGravitDir = Vector3.down;
        [SerializeField] private float velocityScalar = 1;
        [SerializeField] private float collisionSkin = 0.1f;

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

            if (velocityScalar == 0)
            {
                Debug.LogError("Cant have the velocityScalar at 0");
            }
            if (worldGravitDir.x > 1 || worldGravitDir.y > 1)
            {
                Debug.LogError("Keep gravity direction values between 0 and 1");
            }
        }

        private void FixedUpdate()
        {
            for (int i = 0; i < physObjs.Count; i++)
            {
                PhysicsObject currentObj = physObjs[i];

                if (currentObj.HasGravity)
                {
                    Vector3 gravityDir = currentObj.LocalGravityDirection == worldGravitDir ? worldGravitDir : currentObj.LocalGravityDirection;

                    currentObj.velocity += (gravityDir * gravitationalForce * currentObj.GravityMultiplier * Time.fixedDeltaTime);
                }


                currentObj.physPosition += currentObj.velocity * Time.fixedDeltaTime;


                //Zorgh er voor dat de planeet mischien beweegt. maar dat andere objecten aangetrokken zijn.
                //maak een range circle dat bepaald of een object attracted kan worden of niet

                for (int j = 0; j < physObjs.Count; j++)
                {
                    PhysicsObject otherObj = physObjs[j];
                    if (currentObj == otherObj) continue;

                    if (currentObj.velocity == Vector3.zero) continue;

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
            Vector3 displacement = currentObj.physPosition - otherObj.physPosition;
            Vector3 projection = Vector3.Project(displacement, otherObj.GetLineNormal());
            return Vector3.Dot(displacement, otherObj.GetLineNormal()) - currentObj.Radius;
        }

        private void NewVelocity(PhysicsObject currentObj, PhysicsObject otherObj)
        {
            Vector3 thisVelocity = currentObj.velocity;
            Vector3 normal = otherObj.GetLineNormal();
            float normalVelocityDot = Vector2.Dot(thisVelocity, normal);
            float magnitude = thisVelocity.magnitude;

            //add a check if its going down hill 
            if (magnitude < 0.66f)
            {
                //start resting
                float dot = DotProductLineSphere(currentObj, otherObj);
                Vector3 diff = normal * -dot;

                currentObj.physPosition += diff;

                // Stop the bounce.
                currentObj.velocity = Vector3.zero;
            }
            else
            {
                //bounce

                //place above collision line to prevent clipping
                float projDot = DotProductLineSphere(currentObj, otherObj);

                Vector3 diff = otherObj.GetLineNormal() * (-projDot + collisionSkin);

                currentObj.physPosition += diff;

                //bounce code
                Vector3 normalVelocity = normalVelocityDot * normal;

                Vector3 tangentVelocity = thisVelocity - normalVelocity;

                normalVelocity = -normalVelocity * currentObj.Restitution;

                tangentVelocity *= (1f - currentObj.Friction);

                Vector3 reflectedVelocity = normalVelocity + tangentVelocity;

                Debug.Log("Starting velocity: " + thisVelocity + " reflected: " + reflectedVelocity);
                currentObj.velocity = reflectedVelocity;
            }
            
            
        }
        public void AssignPhysicsObject(PhysicsObject physObj)
        {
            physObjs.Add(physObj);
        }
    }
}
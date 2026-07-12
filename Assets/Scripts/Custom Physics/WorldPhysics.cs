using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR.Haptics;

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

                if (currentObj.HasGravity && currentObj.HasPhysics)
                {
                    Vector3 gravityDir = currentObj.LocalGravityDirection == worldGravitDir ? worldGravitDir : currentObj.LocalGravityDirection;

                    currentObj.velocity += (gravityDir * gravitationalForce * currentObj.GravityMultiplier * Time.fixedDeltaTime);
                }

                currentObj.physPosition += currentObj.velocity * Time.fixedDeltaTime;


                //maak een range circle dat bepaald of een object attracted kan worden of niet

                for (int j = 0; j < physObjs.Count; j++)
                {
                    PhysicsObject otherObj = physObjs[j];
                    if (currentObj == otherObj) continue;

                    if (otherObj.IsPlanet && currentObj.HasPhysics)
                    {
                        PlanetPhysics(currentObj, otherObj);
                    }

                    if (currentObj.velocity == Vector3.zero) continue;

                    VerifyCollision(currentObj, otherObj);

                }
                
                currentObj.ApplyPhysics();
                if (currentObj.isGettingDestroyed)
                {
                    physObjs.Remove(currentObj);
                    Destroy(currentObj.gameObject);
                }
            }
        }

        private void VerifyCollision(PhysicsObject current, PhysicsObject other)
        {
            int collisionIndex = (int)current.ColliderShape + (int)other.ColliderShape;
            if (collisionIndex == 2)
            {
                if(SpheresInRange(current, other))
                {
                    
                    if (current.IsTrigger)
                    {
                        current.hasTriggered = true;
                    }
                    else if (other.IsTrigger)
                    {
                        other.hasTriggered = true;
                    }
                    else
                    {
                        current.hasCollided = true;
                        Vector3 normal = (current.physPosition - other.physPosition).normalized;
                        float distanceOffset = current.Radius + other.Radius;
                        BounceOfCollider(current, other, normal, distanceOffset);
                    }
                }
                else
                {
                    if (current.hasTriggered) current.hasTriggered = false;
                    if (current.hasCollided) current.hasCollided = false;
                    if (other.hasTriggered) other.hasTriggered = false;
                    if (other.hasCollided) other.hasCollided = false;
                }
            }
            else if (collisionIndex == 3)
            {
                //sphere + line

                //check which object is which shape.
                PhysicsObject sphere = (int) current.ColliderShape == 1 ? current : other;
                PhysicsObject line = (int)other.ColliderShape == 1 ? current : other;

                if(ProjectionDot(sphere, line, line.GetLineNormal(), sphere.Radius) < 0)
                {
                    if (current.IsTrigger)
                    {
                        current.hasTriggered = true;
                    }
                    else if (other.IsTrigger)
                    {
                        other.hasTriggered = true;
                    }
                    else
                    {
                        current.hasCollided = true;
                        BounceOfCollider(current, other, line.GetLineNormal(), sphere.Radius);
                    }
                }
                else
                {
                    if (current.hasTriggered) current.hasTriggered = false;
                    if (current.hasCollided) current.hasCollided = false;
                    if (other.hasTriggered) other.hasTriggered = false;
                    if (other.hasCollided) other.hasCollided = false;
                }
                
            }
            else if (collisionIndex == 4)
            {
                //Line + line
            }
            else if (collisionIndex == 8)
            {
                //square + square
            }
        }

        private bool SpheresInRange(PhysicsObject current, PhysicsObject other)
        {
            return Vector3.Distance(current.physPosition, other.physPosition) - current.Radius - other.Radius <= 0;
        }

        private float ProjectionDot(PhysicsObject currentObj, PhysicsObject otherObj, Vector3 targetNormal, float someDistance)
        {
            Vector3 displacement = currentObj.physPosition - otherObj.physPosition;
            Vector3 projection = Vector3.Project(displacement, targetNormal);
            return Vector3.Dot(displacement, targetNormal) - someDistance;
        }

        private void BounceOfCollider(PhysicsObject currentObj, PhysicsObject otherObj, Vector3 targetNormal, float someDistance)
        {
            Vector3 thisVelocity = currentObj.velocity;
            Vector3 normal = targetNormal;
            float normalVelocityDot = Vector2.Dot(thisVelocity, normal);
            float magnitude = thisVelocity.magnitude;

            //add a check if its going down hill 
            if (magnitude < 0.66f)
            {
                //start resting
                float dot = ProjectionDot(currentObj, otherObj, targetNormal, someDistance);
                Vector3 diff = normal * -dot;

                currentObj.physPosition += diff;

                // Stop the bounce.
                currentObj.velocity = Vector3.zero;
            }
            else
            {
                //bounce

                //place above collision line to prevent clipping
                float projDot = ProjectionDot(currentObj, otherObj, targetNormal, someDistance);

                Vector3 diff = normal * (-projDot + collisionSkin);

                currentObj.physPosition += diff;

                //bounce code
                Vector3 normalVelocity = normalVelocityDot * normal;

                Vector3 tangentVelocity = thisVelocity - normalVelocity;

                normalVelocity = -normalVelocity * currentObj.Restitution;

                tangentVelocity *= (1f - currentObj.Friction);

                Vector3 reflectedVelocity = normalVelocity + tangentVelocity;

                currentObj.velocity = reflectedVelocity;
            }
        }

        private void PlanetPhysics(PhysicsObject currentObj, PhysicsObject otherObj)
        {
            if (Vector3.Distance(otherObj.physPosition, currentObj.physPosition) - otherObj.PlanetPullDistance - currentObj.Radius <= 0)
            {
                Vector3 direction = otherObj.physPosition - currentObj.physPosition;
                float distance = direction.magnitude;

                if (distance < 0) return;

                float gForce = 1 * (currentObj.Mass * otherObj.Mass) / Mathf.Pow(distance, 2);
                float devidedG = gForce / currentObj.Mass;

                Vector3 acceleration = direction.normalized * gForce;

                currentObj.velocity += acceleration;
            }
        }

        private void RemoveEmpty()
        {
            for (int i = 0; i < physObjs.Count; i++)
            {
                if (physObjs[i] == null)
                {
                    physObjs.RemoveAt(i);
                    i = 0;
                }
            }
        }

        public void AssignPhysicsObject(PhysicsObject physObj)
        {
            physObjs.Add(physObj);
        }
    }
}
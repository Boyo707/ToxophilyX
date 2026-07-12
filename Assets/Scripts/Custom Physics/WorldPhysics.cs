using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
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
            
                PhysicsStep(currentObj, physObjs).ApplyPhysics();
                if (currentObj.isGettingDestroyed)
                {
                    physObjs.Remove(currentObj);
                    Destroy(currentObj.gameObject);
                    i--;
                }
            }


            /*for (int i = 0; i < physObjs.Count; i++)
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
            }*/
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
                        BounceOfCollider(current, other.physPosition, normal, distanceOffset);
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

                float dot = ProjectionDot(sphere.physPosition, line.physPosition, line.GetLineNormal(), sphere.Radius);
                float lowerDot = dot - line.LineHeight;

                Vector3 linePos = Vector3.zero;
                Vector3 normal = Vector3.zero;
                if(dot > -2)
                {
                    //do normal above 0 check
                    if (dot < 0)
                    {
                        linePos = line.physPosition;
                        normal = line.GetLineNormal();
                    }
                }
                if(dot <= -2)
                {
                    //do below -4 check
                    if (dot > -line.LineHeight)
                    {
                        linePos = line.physPosition;
                        linePos += -line.GetLineNormal() * line.LineHeight;
                        normal = -line.GetLineNormal();

                    }
                }

                if (linePos != Vector3.zero && normal != Vector3.zero && InLineRange(sphere, line, normal))
                {
                    //check if line is in range AND if line is on edge.
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
                        normal = CheckEdge(sphere, line, normal);
                        current.hasCollided = true;
                        BounceOfCollider(current, linePos, normal, sphere.Radius);
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

        private float ProjectionDot(Vector3 currentPos, Vector3 otherPos, Vector3 targetNormal, float offsetDistance)
        {
            Vector3 displacement = currentPos - otherPos;
            displacement -= displacement * offsetDistance;
            Vector3 projection = Vector3.Project(displacement, targetNormal);
            return Vector3.Dot(displacement, targetNormal);
        }
        private bool InLineRange(PhysicsObject sphereObj, PhysicsObject lineObj, Vector3 normal)
        {
            Vector3 displacement = sphereObj.physPosition - lineObj.physPosition;
            Vector3 projection = Vector3.ProjectOnPlane(displacement, normal);
            return projection.magnitude < lineObj.LineWidth;
        }

        private Vector3 CheckEdge(PhysicsObject sphereObj, PhysicsObject lineObj, Vector3 normal)
        {
            Vector3 displacement = sphereObj.physPosition - lineObj.physPosition;
            Vector3 projection = Vector3.ProjectOnPlane(displacement, normal);
            if(projection.magnitude > lineObj.LineWidth - lineObj.EdgeLength)
            {
                return displacement.normalized;
            }
            return normal;
        }

        private void BounceOfCollider(PhysicsObject currentObj, Vector3 otherPos, Vector3 targetNormal, float someDistance)
        {
            Vector3 thisVelocity = currentObj.velocity;
            float normalVelocityDot = Vector2.Dot(thisVelocity, targetNormal);
            float magnitude = thisVelocity.magnitude;

            float dot = ProjectionDot(currentObj.physPosition, otherPos, targetNormal, someDistance);

            //add a check if its going down hill 
            if (magnitude < 0.66f)
            {
                //start resting
                Vector3 diff = targetNormal * Mathf.Abs(dot);

                currentObj.physPosition += diff;

                // Stop the bounce.
                currentObj.velocity = Vector3.zero;
            }
            else
            {
                //bounce

                //place above collision line to prevent clipping

                Vector3 diff = targetNormal * (Mathf.Abs(dot) + collisionSkin);

                currentObj.physPosition += diff;

                //bounce code
                Vector3 normalVelocity = normalVelocityDot * targetNormal;

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

        private PhysicsObject PhysicsStep(PhysicsObject currentObj, List<PhysicsObject> otherObjects)
        {

            if (currentObj.HasGravity && currentObj.HasPhysics)
            {
                Vector3 gravityDir = currentObj.LocalGravityDirection == worldGravitDir ? worldGravitDir : currentObj.LocalGravityDirection;

                currentObj.velocity += (gravityDir * gravitationalForce * currentObj.GravityMultiplier * Time.fixedDeltaTime);
            }

            currentObj.physPosition += currentObj.velocity * Time.fixedDeltaTime;


            //maak een range circle dat bepaald of een object attracted kan worden of niet

            foreach(PhysicsObject otherObj in otherObjects)
            {
                if (currentObj == otherObj) continue;

                if (otherObj.IsPlanet && currentObj.HasPhysics)
                {
                    PlanetPhysics(currentObj, otherObj);
                }

                if (currentObj.velocity == Vector3.zero) continue;

                VerifyCollision(currentObj, otherObj);
            }

            return currentObj;
                
        }

        public List<Vector3> GetSimulatedPos(PhysicsObject objectToSimulate, int steps, Vector3 startPosition, Vector3 startVelocity)
        {

            objectToSimulate.velocity = startVelocity;
            objectToSimulate.physPosition = startPosition;

            List<Vector3> positions = new();
            for (int i = 0; i < steps; i++)
            {
                positions.Add(PhysicsStep(objectToSimulate, physObjs).physPosition);
            }
            return positions;
        }

        public void AssignPhysicsObject(PhysicsObject physObj)
        {
            physObjs.Add(physObj);
        }
    }
}
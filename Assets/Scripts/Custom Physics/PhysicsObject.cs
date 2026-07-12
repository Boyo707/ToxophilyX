using UnityEngine;


namespace CustomPhysics
{
    public enum ColliderType
    {
        Circle = 1,
        Line = 2,
        Square = 4
    }
    public class PhysicsObject : MonoBehaviour
    {

        [Header("Physics")]
        [SerializeField] private bool hasPhysics = true;
        [SerializeField] private float mass = 1;
        [SerializeField] private bool isTrigger = false;
        [SerializeField] private bool isPlanet = false;
        [SerializeField] private float planetPullDistance = 0;
        [SerializeField] private float restitution = 0.9f;
        [SerializeField] private float friction = 0.001f;
        [SerializeField] private bool lockRotation = false;
        [SerializeField] private bool hasGravity = true;
        [SerializeField] private float gravityMultiplier = 1;
        [SerializeField] private Vector3 localGravityDirection = Vector3.down;

        [Header("Collision")]
        [SerializeField] private ColliderType type = ColliderType.Circle;
        [SerializeField] private Vector3 offset;
        [Header("Circle")]
        [SerializeField] private float radius;
        [Header("Line")]
        [SerializeField] private float lineWidth;
        [SerializeField] private float lineHeight;
        [SerializeField] private float edgelength;
        private Vector2 netForce;




        //velocity
        public bool HasPhysics => hasPhysics;
        public float Mass => mass;
        public bool IsTrigger => isTrigger;
        public bool IsPlanet => isPlanet;
        public float PlanetPullDistance => planetPullDistance;
        public float Restitution => restitution;
        public float Friction => friction;
        public bool HasGravity => hasGravity;
        public float GravityMultiplier => gravityMultiplier;
        public Vector3 LocalGravityDirection => localGravityDirection;
        public float Radius
        {
            get 
            {
                if(transform.localScale.x == transform.localScale.y)
                {
                    return radius * transform.localScale.x / 2;
                }
                else if(transform.localScale.x > transform.localScale.y)
                {
                    return radius * transform.localScale.x / 2;
                }
                else
                {
                    return radius * transform.localScale.y / 2;
                }
            }
            set
            {
                radius = value;
            }
        }
        public Vector2 NetForce => netForce;

        public Vector3 physPosition = Vector3.zero;

        public Vector3 velocity = Vector3.zero;

        public float rotation = 0;

        public bool isGettingDestroyed = false;

        //collision
        //line
        public bool hasCollided = false;
        public bool hasTriggered = false;

        public ColliderType ColliderShape => type;

        public float LineWidth => lineWidth / 2;
        public float LineHeight => lineHeight;
        public float EdgeLength => edgelength;

        public PhysicsObject interactedObject;




        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            physPosition = transform.position + offset;

            WorldPhysics.instance.AssignPhysicsObject(this);
        }

        private void LateUpdate()
        {
            physPosition = transform.position;
        }

        public void ApplyPhysics()
        {
            transform.position = physPosition - offset;

            if (!lockRotation)
            {
                Vector2 normalized = velocity.normalized;

                float radians = Mathf.Atan2(normalized.x, normalized.y);

                rotation = radians * Mathf.Rad2Deg;

                transform.eulerAngles = new Vector3(0, 0, -rotation);
            }

        }

        public void SetVelocity(Vector3 newVelocity)
        {
            velocity = newVelocity;
        }

        public void AddVelocity(Vector3 velocityAddition)
        {
            velocity += velocityAddition;
        }

        public Vector3 GetLineNormal()
        {
            Vector3 newOffset = offset;
            return Vector2.Perpendicular(transform.right).normalized;
        }

        public void DestroyObject()
        {
            isGettingDestroyed = true;
        }

        private void OnDrawGizmos()
        {
            Vector3 newOffset = offset;
            Vector3 center = transform.position + newOffset;
            Gizmos.color = Color.red;
            //circle
            Gizmos.DrawWireSphere(center, Radius);

            Gizmos.DrawRay(transform.position + offset, velocity);

            //line
            float lineBaseLength = lineWidth / 2 - edgelength ;
            Gizmos.DrawLine(center - transform.right * lineBaseLength, center + transform.right * lineBaseLength);


            Vector3 bottomLine = center + -GetLineNormal() * LineHeight;
            Gizmos.DrawLine(bottomLine - transform.right * lineBaseLength, bottomLine + transform.right * lineBaseLength);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(center + transform.right * lineBaseLength, center + transform.right * lineWidth / 2);
            Gizmos.DrawLine(center - transform.right * lineBaseLength, center - transform.right * lineWidth / 2);

            Gizmos.DrawLine(bottomLine + transform.right * lineBaseLength, bottomLine + transform.right * lineWidth / 2);
            Gizmos.DrawLine(bottomLine - transform.right * lineBaseLength, bottomLine - transform.right * lineWidth / 2);

            Gizmos.DrawRay(bottomLine, -GetLineNormal());
            Gizmos.DrawRay(center, GetLineNormal());

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(center, planetPullDistance);
        }
    }
}
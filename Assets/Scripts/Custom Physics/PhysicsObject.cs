using UnityEngine;

namespace CustomPhysics
{
    public enum ColliderType
    {
        Circle,
        Line,
        Square
    }
    public class PhysicsObject : MonoBehaviour
    {

        [Header("Physics")]
        [SerializeField] private float mass = 1;
        [SerializeField] private float restitution = 0.9f;
        [SerializeField] private float friction = 0.001f;
        [SerializeField] private bool hasGravity = true;
        [SerializeField] private float gravityMultiplier = 1;
        [SerializeField] private Vector3 localGravityDirection = Vector3.down;

        [Header("Collision")]
        [SerializeField] private ColliderType type = ColliderType.Circle;
        [SerializeField] private Vector3 offset;
        [Header("Circle")]
        [SerializeField] private float radius;
        [Header("Line")]
        [SerializeField] private float lineLength;
        [SerializeField] private float edgelength;
        private Vector2 netForce;




        //velocity
        public float Mass => mass;
        public float Restitution => restitution;
        public float Friction => friction;
        public bool HasGravity => hasGravity;
        public float GravityMultiplier => gravityMultiplier;
        public Vector3 LocalGravityDirection => localGravityDirection;
        public float Radius => radius;
        public Vector2 NetForce => netForce;

        public Vector3 physPosition = Vector3.zero;

        public Vector3 velocity = Vector3.zero;

        //collision
        //line
        public bool hasCollided = false;

        public ColliderType ColliderShape => type;


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

        private void OnDrawGizmos()
        {
            Vector3 newOffset = offset;
            Vector3 center = transform.position + newOffset;
            Gizmos.color = Color.red;
            //circle
            Gizmos.DrawWireSphere(center, radius);

            Gizmos.DrawRay(transform.position + offset, velocity);

            //line
            float lineBaseLength = (lineLength - edgelength) / 2;
            Gizmos.DrawLine(center - -transform.right * lineBaseLength, center - transform.right * lineBaseLength);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(center - -transform.right * lineBaseLength, center - -transform.right * lineLength / 2);
            Gizmos.DrawLine(center - transform.right * lineBaseLength, center - transform.right * lineLength / 2);
            //Gizmos.DrawRay(center, Vector2.Perpendicular(transform.right * lineBaseLength).normalized);
            Gizmos.DrawRay(center, GetLineNormal());
        }
    }
}

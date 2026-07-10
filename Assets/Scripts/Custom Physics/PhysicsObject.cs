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
        [SerializeField] private float gravityMultiplier = 1;

        [Header("Collision")]
        [SerializeField] private ColliderType type = ColliderType.Circle;
        [SerializeField] private Vector2 offset;
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
        public float GravityMultiplier => gravityMultiplier;
        public float Radius => radius;
        public Vector2 NetForce => netForce;

        [SerializeField] private float[] position = new float[2];
        public float[] Position
        {
            get { return position; }
            set { position = value; }
        }

        [SerializeField] private float[] velocity = new float[2];
        public float[] Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }

        //collision
        //line
        public bool hasCollided = false;

        public ColliderType ColliderShape => type;


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            position[0] = transform.position.x + offset.x;
            position[1] = transform.position.y + offset.y;

            WorldPhysics.instance.AssignPhysicsObject(this);
        }

        public void ApplyPhysics()
        {
            if (gameObject.name == "Arrow")
            {
                
                transform.position = vecPosition() - new Vector3(offset.x, offset.y, 0);
            }
        }

        public Vector3 vecPosition()
        {
            return new Vector3(position[0], position[1], 0);
        }
        public Vector2 vecVelocity()
        {
            return new Vector2(velocity[0], velocity[1]);
        }

        public Vector3 GetLineNormal()
        {
            Vector3 newOffset = new Vector3(offset.x, offset.y, 0);
            //return Vector2.Perpendicular(transform.position + newOffset - transform.right).normalized;
            return Vector2.Perpendicular(transform.right).normalized;
        }

        private void OnDrawGizmos()
        {
            Vector3 newOffset = new Vector3(offset.x, offset.y, 0);
            Vector3 center = transform.position + newOffset;
            Gizmos.color = Color.red;
            //circle
            Gizmos.DrawWireSphere(center, radius);

            Gizmos.DrawRay(transform.position + new Vector3(offset.x, offset.y, 0), new Vector3(velocity[0], velocity[1], 0));

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

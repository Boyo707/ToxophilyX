using CustomPhysics;
using UnityEngine;

public class PlayerControlls : MonoBehaviour
{
    [SerializeField] private Transform bow;
    [SerializeField] private GameObject arrow;
    [SerializeField] private Transform shootingPoint;
    [SerializeField] private float maxPullDistance;
    [SerializeField] private float maxShootStrength = 5;


    private Vector3 direction;
    public float distance;
    public float strength;

    public Vector3 arrowSpeed;

    public Vector3 mousePos;

    private GameObject previousArrow;

    // Update is called once per frame
    void Update()
    {
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        direction = mousePos - bow.position;
        distance = direction.magnitude;


        strength = Mathf.Clamp01(distance / maxPullDistance) * maxShootStrength;
        arrowSpeed = direction.normalized * strength;

        Vector2 normalized = direction.normalized;

        float radians = Mathf.Atan2(normalized.x, normalized.y);

        float rotation = radians * Mathf.Rad2Deg;

        if(rotation > 0)
        {
            bow.eulerAngles = new Vector3(0, 0, -rotation);
        }


        if (Input.GetMouseButtonDown(0))
        {
            if (previousArrow != null) previousArrow.GetComponent<PhysicsObject>().DestroyObject();
            previousArrow = Instantiate(arrow, shootingPoint.position, Quaternion.identity);
            previousArrow.GetComponent<PhysicsObject>().SetVelocity(direction.normalized * strength);
        }

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(mousePos, 1);
        Gizmos.DrawRay(bow.position, direction);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(bow.position, direction.normalized * strength);
    }
}

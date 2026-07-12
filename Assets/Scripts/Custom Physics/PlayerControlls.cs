using CustomPhysics;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControlls : MonoBehaviour
{
    [SerializeField] private Transform bow;
    [SerializeField] private GameObject arrow;
    [SerializeField] private Transform shootingPoint;
    [SerializeField] private float maxPullDistance;
    [SerializeField] private float maxShootStrength = 5;

    [Header("Aim Prediction")]
    [SerializeField] GameObject trajectoryOrb;
    [SerializeField] private int predictionSteps = 8;
    [SerializeField] private int visualSteps = 8;


    private Vector3 direction;
    private float distance;
    private float strength;

    private Vector3 mousePos;

    private GameObject previousArrow;

    public List<Vector3> predictedPositions = new();
    public List<Transform> visualObjects = new();

    private void Start()
    {
        for (int i = 0; i < visualSteps; i++)
        {
            visualObjects.Add(Instantiate(trajectoryOrb, new Vector3(50, -50, 0), Quaternion.identity).transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        direction = mousePos - bow.position;
        distance = direction.magnitude;


        strength = Mathf.Clamp01(distance / maxPullDistance) * maxShootStrength;

        Vector2 normalized = direction.normalized;

        float radians = Mathf.Atan2(normalized.x, normalized.y);

        float rotation = radians * Mathf.Rad2Deg;

        if(rotation > 0)
        {
            bow.eulerAngles = new Vector3(0, 0, -rotation);
        }

        predictedPositions = WorldPhysics.instance.GetSimulatedPos(arrow.GetComponent<PhysicsObject>(), predictionSteps, shootingPoint.position, direction.normalized * strength);

        for (int i = 0; i < visualSteps; i++)
        {
            int steps = (int)(predictionSteps / 9 * i);
            visualObjects[i].position = predictedPositions[steps];
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
        Gizmos.DrawWireSphere(shootingPoint.position, 1);
        Gizmos.DrawRay(shootingPoint.position, direction);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(shootingPoint.position, direction.normalized * strength);
    }
}

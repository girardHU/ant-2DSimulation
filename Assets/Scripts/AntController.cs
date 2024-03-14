using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntController : MonoBehaviour
{
    // public GameObject target;
    public float wanderStrength = 0.5f;
    public float maxSpeed = 4f;
    public float steerStrength = 4f;
    public float foodDetectionRadius = 20.0f;

    private Vector2 position;
    private Vector2 velocity;
    private Vector2 desiredDirection;

    private Vector2 randomSteerForce;
    private readonly float randomSteerInterval = 1.0f;
    private float randomSteeringTimer;

    private GameObject foodToPickUp = null;
    // Start is called before the first frame update
    void Start()
    {
        position = transform.position;
        randomSteeringTimer = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        randomSteeringTimer += Time.deltaTime;

        if (foodToPickUp != null)
        {
            // TODO: CHECK IF THE FOOD IS STILL AVAILABLE
            PickUpFood();
        }
        else
        {
            Wander();
        }
    }

    void Move()
    {
        float faceUp = 90.0f;

        AddRandomSteer();

        Vector2 desiredVelocity = (desiredDirection + randomSteerForce).normalized * maxSpeed;
        Vector2 desiredSteeringForce = (desiredVelocity - velocity) * steerStrength;
        Vector2 acceleration = Vector2.ClampMagnitude(desiredSteeringForce, steerStrength) / 1;

        velocity = Vector2.ClampMagnitude(velocity + acceleration * Time.deltaTime, maxSpeed);
        position += velocity * Time.deltaTime;

        float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg + faceUp;
        transform.SetPositionAndRotation(position, Quaternion.Euler(0, 0, angle));

    }

    void AddRandomSteer()
    {
        // if (foodToPickUp != null)
        // {
        //     randomSteerForce = Vector2.zero;
        // }
        if (randomSteeringTimer >= randomSteerInterval)
        {
            randomSteerForce = GetRandomSteer(desiredDirection);
            randomSteeringTimer = 0.0f;
        }
    }

    Vector2 GetRandomSteer(Vector2 originalDirection)
    {
        Vector2 smallestSteer = Vector2.zero;
        float associatedDot = -1;
        for (int iterations = 8; iterations > 0; iterations--)
        {
            Vector2 randomSteer = Random.insideUnitCircle.normalized;
            float dot = Vector2.Dot(originalDirection, randomSteer);
            if (dot > associatedDot) {
                smallestSteer = randomSteer;
                associatedDot = dot;
            }
        }
        return smallestSteer;
    }

    void Wander()
    {
        // Find a random direction to follow
        desiredDirection = (desiredDirection + Random.insideUnitCircle * wanderStrength).normalized;
        Move();
        DetectFood();
    }

    void DetectFood()
    {
        Collider2D[] foodNearby = Physics2D.OverlapCircleAll(position, foodDetectionRadius, LayerMask.GetMask("Food"));
        if (foodNearby.Length > 0)
        {
            // Debug.Log("Sniffing Food...");
            foodToPickUp = foodNearby[0].gameObject;
        }
    }

    void PickUpFood()
    {
        // Set direction towards the food
        desiredDirection = (desiredDirection + (Vector2)foodToPickUp.transform.position - (Vector2)transform.position).normalized;

        Move();
    }
}

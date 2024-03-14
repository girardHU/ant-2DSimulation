using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class AntController : MonoBehaviour
{
    // public GameObject target;
    public float wanderStrength = 0.5f;
    public float maxSpeed = 4f;
    public float steerStrength = 4f;
    public float foodDetectionRadius = 5.0f;
    public float foodPickupRadius = 1.0f;

    private Vector2 position;
    private Vector2 velocity;
    private Vector2 desiredDirection;

    private Vector2 randomSteerForce;
    private readonly float randomSteerInterval = 1.0f;
    private float randomSteeringTimer;

    private Collider2D foodToPickUp = null;
    private Collider2D foodHold = null;

    private GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        // Init
        gameManager = FindObjectOfType<GameManager>();
        position = transform.position;
        randomSteeringTimer = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        // Manage whole Loop
        randomSteeringTimer += Time.deltaTime;
        DetectFood();

        if (foodToPickUp != null)
        {
            GoToFood();
        }
        else if (foodHold != null)
        {
            GoToAnthill();
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

        // Calculate velocity and acceleration, update position and set rotation of the Ant
        Vector2 desiredVelocity = (desiredDirection + randomSteerForce).normalized * maxSpeed;
        Vector2 desiredSteeringForce = (desiredVelocity - velocity) * steerStrength;
        Vector2 acceleration = Vector2.ClampMagnitude(desiredSteeringForce, steerStrength) / 1;

        velocity = Vector2.ClampMagnitude(velocity + acceleration * Time.deltaTime, maxSpeed);
        position += velocity * Time.deltaTime;

        float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg + faceUp;
        transform.SetPositionAndRotation(position, Quaternion.Euler(0, 0, angle));

        if (foodHold != null)
        {
            // TODO: Change for proper food holding
            foodHold.transform.position = transform.position;
            foodHold.transform.position += new Vector3(0, 1);
            // foodHold.transform.Translate(new Vector3(0, 0, angle) * 3, );
        }

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
            if (dot > associatedDot)
            {
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
    }

    void DetectFood()
    {
        Collider2D[] foodNearby = Physics2D.OverlapCircleAll(position, foodDetectionRadius, LayerMask.GetMask("FoodOnGround"));
        if (foodNearby.Length > 0)
        {
            if (foodToPickUp == null || !foodNearby.Contains<Collider2D>(foodToPickUp))
            {
                foodToPickUp = foodNearby[0];
            }
        }
        else
        {
            foodToPickUp = null;
        }
    }

    void GoToFood()
    {
        // Set direction towards the food
        desiredDirection = (desiredDirection + (Vector2)foodToPickUp.transform.position - (Vector2)transform.position).normalized;

        Move();
    }

    void PickUpFood(Collider2D food)
    {
        foodHold = food;
        food.gameObject.layer = LayerMask.NameToLayer("FoodOnAnts");
        food.gameObject.tag = "FoodOnAnt";
    }

    void GoToAnthill()
    {
        desiredDirection = (desiredDirection + Vector2.zero - (Vector2)transform.position).normalized;

        Move();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Anthill"))
        {
            if (foodHold != null)
            {
                DropFood();
            }
        }
        else if (col.CompareTag("FoodOnGround"))
        {
            PickUpFood(col);
        }
    }

    void DropFood()
    {
        Destroy(foodHold);
        gameManager.AddFoodToAnthill(1);
    }
}

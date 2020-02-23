using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

// <<warning>>
// this class should not be called unless there are players!!
public class MoveAroundObject : NetworkBehaviour
{
    public float speed;
    public float closeApproximity;

    public float defaultRadiusMovement;
    private float currentRadiusMovement;
    private float timeCounter;

    private int attackType;
    private Rigidbody enemyRigidBody;
    private GameObject[] players;
    private int playerIndexToAttack;

    private Vector3 randomTarget;
    private float gameRadius;
    private bool decidedRandomPosition;
    private float waitTimeRandomMovementPassed;

    private bool isSteerer;
    private bool steering;
    private float steeringTime;
    private Vector3 steerLocation;
    private float justStartedTime;
    private float EnoughTimePassed = 15.0f;

    private float trickTimePassed;
    private float trickTime;
    private bool currentlyTricking;
    private Vector3 trickDirection;
    private GameController gameController;
    private void Start()
    {
        if (isServer == false) return;
        trickTime = Random.Range(40.0f, 90.0f);
        trickTimePassed = 0.0f;
        currentlyTricking = false;
        currentRadiusMovement = defaultRadiusMovement;
        justStartedTime = 0.0f;
        isSteerer = Random.value <= 0.5 ? true : false;
        steering = false;
        steeringTime = 0.0f;
        steerLocation = Vector3.zero;
        waitTimeRandomMovementPassed = 0.0f;
        decidedRandomPosition = false;
        gameRadius = Utils.getGameBoundaryRadius(GameObject.FindGameObjectWithTag(Utils.TagBackground));
        timeCounter = 0.0f;
        enemyRigidBody = GetComponent<Rigidbody>();
        if (enemyRigidBody == null) throw new MissingComponentException("Enemy: Rigidbody is missing.");

        playerIndexToAttack = -1;
        ChoosePlayerToAttack();
        if (players.Length <= 0) throw new System.Exception("Not enough players. Should not load Enemy");


        attackType = -1;
        DecideAttackType();
        GameObject gameConrollerObject = GameObject.FindWithTag(Utils.TagGameConroller);
        if (gameConrollerObject != null)
        {
            gameController = gameConrollerObject.GetComponent<GameController>();
        }
    }

    private Transform FindTransformInChildWithTag(string _tag)
    {
        foreach (Transform child in gameObject.transform)
        {
            if( child.tag == _tag)
            {
                return child;
            }
        }
        return null;
    }

    // physics
    private void FixedUpdate()
    {
        if (isServer == false) return;
        if (playerIndexToAttack == -1) return;
        GameObject player = players[playerIndexToAttack];
        FaceTarget(player.transform);
        AttackPlan(player.transform);
    }

    private void AttackPlan(Transform targetTransform)
    {
        if (gameController.isGameOver)
        {
            RandomMovement();
            return;
        }
        if (shouldSteerFromEnemies())
            SteerFromCloseEnemies();
        else if (shouldTrickPlayer())
            TrickPlayer(targetTransform);
        else if (attackType == 1)
            MoveTowardsTarget(targetTransform);
        else if (attackType == 2)
            CirculateAroundTarget(targetTransform);
        else if (attackType == 3)
            RandomMovement();
        else
            throw new System.Exception("Incorrect attack type was = " + attackType);
    }

    private void TrickPlayer(Transform playerTransform)
    {
        if (currentlyTricking == false)
            trickDirection = playerTransform.right * Random.Range(-1, 1) + playerTransform.forward * Random.Range(-1, 0);
        currentlyTricking = true;

        float distanceFromPlayer = 18.0f;
        Vector3 newPosition = trickDirection * distanceFromPlayer + playerTransform.position;
        if (Vector3.Distance(newPosition, transform.position) <= 2.0f)
        {
            trickTimePassed = 0.0f;
            currentlyTricking = false;
        }
        MoveTowardsTarget(newPosition);
    }

    private bool shouldTrickPlayer()
    {
        if (currentlyTricking == true) return true;
        trickTimePassed += Time.deltaTime;
        if (trickTimePassed > trickTime)
        {
            trickTimePassed = 0.0f;
            return true;
        }
        return false;
    }

    private bool shouldSteerFromEnemies()
    {
        if (isSteerer == false) return false;
        return (Random.value <= 0.1 && steering == false) || // check if we should try to steer & start steering if so!
        (steering == true); // we are currently steering, so continue doing that
    }

    private void SteerFromCloseEnemies()
    {
        float radiusToSteerTo = 20.0f;
        if (steering == false)
        {
            Collider[] closeObjects = Physics.OverlapSphere(transform.position, 5.0f);
            foreach (Collider col in closeObjects)
            {
                if (col.tag == Utils.TagEnemy && col.gameObject.GetComponent<NetworkIdentity>().netId != gameObject.GetComponent<NetworkIdentity>().netId)
                {
                    Vector3 randomDir = Utils.getRandomDirection();
                    steerLocation = randomDir * radiusToSteerTo + transform.position;
                    MoveTowardsTarget(steerLocation);
                    steering = true;
                    break;
                }
            }
        }
        else if (steeringTime >= 6.5f)
        {
            // should stop steering.
            steering = false;
            steeringTime = 0.0f;
        }
        else if (steering == true)
        {
            if (Vector3.Distance(steerLocation, transform.position) <= 0.2f)
                steerLocation = Utils.getRandomDirection() * radiusToSteerTo + transform.position;
            steeringTime += Time.deltaTime;
            FaceTarget(steerLocation);
            SteerToTarget(steerLocation);
        }
    }

    private void RandomMovement()
    {
        waitTimeRandomMovementPassed += Time.deltaTime;
        if (decidedRandomPosition == false || randomTarget == transform.position)
        {
            randomTarget = Random.insideUnitSphere * gameRadius * 0.9f;
            decidedRandomPosition = true;
        }
        RandomMovementHelper();
        if (waitTimeRandomMovementPassed >= 10.0f)
        {
            DecideAttackType();
            decidedRandomPosition = false;
            waitTimeRandomMovementPassed = 0.0f;
        }
    }

    private void RandomMovementHelper()
    {
        FaceTarget(randomTarget);
        float realSpeed = decideSpeed(randomTarget);
        Vector3 direction = (randomTarget - transform.position).normalized;
        enemyRigidBody.velocity = direction * realSpeed * Time.deltaTime;
        Debug.DrawLine(transform.position, randomTarget, Color.green);
    }

    private void CirculateAroundTarget(Transform targetTransform)
    {
        justStartedTime += Time.deltaTime;
        timeCounter += Time.deltaTime;
        currentRadiusMovement -= Time.deltaTime;
        if (Mathf.Abs(currentRadiusMovement) > Mathf.Abs(defaultRadiusMovement))
            currentRadiusMovement = defaultRadiusMovement;
        if (justStartedTime <= EnoughTimePassed) currentRadiusMovement = 0.0f;
        float distanceFromTarget = Vector3.Distance(targetTransform.position, transform.position);
        if (distanceFromTarget <= 20.0f) currentRadiusMovement = 0.0f;
        if (distanceFromTarget >= 50.0f) currentRadiusMovement = 0.0f;
        float realSpeed = decideSpeed(targetTransform);
        Vector3 direction = (targetTransform.position - transform.position).normalized;
        enemyRigidBody.velocity = Time.deltaTime * realSpeed * (direction + new Vector3(Mathf.Cos(timeCounter), Mathf.Sin(timeCounter), 0.0f) * currentRadiusMovement);
        if (timeCounter >= 360.0f) timeCounter = 0.0f;
    }

    private void MoveTowardsTarget(Transform targetTransform)
    {
        MoveTowardsTarget(targetTransform.position);
    }

    private void MoveTowardsTarget(Vector3 targetPosition)
    {
        float realSpeed = decideSpeed(targetPosition);
        Vector3 direction = (targetPosition - transform.position).normalized;
        enemyRigidBody.velocity = direction * realSpeed * Time.deltaTime;
    }

    private void SteerToTarget(Vector3 targetPosition)
    {
        float realSpeed = speed * 0.4f;
        Vector3 direction = (targetPosition - transform.position).normalized;
        enemyRigidBody.velocity = direction * realSpeed * Time.deltaTime;
    }

    private float decideSpeed(Transform targetTransform)
    {
        return decideSpeed(targetTransform.position);
    }

    private float decideSpeed(Vector3 targetPosition)
    {
        float inferredSpeed = inferSpeed();
        if (Vector3.Distance(targetPosition, transform.position) <= closeApproximity)
        {
            return inferredSpeed / 3.0f;
        }
        return inferredSpeed;
    }

    private float inferSpeed()
    {
        if (gameController.getShowMessage()) return 0.0f;
        if (gameController.isGameOver == true) return speed / 6.0f;
        return speed;
    }

    private void FaceTarget(Transform targetTransform)
    {
        Vector3 direction = (targetTransform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        enemyRigidBody.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5.0f);
    }

    private void FaceTarget(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        enemyRigidBody.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5.0f);
    }

    private void Update()
    {
        if (isServer == false) return;
        if (playerIndexToAttack != -1) return;
        ChoosePlayerToAttack();
        DecideAttackType();
    }

    private void DecideAttackType()
    {
        if (Random.value <= 0.2) attackType = 3;
        else  attackType = Random.value <= 0.65 ? 1 : 2;
    }

    private void ChoosePlayerToAttack()
    {
        players = GameObject.FindGameObjectsWithTag(Utils.TagPlayer);
        if (players.Length <= 0) playerIndexToAttack = -1;
        else playerIndexToAttack = Random.Range(0, players.Length);
    }


}

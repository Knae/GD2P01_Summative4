using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentController : MonoBehaviour
{
    public enum STATE
    {
        NONE,
        IDLE,
        WANDER,
        FLEE,
        SEEKFLAG,
        RESCUECOMRADE,
        IMPRISONED,
        GETHIT,
        MAX_NONE,
    }

    public enum COLOUR
	{
        NONE,
        RED,
        BLUE,
        MAX_NONE,
	}

    [Header("ConnectedScripts")]
    [SerializeField] private Rigidbody2D rgdbdAgent;

    [Header("Settings")]
    [SerializeField] public BlackBoard bbHomeBoard;
    [SerializeField] private float fMoveSpeed = 0.2f;
    [SerializeField] private GameObject objMarkerPrefab;
    [SerializeField] private GameObject objMarkerCreated;
    [SerializeField] private Sprite[] sprtAgentSprites;


    [Header("Debug")]
    [SerializeField] private STATE      eCurrentState = STATE.IDLE;
    [SerializeField] private Vector2    v2CurrentVelocity = Vector2.zero;
    [SerializeField] private Vector2    v2TargetPosition = Vector2.zero;
    [SerializeField] private float      fDelayCounter = 0.0f;
    [SerializeField] private float      fWanderCounter = 0.0f;
    [SerializeField] private float      fCurrentSpeed = 0.0f;
    [SerializeField] private float      fCurrentAccel = 0.0f;
    [SerializeField] private bool       bIsRedTeam = true;
    [SerializeField] private bool       bIsPlayerControlled = false;
    [SerializeField] private bool       bDangerNearby = false;
    [SerializeField] private bool       bHoldingFlag = false;

    //Constants
    const float kfMass = 1.0f;
    const float kfMinDelay = 2.0f;
    const float kfMaxDelay = 5.0f;
    const float kfMaxSpeed = 0.5f;
    const float kfMaxForce = 0.025f;
    const float kfFleeDistance = 0.5f;
    const float kfBrakeDistance = 0.5f;
    const float kfMaxWanderSpeed = 0.25f;
    const float kfMaxWanderForce = 0.25f;
    const float kfWanderDistance = 0.16f;
    const float kfWanderAngle = 30.0f;
    const float kfWanderUpdateTime = 0.5f;

    // Start is called before the first frame update
    void Start()
    {

        if (rgdbdAgent == null)
        {
            rgdbdAgent = this.GetComponent<Rigidbody2D>();
        }
    }

	private void Awake()
	{
        ChangeState(STATE.IDLE);
        fDelayCounter = 0.0f;
        bDangerNearby = false;
    }
	// Update is called once per frame
	void Update()
    {
        if (fDelayCounter > 0)
        {
            fDelayCounter -= Time.deltaTime;
        }

        if (fWanderCounter > 0)
        {
            fWanderCounter -= Time.deltaTime;
        }

        if (bIsPlayerControlled)
        {
            v2CurrentVelocity.x = Input.GetAxis("Horizontal");
            v2CurrentVelocity.y = Input.GetAxis("Vertical");
            v2CurrentVelocity.Normalize();
        }
    }
    /// <summary>
    /// Toggle if this agents is being controlled by the player
    /// If is, create marker on agent.
    /// If not, destroy the marker object
    /// </summary>
    public void TogglePlayerControl(GameObject objCamera)
    {
        bIsPlayerControlled = !bIsPlayerControlled;
        if (bIsPlayerControlled)
        {
            objMarkerCreated = Instantiate(objMarkerPrefab, transform.position, Quaternion.identity, transform);
            GameObject camera = Instantiate(objCamera, new Vector3(transform.position.x, transform.position.y, -10), Quaternion.identity, objMarkerCreated.transform);
        }
        else
        {
            Destroy(objMarkerCreated);
            objMarkerCreated = null;
        }
        fDelayCounter = 0;
        ChangeState(STATE.IDLE);
    }
    /// <summary>
    /// Return if this agent is player controlled
    /// </summary>
    /// <returns></returns>
    public bool IsPlayerControlled()
    {
        return bIsPlayerControlled;
    }
    
    public void SetHoldingFlag()
	{
        bHoldingFlag = true;
	}

    public bool IsHoldingFlag()
	{
        return bHoldingFlag;
	}

    public bool GetIfRedTeam()
	{
        return bIsRedTeam;
	}
    /// <summary>
    /// Change the agent sprite to the specified colour
    /// Assumes that the red agent and blue agent sprites
    /// have been assigned to the sprtAgentSprites array
    /// </summary>
    /// <param name="_inColour"></param>
    public void SetAgentColour(COLOUR _inColour)
	{
		if (sprtAgentSprites.Length > 0)
		{
			SpriteRenderer agentSprite = GetComponentInChildren<SpriteRenderer>();

			if (agentSprite != null)
			{
				switch (_inColour)
				{
					case COLOUR.RED:
					{
						agentSprite.sprite = sprtAgentSprites[0];
                        bIsRedTeam = true;
						break;
					}
					case COLOUR.BLUE:
					{
						agentSprite.sprite = sprtAgentSprites[1];
                        bIsRedTeam = false;
                        break;
					}
					case COLOUR.NONE:
					case COLOUR.MAX_NONE:
					default:
						break;
				}  
			}
			else
			{
                print("Unable to find sprite renderer on this agent");
			}
		}
	}

	private void FixedUpdate()
	{
        Vector2 currentPosition = transform.position;

        if (bIsPlayerControlled)
        {
            currentPosition += v2CurrentVelocity * fMoveSpeed * Time.fixedDeltaTime;
            transform.position = currentPosition;
        }
        else
        {
            currentPosition += v2CurrentVelocity * Time.fixedDeltaTime;
            transform.position = currentPosition;

            switch (eCurrentState)
            {
                case STATE.IDLE:
                    {
                        if (fDelayCounter <= 0)
                        {
                            ChangeState(STATE.WANDER);
                        }
                        else
                        {
                        }
                        break;
                    }
                case STATE.WANDER:
                    {
                        if (fDelayCounter <= 0)
                        {
                            ChangeState(STATE.IDLE);
                            fWanderCounter = 0;
                        }
                        else
                        {
                            CalculateVelocity(Wander());
                        }
                        break;
                    }
                case STATE.FLEE:
                    {
                        break;
                    }
                case STATE.GETHIT:
                    {
                        //DoNothing
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }
    }
	private void OnCollisionEnter2D(Collision2D collision)
	{
		if(collision.collider.tag == "WallNS")
		{
            v2CurrentVelocity.y *= -1.0f;
		}
		else if(collision.collider.tag == "WallWE")
		{
            v2CurrentVelocity.x *= -1.0f;
		}
	}
	/// <summary>
	/// Display radius at which agent starts to flee
	/// </summary>
	private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, kfFleeDistance);
    }
    /// <summary>
    /// Change the current state
    /// </summary>
    /// <param name="_inNewState"></param>
    private void ChangeState(STATE _inNewState)
    {
        Vector2 CurrentPosition = transform.position;

        switch (_inNewState)
        {
            case STATE.IDLE:
            {
                fDelayCounter = Random.Range(kfMinDelay, kfMaxDelay);
                v2CurrentVelocity = Vector2.zero;
                eCurrentState = STATE.IDLE;
                break;
            }
            case STATE.WANDER:
            {
                fDelayCounter = Random.Range(kfMinDelay, kfMaxDelay);
                CalculateVelocity(WanderInRandomDirection());
                eCurrentState = STATE.WANDER;
                break;
             }
            case STATE.FLEE:
            {
                break;
            }
            case STATE.GETHIT:
            {
                //do nothing, just wait to be destroyed
                break;

            }
            default:
            {
                break;
            }
        }
    }
    /// <summary>
    /// Calculate the resultant velocity due to
    /// given resultant force(or steering force)
    /// if current distance to y-axis is less than 0.1, then flip
    /// the x force
    /// </summary>
    /// <param name="_SteeringForce"></param>
    private void CalculateVelocity(Vector2 _SteeringForce)
    {
        Vector2 newVelocity = _SteeringForce + v2CurrentVelocity;
        newVelocity = Vector2.ClampMagnitude(newVelocity, kfMaxSpeed);
        v2CurrentVelocity += newVelocity;

        if(eCurrentState == STATE.WANDER)
		{
            v2CurrentVelocity = Vector2.ClampMagnitude(v2CurrentVelocity, kfMaxWanderSpeed);
		}
		else
		{
            v2CurrentVelocity = Vector2.ClampMagnitude(v2CurrentVelocity, kfMaxSpeed);
		}
    }
    /// <summary>
    /// Idle state does nothing until delay counter reaches
    /// 0
    /// </summary>
    /// <returns></returns>
    private bool Idle()
    {
        if (fDelayCounter > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Seek behaviour. Generates force towards the target position
    /// </summary>
    /// <returns></returns>
    private Vector2 Seek()
    {
        Vector2 currentVelocity = v2CurrentVelocity;
        Vector2 currentPosition = this.transform.position;
        Vector2 targetPosition = v2TargetPosition;
        Vector2 desiredVelocity;
        Vector2 steeringForce;

        float angleToTarget = Vector2.Angle(currentPosition, targetPosition);//(float)((atan2(targetPosition.y, targetPosition.x) - atan2(currentPosition.y, currentPosition.x)) * (180 / PI));

        desiredVelocity = (targetPosition - currentPosition).normalized * kfMaxSpeed;
        steeringForce = desiredVelocity - currentVelocity;
        steeringForce /= kfMass;
        steeringForce = Vector2.ClampMagnitude(steeringForce, kfMaxForce);

        return steeringForce;
    }
    /// <summary>
    /// Agent will wander around, updating its movement
    /// every set period as described by m_kfWanderUpdateTime
    /// </summary>
    /// <returns></returns>
    private Vector2 Wander()
    {
        if (fWanderCounter <= 0)
        {
            fWanderCounter = kfWanderUpdateTime;
            Vector2 currentVelocity = v2CurrentVelocity;
            //angle is being changed not modified
            float randomAngle = Random.Range(-kfWanderAngle, kfWanderAngle);
            if(randomAngle < 0.0f)
			{
                randomAngle += 360.0f;
			}
            Vector2 wanderCircle = currentVelocity.normalized * kfWanderDistance;
            Vector2 wanderForce = new Vector2(Mathf.Cos(Mathf.Deg2Rad * randomAngle), Mathf.Sin(Mathf.Deg2Rad * randomAngle)) * kfMaxWanderForce;
            Vector2 displaceForce = wanderCircle + wanderForce;
            return displaceForce;
        }
        else
        {
            return Vector2.zero;
        }
    }
    /// <summary>
    /// Arrive behaviour. Generates reducing force as it approaches the target position
    /// </summary>
    /// <param name="_inEntity"></param>
    /// <returns></returns>
    private Vector2 Arrive()
    {
        Vector2 desiredVelocity;
        Vector2 steeringForce;
        Vector2 currentVelocity = v2CurrentVelocity;
        Vector2 currentPosition = this.transform.position;
        Vector2 targetPosition = v2TargetPosition;
        Vector2 offsetToTarget = targetPosition - currentPosition;
        float offsetDistance = offsetToTarget.sqrMagnitude;

        if (offsetDistance > 0)
        {
            float reducedSpeed = (offsetDistance / kfBrakeDistance) * kfMaxSpeed;
            reducedSpeed = Mathf.Min(reducedSpeed, kfMaxSpeed);
            desiredVelocity = (reducedSpeed / offsetDistance) * offsetToTarget;
            steeringForce = desiredVelocity - currentVelocity;
            steeringForce /= kfMass;
            return steeringForce;
        }
        else
        {
            return Vector2.zero;
        }
    }

    /// <summary>
    /// Flee behaviour. Generates force away the target position
    /// </summary>
    /// <param name="_inEntity"></param>
    /// <returns></returns>
    private Vector2 Flee()
    {
        Vector2 currentPosition = this.transform.position;
        Vector2 targetPosition = v2TargetPosition;
        Vector2 currentVelocity = v2CurrentVelocity;
        Vector2 desiredVelocity;
        Vector2 steeringForce;

        float angleToTarget = Vector2.Angle(targetPosition, currentPosition);

        desiredVelocity = (currentPosition - targetPosition).normalized * kfMaxSpeed;
        steeringForce = desiredVelocity - currentVelocity;
        steeringForce /= kfMass;
        steeringForce = Vector2.ClampMagnitude(steeringForce, kfMaxForce);
        return steeringForce;
    }

    /// <summary>
    /// Combines Arrive and Seek for smooth travel to a position
    /// </summary>
    /// <param name="_inEntity"></param>
    /// <returns></returns>
    private Vector2 GoTo()
    {
        Vector2 currentPosition = this.transform.position;
        Vector2 targetPosition =v2TargetPosition;
        float distanceToTarget = Vector2.Distance(targetPosition, currentPosition);
        if (distanceToTarget < kfBrakeDistance)
        {
            return Arrive();
        }
        else
        {
            return Seek();
        }
    }

    /// <summary>
    /// Pursue behaviour. Tries to predict the target's future position
    /// and generates force towards it
    /// </summary>
    /// <returns></returns>
    private Vector2 Pursue(Transform _intargetTransform, float _inDeltaTime)
    {
        Vector2 currentPosition = this.transform.position;
        Vector2 targetPosition = v2TargetPosition;

        //Guess the lead entity's future position
        Vector2 leadsPosition = _intargetTransform.position;
        Vector2 leadsVelocity = (leadsPosition - targetPosition) / (float)_inDeltaTime;
        targetPosition = leadsPosition + (leadsVelocity * 2.0f);
        v2TargetPosition = targetPosition;

        return GoTo();
    }

    /// <summary>
    /// Evade behaviour. Tries to predict the target's future position
    /// and generates force away from it. If target is far away, it wanders
    /// </summary>
    private Vector2 Evade(Transform _intargetTransform, float _inDeltaTime)
    {
        Vector2 currentPosition = this.transform.position;
        Vector2 targetPosition = v2TargetPosition;

        //Guess the lead entity's future position
        Vector2 leadsPosition = _intargetTransform.position;
        Vector2 leadsVelocity = (leadsPosition - targetPosition) / (float)_inDeltaTime;
        targetPosition = leadsPosition + (leadsVelocity * 2.0f);
        v2TargetPosition = targetPosition;

        float distanceToTarget = Vector2.Distance(currentPosition, targetPosition);

        if (distanceToTarget < kfBrakeDistance)
        {
            return Flee();
        }

        else
        {
            return Wander();
        }
    }
    /// <summary>
    /// Agent start wandering in a random direction
    /// </summary>
    /// <returns></returns>
    private Vector2 WanderInRandomDirection()
    {
        fWanderCounter = kfWanderUpdateTime;
        float randomAngle = Random.Range(0.0f, 360.0f);
        Vector2 wanderForce = new Vector2(Mathf.Cos(Mathf.Deg2Rad * randomAngle), Mathf.Sin(Mathf.Deg2Rad * randomAngle));

        wanderForce *= kfMaxWanderForce;
        return wanderForce;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AgentController : MonoBehaviour
{
    public enum STATE
    {
        NONE,
        IDLE,
        WANDER,
        CHASE,
        ATTACK,
        RETURNHOME,
        IMPRISONED,
        MAX_NONE,
    }

    public enum COLOUR
	{
        NONE,
        RED,
        BLUE,
        MAX_NONE,
	}

    public class PrevPosition
	{
        public Vector2 pos;
	}

    [Header("ConnectedScripts")]
    [SerializeField] private Rigidbody2D rgdbdAgent;

    [Header("Settings")]
    [SerializeField] public BlackBoard bbHomeBoard;
    [SerializeField] private float fMoveSpeed = 0.2f;
    [SerializeField] private GameObject objMarkerPrefab;
    [SerializeField] private GameObject objMarkerCreated;
    [SerializeField] private Sprite[] sprtAgentSprites;
    [SerializeField] private Tilemap tlmpHomeArea;


    [Header("Debug")]
    [SerializeField] private Dictionary<GameObject, PrevPosition> dictOpponentAgentsNearby;
    [SerializeField] private GameObject objTarget;
    [SerializeField] private STATE      eCurrentState = STATE.IDLE;
    [SerializeField] private Vector2    v2CurrentVelocity = Vector2.zero;
    [SerializeField] private Vector2    v2TargetPosition = Vector2.zero;
    [SerializeField] private Vector2    v2HostileFlagAreaPos = Vector2.zero;
    [SerializeField] private Vector2    v2HostilePrisonAreaPos = Vector2.zero;
    [SerializeField] private float      fDelayCounter = 0.0f;
    [SerializeField] private float      fWanderCounter = 0.0f;
    [SerializeField] private bool       bIsFleeing = false;
    [SerializeField] private bool       bIsRedTeam = true;
    [SerializeField] private bool       bIsPlayerControlled = false;
    [SerializeField] private bool       bHoldingFlag = false;
	[SerializeField] private bool       bTargetIntruderSighted = false;

    //Constants
    const float kfMass = 1.0f;
    const float kfMinDelay = 2.0f;
    const float kfMaxDelay = 5.0f;
    const float kfAttackTimeout = 10.0f;
    const float kfMaxSpeed = 0.5f;
    const float kfMaxFleeSpeed = 0.55f;
    const float kfMaxForce = 0.25f;
    const float kfMaxFleeForce = 0.4f;
    const float kfFleeDistance = 0.3f;
    const float kfBrakeDistance = 0.5f;
    const float kfMaxWanderSpeed = 0.25f;
    const float kfMaxWanderForce = 0.25f;
    const float kfWanderDistance = 0.16f;
    const float kfWanderAngle = 30.0f;
    const float kfWanderUpdateTime = 0.5f;
    const int   kiTooManyHostiles = 2;

    // Start is called before the first frame update
    void Start()
    {
		if (dictOpponentAgentsNearby == null)
		{
			dictOpponentAgentsNearby = new Dictionary<GameObject, PrevPosition>(); 
		}

        if (rgdbdAgent == null)
        {
            rgdbdAgent = this.GetComponent<Rigidbody2D>();
        }
    }

	private void Awake()
	{
        ChangeState(STATE.IDLE);
        objTarget = null;
        fDelayCounter = 0.0f;
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

        if (bIsPlayerControlled && !GetIfImprisoned())
        {
            v2CurrentVelocity.x = Input.GetAxis("Horizontal");
            v2CurrentVelocity.y = Input.GetAxis("Vertical");
            v2CurrentVelocity.Normalize();
        }

        if(bHoldingFlag)
		{
            float safeZoneLine = 0.25f;
            Flag heldFlag = this.GetComponentInChildren<Flag>();
            if ((bIsRedTeam && transform.position.x < -safeZoneLine) ||
                (!bIsRedTeam && transform.position.x > safeZoneLine))
			{
                heldFlag.FlagCaptured();
                bHoldingFlag = false;
                if(!bIsPlayerControlled)
				{
                    bbHomeBoard.AttackSuccess();
				}

			}
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

        if(!GetIfImprisoned())
		{
            ChangeState(STATE.IDLE);
		}

    }
    /// <summary>
    /// Return if this agent is player controlled
    /// </summary>
    /// <returns></returns>
    public bool IsPlayerControlled()
    {
        return bIsPlayerControlled;
    }
    
    //This agent is now holding a flag
    //It should now try to return home
    public void SetHoldingFlag()
	{
        bHoldingFlag = true;
        ChangeState(STATE.RETURNHOME);
	}

    public bool IsHoldingFlag()
	{
        return bHoldingFlag;
	}

    public bool GetIfRedTeam()
	{
        return bIsRedTeam;
	}

    public bool GetIfImprisoned()
	{
        return eCurrentState == STATE.IMPRISONED ? true : false; ;
	} 
    
    public bool GetIfAttacking()
	{
        return eCurrentState == STATE.ATTACK ? true : false;
        //return bIsAttacking;
	}
    
    public bool GetIfReturningHome()
	{
        return eCurrentState == STATE.RETURNHOME ? true : false;
	}
    
    public bool GetIfPursuing()
	{
        return eCurrentState == STATE.CHASE ? true : false;
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

    public void SetAreas(Vector2 _inFlagArea, Vector2 _inPrisonArea, Tilemap _inHomeArea)
	{
        v2HostileFlagAreaPos = _inFlagArea;
        v2HostilePrisonAreaPos = _inPrisonArea;
        tlmpHomeArea = _inHomeArea;
	}

    public void ImprisonThisAgent()
	{
        bbHomeBoard.AttackFailed(this.gameObject);
        ChangeState(STATE.IMPRISONED);

        transform.position = v2HostilePrisonAreaPos;
        if(bHoldingFlag)
		{
            bHoldingFlag = false;
            Flag heldFlag = this.GetComponentInChildren<Flag>();
            heldFlag.FlagFreed();
		}
	}
    /// <summary>
    /// blackboard will 
    /// </summary>
    /// <returns></returns>
    public float DecideIfAttack()
	{
		//Distance in game is actually tiny, so in most cases distance to the center
		//will be less than one. We can take advantage of this to generate a higher 
		//score for smaller distance by subtracting from one.
		float distanceToMiddle = Mathf.Sqrt(transform.position.x * transform.position.x);
		float RandomScore = Random.Range(distanceToMiddle, (1.0f - distanceToMiddle));
		return RandomScore; 
	}

    public void DetectedIntruder(GameObject _inHostile)
	{
        float distanceToIntruder = Vector2.Distance(_inHostile.transform.position, transform.position);

        if(distanceToIntruder < (kfFleeDistance*2))
		{
            bbHomeBoard.RequestToPursue(this.gameObject,_inHostile);
		}
	}

    public void ApproveAttackRequest(bool _inCapturedFriends)
	{
        int randomMode = _inCapturedFriends?Random.Range(0,10):10;
        if(randomMode>5)
		{
            v2TargetPosition = v2HostileFlagAreaPos;
		}
        else
		{
            v2TargetPosition = v2HostilePrisonAreaPos;
        }
        ChangeState(STATE.ATTACK);
    }


    public void ApprovePursueRequest(GameObject _inTargetHostile,Vector2 _inPosition)
	{
        objTarget = _inTargetHostile;
        bTargetIntruderSighted = false;
        v2TargetPosition = _inPosition;
        ChangeState(STATE.CHASE);
	}

    public void AnnouncedIntruderNonIssue(GameObject _inHostile)
	{
        //Switch out from chase mode if the hostile was target and captured or escaped
        //OR the target is somehow null
        if(_inHostile==objTarget || objTarget == null )
		{
            objTarget = null;
            bTargetIntruderSighted = false;
            ChangeState(STATE.IDLE);
		}
	}

    /// <summary>
    /// Change the current state
    /// </summary>
    /// <param name="_inNewState"></param>
    public void ChangeState(STATE _inNewState)
    {
        Vector2 CurrentPosition = transform.position;


        switch (_inNewState)
        {
            case STATE.IDLE:
                {
                    eCurrentState = STATE.IDLE;
                    fDelayCounter = Random.Range(kfMinDelay, kfMaxDelay);
                    v2CurrentVelocity = Vector2.zero;
                    break;
                }
            case STATE.WANDER:
                {
                    eCurrentState = STATE.WANDER;
                    fDelayCounter = Random.Range(kfMinDelay, kfMaxDelay);
                    bbHomeBoard.CheckIfStillOnReturningSet(this.gameObject);
                    CalculateVelocity(WanderInRandomDirection(Mathf.Sqrt(transform.position.x * transform.position.x)));
                    break;
                }
            case STATE.CHASE:
				{
                    eCurrentState = STATE.CHASE;
                    CalculateVelocity(GoTo());
                    return;
				}
            case STATE.ATTACK:
                {
                    eCurrentState = STATE.ATTACK;
                    fDelayCounter = kfAttackTimeout;
                    CalculateVelocity(Attack());
                    break;
                }
            case STATE.RETURNHOME:
                {
                    eCurrentState = STATE.RETURNHOME;
                    bbHomeBoard.ReturningHome(this.gameObject);
                    v2TargetPosition.x = -v2HostileFlagAreaPos.x;
                    CalculateVelocity(Attack());
                    break;
                }
            case STATE.IMPRISONED:
				{
                    eCurrentState = STATE.IMPRISONED;
                    v2CurrentVelocity = Vector2.zero;
                    break;
				}
            default:
                {
                    break;
                }
        }
    }
    private void FixedUpdate()
	{
        Vector2 currentPosition = transform.position;

        if (bIsPlayerControlled && !GetIfImprisoned())
        {
            currentPosition += v2CurrentVelocity * fMoveSpeed * Time.fixedDeltaTime;
            transform.position = currentPosition;
        }
        else
        {
            currentPosition += v2CurrentVelocity * Time.fixedDeltaTime;
            transform.position = currentPosition;

            int iSideModifier = bIsRedTeam ? -1 : 1;

            //If we're pass the middle line, go home. but only if we're no attacking or am imprisoned
            if ( !GetIfAttacking() && !GetIfImprisoned()  &&
                (bIsRedTeam ? transform.position.x > 0.0f : transform.position.x < 0.0f))
            {
                ChangeState(STATE.RETURNHOME);
                return;
            }

            //If we're pursuing a target and it's gone past the middle, then revert to idle
            if (GetIfPursuing() &&
                (bIsRedTeam ? v2TargetPosition.x > 0.0f : v2TargetPosition.x < 0.0f))
            {
                if(objTarget != null)
				{
                    bbHomeBoard.IntruderDealtWith(objTarget);
				}
                objTarget = null;
                bTargetIntruderSighted = false;
                ChangeState(STATE.IDLE);
                return;
            }

            switch (eCurrentState)
            {
                case STATE.IDLE:
                    {
                        if (fDelayCounter <= 0)
                        {
                            ChangeState(STATE.WANDER);
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
                            CalculateVelocity(Wander(Mathf.Sqrt(transform.position.x * transform.position.x)));
                        }
                        break;
                    }
                case STATE.CHASE:
                    {
                        if(bTargetIntruderSighted)
						{
                            v2TargetPosition = objTarget.transform.position;
                            CalculateVelocity(GoTo());
                        }
						else
						{
                            float distanceToIntruder = Vector2.Distance(v2TargetPosition, transform.position);

                            if (distanceToIntruder > (kfBrakeDistance/2.0f))
						    {
                                CalculateVelocity(GoTo());
						    }
						    else
						    {
                                bbHomeBoard.RequestLastPosition(objTarget, v2TargetPosition);
                                CalculateVelocity(GoTo());
						    }
						}

                        

                        break;
                    }
                case STATE.ATTACK:
					{
                        if(dictOpponentAgentsNearby.Count >= kiTooManyHostiles || fDelayCounter < 0)
						{
                            print("Situation abnormal. Returning home");
                            ChangeState(STATE.RETURNHOME);
						}
						else
						{
                            CalculateVelocity(Attack());
						}

                        break;
					}
                case STATE.RETURNHOME:
					{
                        v2TargetPosition.y = transform.position.y;
                        if(Vector2.Distance(v2TargetPosition,transform.position) < kfBrakeDistance*2)
						{
                            bbHomeBoard.SafelyReturnedHome(this.gameObject);
                            ChangeState(STATE.IDLE);
						}
                        else
						{
                          CalculateVelocity(Attack());
						}

                        break;
					}
                default:
                    {
                        break;
                    }
            }
        }
    }
    /// <summary>
    /// Invert move directions when colliding with
    /// the corresponding wall.
    /// I.e.: flip x direction when colliding with
    /// West/East wall
    /// Or send opponent to prison if collided over home territory
    /// </summary>
    /// <param name="collision"></param>
	private void OnCollisionEnter2D(Collision2D collision)
	{
        if (collision.collider.tag == "WallNS")
        {
            v2CurrentVelocity.y *= -1.0f;
        }
        else if (collision.collider.tag == "WallWE")
        {
            v2CurrentVelocity.x *= -1.0f;
        }
        else if (collision.collider.tag == "Agent")
        {
            AgentController collidedAgent = collision.gameObject.GetComponent<AgentController>();
            if (collidedAgent && collidedAgent.bIsRedTeam != bIsRedTeam && !collidedAgent.GetIfImprisoned() &&
                tlmpHomeArea.HasTile(tlmpHomeArea.WorldToCell(collision.transform.position)))
            {
                print("Collided with opponent");
                collidedAgent.ImprisonThisAgent();
                bbHomeBoard.IntruderDealtWith(collision.gameObject);
            }
            else if (collidedAgent && collidedAgent.bIsRedTeam == bIsRedTeam &&
                collidedAgent.GetIfImprisoned() && !GetIfImprisoned())
			{
                print("Rescued a captive");
                bbHomeBoard.RescueSuccess();
                //bbHomeBoard.ReturningHome();
                collidedAgent.ChangeState(STATE.RETURNHOME);
                ChangeState(STATE.RETURNHOME);
			}
		}
    }

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if(collision.tag== "Agent" )
		{
            AgentController collidedAgent = collision.gameObject.GetComponent<AgentController>();
            if (collidedAgent)
			{
				if (collidedAgent.bIsRedTeam != bIsRedTeam)
				{
					if (!dictOpponentAgentsNearby.ContainsKey(collision.gameObject))
					{
						print("Hostile agent is close by and registered");
						dictOpponentAgentsNearby.Add(collision.gameObject, new PrevPosition { pos = collision.transform.position });
					}

                    //Unless imprisoned or attacking, inform blackboard
                    if (!GetIfImprisoned() && !GetIfAttacking() && !collidedAgent.GetIfImprisoned())
                    {
                        bbHomeBoard.DetectedHostile(collision.gameObject);
                        bbHomeBoard.AgentPursuingHostile(collision.gameObject);
                        objTarget = collision.gameObject;
                        v2TargetPosition = objTarget.transform.position;
                        ChangeState(STATE.CHASE);
                    }

                    if(GetIfPursuing() && collision.gameObject == objTarget)
					{
                        bTargetIntruderSighted = true;
					}
                }
				else if(GetIfAttacking() && collidedAgent.bIsRedTeam == bIsRedTeam && collidedAgent.GetIfImprisoned())
				{
                    print("Captive nearby");
                    v2TargetPosition = collidedAgent.transform.position;
                } 
			}
        }
        else if(GetIfAttacking() && collision.tag == "Flag" && !bHoldingFlag)
		{
            Flag flag = collision.gameObject.GetComponent<Flag>();
            if(flag)
			{
                v2TargetPosition = flag.transform.position;
            }
        }
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
        if (collision.tag == "Agent" )
        {
            AgentController collidedAgent = collision.gameObject.GetComponent<AgentController>();
            if (collidedAgent && collidedAgent.bIsRedTeam != bIsRedTeam)
            {
                print("Lost sight of a hostile agent");
                dictOpponentAgentsNearby.Remove(collision.gameObject);
            }
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
            v2CurrentVelocity = Vector2.ClampMagnitude(v2CurrentVelocity, bIsFleeing ? kfMaxFleeSpeed : kfMaxSpeed);
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

    private Vector2 Attack()
	{
        Vector2 fleeForce = Vector2.zero;
        Vector2 currentTargetPosition = v2TargetPosition;

        if (dictOpponentAgentsNearby.Count > 0)
		{
            foreach (var hostile in dictOpponentAgentsNearby.Keys)
			{
                //Only add flee force if the hostile is not on home tiles
                if( !tlmpHomeArea.HasTile(tlmpHomeArea.WorldToCell(hostile.transform.position)) )
				{
                    fleeForce += Flee(hostile.transform.position);
				}
			}
		}

        v2TargetPosition = currentTargetPosition;

        Vector2 GoToForce = GoTo();
        Vector2 resultantForce =    (GoToForce * (fleeForce.magnitude>0?0.1f:1.0f)) + 
                                    (fleeForce * (GoToForce.magnitude>0?0.9f:1.0f));
        bIsFleeing = fleeForce.magnitude > 0 ? true : false;
        return Vector2.ClampMagnitude( resultantForce, bIsFleeing ? kfMaxFleeSpeed : kfMaxSpeed );
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

        float angleToTarget = Vector2.Angle(currentPosition, targetPosition);

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
    private Vector2 Wander(float _inDistanceToMiddle)
    {
        //the closet this agent is to the middle, the higher this ratio will be
        float distanceToMiddleRatio = StaticVariables.fNoManLandWidth / _inDistanceToMiddle;// Mathf.Sqrt(transform.position.x * transform.position.x);

        if (fWanderCounter <= 0 || distanceToMiddleRatio>=1.0f)
        {
            fWanderCounter = kfWanderUpdateTime;
            Vector2 currentVelocity = v2CurrentVelocity;

            float randomAngle = (Random.Range(-kfWanderAngle, kfWanderAngle) * (1 - distanceToMiddleRatio)) +
                              ((bIsRedTeam ? Random.Range(135.0f, 225.0f) : Random.Range(315.0f, 405.0f)) * distanceToMiddleRatio); ;
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
    private Vector2 Flee(Vector2 _inPositionToAvoid)
    {
        Vector2 currentPosition = this.transform.position;
        Vector2 targetPosition = _inPositionToAvoid;
        Vector2 currentVelocity = v2CurrentVelocity;
        Vector2 desiredVelocity;
        Vector2 steeringForce;

        desiredVelocity = (currentPosition - targetPosition).normalized * kfMaxSpeed;
        steeringForce = desiredVelocity - currentVelocity;
        steeringForce /= kfMass;
        steeringForce = Vector2.ClampMagnitude(steeringForce, kfMaxFleeForce);
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
    private Vector2 Evade(GameObject _Hostile)
    {
        Vector2 currentPosition = this.transform.position;
        Vector2 targetPosition = Vector2.zero;

        //Guess the hostile entity's future position using the stored previous known position
        Vector2 hostilePosition = _Hostile.transform.position;
        Vector2 hostileVelocity = (hostilePosition - dictOpponentAgentsNearby[_Hostile].pos) / Time.fixedDeltaTime;
        //Guess position in 5 frames
        targetPosition = hostilePosition + (hostileVelocity * 5.0f);
        //Update the paired list with the hostile's current position
        dictOpponentAgentsNearby[_Hostile].pos = hostilePosition;

        float distanceToTarget = Vector2.Distance(currentPosition, targetPosition);


         return Flee(targetPosition);

    }
    /// <summary>
    /// Agent start wandering in a random direction
    /// </summary>
    /// <returns></returns>
    private Vector2 WanderInRandomDirection(float _inDistanceToMiddle)
    {
        //the closest this agent is to the middle, the higher this ratio will be
        float distanceToMiddleRatio = StaticVariables.fNoManLandWidth / _inDistanceToMiddle;// Mathf.Sqrt(transform.position.x * transform.position.x);
        fWanderCounter = kfWanderUpdateTime;
        float randomAngle = (Random.Range(0.0f,360.0f) * (1-distanceToMiddleRatio)) + 
                            ((bIsRedTeam ? Random.Range(135.0f, 225.0f) : Random.Range(315.0f, 405.0f)) * distanceToMiddleRatio);
        Vector2 wanderForce = new Vector2(Mathf.Cos(Mathf.Deg2Rad * randomAngle), Mathf.Sin(Mathf.Deg2Rad * randomAngle));

        wanderForce *= kfMaxWanderForce;
        return wanderForce;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Main behaviour script for slime
/// </summary>
public class SlimeBehaviour : MonoBehaviour
{
    public enum STATE
    {
        IDLE,
        WANDER,
        MOVE,
        SEEKPLAYER,
        FIGHT,
        FLEE,
        GETHIT,
    }
    [Header("ConnectedScripts")]
    [SerializeField] private Rigidbody2D m_rgdbdSlime;
    [SerializeField] private UnityEngine.AI.NavMeshAgent m_NavMeshAgent;
    [SerializeField] private Animator m_Animator = null;

    [Header("Settings")]
    [SerializeField] private float m_fMoveSpeed = 0.5f;
    [SerializeField] private float m_fAcceleration = 1.0f;
    [SerializeField] private float m_fMoveDistance = 1.0f;
    [SerializeField] private float m_fChargeForce = 0.25f;
    [SerializeField] private float m_fIdleTime = 2.0f;
    [SerializeField] private float m_fFriendSearchRadius = 2.0f;
    [SerializeField] private int m_iMaxSearchTries = 30;
    [SerializeField] private float m_fDeathAnimLength = 1.2f;
    [SerializeField] private float m_fAttackDuraation = 1.5f;

    [Header("Debug")]
    [SerializeField] private STATE m_eCurrentState = STATE.IDLE;
    [SerializeField] private float m_fMass = 1.0f;
    //[SerializeField] private Vector2 m_v2CurrentPosition = Vector2.zero;
    [SerializeField] private Vector2 m_v2CurrentVelocity = Vector2.zero;
    [SerializeField] private Vector2 m_v2CurrentFacing = Vector2.zero;
    [SerializeField] private Vector2 m_v2TargetPosition = Vector2.zero;
    [SerializeField] private float m_fDelayCounter = 0.0f;
    [SerializeField] private float m_fCurrentSpeed = 0.0f;
    [SerializeField] private float m_fCurrentAccel = 0.0f;
    [SerializeField] private bool m_bIsPlayerControlled = false;
    [SerializeField] private bool m_bDangerNearby = false;
    [SerializeField] private bool m_bAnimatorAttached = false;
    [SerializeField] private GameObject m_objPlayer;
    [SerializeField] private LayerMask m_maskSlimes;

    [Header("Constants")]
    [SerializeField] private const float m_fMaxSpeed = 0.25f;
    [SerializeField] private const float m_fMaxWanderSpeed = 0.1f;
    [SerializeField] private const float m_fMaxWanderForce = 2.0f;
    [SerializeField] private const float m_fMaxForce = 0.025f;
    [SerializeField] private const float m_fFleeDistance = 100.0f;
    [SerializeField] private const float m_fBrakeDistance = 150.0f;
    [SerializeField] private const float m_fWanderDistance = 50.0f;
    [SerializeField] private const float m_fWanderRadius = 20.0f;
    [SerializeField] private const float m_fDistanceFromLead = 75.0f;
    [SerializeField] private const float m_fSeparationRange = 25.0f;
    [SerializeField] private const float m_fGroupCohesionRange = 100.0f;
    [SerializeField] private const int m_fWanderAngleMin = 45;
    [SerializeField] private const int m_fWanderAngleMax = 60;

    // Start is called before the first frame update\
    //Make sure non-settings variables aare set to default
    private void Start()
    {
        if (m_Animator == null)
        {
            print("A slime has no animator attached.");
        }
        else
        {
            m_bAnimatorAttached = true;
        }

        m_objPlayer = GameObject.FindGameObjectWithTag("Player"); ;
    }

    private void Awake()
    {
        //m_objHive = GameObject.FindGameObjectWithTag("HiveMind").GetComponent<SlimeManagement>();
        ChangeState(STATE.IDLE);
        //m_v2Destination = Vector2.zero;
        m_v2CurrentFacing = Vector2.zero;
        m_fDelayCounter = 0.0f;
        m_bDangerNearby = false;
        m_NavMeshAgent.acceleration = m_fAcceleration;
    }

    /// <summary>
    /// If slime collides with player while attacking, reduce his health
    /// </summary>
    /// <param name="collision"></param>
	private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player" && m_eCurrentState == STATE.FIGHT)
        {
            collision.gameObject.GetComponent<PlayerControls>().GetHit(1.0f);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (m_fDelayCounter > 0)
        {
            m_fDelayCounter -= Time.deltaTime;
        }

        if (m_bAnimatorAttached)
        {
            Vector2 CurrentPosition = transform.position;
            //m_v2CurrentFacing = m_v2Destination - CurrentPosition;
            m_Animator.SetFloat("FacingX", m_v2CurrentFacing.x);
            m_Animator.SetFloat("FacingY", m_v2CurrentFacing.y);
            m_Animator.SetFloat("Speed", m_v2CurrentFacing.magnitude);
        }

        //m_fCurrentSpeed = m_NavMeshAgent.speed;
        //m_fCurrentAccel = m_NavMeshAgent.acceleration;
    }

    private void FixedUpdate()
    {
        //If not in flee,seek or fight state, check
        //if it needs to be
        if (m_eCurrentState != STATE.FLEE &&
            m_eCurrentState != STATE.SEEKPLAYER &&
            m_eCurrentState != STATE.FIGHT)
        {
            CheckIfNeedToFlee();
        }

        switch (m_eCurrentState)
        {
            case STATE.IDLE:
                {
                    if (Idle())
                    {
                        if (m_fDelayCounter <= 0)
                        {

                        }
                        //if(m_iFoodEaten< m_iWhenMultiply)
                        //{
                        //    ChangeState(STATE.WANDER);
                        //    //Everytime slime wants to wander, increase food eaten
                        //    //Temporary until food objects added
                        //    m_iFoodEaten++;
                        //}
                        //else
                        //{
                        //    ChangeState(STATE.MULTIPLY);
                        //}
                    }
                    break;
                }
            case STATE.MOVE:
                {
                    break;
                }
            case STATE.WANDER:
                {
                    //if (Wander())
                    //{
                    //    FullBrake();
                    //    ChangeState(STATE.IDLE);
                    //}
                    break;
                }
            case STATE.SEEKPLAYER:
                {
                    if (SeekPlayer())
                    {
                        FullBrake();
                        ChangeState(STATE.FIGHT);
                    }
                    else
                    {
                        ChangeState(STATE.SEEKPLAYER);
                    }
                    break;
                }
            case STATE.FIGHT:
                {
                    //Should not be in this state
                    ChangeState(STATE.IDLE);
                    break;
                }
            case STATE.FLEE:
                {
                    //if (Flee())
                    //{
                    //    FullBrake();
                    //    ChangeState(STATE.IDLE);
                    //}
                    //else
                    //{
                    //    ChangeState(STATE.FLEE);
                    //}
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

    public void EncounteredDanger()
    {
        m_bDangerNearby = true;
    }

    public void FledFromDanger()
    {
        m_bDangerNearby = false;
    }

    /// <summary>
    /// Slime was hit. gets killed immediately
    /// </summary>
    public void GetHit()
    {

        m_Animator.SetBool("Dead", true);
        m_Animator.SetTrigger("GetHit");
        GetComponent<Collider2D>().enabled = false;
        StartCoroutine(PrepareToDisappear());
        FullBrake();
        ChangeState(STATE.GETHIT);
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
                    //Stop all movement
                    FullBrake();
                    //m_v2Destination = Vector3.zero;
                    m_fDelayCounter = 0.0f;
                    m_eCurrentState = STATE.IDLE;
                    break;
                }
            case STATE.MOVE:
                {
                    break;
                }
            case STATE.WANDER:
                {
                    ////Find a position nearby to move to
                    //int iDestSearchTries = 0;
                    //bool bDestFound = false;
                    //int fDistance = Random.Range(1,3);

                    ////Try 30 times to find a valid destination
                    //while( (iDestSearchTries<m_iMaxSearchTries) && !bDestFound)
                    //{
                    //    m_v2Destination = PickRandomDestOnNavMesh(fDistance * m_fMoveDistance);
                    //    if((m_v2Destination - CurrentPosition).magnitude > m_fMoveDistance)
                    //    {
                    //        bDestFound = true;
                    //        //m_ScriptAINavigate.GoToPosition(m_v2Destination, m_fMoveSpeed);
                    //    }
                    //    iDestSearchTries++;
                    //}
                    //m_eCurrentState = STATE.WANDER;

                    ////If we couldn't find a valid destination, change back to idle state
                    //if (!bDestFound)
                    //{
                    //    ChangeState(STATE.IDLE);
                    //}
                    break;
                }
            case STATE.SEEKPLAYER:
                {
                    //Slime gonna try attack, move to his current position
                    //m_ScriptAINavigate.GoToPosition(m_objPlayer.transform.position, m_fMoveSpeed, m_fMoveDistance * 3.0f);
                    m_eCurrentState = STATE.SEEKPLAYER;
                    break;
                }
            case STATE.FIGHT:
                {
                    //Slime should be in range to attack
                    StartCoroutine(Fight());
                    break;
                }
            case STATE.FLEE:
                {
                    if (m_objPlayer != null)
                    {
                        //               //Check if there are nearby slimes. if so, move towards them. 
                        //               //if not, get the direction the player is at and move to aa position=
                        //               //in the opposite direction
                        //               Vector2 m_vec3FleeToDirection = transform.position;
                        //               if (m_iNearbyFriends>0)
                        //{
                        //                       Collider2D[] friendly = Physics2D.OverlapCircleAll(transform.position, m_fFriendSearchRadius, m_maskSlimes);
                        //                       float CurrentClosestFriendly = 9999.0f;
                        //                       float DistanceToPlayer = (transform.position - m_objPlayer.transform.position).sqrMagnitude;
                        //                       Collider2D ChosenFriendly = null;

                        //                       foreach(Collider2D slime in friendly)
                        //		{
                        //                           float distanceToFriendly = (transform.position - slime.transform.position).sqrMagnitude;
                        //                           if(distanceToFriendly < CurrentClosestFriendly)
                        //			{
                        //                               ChosenFriendly = slime;
                        //                               CurrentClosestFriendly = distanceToFriendly;
                        //			}
                        //		}

                        //                       if(CurrentClosestFriendly < DistanceToPlayer)
                        //		{
                        //                           m_vec3FleeToDirection = ChosenFriendly.transform.position;
                        //		}
                        //		else
                        //		{
                        //                           m_vec3FleeToDirection = transform.position - m_objPlayer.transform.position;
                        //		}

                        //                   }
                        //               else
                        //{
                        //                   m_vec3FleeToDirection = transform.position - m_objPlayer.transform.position;
                        //               }
                        //               m_vec3FleeToDirection.Normalize();
                        //               m_v2CurrentFacing = m_vec3FleeToDirection;
                        //               m_v2Destination = m_vec3FleeToDirection * 2 * m_fMoveDistance;
                        //               //m_ScriptAINavigate.GoToPosition(CurrentPosition + m_v2Destination, m_fMoveSpeed*1.5f);
                        //               m_eCurrentState = STATE.FLEE;
                    }
                    else
                    {
                        print("No player in scene");
                        ChangeState(STATE.IDLE);
                    }
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
    /// Idle state does nothing until delay counter reaches
    /// 0
    /// </summary>
    /// <returns></returns>
    private bool Idle()
    {
        if (m_fDelayCounter > 0)
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
        Vector2 currentVelocity = m_v2CurrentVelocity;
        Vector2 currentPosition = this.transform.position;
        Vector2 targetPosition = m_v2TargetPosition;
        Vector2 desiredVelocity;
        Vector2 steeringForce;

        float angleToTarget = Vector2.Angle(currentPosition, targetPosition);//(float)((atan2(targetPosition.y, targetPosition.x) - atan2(currentPosition.y, currentPosition.x)) * (180 / PI));

        desiredVelocity = (targetPosition - currentPosition).normalized * m_fMaxSpeed;
        steeringForce = desiredVelocity - currentVelocity;
        steeringForce /= m_fMass;
        steeringForce = Vector2.ClampMagnitude(steeringForce, m_fMaxForce);

        return steeringForce;
    }

    /// <summary>
    /// Slime will wonder for a previously set time. Returns true
    /// once the set time has passed
    /// </summary>
    /// <returns></returns>
    private Vector2 Wander()
    {
        Vector2 currentVelocity = m_v2CurrentVelocity;

        float randomAngle = Random.Range(m_fWanderAngleMin, m_fWanderAngleMax);
        Vector2 wanderCircle = currentVelocity.normalized * m_fWanderDistance;
        Vector2 wanderForce = new Vector2(Mathf.Cos(randomAngle / 1.0f), Mathf.Sin(randomAngle / 1.0f)) * m_fMaxWanderForce;
        Vector2 displaceForce = wanderCircle + wanderForce;
        return displaceForce;
        //if(m_ScriptAINavigate.GetIfReachedPosition())
        //{
        //    return true;
        //}
        //else
        //{
        //    return false;
        //}]
        //return true;
    }
    /// <summary>
    /// Arrive behaviour. Generates reducing force as it approcahes the target position
    /// </summary>
    /// <param name="_inEntity"></param>
    /// <returns></returns>
    private Vector2 Arrive()
	{
        Vector2 desiredVelocity;
        Vector2 steeringForce;
        Vector2 currentVelocity = m_v2CurrentVelocity;
        Vector2 currentPosition = this.transform.position;
        Vector2 targetPosition = m_v2TargetPosition;
        Vector2 offsetToTarget = targetPosition - currentPosition;
        float offsetDistance = offsetToTarget.sqrMagnitude;

        if (offsetDistance > 0)
        {
            float reducedSpeed = (offsetDistance / m_fBrakeDistance) * m_fMaxSpeed;
            reducedSpeed = Mathf.Min(reducedSpeed, m_fMaxSpeed);
            desiredVelocity = (reducedSpeed / offsetDistance) * offsetToTarget;
            steeringForce = desiredVelocity - currentVelocity;
            steeringForce /= m_fMass;
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
        Vector2 targetPosition = m_v2TargetPosition;
        Vector2 currentVelocity = m_v2CurrentVelocity;
        Vector2 desiredVelocity;
        Vector2 steeringForce;

        float angleToTarget = Vector2.Angle(targetPosition, currentPosition);//(float)((atan2(currentPosition.y, currentPosition.x) - atan2(targetPosition.y, targetPosition.x)) * (180 / PI));

        desiredVelocity = (currentPosition - targetPosition).normalized * m_fMaxSpeed;
        steeringForce = desiredVelocity - currentVelocity;
        steeringForce /= m_fMass;
        steeringForce = Vector2.ClampMagnitude(steeringForce, m_fMaxForce);
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
        Vector2 targetPosition = m_v2TargetPosition;
        float distanceToTarget = Vector2.SqrMagnitude(targetPosition - currentPosition);
        if (distanceToTarget < m_fBrakeDistance)
        {
            return Arrive();
        }
        else
        {
            return Seek();
        }
    }

    /// <summary>
    /// Commented code is suppose to have the slime move to a set distance
    /// then attack. But instead makes them try to hug the player
    /// </summary>
    /// <returns></returns>
    private bool SeekPlayer()
    {
        //if (m_ScriptAINavigate.GetIfReachedPosition())
        //{
        return true;
        //}
        //else
        //{
        //    return false;
        //}
    }

    /// <summary>
    /// In flee state, keep checking if danger is nearby. 
    /// </summary>
    /// <returns>false if danger still close, true if successfuly fled</returns>
    //private bool Flee()
    //{
    //    if (m_bDangerNearby)
    //    {
    //        return false;
    //    }
    //    else
    //    {
    //        ChangeState(STATE.IDLE);
    //        return true;
    //    }
    //}

    /// <summary>
    /// return a random location on the navmesh in a radius around the slime
    /// </summary>
    /// <param name="_inDistance"></param>
    /// <returns></returns>
    private Vector3 PickRandomDestOnNavMesh(float _inDistance)
    {
        Vector3 randomDirection = Random.insideUnitSphere * _inDistance;
        randomDirection += transform.position;
        NavMeshHit suitablePosition;
        Vector3 targetPosition = Vector3.zero;

        if (NavMesh.SamplePosition(randomDirection, out suitablePosition, _inDistance, 1))
        {
            targetPosition = suitablePosition.position;
        }

        return targetPosition;
    }

    private void CheckIfNeedToFlee()
    {
        if (m_bDangerNearby)
        {
            //m_fDelayCounter = 0.0f;
            //if (m_iNearbyFriends < 5)
            //{
            //    ChangeState(STATE.FLEE);
            //}
            //else
            //{
            //    print("This slime wants to fight");
            //    ChangeState(STATE.SEEKPLAYER);
            //}
        }
    }

    private void FullBrake()
    {
        //m_ScriptAINavigate.FullBrake();
    }

    //run the death aanimation, then destroy self after a set time
    private IEnumerator PrepareToDisappear()
    {
        yield return new WaitForSeconds(m_fDeathAnimLength);
        //m_objHive.SlimeDied();
        //Destroy(gameObject);
    }

    //Add an impulse force to launch slime at player
    private IEnumerator Fight()
    {
        m_eCurrentState = STATE.FIGHT;
        Vector2 ForceDirection = (m_objPlayer.transform.position - transform.position).normalized;
        m_rgdbdSlime.AddForce(ForceDirection * m_fChargeForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(m_fAttackDuraation);

        ChangeState(STATE.FLEE);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, m_fFriendSearchRadius);
    }

    private void CalculateVelocity()
    {

    }
}

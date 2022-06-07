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
    //[SerializeField] private UnityEngine.AI.NavMeshAgent m_NavMeshAgent;
    [SerializeField] private Animator m_Animator = null;

    [Header("Settings")]
    [SerializeField] private float m_fMoveSpeed = 0.2f;
    //[SerializeField] private float m_fAcceleration = 1.0f;
    //[SerializeField] private float m_fMoveDistance = 1.0f;
    //[SerializeField] private float m_fChargeForce = 0.25f;
    //[SerializeField] private float m_fIdleTime = 2.0f;
    //[SerializeField] private int m_iMaxSearchTries = 30;
    //[SerializeField] private float m_fDeathAnimLength = 1.2f;
    //[SerializeField] private float m_fAttackDuration = 1.5f;

    [Header("Debug")]
    [SerializeField] private STATE m_eCurrentState = STATE.IDLE;
    [SerializeField] private Vector2 m_v2CurrentVelocity = Vector2.zero;
    [SerializeField] private Vector2 m_v2CurrentFacing = Vector2.zero;
    [SerializeField] private Vector2 m_v2TargetPosition = Vector2.zero;
    [SerializeField] private float m_fDelayCounter = 0.0f;
    [SerializeField] private float m_fWanderCounter = 0.0f;
    [SerializeField] private float m_fCurrentSpeed = 0.0f;
    [SerializeField] private float m_fCurrentAccel = 0.0f;
    [SerializeField] private bool m_bIsPlayerControlled = false;
    [SerializeField] private bool m_bDangerNearby = false;
    [SerializeField] private bool m_bAnimatorAttached = false;
    [SerializeField] private LayerMask m_maskSlimes;

    [Header("Constants")]
    [SerializeField] private const float m_kfMass = 1.0f;
    [SerializeField] private const float m_kfMinDelay = 2.0f;
    [SerializeField] private const float m_kfMaxDelay = 5.0f;
    [SerializeField] private const float m_kfMaxSpeed = 0.16f;
    [SerializeField] private const float m_kfMaxForce = 0.025f;
    [SerializeField] private const float m_kfFleeDistance = 0.5f;
    [SerializeField] private const float m_kfBrakeDistance = 0.5f;
    [SerializeField] private const float m_kfMaxWanderSpeed = 0.1f;
    [SerializeField] private const float m_kfMaxWanderForce = 0.5f;
    [SerializeField] private const float m_kfWanderDistance =0.16f;
    [SerializeField] private const float m_kfWanderRadius = 0.2f;
    [SerializeField] private const float m_kfWanderAngleMin = -90.0f;
	[SerializeField] private const float m_kfWanderAngleMax = 90.0f;
	[SerializeField] private const float m_kfWanderUpdatetime = 0.5f;


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

        if(m_rgdbdSlime == null)
		{
            m_rgdbdSlime = this.GetComponent<Rigidbody2D>();
		}

       // m_objPlayer = GameObject.FindGameObjectWithTag("Player"); ;
    }

    private void Awake()
	{ 
        ChangeState(STATE.IDLE);
        m_v2CurrentFacing = Vector2.zero;
        m_fDelayCounter = 0.0f;
        m_bDangerNearby = false;
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

        if(m_fWanderCounter>0)
		{
            m_fWanderCounter -= Time.deltaTime;
		}

        if(Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E) )
		{
            TogglePlayerControl();
            if (m_bIsPlayerControlled)
                print("Assuming Direct Control");
            else
                print("Releasing Direct Control");
		}

        if (m_bIsPlayerControlled)
        {
            m_v2CurrentVelocity.x = Input.GetAxis("Horizontal");
            m_v2CurrentVelocity.y = Input.GetAxis("Vertical");
            m_v2CurrentVelocity.Normalize();
        }

        if (m_bAnimatorAttached)
        {
            Vector2 CurrentPosition = transform.position;
            //m_v2CurrentFacing = m_v2Destination - CurrentPosition;
            m_Animator.SetFloat("FacingX", m_v2CurrentFacing.x);
            m_Animator.SetFloat("FacingY", m_v2CurrentFacing.y);
            m_Animator.SetFloat("Speed", m_v2CurrentFacing.magnitude);
        }

    }

    private void FixedUpdate()
    {
        //If not in flee,seek or fight state, check
        //if it needs to be
        //if (m_eCurrentState != STATE.FLEE &&
        //    m_eCurrentState != STATE.SEEKPLAYER &&
        //    m_eCurrentState != STATE.FIGHT)
        //{
        //    CheckIfNeedToFlee();
        //}
        Vector2 currentPosition = transform.position;

        if (m_bIsPlayerControlled)
		{
            currentPosition += m_v2CurrentVelocity * m_fMoveSpeed * Time.fixedDeltaTime;
            transform.position = currentPosition;
        }
		else
		{
            currentPosition += m_v2CurrentVelocity * Time.fixedDeltaTime;
            transform.position = currentPosition;

            switch (m_eCurrentState)
            {
                case STATE.IDLE:
                    {
                        if (m_fDelayCounter <= 0)
                        {
                            ChangeState(STATE.WANDER);
                        }
                        else
                        {
                        }
                        break;
                    }
                case STATE.MOVE:
                    {
                        break;
                    }
                case STATE.WANDER:
                    {
                        if (m_fDelayCounter <= 0)
                        {
                            ChangeState(STATE.IDLE);
                            m_fWanderCounter = 0;
                        }
                        else
                        {
                            CalculateVelocity(Wander());
                        }
                        break;
                    }
                case STATE.SEEKPLAYER:
                    {
                        if (SeekPlayer())
                        {
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

    /// <summary>
    /// Slime was hit. gets killed immediately
    /// </summary>
    public void GetHit()
    {

        m_Animator.SetBool("Dead", true);
        m_Animator.SetTrigger("GetHit");
        GetComponent<Collider2D>().enabled = false;
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
                    m_fDelayCounter = Random.Range(m_kfMinDelay, m_kfMaxDelay);
                    m_v2CurrentVelocity = Vector2.zero;
                    m_eCurrentState = STATE.IDLE;
                    break;
                }
            case STATE.MOVE:
                {
                    break;
                }
            case STATE.WANDER:
                {
                    m_fDelayCounter = Random.Range(m_kfMinDelay, m_kfMaxDelay);
                    m_eCurrentState = STATE.WANDER;
                    break;
                }
            case STATE.SEEKPLAYER:
                {
                    //Slime gonna try attack, move to his current position
                    //m_ScriptAINavigate.GoToPosition(m_objPlayer.transform.position, m_fMoveSpeed, m_fMoveDistance * 3.0f);
                    m_eCurrentState = STATE.SEEKPLAYER;
                    break;
                }
            case STATE.FLEE:
                {
                    //if (m_objPlayer != null)
                    //{

                    //}
                    //else
                    //{
                    //    print("No player in scene");
                    //    ChangeState(STATE.IDLE);
                    //}
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

        desiredVelocity = (targetPosition - currentPosition).normalized * m_kfMaxSpeed;
        steeringForce = desiredVelocity - currentVelocity;
        steeringForce /= m_kfMass;
        steeringForce = Vector2.ClampMagnitude(steeringForce, m_kfMaxForce);

        return steeringForce;
    }

    /// <summary>
    /// Slime will wonder for a previously set time. Returns true
    /// once the set time has passed
    /// </summary>
    /// <returns></returns>
    private Vector2 Wander()
    {
		if (m_fWanderCounter <= 0)
		{
			m_fWanderCounter = m_kfWanderUpdatetime;
			Vector2 currentVelocity = m_v2CurrentVelocity;
			float randomAngle = Random.Range(m_kfWanderAngleMin, m_kfWanderAngleMax);
			Vector2 wanderCircle = currentVelocity.normalized * m_kfWanderDistance;
			Vector2 wanderForce = new Vector2(Mathf.Cos(Mathf.Deg2Rad * randomAngle), Mathf.Sin(Mathf.Deg2Rad * randomAngle)) * m_kfMaxWanderForce;
			Vector2 displaceForce = wanderCircle + wanderForce;
			return displaceForce;
	}
		else
		{
            return Vector2.zero;
		}
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
            float reducedSpeed = (offsetDistance / m_kfBrakeDistance) * m_kfMaxSpeed;
            reducedSpeed = Mathf.Min(reducedSpeed, m_kfMaxSpeed);
            desiredVelocity = (reducedSpeed / offsetDistance) * offsetToTarget;
            steeringForce = desiredVelocity - currentVelocity;
            steeringForce /= m_kfMass;
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

        float angleToTarget = Vector2.Angle(targetPosition, currentPosition);

        desiredVelocity = (currentPosition - targetPosition).normalized * m_kfMaxSpeed;
        steeringForce = desiredVelocity - currentVelocity;
        steeringForce /= m_kfMass;
        steeringForce = Vector2.ClampMagnitude(steeringForce, m_kfMaxForce);
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
        float distanceToTarget = Vector2.Distance(targetPosition, currentPosition);
        if (distanceToTarget < m_kfBrakeDistance)
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
    Vector2 Pursue(Transform _intargetTransform,float _inDeltaTime)
    {
	    Vector2 currentPosition = this.transform.position;
        Vector2 targetPosition = m_v2TargetPosition;

        //Guess the lead entity's future position
        Vector2 leadsPosition = _intargetTransform.position;
        Vector2 leadsVelocity = (leadsPosition - targetPosition) / (float)_inDeltaTime;
        targetPosition = leadsPosition + (leadsVelocity* 2.0f );
	    m_v2TargetPosition = targetPosition;

	    return GoTo();
    }

    /// <summary>
    /// Evade behaviour. Tries to predict the target's future position
    /// and generates force away from it. If target is far away, it wanders
    /// </summary>
    /// <returns></returns>
    Vector2 Evade(Transform _intargetTransform, float _inDeltaTime)
    {
        Vector2 currentPosition = this.transform.position;
        Vector2 targetPosition = m_v2TargetPosition;

        //Guess the lead entity's future position
        Vector2 leadsPosition = _intargetTransform.position;
        Vector2 leadsVelocity = (leadsPosition - targetPosition) / (float)_inDeltaTime;
        targetPosition = leadsPosition + (leadsVelocity * 2.0f);
        m_v2TargetPosition = targetPosition;

        float distanceToTarget = Vector2.Distance(currentPosition, targetPosition);

	if (distanceToTarget<m_kfBrakeDistance)
	{
		return Flee();
    }

    else
    {
        return Wander();
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, m_kfFleeDistance);
    }

    private void CalculateVelocity(Vector2 _SteeringForce)
    {
        Vector2 newVelocity = _SteeringForce + m_v2CurrentVelocity;
        newVelocity = Vector2.ClampMagnitude(newVelocity, m_kfMaxSpeed);
        m_v2CurrentVelocity = newVelocity;
    }

    private void TogglePlayerControl()
	{
        m_bIsPlayerControlled = !m_bIsPlayerControlled;
        m_fDelayCounter = 0;
        ChangeState(STATE.IDLE);
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControls : MonoBehaviour
{
    [Header("ConnectedObjects")]
    [SerializeField] private Rigidbody2D m_rgd2dPlayer = null;
    [SerializeField] private Animator m_amrAnimator = null;
    [SerializeField] private GameObject m_objHitMark = null;
    [SerializeField] private Slider m_sldrHPBar = null;
    [SerializeField] private Text m_txtSlimesKilled = null;

    [Header("Settings")]
    [SerializeField] public float m_fMoveSpeed = 1.0f;
    [SerializeField] public float m_fMaxHealth = 10.0f;
    [SerializeField] public float m_fHitOffset = 0.1678f;
    [SerializeField] public float m_fHitRange = 0.05f;
    [SerializeField] public LayerMask m_lyrEnemies;

    [Header("Debug")]
    private Vector2 m_vec2MovementVector = Vector2.zero;
    private bool m_bRGDConnected = false;
    private bool m_bAnimConnected = false;
    private bool m_bHitMarkConnected = false;
    private bool m_bHPBarConnected = false;
    private bool m_bSlimeCountConnected = false;
    private float m_fHealth = 10.0f;
    private bool m_bAttacking = false;
    private bool m_bDead = false;
    private int m_iSlimesKilled = 0;

    public int GetNumberKilled()
	{
        return m_iSlimesKilled;
	}

    // Start is called before the first frame update
    private void Start()
    {
        if (m_rgd2dPlayer == null)
        {
            print("Player Controller not linked to player rigidbody");
        }
        else
        {
            m_bRGDConnected = true;
        }

        if (m_amrAnimator == null)
        {
            print("Player Controller not linked to animator");
        }
        else
        {
            m_bAnimConnected = true;
        }

        if(m_objHitMark == null)
        {
            print("Player controller not linked to hit mark");
        }
        else
        {
            m_bHitMarkConnected = true;
        }

        if(m_sldrHPBar == null)
		{
            print("Player controller not linked a HP bar");
        }
		else
		{
            m_bHPBarConnected = true;
		}

        if (m_txtSlimesKilled == null)
        {
            print("Player controller not kill count");
        }
        else
        {
            m_bSlimeCountConnected = true;
        }
    }

    private void Awake()
    {
        m_fHealth = m_fMaxHealth;
        m_iSlimesKilled = 0;

        if (m_bHPBarConnected)
		{
            m_sldrHPBar.maxValue = m_fMaxHealth;
            m_sldrHPBar.value = m_fHealth;
		}
    }

    // Update is called once per frame
    private void Update()
    {
        UpdateHP();
        UpdateKillCount();

		if (!m_bAttacking && (m_fHealth>0))
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
                //m_vec2MovementVector = Vector2.zero;
				if (m_bHitMarkConnected)
				{
					StartCoroutine(Attack());
				}
			}
			else
			{
				m_vec2MovementVector.x = Input.GetAxis("Horizontal");
				m_vec2MovementVector.y = Input.GetAxis("Vertical");
				m_vec2MovementVector = m_vec2MovementVector.normalized;

				if (m_bAnimConnected)
				{
					m_amrAnimator.SetFloat("Horizontal", m_vec2MovementVector.x);
					m_amrAnimator.SetFloat("MovementSpeed", m_vec2MovementVector.magnitude);
				}
			} 
		}
    }

    private void FixedUpdate()
    {
        if (m_bRGDConnected)
        {
            m_rgd2dPlayer.velocity = m_vec2MovementVector * m_fMoveSpeed * Time.fixedDeltaTime;
            m_objHitMark.transform.position = m_vec2MovementVector * m_fHitOffset;
            m_objHitMark.transform.position += transform.position;
        }

        if(m_fHealth<=0)
        {
            GetHit(0.0f);
        }
    }

    public bool GetIfDead()
	{
        return m_bDead;
	}

    public void GetHit(float _inDamage)
    {
        m_fHealth -= _inDamage;
        if (m_fHealth <= 0.0f)
        {
            Die();
        }
    }

    private void Die()
    {
        m_bDead = true;
        if (m_bAnimConnected)
        {
            m_amrAnimator.SetBool("Dead", true);
        }
    }

    private void UpdateHP()
	{
        if (m_bHPBarConnected)
        {
            m_sldrHPBar.value = m_fHealth;
        }
    }

    private void UpdateKillCount()
	{
        if(m_bSlimeCountConnected)
		{
            m_txtSlimesKilled.text = m_iSlimesKilled.ToString();
		}
  
	}

    private IEnumerator Attack()
    {
        m_bAttacking = true;
        m_amrAnimator.SetTrigger("Attack");
        yield return new WaitForSeconds(0.2f);

        Collider2D[] registeredHits = Physics2D.OverlapCircleAll(m_objHitMark.transform.position, m_fHitRange, m_lyrEnemies);
        foreach(Collider2D enemy in registeredHits)
        {
            print("Player hit " + enemy.name);
            SlimeBehaviour slime = enemy.GetComponent<SlimeBehaviour>();
            if(slime != null)
			{
                slime.GetHit();
                m_iSlimesKilled++;
			}
        }
        yield return new WaitForSeconds(0.2f);
        m_bAttacking = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(m_objHitMark.transform.position, m_fHitRange);
    }
}

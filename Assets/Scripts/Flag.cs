using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Vector3 v2FlagArea_Red;
    [SerializeField] private Vector3 v2FlagArea_Blue;
    [Header("Debug")]
    [SerializeField] private bool bIsRedFlag = true;
    [SerializeField] private bool bIsHeld = false;
    [SerializeField] private bool bIsFlagAreaSet = false;
    [Header("FlagSprites")]
    [SerializeField] private Sprite[] sprtFlagSprites;

    public void SetFlagColourRed(bool _inIsRed)
	{
		if (sprtFlagSprites.Length > 0)
		{
            SpriteRenderer flagSprite = GetComponentInChildren<SpriteRenderer>();
            if (_inIsRed)
            {
                flagSprite.sprite = sprtFlagSprites[0];
                bIsRedFlag = true;
            }
			else
			{
                flagSprite.sprite = sprtFlagSprites[1];
                bIsRedFlag = false;
            }
        }
	}

    public void SetFlagAreaPositons(Vector3 _inRedArea, Vector3 _inBlueArea)
	{
        v2FlagArea_Red = _inRedArea;
        v2FlagArea_Blue = _inRedArea;
        bIsFlagAreaSet = true;
    }

    public void FlagCaptured()
	{
        if (bIsRedFlag)
        {
            print("Red flag captured");
        }
        else
        {
            print("Blue flag captured");
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        AgentController agent = collision.gameObject.GetComponent<AgentController>();
		if(!bIsHeld && agent != null && (agent.GetIfRedTeam() != bIsRedFlag))
		{
			if (!agent.IsHoldingFlag())
			{
				agent.SetHoldingFlag();
                bIsHeld = true;
				transform.SetParent(agent.transform);
				//transform.position = Vector3.zero; 
			}
		}
	}

}

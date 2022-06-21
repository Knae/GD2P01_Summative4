using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private FlagControl flgctrlCentral;
    [Header("Debug")]
    [SerializeField] private bool bIsRedFlag = true;
    [SerializeField] private bool bIsHeld = false;
    [SerializeField] private bool bIsCaptured = false;
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

    public void SetFlagVariables(FlagControl _inCentralCommand)
	{
        flgctrlCentral = _inCentralCommand;
    }

    public void FlagCaptured()
	{
        transform.SetParent(null);
        bIsHeld = false;
        bIsCaptured = true;

        //If it's a red flag, then we want it moved to the blue area
        //Similarly for the score
        StaticVariables.IncrementScore(!bIsRedFlag);
        transform.position = flgctrlCentral.GeneratePositionInFlagArea(!bIsRedFlag);

        if (bIsRedFlag)
        {
            print("Red flag captured, now in blue area");
        }
        else
        {
            print("Blue flag captured, now in red area");
        }
    }

    public void FlagFreed()
    {
        transform.SetParent(null);
        bIsHeld = false;
        bIsCaptured = false;

        transform.position = flgctrlCentral.GeneratePositionInFlagArea(bIsRedFlag);

        if (bIsRedFlag)
        {
            print("Red flag freed, now in red area");
        }
        else
        {
            print("Blue flag freed, now in blue area");
        }
    }

    public void SetIsHeld(bool _inIsHeld)
	{
        bIsHeld = _inIsHeld;
	}

    public bool GetIfHeld()
	{
        return bIsHeld;
	}

    public bool IsCaptured()
	{
        return bIsCaptured;
	}

    public bool IsRedFlag()
	{
        return bIsRedFlag;
	}
    private void OnTriggerEnter2D(Collider2D collision)
    {
        AgentController agent = collision.gameObject.GetComponent<AgentController>();
		if(!collision.isTrigger && !bIsHeld && agent != null && (agent.GetIfRedTeam() != bIsRedFlag))
		{
			if (!agent.IsHoldingFlag())
			{
				agent.SetHoldingFlag();
                bIsHeld = true;
				transform.SetParent(agent.transform);
				transform.position = agent.transform.position;
                agent.ChangeState(AgentController.STATE.RETURNHOME);
			}
		}
	}
}

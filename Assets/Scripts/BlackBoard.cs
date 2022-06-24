using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Blackboard script for coordinating all the agents in
/// the team
/// </summary>
public class BlackBoard : MonoBehaviour
{
    /// <summary>
    /// class specifically to store information
    /// in a dictionary that can be change when iterating through
    /// without Unity bitching and moaning about changing values
    /// </summary>
    public class LastLocation
	{
        public Vector2 v2LastPosition = Vector2.zero;
        public int iNoOfPursuingAgents = 0;

        public LastLocation(Vector2 _inLocation)
		{
            v2LastPosition = _inLocation;
		}
	}

    [Header("UsedAssets")]
    [SerializeField] GameObject objAgentPrefab;
    [SerializeField] GameObject objCameraPrefab;

    [Header("Settings")]
    [SerializeField] private bool bIsRedSide = true;
    [SerializeField] private float fSpawnRadius = 0.5f;

    [Header("AgentsInfo")]
    [SerializeField] private GameObject objAttackingAgent;
    [SerializeField] private GameObject objControlledAgent;

    [Header("TeamAreas")]
    [SerializeField] private GameObject objOppFlageArea;
    [SerializeField] private GameObject objOppPrisonArea;
    [SerializeField] private Tilemap    tlmpHomeArea;

    [Header("Debug")]
    [SerializeField] private FlagControl flgctrlDesperateMeasures;
    [SerializeField] private GameObject[] objTeamObjects;
    [SerializeField] private HashSet<GameObject> setReturningObjects;
    [SerializeField] private Dictionary<GameObject, LastLocation> dictDetectedHostiles;
    [SerializeField] private bool bIsAttacking = false;
    [SerializeField] private int iAgentsCaptured = 0;
    [SerializeField] private int iNumberOfAgents = 5;
    [SerializeField] private float fCountUpTimer = 0;

    const float kfAttackTimeout = 15.0f;

    // Start is called before the first frame update
    void Start()
    {
        iNumberOfAgents = StaticVariables.iTeamSize;

        if(objCameraPrefab == null)
		{
            print("WARNING: No camera prefab attached to blackboard");
		} 
        
        if(objAgentPrefab == null)
		{
            print("WARNING: No agent prefab attached to blackboard");
		}

        if(dictDetectedHostiles == null)
		{
            dictDetectedHostiles = new Dictionary<GameObject, LastLocation>();
		}
        
        if(setReturningObjects == null)
		{
            setReturningObjects = new HashSet<GameObject>();
		}

        //Generate the agents for this blackboard and add them to the array of agents
        //Spawn them in random positions and set the opponent's flag and prison area
        //set the colour accordingly
        objTeamObjects = new GameObject[iNumberOfAgents];
        for(int i=0; i<iNumberOfAgents; i++)
		{
            //Generate a random position within a radius and add it to this object's position
            Vector3 randomPos = new Vector3(Random.Range(-fSpawnRadius,fSpawnRadius), Random.Range(-fSpawnRadius, fSpawnRadius), 0.0f);
            randomPos += transform.position;

            GameObject newAgent = Instantiate(objAgentPrefab, randomPos, Quaternion.identity);
            AgentController agentController = newAgent.GetComponent<AgentController>();
            if (agentController != null)
			{
                agentController.bbHomeBoard = this;
                agentController.SetAreas(objOppFlageArea.transform.position, objOppPrisonArea.transform.position, tlmpHomeArea);

                if (bIsRedSide)
                {
                    agentController.SetAgentColour(AgentController.COLOUR.RED);
                    if(i==0)
					{
                        agentController.TogglePlayerControl(objCameraPrefab);
					}
                }
                else
                {
                    agentController.SetAgentColour(AgentController.COLOUR.BLUE);
                }

            }

            objTeamObjects[i] = newAgent;
		}
    }
    /// <summary>
    /// This is mainly for making sure the blackboard isn't stuck
    /// on attacking for unusually long time. Usually means something 
    /// forgot to tell the blackboard it was done or went home
    /// </summary>
	private void FixedUpdate()
	{
		if(fCountUpTimer>0)
		{
            fCountUpTimer -= Time.fixedDeltaTime;
		}
	}

	// Update is called once per frame
	void Update()
    {
		if (bIsRedSide)
		{
            //Check keys for player switching agents
			if (Input.GetKeyDown(KeyCode.Q))
			{
				SwitchPlayerToPrevAgent();
			}
			else if (Input.GetKeyDown(KeyCode.E))
			{
				SwitchPlayerToNextAgent();
			} 

            //if no agents left for the player, trigger end game
            if(iAgentsCaptured >= iNumberOfAgents && flgctrlDesperateMeasures!=null)
			{
                flgctrlDesperateMeasures.RedHasNoAgents();
			}
		}

        //If we're the blue side and not already attacking and we still have 1/3 active agents and 
        //am not waiting for returning agents
        //Check each available agent for a random score to attack
        if(!bIsRedSide && (!bIsAttacking || fCountUpTimer<=0) && 
           setReturningObjects.Count<=0 && iAgentsCaptured < ( (2*iNumberOfAgents) /3))
		{
            float highestScore = 0;
            int indexOfHighestScore =-1;
            bool someoneIsAlreadyAttacking = false;
            for(int i = 0; (i<iNumberOfAgents) && !someoneIsAlreadyAttacking;i++)
			{
                AgentController agentController = objTeamObjects[i].GetComponent<AgentController>();
                if(agentController && !agentController.GetIfImprisoned() && !agentController.GetIfAttacking())
				{
                    float score = agentController.DecideIfAttack();
                    if(score > highestScore)
					{
                        indexOfHighestScore = i;
					}
					else if(score == -1)
					{
                        someoneIsAlreadyAttacking = true;
                        bIsAttacking = true;
                        fCountUpTimer = kfAttackTimeout;
                    }
				}
            }

            //Only proceed if we're doubly sure no one is attacking and
            //we found a volunteer
			if (!someoneIsAlreadyAttacking && indexOfHighestScore >= 0)
			{
				//Get the highest score and tell the agent 
				//its request to attack is approved
				AgentController volunteeredAgent = objTeamObjects[indexOfHighestScore].GetComponent<AgentController>();
				volunteeredAgent.ApproveAttackRequest(iAgentsCaptured>0);
                fCountUpTimer = kfAttackTimeout;
				bIsAttacking = true; 
			}
        }
    }

    //========================================================
    //this section is mostly function for agents to communicate
    //with the blacboard
    //========================================================

    /// <summary>
    /// an agent is now returning home. Add it to the hashSet
    /// </summary>
    /// <param name="_inAgentObject"></param>
    public void ReturningHome(GameObject _inAgentObject)
    {
		if (!setReturningObjects.Contains(_inAgentObject))
		{
			setReturningObjects.Add(_inAgentObject); 
		}
    }
    /// <summary>
    /// An agent reached home safely. Remove it from the hashSet
    /// </summary>
    /// <param name="_inAgentObject"></param>
    public void SafelyReturnedHome(GameObject _inAgentObject)
	{
        if(setReturningObjects.Contains(_inAgentObject))
		{
            setReturningObjects.Remove(_inAgentObject);
		}
    }
    /// <summary>
    /// An agent is checking if it's still in the list.
    /// Usually means it forgot to inform us it's home
    /// </summary>
    /// <param name="_inAgentObject"></param>
    public void CheckIfStillOnReturningSet(GameObject _inAgentObject)
	{
        if (setReturningObjects.Contains(_inAgentObject))
        {
            setReturningObjects.Remove(_inAgentObject);
        }
    }
    /// <summary>
    /// agent informing that it successfully released
    /// a captive. 
    /// </summary>
    public void RescueSuccess()
	{
        iAgentsCaptured--;
    }

    /// <summary>
    /// An agent that was attacking is reporting success
    /// </summary>
    public void AttackSuccess()
	{
        bIsAttacking = false;
	}
    /// <summary>
    /// An agent that was attacking is reporting failure and was
    /// captured
    /// </summary>
    /// <param name="_inAgentObject"></param>
    public void AttackFailed(GameObject _inAgentObject)
    {
        if (setReturningObjects.Contains(_inAgentObject))
        {
            setReturningObjects.Remove(_inAgentObject);
        }
        bIsAttacking = false;
        iAgentsCaptured++;
    }
    /// <summary>
    /// An agent is reporting that it spotted an hostile
    /// Add the intruder to the collection of detected enemies
    /// </summary>
    /// <param name="_inHostile"></param>
    public void DetectedHostile(GameObject _inHostile)
	{
        //Unused, just to enable the TryGetValue function
        LastLocation temp;

        //If we already know of this hostile, update its know position
        //Otherwise add it to the list of known hostiles
        if(dictDetectedHostiles.TryGetValue(_inHostile, out temp))
		{
            dictDetectedHostiles[_inHostile].v2LastPosition = _inHostile.transform.position;
        }
		else
		{
            dictDetectedHostiles.Add(_inHostile, new LastLocation(_inHostile.transform.position));
            BroadcastToAllDetectedHostile(_inHostile);
        }
    }
    /// <summary>
    /// An agent is reporting that it is pursuing a hostile
    /// </summary>
    /// <param name="_inHostile"></param>
    public void AgentPursuingHostile(GameObject _inHostile)
	{
        //Unused, just to enable the TryGetValue function
        LastLocation temp;
        //Agent is chasing it, so it should already be known.
        //But just in case
        if (dictDetectedHostiles.TryGetValue(_inHostile, out temp))
        {
            dictDetectedHostiles[_inHostile].iNoOfPursuingAgents ++;
        }
        else
        {
            dictDetectedHostiles.Add(_inHostile, new LastLocation(_inHostile.transform.position));
            dictDetectedHostiles[_inHostile].iNoOfPursuingAgents++;
        }
    }
    /// <summary>
    /// An agent is updating the last known location of an intruder it is chasing
    /// </summary>
    /// <param name="_inHostile"></param>
    public void UpdatePursuedOnHostile(GameObject _inHostile)
	{
        //Unused, just to enable the TryGetValue function
        LastLocation temp;
        //Agent is chasing it, so it should already be known.
        //But just in case
        if (dictDetectedHostiles.TryGetValue(_inHostile, out temp))
        {
            dictDetectedHostiles[_inHostile].v2LastPosition = _inHostile.transform.position;
        }
        else
        {
            dictDetectedHostiles.Add(_inHostile, new LastLocation(_inHostile.transform.position));
        }
    }


    /// <summary>
    /// an agent intends to attend to pursue a reported intruder
    /// Check that no more than 2 agents are already pursuing
    /// </summary>
    /// <param name="_inAgent"></param>
    /// <param name="_inHostile"></param>
    public void RequestToPursue(GameObject _inAgent ,GameObject _inHostile)
	{
        //Unused, just to enable the TryGetValue function
        LastLocation temp;
        //Agent is requesting to chase, so it should already be known.
        //But just in case
        if (dictDetectedHostiles.TryGetValue(_inHostile, out temp))
        {
            if(temp.iNoOfPursuingAgents < 2)
			{
                AgentController requester = _inAgent.GetComponent<AgentController>();
                if(requester)
				{
                    dictDetectedHostiles[_inHostile].iNoOfPursuingAgents++;
                    requester.ApprovePursueRequest(_inHostile, dictDetectedHostiles[_inHostile].v2LastPosition);
				}
            }
        }
        else
        {
            dictDetectedHostiles.Add(_inHostile, new LastLocation(_inHostile.transform.position));
        }
    }
    /// <summary>
    /// An agent is reporting a hostile captured or chased off
    /// Inform all pursuing agents of the specific hostile no longer an issue
    /// </summary>
    /// <param name="_inHostile"></param>
    public void IntruderDealtWith(GameObject _inHostile)
	{
        LastLocation temp;
        //Agent is informing a captured intruder, so it should already be known.
        //But just in case, check. if not, then just broadcast the capture
        if (dictDetectedHostiles.TryGetValue(_inHostile, out temp))
        {
            dictDetectedHostiles.Remove(_inHostile);
        }

        for (int i = 0; (i < iNumberOfAgents); i++)
        {
            AgentController agentController = objTeamObjects[i].GetComponent<AgentController>();
            if (agentController && agentController.GetIfPursuing())
            {
                agentController.AnnouncedIntruderNonIssue(_inHostile);
            }
        }
    }
    /// <summary>
    /// An agent is requesting the last known location of a reported intruder
    /// </summary>
    /// <param name="_inTargetHostile"></param>
    /// <param name="_outRequestedPosition"></param>
    public void RequestLastPosition(GameObject _inTargetHostile, Vector2 _outRequestedPosition)
	{
        //Unused, just to enable the TryGetValue function
        LastLocation temp;
        //Agent was moving to last location, so it should already be known.
        //But just in case it's not in the list, then add it
        if (!dictDetectedHostiles.TryGetValue(_inTargetHostile, out temp))
        {
            dictDetectedHostiles.Add(_inTargetHostile, new LastLocation(_inTargetHostile.transform.position));

        }

        _outRequestedPosition = dictDetectedHostiles[_inTargetHostile].v2LastPosition;
    }
    //========================================================

    /// <summary>
    /// Display radius at which agent starts to flee
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, fSpawnRadius);
    }

    /// <summary>
    /// Swicth player to the next agent in the array
    /// </summary>
    private void SwitchPlayerToNextAgent()
	{
        int i = 0;
        for(i=0;i<objTeamObjects.Length;i++)
		{
            AgentController agent = objTeamObjects[i].GetComponent<AgentController>();
            if(agent.IsPlayerControlled())
			{
                agent.TogglePlayerControl(objCameraPrefab);
                i++;
                if(i>= objTeamObjects.Length)
				{
                    i = 0;
				}
                agent = objTeamObjects[i].GetComponent <AgentController>();
                agent.TogglePlayerControl(objCameraPrefab);
                break;
			}
		}

    }
    /// <summary>
    /// Swicth player to the previous agent in the array
    /// </summary>
    private void SwitchPlayerToPrevAgent()
    {
        int i = 0;
        for (i = objTeamObjects.Length-1; i >=0; i--)
        {
            AgentController agent = objTeamObjects[i].GetComponent<AgentController>();
            if (agent.IsPlayerControlled())
            {
                agent.TogglePlayerControl(objCameraPrefab);
                i--;
                if (i <= 0)
                {
                    i = objTeamObjects.Length - 1;
                }
                agent = objTeamObjects[i].GetComponent<AgentController>();
                agent.TogglePlayerControl(objCameraPrefab);
                break;
            }
        }
    }
    /// <summary>
    /// Inform all available agents of detected hostile
    /// </summary>
    /// <param name="_inHostile"></param>
    private void BroadcastToAllDetectedHostile(GameObject _inHostile)
    {
        for (int i = 0; (i < iNumberOfAgents); i++)
        {
            AgentController agentController = objTeamObjects[i].GetComponent<AgentController>();
            if (agentController && !agentController.GetIfImprisoned() &&
                !agentController.GetIfAttacking() && !agentController.GetIfReturningHome())
            {
                agentController.DetectedIntruder(_inHostile);
            }
        }
    }
}

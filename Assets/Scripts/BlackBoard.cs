using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackBoard : MonoBehaviour
{
    [Header("UsedAssets")]
    [SerializeField] GameObject objAgentPrefab;
    [SerializeField] GameObject objCameraPrefab;

    [Header("Settings")]
    [SerializeField] private int iNumberOfAgents = 7;
    [SerializeField] private bool bIsRedSide = true;
    [SerializeField] private float fSpawnRadius = 1.0f;
    [SerializeField] private float fDistanceBetweenSpawn = 0.25f;

    [Header("Boards")]
    [SerializeField] private GameObject objAttackingAgent;
    [SerializeField] private GameObject objControlledAgent;

    [Header("Debug")]
    [SerializeField] private GameObject[] objTeamObjects;

    // Start is called before the first frame update
    void Start()
    {
        if(objCameraPrefab == null)
		{
            print("WARNING: No camera prefab attached to blackboard");
		} 
        
        if(objAgentPrefab == null)
		{
            print("WARNING: No agent prefab attached to blackboard");
		}

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

                if (bIsRedSide)
                {
                    agentController.SetAgentColour(AgentController.COLOUR.RED);
                    if(i==1)
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

    // Update is called once per frame
    void Update()
    {
		if (bIsRedSide)
		{
			if (Input.GetKeyDown(KeyCode.Q))
			{
				SwitchPlayerToPrevAgent();
			}
			else if (Input.GetKeyDown(KeyCode.E))
			{
				SwitchPlayerToNextAgent();
			} 
		}
    }

    /// <summary>
    /// Display radius at which agent starts to flee
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, fSpawnRadius);
    }

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
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script to manage generating the flags and
/// the positions they are moved to.
/// </summary>
public class FlagControl : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] public float fFlagSpawnRadius = 0.25f;
    [SerializeField] public int iNumberOfFlags = 4;
    [Header("FlagArea")]
    [SerializeField] private GameObject objFlagArea_Red;
    [SerializeField] private GameObject objFlagArea_Blue;
    [Header("ScoreDisplay")]
    [SerializeField] private Text txtdispScore_Red;
    [SerializeField] private Text txtdispScore_Blue;
    [SerializeField] static private int iScore_Red = 0;
    [SerializeField] static private int iScore_Blue = 0;
    [Header("Prefabs")]
    [SerializeField] private GameObject objFlagPrefab;
    [Header("Connected Objects")]
    [SerializeField] private GameObject objEndGameMenu;

    // Start is called before the first frame update
    void Start()
    {
        if (objFlagArea_Red != null && objFlagArea_Blue != null)
        {
            PutDownFlags();
        }
        txtdispScore_Red.text = "0";
        txtdispScore_Blue.text = "0";
    }

    /// <summary>
    /// Quick and dirty end game algorithm
    /// </summary>
	private void Update()
	{
		if(iScore_Blue == 4)
		{
            objEndGameMenu.SetActive(true);
            objEndGameMenu.GetComponent<EndGameText>().EndGame(false) ;
		}
        else if (iScore_Red == 4)
		{
            objEndGameMenu.SetActive(true);
            objEndGameMenu.GetComponent<EndGameText>().EndGame(true);
        }

        if(Input.GetKeyDown(KeyCode.Escape))
		{
            objEndGameMenu.SetActive(!objEndGameMenu.activeInHierarchy);
            objEndGameMenu.GetComponent<EndGameText>().ExitMenu();
        }
	}

    /// <summary>
    /// Alternate end game route if player gets all agents captured
    /// </summary>
    public void RedHasNoAgents()
	{
        objEndGameMenu.SetActive(true);
        objEndGameMenu.GetComponent<EndGameText>().EndGame(false);
    }

    /// <summary>
    /// Increment score, depending on the team
    /// </summary>
    /// <param name="_inIsRedTeam"></param>
	public void IncrementScore(bool _inIsRedTeam)
	{
		if (_inIsRedTeam)
		{
			iScore_Red++;
		}
		else
		{
			iScore_Blue++;
		}
	}

    /// <summary>
    /// Generate a random position for where the flag will be spawned
    /// Random range depended on which area to spawn in. True to spawn
    /// in Red area
    /// </summary>
    /// <param name="_inIsRedArea"></param>
    /// <returns></returns>
	public Vector2 GeneratePositionInFlagArea(bool _inIsRedArea)
    {
        txtdispScore_Red.text = iScore_Red.ToString();
        txtdispScore_Blue.text = iScore_Blue.ToString();

        Vector3 generatedPosition = new Vector2(Random.Range(-fFlagSpawnRadius, fFlagSpawnRadius), Random.Range(-fFlagSpawnRadius, fFlagSpawnRadius));
        generatedPosition = Vector3.ClampMagnitude(generatedPosition, fFlagSpawnRadius);

        if(_inIsRedArea)
		{
            generatedPosition += objFlagArea_Red.transform.position;
        }
        else
		{
            generatedPosition += objFlagArea_Blue.transform.position;
        }
        return generatedPosition;
    }
    /// <summary>
    /// Function to generate the flags at the start of the game
    /// </summary>
    private void PutDownFlags()
	{

		for (int i = 0; i < iNumberOfFlags; i++)
		{
            Flag newFlagScript;
			Vector3 generatedPosition = new Vector3(Random.Range(-fFlagSpawnRadius, fFlagSpawnRadius),Random.Range(-fFlagSpawnRadius, fFlagSpawnRadius),0f);
            generatedPosition = Vector3.ClampMagnitude(generatedPosition, fFlagSpawnRadius);

            GameObject newRedFlag = Instantiate(objFlagPrefab,generatedPosition + objFlagArea_Red.transform.position, Quaternion.identity);
            newFlagScript = newRedFlag.GetComponent<Flag>();
            newFlagScript.SetFlagColourRed(true);
            newFlagScript.SetFlagVariables(this);
            GameObject newBlueFlag = Instantiate(objFlagPrefab,generatedPosition + objFlagArea_Blue.transform.position, Quaternion.identity);
            newFlagScript = newBlueFlag.GetComponent<Flag>();
            newFlagScript.SetFlagColourRed(false);
            newFlagScript.SetFlagVariables(this);

        } 
    }
}

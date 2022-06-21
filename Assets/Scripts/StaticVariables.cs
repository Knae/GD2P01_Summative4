using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticVariables : MonoBehaviour
{
    [Header("Game Variables")]
    [SerializeField] static public int iTeamSize = 7;
    [SerializeField] static public float fNoManLandWidth= 0.25f;
    
    [Header("Score")]
    [SerializeField] static private int iScore_Red = 0;
    [SerializeField] static private int iScore_Blue = 0;
    //[Header("FlagControl")]
    //[SerializeField] static FlagControl sceneFlagControl;
    // Start is called before the first frame update
    
    static public void IncrementScore(bool _inIsRedTeam)
	{
        if(_inIsRedTeam)
		{
            iScore_Red++;
		}
        else
		{
            iScore_Blue++;
		}
	}

    static public int GetScore(bool _inIsRedTeam)
	{
        if (_inIsRedTeam)
        {
            return iScore_Red;
        }
        else
        {
            return iScore_Blue;
        }
    }
    private void Start()
    {
        DontDestroyOnLoad(transform.gameObject);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticVariables : MonoBehaviour
{
    [Header("Game Variables")]
    [SerializeField] static public int iTeamSize = 10;
    [Header("Score")]
    [SerializeField] static private int iScore_Red = 0;
    [SerializeField] static private int iScore_Blue = 0;
    //[Header("FlagControl")]
    //[SerializeField] static FlagControl sceneFlagControl;
    // Start is called before the first frame update
    private void Start()
    {
        DontDestroyOnLoad(transform.gameObject);
    }
}

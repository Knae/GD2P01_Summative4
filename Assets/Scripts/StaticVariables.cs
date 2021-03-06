using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Static variables that need to be carried between scenes
/// </summary>
public class StaticVariables : MonoBehaviour
{
    [Header("Game Variables")]
    [SerializeField] static public int iTeamSize = 7;
    [SerializeField] static public float fNoManLandWidth= 0.25f;
    
    
    private void Start()
    {
        DontDestroyOnLoad(transform.gameObject);
    }
}

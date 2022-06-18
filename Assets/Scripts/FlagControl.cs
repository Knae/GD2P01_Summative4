using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagControl : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] public float fFlagSpawnRadius = 0.25f;
    [SerializeField] public int iNumberOfFlags = 4;
    [Header("FlagArea")]
    [SerializeField] private GameObject objFlagArea_Red;
    [SerializeField] private GameObject objFlagArea_Blue;
    [Header("Prefabs")]
    [SerializeField] private GameObject objFlagPrefab;

    // Start is called before the first frame update
    void Start()
    {
        if (objFlagArea_Red != null && objFlagArea_Blue != null)
        {
            PutDownFlags();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

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
            newFlagScript.SetFlagAreaPositons(objFlagArea_Red.transform.position, objFlagArea_Blue.transform.position);
            GameObject newBlueFlag = Instantiate(objFlagPrefab,generatedPosition + objFlagArea_Blue.transform.position, Quaternion.identity);
            newFlagScript = newBlueFlag.GetComponent<Flag>();
            newFlagScript.SetFlagColourRed(false);
            newFlagScript.SetFlagAreaPositons(objFlagArea_Red.transform.position, objFlagArea_Blue.transform.position);

        } 
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    [Header("Prefabs")]
    [SerializeField] private GameObject objFlagPrefab;

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

    public Vector2 GeneratePositionInFlagArea(bool _inIsRedArea)
    {
        txtdispScore_Red.text = StaticVariables.GetScore(true).ToString();
        txtdispScore_Blue.text = StaticVariables.GetScore(false).ToString();

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

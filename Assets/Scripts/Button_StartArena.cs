using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Button_StartArena : MonoBehaviour
{
	public void StartArena()
	{
		SceneManager.LoadScene("Arena");
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Script for button to start the game.
/// Loads the Arena scene
/// </summary>
public class Button_StartArena : MonoBehaviour
{
	public void StartArena()
	{
		SceneManager.LoadScene("Arena");
	}
}

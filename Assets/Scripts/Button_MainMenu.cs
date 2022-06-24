using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Button_MainMenu : MonoBehaviour
{   
    public void StartMainMenu()
	{
		SceneManager.LoadScene("MainMenu");
	}
}

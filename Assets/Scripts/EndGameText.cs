using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script responsible for setting the appropriate end game message
/// </summary>
public class EndGameText : MonoBehaviour
{
    public Text txtEndText;

	private void Start()
	{
		if(txtEndText==null)
		{
			txtEndText = GetComponentInChildren<Text>();
		}
	}

	public void EndGame(bool _inDidRedWin)
	{
		if(_inDidRedWin)
		{
			txtEndText.text = "YOU WON!";
		}
		else
		{
			txtEndText.text = "YOU LOST!";
		}
	}

	public void ExitMenu()
	{
		txtEndText.text = "";
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// script for setting the number of agents through
/// the slider
/// </summary>
public class SliderValueSetter : MonoBehaviour
{
    public Text txtSliderValue;
    public Slider sldSource;

    // Start is called before the first frame update
    void Start()
    {
        if(txtSliderValue==null)
		{
            txtSliderValue = GetComponentInChildren<Text>();
        }
        
        if(sldSource == null)
		{
            sldSource = GetComponentInChildren<Slider>();
        }

        UpdateLabelDisplay();
    }

    public void UpdateLabelDisplay()
	{
        StaticVariables.iTeamSize = (int)sldSource.value;
        txtSliderValue.text = sldSource.value.ToString();
	}
}

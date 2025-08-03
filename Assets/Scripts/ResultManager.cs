using System;
using TMPro;
using UnityEngine;

public class ResultManager : MonoBehaviour
{
    private float total, red, blue, green, yellow;
    [SerializeField] private TMP_Text ratioText, redTxt, blueTxt, greenTxt, yellowTxt;

    public void SetResults(float total, float red, float blue, float green, float yellow)
    {
        ratioText.text = "Total Painted Ratio: " +  Convert.ToString(total);
        redTxt.text = "Red Ratio: " + Convert.ToString(red);
        blueTxt.text = "Blue Ratio: " + Convert.ToString(blue);
        greenTxt.text = "Green Ratio: " + Convert.ToString(green);
        yellowTxt.text = "Yellow Ratio: " + Convert.ToString(yellow);
    }

}

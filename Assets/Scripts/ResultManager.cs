using System;
using TMPro;
using Unity.FPS.Gameplay;
using UnityEngine;

public class ResultManager : MonoBehaviour
{
    private float total, red, blue, green, yellow;
    [SerializeField] private TMP_Text ratioText, redTxt, blueTxt, greenTxt, yellowTxt;

    PaintWinChecker paintWinChecker;

    private void Start()
    {
        paintWinChecker = FindFirstObjectByType<PaintWinChecker>();
        red = paintWinChecker.valueArray[0];
        blue = paintWinChecker.valueArray[1];
        green = paintWinChecker.valueArray[2];
        yellow = paintWinChecker.valueArray[3];
        total = paintWinChecker.totalPaintedPercent;


        ratioText.text = "Total Painted Ratio: " + Convert.ToString(total);
        redTxt.text = "Red Ratio: " + Convert.ToString(red);
        blueTxt.text = "Blue Ratio: " + Convert.ToString(blue);
        greenTxt.text = "Green Ratio: " + Convert.ToString(green);
        yellowTxt.text = "Yellow Ratio: " + Convert.ToString(yellow);
    }



}

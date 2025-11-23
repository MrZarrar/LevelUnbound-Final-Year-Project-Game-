using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using TMPro; 

public class XPBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI LevelText;
    [SerializeField] private TextMeshProUGUI XPLeft;


    public void SetLevel(int level, int currentXP, int xpToNextLevel)
    {
        slider.maxValue = xpToNextLevel;
        slider.value = currentXP;

        if (LevelText != null)
        {
            LevelText.text = level.ToString();
        }

         if (XPLeft != null)
        {
            XPLeft.text = $"{currentXP} / {xpToNextLevel}";
        }
    }

    public void SetXP(int currentXP)
    {
        slider.value = currentXP;

        if (XPLeft != null)
        {
            XPLeft.text = $"{currentXP} / {slider.maxValue} XP";
        }
    }
}

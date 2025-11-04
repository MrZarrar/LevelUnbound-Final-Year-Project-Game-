using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public Gradient gradient;
    public Image fill;

    [Header("Text Display")] 
    [SerializeField] private TextMeshProUGUI hpText; 
    private int currentMaxHealth; 



    public void SetMaxHP(int hp)
    {
        slider.maxValue = hp;
        slider.value = hp;

        fill.color = gradient.Evaluate(1f);

        currentMaxHealth = hp;
        if (hpText != null) 
        {
            
            hpText.text = $"{hp} / {hp}";
        }

    }
    public void SetHP(int hp)
    {
        slider.value = hp;

        fill.color = gradient.Evaluate(slider.normalizedValue);

        if (hpText != null) 
        {

            hpText.text = $"{hp} / {currentMaxHealth}";
        }

    }
}

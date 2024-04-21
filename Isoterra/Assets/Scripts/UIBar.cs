using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBar : MonoBehaviour
{

    public Image healthbarSprite;
    public Image staminabarSprite;

    public void updateHealthbar(float maxhealth, float curenthealth)
    {
        healthbarSprite.fillAmount = curenthealth / maxhealth;
    }

    public void updateStaminabar(float maxStamina, float curentStamina)
    {
        staminabarSprite.fillAmount = curentStamina / maxStamina;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class playerHud : MonoBehaviour
{
    [SerializeField] public GameManager gm;
    public Slider hpSlider;
    public Gradient hpGradient;
    public Image hpFill;

    public Slider staminaSlider;
    public Gradient staminaGradient;
    public Image staminaFill;
    public void SetMaxHealth(float health)
    {
        hpSlider.maxValue = gm.maxHP;
        hpSlider.value = gm.hp;
        hpGradient.Evaluate(1f);

        hpFill.color = hpGradient.Evaluate(1f);
    }
    public void SetHealth(float health)
    {
        hpSlider.value = gm.hp;

        hpFill.color = hpGradient.Evaluate(hpSlider.normalizedValue);
    }
    public void SetMaxStamina(float health)
    {
        staminaSlider.maxValue = gm.maxStamina;
        staminaSlider.value = gm.stamina;
        staminaGradient.Evaluate(1f);

        staminaFill.color = staminaGradient.Evaluate(1f);
    }
    public void SetStamina(float health)
    {
        staminaSlider.value = gm.stamina;

        staminaFill.color = staminaGradient.Evaluate(staminaSlider.normalizedValue);
    }
}

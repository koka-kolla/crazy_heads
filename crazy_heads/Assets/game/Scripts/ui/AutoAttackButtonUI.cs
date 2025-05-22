// AutoAttackButtonUI.cs
using UnityEngine;
using UnityEngine.UI;

public class AutoAttackButtonUI : MonoBehaviour
{
    [Tooltip("Ссылка на HeroController")]
    public HeroController heroController;

    [Tooltip("Image кнопки")]
    public Image buttonImage;

    [Tooltip("Цвет, когда автоатака включена")]
    public Color pressedColor = Color.green;
    [Tooltip("Цвет по умолчанию")]
    public Color normalColor = Color.white;

    [Tooltip("Текст кнопки")]
    public Text buttonText;

    void Start()
    {
        if (heroController != null)
            UpdateVisual(heroController.AutoAttackEnabled);
    }

    public void OnClick()
    {
        if (heroController == null) return;
        heroController.ToggleAutoAttack();
        UpdateVisual(heroController.AutoAttackEnabled);
    }

    private void UpdateVisual(bool isOn)
    {
        if (buttonImage  != null) buttonImage.color = isOn ? pressedColor : normalColor;
        if (buttonText   != null) buttonText.text  = isOn ? "AUTO\nON" : "AUTO\nOFF";
    }
}

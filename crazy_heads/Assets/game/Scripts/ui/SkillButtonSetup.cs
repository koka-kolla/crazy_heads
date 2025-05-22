// Assets/Scripts/UI/SkillButtonSetup.cs
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SkillButtonSetup : MonoBehaviour
{
    public HeroController heroController;
    public int skillIndex;

    void Awake()
    {
        var btn = GetComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            Debug.Log($"[SkillButtonSetup] на кнопку #{skillIndex} кликнули");
            heroController.OnSkillButton(skillIndex);
        });
    }
}

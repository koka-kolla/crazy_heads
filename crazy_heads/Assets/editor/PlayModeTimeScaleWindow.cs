#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Небольшое окно со слайдером Time.timeScale,
/// работает только в Play Mode (редактор).
/// </summary>
public class PlayModeTimeScaleWindow : EditorWindow
{
    const float MinScale = 0.1f;
    const float MaxScale = 2f;

    float _baseFixedDelta;        // 0.02 по умолчанию

    /* ─────────  Меню  ───────── */
    [MenuItem("Tools/Play-Mode Time Scale %#T")]   // Ctrl+Shift+T
    static void Open() => GetWindow<PlayModeTimeScaleWindow>("Time");

    /* ─────────  Жизненный цикл  ───────── */
    void OnEnable()
    {
        _baseFixedDelta = Time.fixedDeltaTime / Mathf.Max(Time.timeScale, 0.0001f);
    }

    void OnGUI()
    {
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play Mode to adjust time scale.", MessageType.Info);
            return;
        }

        GUILayout.Label($"Time Scale: x {Time.timeScale:0.00}", EditorStyles.boldLabel);

        // основное управление
        float newScale = GUILayout.HorizontalSlider(Time.timeScale, MinScale, MaxScale, GUILayout.Height(18));
        newScale = EditorGUILayout.FloatField("Value", newScale);
        newScale = Mathf.Clamp(newScale, MinScale, MaxScale);

        if (!Mathf.Approximately(newScale, Time.timeScale))
        {
            Time.timeScale      = newScale;
            Time.fixedDeltaTime = _baseFixedDelta * newScale;   // физика синхронизируется
            Repaint();
        }
    }
}
#endif

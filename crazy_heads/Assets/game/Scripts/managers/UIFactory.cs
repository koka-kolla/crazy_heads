// Assets/game/Scripts/managers/UIFactory.cs
using UnityEngine;

public static class UIFactory
{
    /// <summary>
    /// Создаёт и возвращает новую полосу здоровья.
    /// </summary>
    /// <param name="prefab">Префаб HealthBarUI (GameObject с компонентом HealthBar).</param>
    /// <param name="canvas">Transform вашего Canvas.</param>
    public static HealthBar CreateHealthBar(GameObject prefab, Transform canvas)
    {
        var go = Object.Instantiate(prefab, canvas);
        return go.GetComponent<HealthBar>();
    }

    /// <summary>
    /// Удаляет полосу здоровья из сцены.
    /// </summary>
    public static void DestroyHealthBar(HealthBar hb)
    {
        if (hb != null && hb.gameObject != null)
            Object.Destroy(hb.gameObject);
    }
}

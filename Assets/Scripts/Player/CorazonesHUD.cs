/**************************
 * Proyecto: BarrerasInvisibles
 * Universidad de la Ciudad de Buenos Aires
 * Periodo: 2do cuatrimestre 2026 
 * Materia: Programación 2
 * Integrantes: Ariana Ruiz, Eduardo Ortega, Ruth Wulf
 * Función: Representación de la vida de Nia mediante corazones 3D.
 * Explicación: Escucha los cambios de PlayerHealth y activa u oculta
 * Vida_1, Vida_2 y Vida_3. Restaura los corazones al curarse o reaparecer.
 *********************************************************************************************/

using UnityEngine;

[DisallowMultipleComponent]
public class CorazonesHUD : MonoBehaviour
{
    [Header("Arrastrar Nia, que contiene PlayerHealth")]
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Corazones: un objeto por punto de vida")]
    [SerializeField] private GameObject vida1;
    [SerializeField] private GameObject vida2;
    [SerializeField] private GameObject vida3;

    private PlayerHealth subscribedHealth;

    private void OnEnable()
    {
        subscribedHealth = playerHealth;
        if (subscribedHealth == null)
        {
            Debug.LogWarning("CorazonesHUD: arrastrá Nia al campo Player Health.", this);
            return;
        }
        subscribedHealth.HealthChanged += RefreshHearts;
        RefreshHearts(subscribedHealth.CurrentHealth, subscribedHealth.MaxHealth);
    }

    private void Start()
    {
        // Sincronizar después de que todos los Awake hayan inicializado la salud.
        if (subscribedHealth != null)
            RefreshHearts(subscribedHealth.CurrentHealth, subscribedHealth.MaxHealth);
    }

    private void OnDisable()
    {
        if (subscribedHealth != null)
            subscribedHealth.HealthChanged -= RefreshHearts;
        subscribedHealth = null;
    }

    private void RefreshHearts(int currentHealth, int maxHealth)
    {
        int visibleHearts = Mathf.Clamp(currentHealth, 0, Mathf.Min(3, maxHealth));
        SetHeart(vida1, visibleHearts >= 1);
        SetHeart(vida2, visibleHearts >= 2);
        SetHeart(vida3, visibleHearts >= 3);
    }

    private void SetHeart(GameObject heart, bool visible)
    {
        if (heart == null) return;
        // Mantener el script en Corazones3D, no en un corazón que deba ocultarse.
        if (heart == gameObject || transform.IsChildOf(heart.transform)) return;
        if (heart.activeSelf != visible) heart.SetActive(visible);
    }
}

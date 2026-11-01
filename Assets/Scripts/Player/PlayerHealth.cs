/**************************
 * Proyecto: BarrerasInvisibles
 * Universidad de la Ciudad de Buenos Aires
 * Periodo: 2do cuatrimestre 2026 
 * Materia: Programación 2
 * Integrantes: Ariana Ruiz, Eduardo Ortega, Ruth Wulf
 * Función: Gestión de la salud y daño del personaje del jugador (Nia).
 * Explicación: Implementa la interfaz IDamageable para recibir daño de los 
 * enemigos, administrar la salud máxima y controlar el evento de derrota/respawn.
 *********************************************************************************************/

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
#if BARRERAS_FMOD
using FMODUnity;
#endif

[DisallowMultipleComponent]
public class PlayerHealth : MonoBehaviour, IDamageable, IHealth
{
    [Header("Configuración de Salud")]
    [SerializeField, Min(1)] private int maxHealth = 3;
    private int currentHealth;
    [SerializeField, Min(0f)] private float invulnerabilityDuration = 0.75f;

    [Header("Reaparición")]
    [SerializeField] private bool respawnOnDeath = true;
    [Tooltip("Vacío: reaparece en la posición y rotación iniciales de Nia.")]
    [SerializeField] private Transform respawnPoint;
    [SerializeField, Min(0f)] private float respawnDelay = 1f;

    [Header("Audio de daño (Unity, opcional)")]
    [SerializeField] private AudioSource damageAudioSource;
    [SerializeField] private AudioClip damageClip;
#if BARRERAS_FMOD
    [Header("Audio de daño (FMOD, opcional)")]
    [SerializeField] private EventReference damageSound;
#endif

    [Header("Eventos para HUD y derrota")]
    [SerializeField] private UnityEvent<int> onHealthChanged = new UnityEvent<int>();
    [SerializeField] private UnityEvent onDeath = new UnityEvent();
    [SerializeField] private UnityEvent onRespawn = new UnityEvent();
    public event Action<int, int> HealthChanged;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsAlive => currentHealth > 0;

    private float protectedUntil;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private CharacterController controller;
    private Coroutine respawnRoutine;

    private void Awake()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = maxHealth;
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        controller = GetComponent<CharacterController>();
    }

    private void Start() { NotifyHealth(); }

    public void TakeDamage(int damageAmount)
    {
        if (!IsAlive || damageAmount <= 0 || Time.time < protectedUntil) return;
        currentHealth = Mathf.Max(0, currentHealth - damageAmount);
        protectedUntil = Time.time + invulnerabilityDuration;
        PlayDamageSound();
        Debug.Log($"Nia recibió {damageAmount} de daño. Vida restante: {currentHealth}/{maxHealth}", this);
        NotifyHealth();
        if (!IsAlive) Die();
    }

    // Se conserva este nombre para ser compatible con IHealth de Rossie.
    public void healing(int amount)
    {
        if (!IsAlive || amount <= 0) return;
        currentHealth += Mathf.Min(amount, maxHealth - currentHealth);
        NotifyHealth();
    }

    private void Die()
    {
        Debug.Log("Nia se quedó sin vida.", this);
        onDeath.Invoke();
        if (respawnOnDeath && isActiveAndEnabled && respawnRoutine == null)
            respawnRoutine = StartCoroutine(RespawnAfterDelay());
        // Sin respawn automático queda sin vida y el controlador bloquea las entradas.
        // El evento On Death permite mostrar la pantalla de derrota.
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);
        respawnRoutine = null;
        Respawn();
    }

    public void Respawn()
    {
        if (respawnRoutine != null)
        {
            StopCoroutine(respawnRoutine);
            respawnRoutine = null;
        }
        bool restoreController = controller != null && controller.enabled;
        if (restoreController) controller.enabled = false;
        Vector3 position = respawnPoint != null ? respawnPoint.position : initialPosition;
        Quaternion rotation = respawnPoint != null ? respawnPoint.rotation : initialRotation;
        transform.SetPositionAndRotation(position, rotation);
        if (restoreController) controller.enabled = true;
        var movement = GetComponent<PlayerController3D>();
        if (movement != null) movement.ResetMotion();
        currentHealth = maxHealth;
        protectedUntil = Time.time + invulnerabilityDuration;
        NotifyHealth();
        onRespawn.Invoke();
    }

    private void PlayDamageSound()
    {
#if BARRERAS_FMOD
        if (!damageSound.IsNull)
        {
            RuntimeManager.PlayOneShot(damageSound, transform.position);
            return;
        }
#endif
        if (damageAudioSource != null && damageClip != null)
            damageAudioSource.PlayOneShot(damageClip);
    }

    private void NotifyHealth()
    {
        onHealthChanged.Invoke(currentHealth);
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void OnDisable()
    {
        if (respawnRoutine == null) return;
        StopCoroutine(respawnRoutine);
        respawnRoutine = null;
    }
}

/**************************
 * Proyecto: BarrerasInvisibles
 * Universidad de la Ciudad de Buenos Aires
 * Período: 2.º cuatrimestre de 2026
 * Materia: Programación 2
 * Programador: Eduardo Ortega
 * Función: Asigna y permite consultar la textura de la letra de cada cara
 * del dado. Cuando las seis caras tienen textura, permite sortear una
 * cara y obtener su índice y su textura.
 *********************************************************************************************/
using TMPro;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour, IDamageable
{
    [Header("Atributos del Enemigo")]
    [SerializeField] protected int maxHealth = 3;
    [SerializeField] protected float speed = 3f;
    [SerializeField] protected float detectionRadius = 8f;
    [SerializeField] protected float attackRadius = 1.5f;

    [Header("Recompensa")]
    [SerializeField] protected int diceDropAmount = 1;
    [SerializeField] private InventarioLetras inventarioLetras;

    [Header("Indicador de Vida")]
    [SerializeField] private GameObject dadoVidaPrefab;
    [SerializeField, Min(0f)] private float alturaSignoDanio = 1.2f;
    [SerializeField, Min(0.01f)] private float duracionSignoDanio = 0.75f;

    protected int currentHealth;
    protected Transform playerTransform;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    protected virtual void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    protected virtual void Update()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= detectionRadius)
        {
            if (distanceToPlayer <= attackRadius)
            {
                AttackPlayer();
            }
            else
            {
                ChasePlayer();
            }
        }
    }

    protected abstract void ChasePlayer();
    protected abstract void AttackPlayer();

    public virtual void TakeDamage(int damageAmount)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damageAmount;
        Debug.Log($"{gameObject.name} recibió {damageAmount} de daño. Vida restante: {currentHealth}");

        currentHealth -= damageAmount;

        // Muestra cantidad de Vida restante
        if (damageAmount > 0)
            MostrarDadoVida(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void MostrarDadoVida(int vidaRestante)
    {
        if (dadoVidaPrefab == null) return;

        Vector3 posicion = transform.position + Vector3.up * alturaSignoDanio;
        Camera camara = Camera.main;
        Quaternion rotacion = camara != null
            ? camara.transform.rotation
            : dadoVidaPrefab.transform.rotation;

        GameObject dado = Instantiate(dadoVidaPrefab, posicion, rotacion);
        TextMeshPro texto = dado.GetComponentInChildren<TextMeshPro>();

        if (texto != null)
        {
            int vidas = Mathf.Clamp(vidaRestante, 0, 3);
            texto.text = string.Join(" ", new string('●', vidas).ToCharArray());
        }

        Destroy(dado, duracionSignoDanio);
    }

    protected virtual void Die()
    {
        if (TryGetComponent<DadoCara>(out var dadoLetras))
        {
            if (inventarioLetras == null)
                inventarioLetras = FindFirstObjectByType<InventarioLetras>();

            if (inventarioLetras == null)
            {
                Debug.LogError("[NIA] Falta InventarioLetras en HUD/Inventario. El dado no se recogió.", this);
                currentHealth = 1;
                return;
            }

            if (inventarioLetras.EstaLleno)
            {
                Debug.LogWarning("[NIA] El inventario de letras está lleno. El dado sigue en la escena.", this);
                currentHealth = 1;
                return;
            }

            if (!inventarioLetras.RecogerDado(dadoLetras))
            {
                currentHealth = 1;
                return;
            }
        }
        else if (playerTransform != null && playerTransform.TryGetComponent<DiceInventory>(out var inventory))
        {
            inventory.AddDice(diceDropAmount);
        }

        Debug.Log($"{gameObject.name} ha sido derrotado.");
        FindFirstObjectByType<GameManager>()?.RegistrarDadoDerrotado(transform);
        Destroy(gameObject);
    }

    protected virtual void OnDrawGizmosSelected()
    {
        // Rango de Detección (Amarillo)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Rango de Ataque (Rojo)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}

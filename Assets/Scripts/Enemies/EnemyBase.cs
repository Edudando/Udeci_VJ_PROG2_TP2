/**************************
 * Proyecto: BarrerasInvisibles
 * Universidad de la Ciudad de Buenos Aires
 * Periodo: 2do cuatrimestre 2026 
 * Materia: Programación 2
 * Integrantes: Ariana Ruiz, Eduardo Ortega, Ruth Wulf
 * Función: Clase base abstracta para todos los enemigos del juego.
 * Explicación: Implementa IDamageable para gestionar la salud y recepción de daño,
 * y define el flujo básico del ciclo de vida del enemigo (detectar, atacar y morir),
 * permitiendo aplicar polimorfismo en tipos específicos de enemigos.
 *********************************************************************************************/

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

    [Header("Inventario de letras (opcional: se busca en la escena)")]
    [SerializeField] private InventarioLetras inventarioLetras;
    private bool muerto;

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
        if (muerto || playerTransform == null) return;

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
        if (muerto || damageAmount <= 0) return;
        currentHealth -= damageAmount;
        Debug.Log($"{gameObject.name} recibió {damageAmount} de daño. Vida restante: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        if (muerto) return;

        // Los dados con letras entregan una cara al inventario inferior.
        // El resto de los enemigos conserva su recompensa de munición.
        if (TryGetComponent<DadoCara>(out var dadoLetras))
        {
            if (inventarioLetras == null)
                inventarioLetras = FindFirstObjectByType<InventarioLetras>();
            if (inventarioLetras == null)
            {
                Debug.LogError("Falta InventarioLetras en HUD/Inventario. El dado no se recoge.", this);
                currentHealth = 1;
                return;
            }
            if (inventarioLetras.EstaLleno)
            {
                Debug.Log("Inventario de letras lleno. El dado permanece en la escena.", this);
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

        muerto = true;
        Debug.Log($"{gameObject.name} ha sido derrotado.");

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
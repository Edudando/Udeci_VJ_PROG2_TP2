/**************************
 * Proyecto: BarrerasInvisibles
 * Universidad de la Ciudad de Buenos Aires
 * Periodo: 2do cuatrimestre 2026 
 * Materia: Programación 2
 * Integrantes: Ariana Ruiz, Eduardo Ortega, Ruth Wulf
 * Función: Control de movimiento, ataques (Melee y Ranged) e interacción de Nia.
 * Explicación: Gestiona el desplazamiento 3D, interactúa con pedestales mediante E,
 * dispara proyectiles de dados usando ObjectPool/ProjectileDataSO al hacer clic izquierdo 
 * y ejecuta ataques cuerpo a cuerpo (Melee) al presionar la barra espaciadora o clic derecho.
 *********************************************************************************************/

using TMPro;
using UnityEngine;
using UnityEngine.AI;

// Control independiente para SlimePBR. No requiere EnemyTemplate ni EnemyController.
[DisallowMultipleComponent]
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyRanger : MonoBehaviour, IDamageable
{
    public enum RangerState { Idle, Chase, Attack, Death }

    [Header("Objetivo: arrastrar Nia (raíz con IDamageable)")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 0.5f, 0f);

    [Header("Vida y detección")]
    [SerializeField, Min(1f)] private float maxHealth = 20f;
    [SerializeField, Min(0.1f)] private float detectionRange = 1.5f;
    [SerializeField, Min(0.1f)] private float loseTargetRange = 5f;
    [SerializeField, Min(0.1f)] private float attackRange = 3f;
    [SerializeField, Min(0.01f)] private float rotationSpeed = 360f;

    [Header("Disparo")]
    [SerializeField] private Transform firePoint;
    [Tooltip("Opcional. Solo modelo visual: el script controla movimiento y colisiones.")]
    [SerializeField] private GameObject projectileVisualPrefab;
    [SerializeField, Min(0)] private int attackDamage = 1;
    [SerializeField, Min(0.1f)] private float attackCooldown = 1.5f;
    [SerializeField, Min(0.1f)] private float projectileSpeed = 6f;
    [SerializeField, Min(0.1f)] private float projectileLifetime = 4f;
    [SerializeField, Min(0.01f)] private float projectileRadius = 0.12f;
    [Tooltip("Incluir las capas de Nia, suelo y paredes.")]
    [SerializeField] private LayerMask hitLayers = ~0;

    [Header("Aviso de ataque")]
    [Tooltip("Espera mínima desde que aparece el signo hasta el primer disparo de cada ataque.")]
    [SerializeField, Min(0.05f)] private float attackWarningTime = 0.5f;
    [Tooltip("Altura del signo sobre la raíz del Slime, en unidades de mundo.")]
    [SerializeField, Min(0f)] private float attackWarningHeight = 0.9f;
    [SerializeField, Min(0.1f)] private float attackWarningFontSize = 8f;

    [Header("Animator opcional: nombres completos de estados, por ejemplo Base Layer.Idle")]
    [SerializeField] private Animator slimeAnimator;
    [SerializeField] private string idleAnimation = "";
    [SerializeField] private string chaseAnimation = "";
    [SerializeField] private string attackAnimation = "";
    [SerializeField] private string deathAnimation = "";
    [SerializeField, Min(0f)] private float deathDelay = 1f;

    [Header("Estado durante Play")]
    [SerializeField] private RangerState currentState = RangerState.Idle;
    [SerializeField] private float currentHealth;

    private NavMeshAgent agent;
    private GameObject attackWarning;
    private float nextShot;
    private float nextWarning;
    public float CurrentHealth => currentHealth;
    public bool IsAlive => currentHealth > 0f;
    public RangerState CurrentState => currentState;

    private void OnValidate()
    {
        loseTargetRange = Mathf.Max(loseTargetRange, detectionRange + 0.1f, attackRange + 0.1f);
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (slimeAnimator == null) slimeAnimator = GetComponentInChildren<Animator>();
        if (slimeAnimator != null) slimeAnimator.applyRootMotion = false;
        Rigidbody body = GetComponent<Rigidbody>();
        if (body != null) body.isKinematic = true;
        currentHealth = Mathf.Max(1f, maxHealth);
        currentState = RangerState.Idle;
        CreateAttackWarning();
        loseTargetRange = Mathf.Max(loseTargetRange, detectionRange + 0.1f, attackRange + 0.1f);
    }

    private void Start()
    {
        // La referencia del Inspector tiene prioridad sobre el tag.
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
        }
        if (target == null) Debug.LogWarning(name + ": asigná Nia al campo Target.", this);
        PlayAnimation(idleAnimation);
    }

    private void Update()
    {
        if (!IsAlive) return;
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            SetState(RangerState.Idle);
            StopAgent();
            return;
        }

        Vector3 difference = target.position - transform.position;
        difference.y = 0f;
        float distance = difference.magnitude;

        // Se activa al acercarse y vuelve a Idle al superar el rango de pérdida.
        if (currentState == RangerState.Idle && distance > detectionRange) return;
        if (distance >= loseTargetRange)
        {
            SetState(RangerState.Idle);
            StopAgent();
            return;
        }

        if (distance <= attackRange)
        {
            SetState(RangerState.Attack);
            StopAgent();
            if (difference.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.RotateTowards(transform.rotation,
                    Quaternion.LookRotation(difference), rotationSpeed * Time.deltaTime);
            if (Time.time >= nextShot)
            {
                nextShot = Time.time + attackCooldown;
                Shoot();
            }
        }
        else
        {
            SetState(RangerState.Chase);
            if (agent.enabled && agent.isOnNavMesh)
            {
                agent.stoppingDistance = attackRange * 0.9f;
                agent.isStopped = false;
                agent.SetDestination(target.position);
            }
            else if (Time.time >= nextWarning)
            {
                nextWarning = Time.time + 3f;
                Debug.LogWarning(name + ": colocá al Slime sobre el NavMesh y revisá el Bake y Agent Type.", this);
            }
        }
    }

    private void CreateAttackWarning()
    {
        attackWarning = new GameObject("AvisoAtaque");
        attackWarning.SetActive(false);
        attackWarning.transform.SetParent(transform, false);

        TextMeshPro warningText = attackWarning.AddComponent<TextMeshPro>();
        warningText.text = "!";
        warningText.fontSize = attackWarningFontSize;
        warningText.fontStyle = FontStyles.Bold;
        warningText.color = new Color(1f, 0.78f, 0.12f);
        warningText.alignment = TextAlignmentOptions.Center;
    }

    private void BeginAttackWarning()
    {
        attackWarning.SetActive(true);
        nextShot = Mathf.Max(nextShot, Time.time + attackWarningTime);
    }

    private void LateUpdate()
    {
        if (!attackWarning.activeSelf) return;

        attackWarning.transform.position = transform.position + Vector3.up * attackWarningHeight;
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
            attackWarning.transform.rotation = mainCamera.transform.rotation;
    }

    private void Shoot()
    {
        Vector3 origin = firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.5f;
        Vector3 aim = target.position + targetOffset;
        Vector3 direction = (aim - origin).normalized;
        if (direction.sqrMagnitude < 0.001f) return;

        // Cada proyectil es independiente: puede terminar su recorrido aunque muera el Slime.
        GameObject shot = new GameObject("Proyectil_Slime");
        shot.transform.SetPositionAndRotation(origin, Quaternion.LookRotation(direction));
        GameObject visual = projectileVisualPrefab != null
            ? Instantiate(projectileVisualPrefab, shot.transform)
            : GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visual.transform.SetParent(shot.transform, false);
        visual.transform.localPosition = Vector3.zero;
        if (projectileVisualPrefab == null) visual.transform.localScale = Vector3.one * projectileRadius * 2f;
        // El barrido del proyectil resuelve los impactos; no usar física duplicada.
        foreach (Collider c in visual.GetComponentsInChildren<Collider>(true)) c.enabled = false;
        foreach (Rigidbody body in visual.GetComponentsInChildren<Rigidbody>(true)) body.isKinematic = true;
        shot.AddComponent<EnemyRangerProjectile>().Initialize(transform, target, direction,
            projectileSpeed, attackDamage, projectileRadius, projectileLifetime, hitLayers);
        PlayAnimation(attackAnimation, true);
    }

    public void TakeDamage(int damage)
    {
        if (!IsAlive || damage <= 0f) return;
        currentHealth = Mathf.Max(0f, currentHealth - damage);
        Debug.Log(name + " recibió " + damage + " de daño. Vida: " + currentHealth, this);
        if (IsAlive) return;
        StopAgent();
        SetState(RangerState.Death);
        agent.enabled = false;
        foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = false;
        Destroy(gameObject, deathDelay);
    }

    private void SetState(RangerState state)
    {
        if (currentState == state) return;
        currentState = state;
        if (state == RangerState.Attack)
            BeginAttackWarning();
        else
            attackWarning.SetActive(false);

        switch (state)
        {
            case RangerState.Idle: PlayAnimation(idleAnimation); break;
            case RangerState.Chase: PlayAnimation(chaseAnimation); break;
            case RangerState.Death: PlayAnimation(deathAnimation); break;
        }
    }

    private void PlayAnimation(string stateName, bool restart = false)
    {
        if (slimeAnimator == null || slimeAnimator.runtimeAnimatorController == null ||
            string.IsNullOrWhiteSpace(stateName)) return;
        int hash = Animator.StringToHash(stateName);
        if (!slimeAnimator.HasState(0, hash)) return;
        if (restart) slimeAnimator.Play(hash, 0, 0f);
        else slimeAnimator.CrossFade(hash, 0.1f, 0);
    }

    private void StopAgent()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;
        agent.isStopped = true;
        agent.ResetPath();
    }

    private void OnEnable()
    {
        if (currentState == RangerState.Attack) BeginAttackWarning();
    }

    private void OnDisable()
    {
        if (attackWarning != null) attackWarning.SetActive(false);
        StopAgent();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, loseTargetRange);
    }
}

// Se agrega automáticamente a cada disparo. No asignarlo manualmente al Slime.
public class EnemyRangerProjectile : MonoBehaviour
{
    private Transform owner, target;
    private Vector3 direction;
    private float speed, radius, remainingLife;
    private int damage;
    private LayerMask layers;

    public void Initialize(Transform source, Transform victim, Vector3 travelDirection,
        float travelSpeed, int hitDamage, float hitRadius, float lifetime, LayerMask hitLayers)
    {
        owner = source;
        target = victim;
        direction = travelDirection;
        speed = travelSpeed;
        damage = hitDamage;
        radius = hitRadius;
        remainingLife = lifetime;
        layers = hitLayers;
    }

    private bool Ignore(Collider collider)
    {
        if (collider.transform.IsChildOf(transform)) return true;
        if (owner != null && collider.transform.IsChildOf(owner)) return true;
        // Ignorar zonas trigger ajenas al objetivo (detección, inventario, etc.).
        return collider.isTrigger && (target == null || !collider.transform.IsChildOf(target));
    }

    private void Update()
    {
        float step = speed * Mathf.Min(Time.deltaTime, remainingLife);
        Collider closest = null;
        float closestDistance = float.PositiveInfinity;
        // Detectar también impactos si el disparo nace dentro de un collider.
        foreach (Collider c in Physics.OverlapSphere(transform.position, radius, layers, QueryTriggerInteraction.Collide))
            if (!Ignore(c)) { closest = c; closestDistance = 0f; break; }
        foreach (RaycastHit hit in Physics.SphereCastAll(transform.position, radius,
            direction, step, layers, QueryTriggerInteraction.Collide))
        {
            if (Ignore(hit.collider) || hit.distance >= closestDistance) continue;
            closest = hit.collider;
            closestDistance = hit.distance;
        }
        if (closest != null)
        {
            // Solo dañar al objetivo; paredes y otros enemigos bloquean el disparo.
            if (target != null && closest.transform.IsChildOf(target))
            {
                IDamageable receiver = closest.GetComponentInParent<IDamageable>();
                if (receiver == null) receiver = target.GetComponent<IDamageable>();
                if (receiver != null) receiver.TakeDamage(damage);
            }
            Destroy(gameObject);
            return;
        }
        transform.position += direction * step;
        remainingLife -= Time.deltaTime;
        if (remainingLife <= 0f) Destroy(gameObject);
    }
}

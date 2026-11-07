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

using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(DiceInventory))]
[RequireComponent(typeof(PlayerHealth))]
[DisallowMultipleComponent]
public class PlayerController3D : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float speed = 6f;
    [SerializeField] private float rotationSpeed = 120f;
    [Tooltip("Activar solo si el modelo mira hacia el eje Z negativo.")]
    [SerializeField] private bool invertForward = false;
    [Tooltip("Desactivada inicialmente para conservar el movimiento plano de Nia.")]
    [SerializeField] private bool useGravity = false;
    [SerializeField] private float gravity = -9.81f;

    [Header("Altura fija de Nia")]
    [SerializeField] private bool lockHeight = true;
    [SerializeField] private float fixedY = 1f;
    [SerializeField] private BoxCollider zonaAlturaPuente;

    [Header("Interacción")]
    [SerializeField] private float interactRange = 2f;
    [SerializeField] private LayerMask interactableLayer;

    [Header("Combate Ranged")]
    [SerializeField] private Transform firePoint;
    [Tooltip("Arrastrar un prefab con DiceProjectile. Se reutiliza mediante un pool propio.")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField, Min(1)] private int initialBulletCount = 10;
    [SerializeField] private ProjectileDataSO activeProjectileData;

    private readonly List<GameObject> bulletInstances = new List<GameObject>();
    private Transform bulletPoolRoot;
    private Transform inactiveCreationRoot;

    [Header("Retroceso visual opcional (como Rossie)")]
    [Tooltip("Solo un hijo visual del arma. No asignar Nia ni la cámara.")]
    [SerializeField] private Transform recoilVisual;
    [SerializeField] private float recoilAngle = -10f;
    [SerializeField, Min(0.01f)] private float recoilResetDelay = 0.25f;
    private Quaternion recoilOriginalRotation;
    private Coroutine recoilRoutine;

    [Header("Combate Melee")]
    [SerializeField] private float meleeRange = 1.5f;
    [SerializeField] private int meleeDamage = 2;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField, Min(0.05f)] private float meleeCooldown = 0.5f;

    private CharacterController controller;
    [SerializeField] private Animator animator;
    private DiceInventory diceInventory;
    private PlayerHealth health;
    private float verticalVelocity;
    private float nextMelee;

    [Header("Diagnóstico de disparo")]
    [SerializeField] private bool debugShooting = true;
    [SerializeField] private KeyCode alternateFireKey = KeyCode.F;
    public bool IsAlive => health != null && health.IsAlive;
    public StateMachine StateMachine { get; private set; }
    public PlayerIdleState IdleState { get; private set; }
    public PlayerMoveState MoveState { get; private set; }
    public PlayerDeadState DeadState { get; private set; }

    public int CurrentHealth => health != null ? health.CurrentHealth : 0;
    public int MaxHealth => health != null ? health.MaxHealth : 0;
    private Vector3 Forward => invertForward ? -transform.forward : transform.forward;



    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        diceInventory = GetComponent<DiceInventory>();
        health = GetComponent<PlayerHealth>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        if (recoilVisual != null) recoilOriginalRotation = recoilVisual.localRotation;
        StateMachine = new StateMachine();
        IdleState = new PlayerIdleState(this);
        MoveState = new PlayerMoveState(this);
        DeadState = new PlayerDeadState(this);
    }

    private void Start()
    {
        EnforceFixedHeight();
        StateMachine.Initialize(IsAlive ? (IState)IdleState : DeadState);
        if (debugShooting)
            Debug.LogWarning($"[Disparo Nia] Controlador activo en {name}. Vida: {CurrentHealth}/{MaxHealth}. Munición: {(diceInventory != null ? diceInventory.CurrentDiceCount : -1)}. Probar clic izquierdo o F dentro de Game.", this);
    }

    private void Update()
    {
        StateMachine?.Update();

        if (animator != null)
        {
            bool caminando = IsAlive &&
                            controller.enabled &&
                            Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.01f;

            animator.SetBool("isWalking", caminando);
        }
    }

    public bool HasMovementInput()
    {
        return Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.01f ||
               Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.01f;
    }

    // Los estados llaman a este método una sola vez por cuadro.
    public void HandleInputs()
    {
        if (!IsAlive) return;
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(alternateFireKey))
        {
            if (debugShooting) Debug.LogWarning("[Disparo Nia] Entrada de disparo detectada.", this);
            TryRangedAttack();
        }
        if (Input.GetKeyDown(KeyCode.E)) TryInteract();
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(1)) ExecuteMeleeAttack();
    }

    public void Move()
    {
        if (!IsAlive || !controller.enabled) return;
        float avance = Input.GetAxisRaw("Vertical");
        float giro = Input.GetAxisRaw("Horizontal");
        transform.Rotate(Vector3.up, giro * rotationSpeed * Time.deltaTime, Space.World);
        Vector3 direccion = Forward;
        direccion.y = 0f;
        direccion.Normalize();
        Vector3 movement = direccion * avance * speed;
        movement.y = GetVerticalVelocity();
        controller.Move(movement * Time.deltaTime);
    }

    public void ApplyGravityOnly()
    {
        if (!IsAlive || !controller.enabled) return;
        float velocity = GetVerticalVelocity();
        if (velocity != 0f) controller.Move(Vector3.up * velocity * Time.deltaTime);
    }

    private float GetVerticalVelocity()
    {
        //if (lockHeight || !useGravity)
        if (!EstaEnZonaDelPuente() && (lockHeight || !useGravity))
        {
            verticalVelocity = 0f;
            return 0f;
        }
        if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
        verticalVelocity += gravity * Time.deltaTime;
        return verticalVelocity;
    }

    private void LateUpdate()
    {
        EnforceFixedHeight();
    }

    private void EnforceFixedHeight()
    {
        if (!lockHeight || EstaEnZonaDelPuente()) return;
        verticalVelocity = 0f;
        Vector3 position = transform.position;
        if (position.y == fixedY) return;
        // El controlador puede modificar la altura al resolver colisiones o escalones.
        bool wasEnabled = controller != null && controller.enabled;
        if (wasEnabled) controller.enabled = false;
        position.y = fixedY;
        transform.position = position;
        if (wasEnabled) controller.enabled = true;
    }

    private void TryInteract()
    {
        if (TryGetComponent<InteraccionBasesFinal>(out var basesFinales) &&
            basesFinales.IntentarInteractuar(firePoint))
            return;
            
        Collider[] hits = Physics.OverlapSphere(transform.position, interactRange, interactableLayer);
        foreach (var hit in hits)
        {
            var interactable = hit.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact(this);
                break;
            }
        }
    }

    private void TryRangedAttack()
    {
        if (health == null)
        {
            Debug.LogWarning("[Disparo Nia] Falta PlayerHealth en el mismo objeto que PlayerController3D.", this);
            return;
        }
        if (!health.IsAlive)
        {
            Debug.LogWarning($"[Disparo Nia] Bloqueado: PlayerHealth indica vida {health.CurrentHealth}. Revisar vida y respawn.", this);
            return;
        }
        if (diceInventory == null)
        {
            Debug.LogWarning("[Disparo Nia] Falta DiceInventory en el mismo objeto que PlayerController3D.", this);
            return;
        }
        if (!diceInventory.HasDice())
        {
            Debug.Log("Sin munición: acercate al banco para cargar el cargador.", this);
            return;
        }
        if (activeProjectileData == null)
        {
            Debug.LogWarning("Asigná el asset ProjectileData de este nivel en Active Projectile Data antes de disparar.", this);
            return;
        }

        GameObject projectileObj;
        if (bulletPrefab != null)
        {
            if (bulletPrefab.GetComponent<DiceProjectile>() == null)
            {
                Debug.LogWarning("Bullet Prefab necesita DiceProjectile en su objeto raíz.", this);
                return;
            }
            projectileObj = GetBulletFromPrefabPool();
        }
        else
        {
            // Compatibilidad con la configuración anterior de la escena.
            if (ObjectPool.Instance == null)
            {
                Debug.LogWarning("Asigná Bullet Prefab o configurá el ObjectPool de la escena.", this);
                return;
            }
            projectileObj = ObjectPool.Instance.GetPooledObject();
        }
        if (projectileObj == null)
        {
            Debug.LogWarning("[Disparo Nia] El pool no entregó ninguna bala.", this);
            return;
        }
        if (!projectileObj.TryGetComponent<DiceProjectile>(out var projectile))
        {
            projectileObj.SetActive(false);
            Debug.LogWarning("La bala necesita DiceProjectile. No se consume el dado.", this);
            return;
        }

        // Configurar con la bala inactiva, antes de ejecutar su OnEnable.
        projectileObj.SetActive(false);
        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position + Forward;
        Quaternion spawnRotation = firePoint != null ? firePoint.rotation : Quaternion.LookRotation(Forward, Vector3.up);
        projectileObj.transform.SetPositionAndRotation(spawnPosition, spawnRotation);
        projectile.SetOwner(transform);
        projectile.SetProjectileData(activeProjectileData);
        projectileObj.SetActive(true);
        diceInventory.UseDie();
        if (animator != null)
            animator.SetTrigger("Shoot");
        if (debugShooting)
            Debug.LogWarning($"[Disparo Nia] Bala activada: {projectileObj.name}. Posición: {spawnPosition}. Dirección: {projectileObj.transform.forward}. Munición: {diceInventory.CurrentDiceCount}.", this);
        ApplyRecoil();
    }

    [ContextMenu("Prueba/Disparar sin mouse")]
    private void TestShoot()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[Disparo Nia] Iniciar Play para ejecutar esta prueba.", this);
            return;
        }
        Debug.LogWarning("[Disparo Nia] Prueba desde Inspector: se omite la entrada del mouse.", this);
        TryRangedAttack();
    }

    private GameObject GetBulletFromPrefabPool()
    {
        if (bulletPoolRoot == null)
        {
            bulletPoolRoot = new GameObject("Pool_Balas_Nia").transform;
            GameObject staging = new GameObject("CreacionInactiva");
            staging.transform.SetParent(bulletPoolRoot, false);
            staging.SetActive(false);
            inactiveCreationRoot = staging.transform;
            for (int i = 0; i < Mathf.Max(1, initialBulletCount); i++) CreatePooledBullet();
        }
        for (int i = bulletInstances.Count - 1; i >= 0; i--)
        {
            GameObject instance = bulletInstances[i];
            if (instance == null)
            {
                bulletInstances.RemoveAt(i);
                continue;
            }
            if (!instance.activeSelf) return instance;
        }
        // Ampliar solo cuando todas las balas existentes siguen en vuelo.
        return CreatePooledBullet();
    }

    private GameObject CreatePooledBullet()
    {
        // El padre inactivo evita disparar OnEnable antes de configurar los datos.
        GameObject instance = Instantiate(bulletPrefab, inactiveCreationRoot, false);
        instance.SetActive(false);
        instance.transform.SetParent(bulletPoolRoot, true);
        bulletInstances.Add(instance);
        return instance;
    }

    private void OnDestroy()
    {
        if (bulletPoolRoot != null) Destroy(bulletPoolRoot.gameObject);
    }

    private void ExecuteMeleeAttack()
    {
        if (Time.time < nextMelee) return;
        nextMelee = Time.time + meleeCooldown;
        Debug.Log("¡Ataque Melee ejecutado!");
        Vector3 attackPoint = transform.position + Forward * (meleeRange * 0.5f);
        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint, meleeRange, enemyLayer);
        var damaged = new HashSet<IDamageable>();
        foreach (var enemy in hitEnemies)
        {
            if (enemy.transform.IsChildOf(transform)) continue;
            var damageable = enemy.GetComponentInParent<IDamageable>();
            if (damageable != null && damaged.Add(damageable))
            {
                damageable.TakeDamage(meleeDamage);
            }
        }
    }

    // Compatibilidad con llamadas directas al controlador de Rossie.
    // Las interfaces de salud pertenecen únicamente a PlayerHealth.
    public void TakeDamage(int damage) { if (health != null) health.TakeDamage(damage); }
    public void healing(int amount) { if (health != null) health.healing(amount); }
    public void ResetMotion() { verticalVelocity = 0f; nextMelee = 0f; }

    private void ApplyRecoil()
    {
        if (recoilVisual == null) return;
        if (recoilRoutine != null) StopCoroutine(recoilRoutine);
        recoilVisual.localRotation = recoilOriginalRotation * Quaternion.Euler(recoilAngle, 0f, 0f);
        recoilRoutine = StartCoroutine(ResetRecoil());
    }

    private IEnumerator ResetRecoil()
    {
        yield return new WaitForSeconds(recoilResetDelay);
        if (recoilVisual != null) recoilVisual.localRotation = recoilOriginalRotation;
        recoilRoutine = null;
    }

    private void OnDisable()
    {
        if (recoilRoutine != null) StopCoroutine(recoilRoutine);
        recoilRoutine = null;
        if (recoilVisual != null) recoilVisual.localRotation = recoilOriginalRotation;
        verticalVelocity = 0f;
    }

    private void OnDrawGizmosSelected()
    {
        // Rango de Interacción (Amarillo)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);

        // Rango Melee (Rojo)
        Gizmos.color = Color.red;
        Vector3 attackPoint = transform.position + Forward * (meleeRange * 0.5f);
        Gizmos.DrawWireSphere(attackPoint, meleeRange);
    }

    private bool EstaEnZonaDelPuente()
    {
        if (zonaAlturaPuente == null)
            return false;

        Vector3 posicionLocal =
            zonaAlturaPuente.transform.InverseTransformPoint(transform.position)
            - zonaAlturaPuente.center;

        Vector3 mitad = zonaAlturaPuente.size * 0.5f;

        return Mathf.Abs(posicionLocal.x) <= mitad.x
            && Mathf.Abs(posicionLocal.z) <= mitad.z;
    }
}

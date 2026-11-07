/**************************
 * Proyecto: BarrerasInvisibles
 * Universidad de la Ciudad de Buenos Aires
 * Periodo: 2do cuatrimestre 2026 
 * Materia: Programación 2
 * Integrantes: Ariana Ruiz, Eduardo Ortega, Ruth Wulf
 * Función: Enemigo de combate cuerpo a cuerpo.
 * Explicación: Hereda de EnemyBase. Persigue a Nia moviendo su Rigidbody
 * en el plano horizontal para que la física resuelva los contactos.
 *********************************************************************************************/

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyMelee : EnemyBase
{
    [Header("Configuración de Ataque Melee")]
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackCooldown = 1.5f;

    private float lastAttackTime;
    private Rigidbody cuerpo;
    private Quaternion orientacionModelo;
    private Vector3 velocidadPersecucion;

    protected override void Awake()
    {
        base.Awake();
        cuerpo = GetComponent<Rigidbody>();
        if (cuerpo == null || cuerpo.isKinematic)
        {
            Debug.LogError("[NIA] EnemyMelee necesita un Rigidbody en el mismo objeto, con Is Kinematic desactivado.", this);
            return;
        }

        Quaternion giroInicial = Quaternion.Euler(0f, cuerpo.rotation.eulerAngles.y, 0f);
        orientacionModelo = Quaternion.Inverse(giroInicial) * cuerpo.rotation;
    }

    // Esta variante ejecuta el comportamiento de EnemyBase en el paso de física.
    protected override void Update() { }

    private void FixedUpdate()
    {
        if (cuerpo == null || cuerpo.isKinematic) return;

        velocidadPersecucion = Vector3.zero;
        if (currentHealth > 0) base.Update();
        cuerpo.linearVelocity = velocidadPersecucion;
    }

    private void OnDisable()
    {
        if (cuerpo != null && !cuerpo.isKinematic)
            cuerpo.linearVelocity = Vector3.zero;
    }

    protected override void ChasePlayer()
    {
        Vector3 direccion = playerTransform.position - cuerpo.position;
        direccion.y = 0f;
        if (direccion.sqrMagnitude < 0.000001f) return;

        velocidadPersecucion = direccion.normalized * speed;
        cuerpo.MoveRotation(Quaternion.LookRotation(direccion) * orientacionModelo);
    }

    protected override void AttackPlayer()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            Debug.Log($"{gameObject.name} ataca a Nia infligiendo {attackDamage} de daño.");

            if (playerTransform != null && playerTransform.TryGetComponent<IDamageable>(out var playerDamageable))
            {
                playerDamageable.TakeDamage(attackDamage);
            }
        }
    }
}

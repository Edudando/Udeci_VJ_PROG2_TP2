/**************************
 * Proyecto: BarrerasInvisibles
 * Universidad de la Ciudad de Buenos Aires
 * Periodo: 2do cuatrimestre 2026 
 * Materia: Programación 2
 * Integrantes: Ariana Ruiz, Eduardo Ortega, Ruth Wulf
 * Función: Enemigo de combate cuerpo a cuerpo.
 * Explicación: Hereda de EnemyBase. Persigue directamente a Nia cuando 
 * entra en su área de detección y aplica daño continuo al estar en rango de ataque.
 *********************************************************************************************/

using UnityEngine;

public class EnemyMelee : EnemyBase
{
    [Header("Configuración de Ataque Melee")]
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackCooldown = 1.5f;

    private float lastAttackTime;

    protected override void ChasePlayer()
    {
        if (playerTransform == null) return;

        // Movimiento básico hacia la posición del jugador
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        direction.y = 0f; // Mantiene el desplazamiento en el plano horizontal XZ

        transform.position += direction * speed * Time.deltaTime;
        if (direction != Vector3.zero)
        {
            transform.forward = direction;
        }
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
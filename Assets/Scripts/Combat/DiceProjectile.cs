/**************************
 * Proyecto: BarrerasInvisibles
 * Universidad de la Ciudad de Buenos Aires
 * Periodo: 2do cuatrimestre 2026 
 * Materia: Programación 2
 * Integrantes: Ariana Ruiz, Eduardo Ortega, Ruth Wulf
 * Función: Proyectil de dado lanzado por el jugador.
 * Explicación: Se desplaza en linea recta utilizando la configuración 
 * provista por un ProjectileDataSO, inflige daño a entidades con 
 * IDamageable y retorna al pool al colisionar o agotar su tiempo de vida.
 *********************************************************************************************/

using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class DiceProjectile : MonoBehaviour
{
    [Header("Configuración vía ScriptableObject")]
    [SerializeField] private ProjectileDataSO projectileData;
    [Header("Detección de impactos")]
    [SerializeField, Min(0.01f)] private float hitRadius = 0.08f;
    [SerializeField] private LayerMask hitLayers = ~0;

    private float currentLifeTimer;
    private Transform owner;
    private Rigidbody body;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        // El script mueve el dado: la gravedad no debe desviarlo.
        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>(true))
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        if (colliders.Length == 0)
        {
            var sphere = gameObject.AddComponent<SphereCollider>();
            sphere.radius = hitRadius;
            colliders = new Collider[] { sphere };
        }
        foreach (Collider c in colliders) c.isTrigger = true;
    }

    private void OnEnable()
    {
        currentLifeTimer = Mathf.Max(0.01f, projectileData != null ? projectileData.LifeTime : 3f);
        if (body != null) body.position = transform.position;
    }

    private void FixedUpdate()
    {
        float speed = Mathf.Max(0f, projectileData != null ? projectileData.Speed : 12f);
        float step = speed * Mathf.Min(Time.fixedDeltaTime, currentLifeTimer);
        Vector3 start = body.position;
        Vector3 direction = transform.forward;
        Collider closest = null;
        float closestDistance = float.PositiveInfinity;

        // También detectar paredes/enemigos si el proyectil nace solapado.
        foreach (Collider c in Physics.OverlapSphere(start, hitRadius, hitLayers, QueryTriggerInteraction.Collide))
            if (!Ignore(c)) { closest = c; closestDistance = 0f; break; }
        if (step > 0f)
        {
            foreach (RaycastHit hit in Physics.SphereCastAll(start, hitRadius, direction,
                step, hitLayers, QueryTriggerInteraction.Collide))
            {
                if (Ignore(hit.collider) || hit.distance >= closestDistance) continue;
                closest = hit.collider;
                closestDistance = hit.distance;
            }
        }
        if (closest != null)
        {
            Hit(closest);
            return;
        }
        body.MovePosition(start + direction * step);
        currentLifeTimer -= Time.fixedDeltaTime;
        if (currentLifeTimer <= 0f) Deactivate();
    }

    private bool Ignore(Collider other)
    {
        if (other.transform.IsChildOf(transform)) return true;
        if (owner != null && other.transform.IsChildOf(owner)) return true;
        if (other.GetComponentInParent<DiceProjectile>() != null) return true;
        // Ignorar a Nia aunque el collider esté en un hijo sin tag Player.
        if (other.GetComponentInParent<PlayerHealth>() != null || other.CompareTag("Player")) return true;
        // Las zonas de detección no deben absorber los disparos.
        return other.isTrigger && other.GetComponentInParent<IDamageable>() == null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!gameObject.activeSelf || (hitLayers.value & (1 << other.gameObject.layer)) == 0 || Ignore(other)) return;
        Hit(other);
    }

    private void Hit(Collider other)
    {
        var damageable = other.GetComponentInParent<IDamageable>();
        int damage = Mathf.Max(0, projectileData != null ? projectileData.DamageAmount : 1);
        // Desactivar antes de notificar para impedir impactos duplicados.
        Deactivate();
        if (damageable != null && damage > 0) damageable.TakeDamage(damage);
    }

    public void SetOwner(Transform source) { owner = source; }

    public void SetProjectileData(ProjectileDataSO data)
    {
        projectileData = data;
        if (gameObject.activeInHierarchy)
            currentLifeTimer = Mathf.Max(0.01f, projectileData != null ? projectileData.LifeTime : 3f);
    }

    private void Deactivate() { gameObject.SetActive(false); }
}


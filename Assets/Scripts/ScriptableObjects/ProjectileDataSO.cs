/**************************
 * Proyecto: BarrerasInvisibles
 * Universidad de la Ciudad de Buenos Aires
 * Periodo: 2do cuatrimestre 2026 
 * Materia: Programación 2
 * Integrantes: Ariana Ruiz, Eduardo Ortega, Ruth Wulf
 * Función: ScriptableObject con datos de configuración del proyectil.
 * Explicación: Contenedor de datos que define la velocidad, el daño y el tipo 
 * de área/minijuego (Lengua, Matemática, Ciencias) para parametrizar
 * los proyectiles de forma desacoplada y reutilizable.
 *********************************************************************************************/

using UnityEngine;

public enum SubjectType
{
    Lengua,
    Matematica,
    Ciencias
}

[CreateAssetMenu(fileName = "NewProjectileData", menuName = "BarrerasInvisibles/Projectile Data")]
public class ProjectileDataSO : ScriptableObject
{
    [Header("Identificación y Área")]
    [SerializeField] private SubjectType subject;
    [SerializeField] private string projectileName = "Dado de Ataque";

    [Header("Atributos de Combate")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private float lifeTime = 3f;

    // Propiedades públicas de solo lectura (Encapsulamiento)
    public SubjectType Subject => subject;
    public string ProjectileName => projectileName;
    public float Speed => speed;
    public int DamageAmount => damageAmount;
    public float LifeTime => lifeTime;
}
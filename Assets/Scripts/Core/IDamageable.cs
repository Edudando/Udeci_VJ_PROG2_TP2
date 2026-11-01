/**************************
 * Proyecto: BarrerasInvisibles
 * Universidad de la Ciudad de Buenos Aires
 * Periodo: 2do cuatrimestre 2026 
 * Materia: Programación 2
 * Integrantes: Ariana Ruiz, Eduardo Ortega, Ruth Wulf
 * Función: Interfaz para entidades que pueden recibir daño.
 * Explicación: Contrato genérico para salud y daño, permitiendo que proyectiles o ataques melee dañen a cualquier objeto o entidad sin importar su clase específica.
 *********************************************************************************************/

public interface IDamageable
{
    void TakeDamage(int damageAmount);
}
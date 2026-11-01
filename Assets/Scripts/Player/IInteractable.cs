/**************************
 * Proyecto: BarrerasInvisibles
 * Universidad de la Ciudad de Buenos Aires
 * Periodo: 2do cuatrimestre 2026 
 * Materia: Programación 2
 * Integrantes: Ariana Ruiz, Eduardo Ortega, Ruth Wulf
 * Función: Interfaz para objetos interactivos del escenario.
 * Explicación: Define el contrato que debe implementar cualquier elemento con el que el jugador pueda interactuar (como el pedestal del puzle). Permite desacoplar el script del jugador de los objetos específicos con los que interactúa (Principio de Inversión de Dependencias / SOLID).
 *********************************************************************************************/

public interface IInteractable
{
    void Interact(PlayerController3D player);
    string GetInteractionPrompt();
}
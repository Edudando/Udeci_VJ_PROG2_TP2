/**************************
 * Proyecto: BarrerasInvisibles
 * Universidad de la Ciudad de Buenos Aires
 * Período: 2.º cuatrimestre de 2026
 * Materia: Programación 2
 * Programador: Eduardo Ortega
 * Función: Inicia la partida desde el menú principal. Restablece la
 * velocidad normal del juego y carga la escena indicada en el Inspector.
 *********************************************************************************************/
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipalNia : MonoBehaviour
{
    [SerializeField] private string escenaJuego;

    public void Jugar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(escenaJuego);
    }
}
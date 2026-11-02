/**************************
 * Proyecto: BarrerasInvisibles
 * Universidad de la Ciudad de Buenos Aires
 * Periodo: 2do cuatrimestre 2026 
 * Materia: Programación 2
 * Integrantes: Ariana Ruiz, Eduardo Ortega, Ruth Wulf
 * Función: Gestión de la interfaz de usuario para puzles.
 * Explicación: Controla la activación del panel de diálogo/puzle y muestra mensajes dinámicos al interactuar con los pedestales.
 *********************************************************************************************/

using UnityEngine;
using TMPro;

public class PuzzleUI : MonoBehaviour
{
    public static PuzzleUI Instance { get; private set; }

    [Header("Componentes UI")]
    [SerializeField] private GameObject puzzlePanel;
    [SerializeField] private TextMeshProUGUI puzzleText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(false);
        }
    }

    public void ShowPuzzle(string message)
    {
        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(true);
        }

        if (puzzleText != null)
        {
            puzzleText.text = message;
        }
    }

    public void HidePuzzle()
    {
        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(false);
        }
    }
}
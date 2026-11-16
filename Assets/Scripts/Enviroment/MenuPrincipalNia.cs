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
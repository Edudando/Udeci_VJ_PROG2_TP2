using UnityEngine;
using System.IO;

public class CargadorJson : MonoBehaviour
{
    public ListaPalabras datosJuego;

    void Start()
    {
        CargarDatos();
    }

    void CargarDatos()
    {
        // Cargar el archivo JSON desde la carpeta Resources
        TextAsset jsonText = Resources.Load<TextAsset>("palabras");

        if (jsonText != null)
        {
            // Convertir el JSON en objetos de C#
            datosJuego = JsonUtility.FromJson<ListaPalabras>(jsonText.text);

            // Ejemplo: Recorrer las palabras y cargar su textura 1024x1024
            foreach (ItemPalabra item in datosJuego.palabras)
            {
                // Quitamos la extensión .png para cargar mediante Resources.Load
                string nombreImagenSinExtension = Path.GetFileNameWithoutExtension(item.imagen);
                
                // Carga la textura desde Assets/Resources/Imagenes/
                Texture2D textura = Resources.Load<Texture2D>("Imagenes/" + nombreImagenSinExtension);

                Debug.Log($"Palabra: {item.palabra} | Dificultad: {item.dificultad} | Textura cargada: {textura != null}");
            }
        }
        else
        {
            Debug.LogError("No se encontró el archivo palabras.json en Assets/Resources/");
        }
    }
}
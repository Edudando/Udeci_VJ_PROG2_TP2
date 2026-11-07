using System.Collections.Generic;
using UnityEngine;

public class ControladorPalabras : MonoBehaviour
{
    [Header("Un dado con seis letras")]
    [SerializeField] private DadoCara dado;

    // Opcional: estas asignaciones tienen prioridad sobre Resources/Alfabeto.
    [System.Serializable]
    public struct LetraData
    {
        public char letra;
        public Texture2D textura;
    }
    public List<LetraData> diccionarioLetras = new List<LetraData>();
    public string PalabraActual { get; private set; }

    // El GameManager llama con la palabra candidata ya normalizada.
    public bool TieneTexturasPara(string palabra, out char letraFaltante)
    {
        foreach (char letra in palabra)
        {
            if (BuscarTexturaDeLetra(letra) != null) continue;

            letraFaltante = letra;
            return false;
        }

        letraFaltante = '\0';
        return true;
    }

    public void MostrarPalabra(string palabra)
    {
        if (string.IsNullOrWhiteSpace(palabra))
        {
            Debug.LogError("La palabra está vacía.", this);
            return;
        }
        palabra = palabra.Trim().ToUpperInvariant();
        if (palabra.Length != 6)
        {
            Debug.LogError("Se necesitan exactamente 6 letras: " + palabra, this);
            return;
        }
        if (dado == null) dado = GetComponent<DadoCara>();
        if (dado == null)
        {
            Debug.LogError("Asigna DadoCara en el campo Dado del ControladorPalabras.", this);
            return;
        }

        Texture2D[] texturas = new Texture2D[6];
        for (int i = 0; i < 6; i++)
        {
            texturas[i] = BuscarTexturaDeLetra(palabra[i]);
            if (texturas[i] == null)
            {
                Debug.LogError("Falta la imagen de " + palabra[i] + " en Assets/Resources/Alfabeto/ (nombre sin extensión).", this);
                return;
            }
        }
        for (int i = 0; i < 6; i++) dado.EstablecerLetraEnCara(i, texturas[i]);
        PalabraActual = palabra;
    }

    private Texture2D BuscarTexturaDeLetra(char letra)
    {
        if (diccionarioLetras != null)
            foreach (LetraData item in diccionarioLetras)
                if (char.ToUpperInvariant(item.letra) == letra && item.textura != null)
                    return item.textura;
        return Resources.Load<Texture2D>("Alfabeto/" + letra);
    }
}

using System;
using System.Collections.Generic;

[Serializable]
public class ItemPalabra
{
    public string palabra;
    public int dificultad;
    public string imagen;
}

[Serializable]
public class ListaPalabras
{
    public List<ItemPalabra> palabras;
}
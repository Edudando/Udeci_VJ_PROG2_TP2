/**************************
 * Proyecto: BarrerasInvisibles
 * Universidad de la Ciudad de Buenos Aires
 * Período: 2.º cuatrimestre de 2026
 * Materia: Programación 2
 * Programador: Eduardo Ortega
 * Función: Carga palabras.json desde Resources al iniciar la escena
 * y convierte su contenido en un objeto ListaPalabras. Para cada
 * palabra, busca su textura en Resources/Imagenes y registra en la
 * consola la palabra, su dificultad y si la textura se cargó.
 *********************************************************************************************/
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
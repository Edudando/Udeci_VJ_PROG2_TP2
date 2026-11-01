/**************************
 * Proyecto: BarrerasInvisibles
 * Universidad de la Ciudad de Buenos Aires
 * Periodo: 2do cuatrimestre 2026 
 * Materia: Programación 2
 * Integrantes: Ariana Ruiz, Eduardo Ortega, Ruth Wulf
 * Función: Pool de objetos genérico para reutilizar instancias en memoria.
 * Explicación: Gestiona la creación y reutilización de proyectiles u otros objetos,
 * evitando la instanciación/destrucción constante para optimizar
 * el rendimiento del recolector de basura (Garbage Collector).
 *********************************************************************************************/

using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }

    [Header("Configuración del Pool")]
    [SerializeField] private GameObject prefab;
    [SerializeField] private int initialPoolSize = 10;

    private List<GameObject> pooledObjects;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializePool();
    }

    private void InitializePool()
    {
        pooledObjects = new List<GameObject>();

        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            pooledObjects.Add(obj);
        }
    }

    public GameObject GetPooledObject()
    {
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if (!pooledObjects[i].activeInHierarchy)
            {
                return pooledObjects[i];
            }
        }

        // Si se agotan los objetos iniciales, expansiona el pool de forma dinámica
        GameObject newObj = Instantiate(prefab, transform);
        newObj.SetActive(false);
        pooledObjects.Add(newObj);
        return newObj;
    }
}
using System.Collections.Generic;
using UnityEngine;

public class TileSpawner : MonoBehaviour
{
    public GameObject tilePrefab;
    public Transform playerTransform; // Certifique-se que esta variável está configurada no Inspector!

    private List<GameObject> activeTiles = new List<GameObject>();
    public float tileLength = 100f;
    private int numberOfTiles = 5;
    private float spawnZ = 0f;

    public void StartGameSpawning()
    {
        ResetTiles();
        
        spawnZ = 0f;
        for (int i = 0; i < numberOfTiles; i++)
        {
            SpawnTile();
        }
    }

    public void ResetTiles()
    {
        foreach (GameObject tile in activeTiles)
        {
            Destroy(tile);
        }
        activeTiles.Clear();
        spawnZ = 0f;
    }

    private void SpawnTile()
    {
        GameObject go;
        go = Instantiate(tilePrefab, transform.forward * spawnZ, transform.rotation);
        activeTiles.Add(go);
        spawnZ += tileLength;
    }

    private void Update()
    {
        if (playerTransform == null || activeTiles.Count == 0) return;

        // Se o jogador estiver além de 75% do tile mais antigo, gere um novo
        if (playerTransform.position.z > activeTiles[0].transform.position.z + tileLength * 0.75f)
        {
            SpawnTile();
            DeleteTile();
        }
    }
    
    private void DeleteTile()
    {
        if (activeTiles.Count > 0)
        {
            Destroy(activeTiles[0]);
            activeTiles.RemoveAt(0);
        }
    }
}
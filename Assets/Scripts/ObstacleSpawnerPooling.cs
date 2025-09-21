using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawnerPooling : MonoBehaviour
{
    [System.Serializable]
    public class ObstaclePool
    {
        public GameObject prefab;
        public int size;
        public Queue<GameObject> pool;
    }

    public ObstaclePool[] obstaclePools;
    private List<GameObject> activeObstacles = new List<GameObject>();

    public static ObstacleSpawnerPooling Instance;

    public Transform player;
    public float spawnDistance = 40f;
    public float laneDistance = 2f;
    public float segmentLength = 5f;

    private float nextSpawnZ = 0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        InitializePools();
    }

    private void InitializePools()
    {
        if (obstaclePools == null) return;
        
        foreach (var obstaclePool in obstaclePools)
        {
            if (obstaclePool.prefab == null) continue;
            
            obstaclePool.pool = new Queue<GameObject>();
            for (int i = 0; i < obstaclePool.size; i++)
            {
                GameObject obj = Instantiate(obstaclePool.prefab);
                obj.SetActive(false);
                obstaclePool.pool.Enqueue(obj);
            }
        }
    }

    public GameObject GetObstacle(int poolIndex)
    {
        if (poolIndex >= obstaclePools.Length || obstaclePools[poolIndex].prefab == null)
        {
            return null;
        }
        
        // Se o pool estiver vazio, cria um novo objeto para o pool.
        if (obstaclePools[poolIndex].pool.Count == 0)
        {
            GameObject newObj = Instantiate(obstaclePools[poolIndex].prefab);
            newObj.SetActive(false);
            obstaclePools[poolIndex].pool.Enqueue(newObj);
        }

        GameObject obj = obstaclePools[poolIndex].pool.Dequeue();
        obj.SetActive(true);
        activeObstacles.Add(obj);
        return obj;
    }

    public void ReturnObstacle(GameObject obj)
    {
        if (obj == null) return;

        for (int i = 0; i < obstaclePools.Length; i++)
        {
            // Usa Contains para verificar se o objeto existe antes de tentar removê-lo
            if (activeObstacles.Contains(obj) && obj.name.StartsWith(obstaclePools[i].prefab.name))
            {
                obj.SetActive(false);
                obstaclePools[i].pool.Enqueue(obj);
                activeObstacles.Remove(obj);
                return;
            }
        }
    }

    public void ResetSpawner()
    {
        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            ReturnObstacle(activeObstacles[i]);
        }
        activeObstacles.Clear();
        
        nextSpawnZ = 0f + spawnDistance;
    }

    private void Update()
    {
        if (player == null) return;

        if (player.position.z + spawnDistance > nextSpawnZ)
        {
            SpawnSegment(nextSpawnZ);
            nextSpawnZ += segmentLength;
        }
    }

    void SpawnSegment(float zPos)
    {
        int lanes = 3;

        for (int lane = 0; lane < lanes; lane++)
        {
            float rand = Random.value;

            // Obstáculo
            if (rand < 0.25f)
            {
                GameObject obstacle = GetObstacle(0);
                if (obstacle != null)
                {
                    Vector3 pos = new Vector3((lane - 1) * laneDistance, 0.5f, zPos + Random.Range(-1f, 1f));
                    obstacle.transform.position = pos;
                }
            }
            // Moeda
            else if (rand < 0.40f)
            {
                GameObject coin = GetObstacle(1);
                if (coin != null)
                {
                    Vector3 pos = new Vector3((lane - 1) * laneDistance, 0.5f, zPos + Random.Range(-0.5f, 0.5f));
                    coin.transform.position = pos;
                }
            }
        }
    }
}
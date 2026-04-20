using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public Transform player;

    private const float SEGMENT_LEN = 20f;
    private const int SEGMENTS_AHEAD = 6;
    private const float DESPAWN_BEHIND = 30f;
    private const int CLEAR_START_SEGS = 2;

    [Header("Monedas")]
    [Tooltip("Separación entre monedas de una misma fila")]
    public float coinSpacing = 1.8f;

    private class Segment
    {
        public float startZ;
        public float endZ;
        public GameObject ground;
        public readonly List<GameObject> obstacles = new(); // solo obstáculos, sin monedas
    }

    private readonly List<Segment> _active = new();
    private float _nextSpawnZ = -20f;
    private int _segCount = 0;

    private void Update()
    {
        if (player == null) return;
        SpawnAhead();
        DespawnBehind();
    }

    private void SpawnAhead()
    {
        float spawnUntil = player.position.z + SEGMENTS_AHEAD * SEGMENT_LEN;
        while (_nextSpawnZ < spawnUntil)
        {
            SpawnSegment(_nextSpawnZ);
            _nextSpawnZ += SEGMENT_LEN;
            _segCount++;
        }
    }

    private void SpawnSegment(float startZ)
    {
        var seg = new Segment
        {
            startZ = startZ,
            endZ = startZ + SEGMENT_LEN
        };

        GameObject g = SpawnPool.Instance.GetGround();
        g.transform.position = new Vector3(0f, -0.1f, startZ + SEGMENT_LEN * 0.5f);
        seg.ground = g;

        if (_segCount >= CLEAR_START_SEGS)
        {
            SpawnObstacleRow(seg, startZ + SEGMENT_LEN * 0.5f);
            SpawnCoins(startZ); // las monedas ya no se guardan en el segmento
        }

        _active.Add(seg);
    }

    private void SpawnObstacleRow(Segment seg, float z)
    {
        int roll = WeightedRandom(new[] { 20, 28, 18, 14, 12, 8 });

        switch (roll)
        {
            case 0: break;

            case 1:
                {
                    int lane = Random.Range(0, 3);
                    var obs = SpawnPool.Instance.GetNormalObstacle();
                    obs.transform.position = new Vector3(SpawnPool.Lanes[lane], 1.1f, z);
                    seg.obstacles.Add(obs);
                    break;
                }

            case 2:
                {
                    var pair = SpawnPool.DoubleLanePairs[Random.Range(0, SpawnPool.DoubleLanePairs.Length)];
                    var obs = SpawnPool.Instance.GetDoubleObstacle(pair.laneA, pair.laneB);
                    obs.transform.position = new Vector3(0f, 1.1f, z);
                    seg.obstacles.Add(obs);
                    break;
                }

            case 3:
                {
                    int track = Random.Range(0, 3);
                    var bar = SpawnPool.Instance.GetFullBarrier();
                    bar.transform.position = new Vector3(0f, 1.4f, z);
                    bar.GetComponent<MusicBarrierFull>().Configure(track);
                    seg.obstacles.Add(bar);
                    break;
                }

            case 4:
                {
                    int lane = Random.Range(0, 3);
                    int track = Random.Range(0, 3);
                    var bar = SpawnPool.Instance.GetPartialBarrier();
                    bar.transform.position = new Vector3(SpawnPool.Lanes[lane], 1.4f, z);
                    bar.GetComponent<MusicBarrierPartial>().Configure(track);
                    seg.obstacles.Add(bar);
                    break;
                }

            case 5:
                {
                    int musicLane = Random.Range(0, 3);
                    int track = Random.Range(0, 3);
                    var bar = SpawnPool.Instance.GetPartialBarrier();
                    bar.transform.position = new Vector3(SpawnPool.Lanes[musicLane], 1.4f, z);
                    bar.GetComponent<MusicBarrierPartial>().Configure(track);
                    seg.obstacles.Add(bar);

                    for (int lane = 0; lane < 3; lane++)
                    {
                        if (lane == musicLane) continue;
                        var wall = SpawnPool.Instance.GetNormalObstacle();
                        wall.transform.position = new Vector3(SpawnPool.Lanes[lane], 1.1f, z);
                        seg.obstacles.Add(wall);
                    }
                    break;
                }
        }
    }

    private void SpawnCoins(float startZ)
    {
        // Las monedas NO se guardan en el segmento — se reciclan solas desde Coin.cs
        int rowCount = Random.Range(1, 3);
        float[] rowPositions = { startZ + SEGMENT_LEN * 0.25f, startZ + SEGMENT_LEN * 0.65f };

        for (int row = 0; row < rowCount; row++)
        {
            if (Random.value < 0.4f) continue;

            int lane = Random.Range(0, 3);
            int coinCount = Random.Range(3, 6);
            float centerZ = rowPositions[row];
            float totalLen = (coinCount - 1) * coinSpacing;
            float startZ2 = centerZ - totalLen * 0.5f;

            for (int i = 0; i < coinCount; i++)
            {
                var coin = SpawnPool.Instance.GetCoin();
                float z = startZ2 + i * coinSpacing;
                coin.transform.position = new Vector3(SpawnPool.Lanes[lane], 1f, z);
            }
        }
    }

    private void DespawnBehind()
    {
        while (_active.Count > 0 &&
               _active[0].endZ < player.position.z - DESPAWN_BEHIND)
        {
            RecycleSegment(_active[0]);
            _active.RemoveAt(0);
        }
    }

    private void RecycleSegment(Segment seg)
    {
        if (seg.ground != null)
            SpawnPool.Instance.ReturnGround(seg.ground);

        // Solo recicla obstáculos, las monedas se manejan solas
        foreach (var obj in seg.obstacles)
        {
            if (obj == null) continue;
            if (obj.name == "DoubleObstacle")
                SpawnPool.Instance.ReturnDoubleObstacle(obj);
            else if (obj.TryGetComponent<NormalObstacle>(out _))
                SpawnPool.Instance.ReturnNormalObstacle(obj);
            else if (obj.TryGetComponent<MusicBarrierFull>(out _))
                SpawnPool.Instance.ReturnFullBarrier(obj);
            else if (obj.TryGetComponent<MusicBarrierPartial>(out _))
                SpawnPool.Instance.ReturnPartialBarrier(obj);
        }
        seg.obstacles.Clear();
    }

    private static int WeightedRandom(int[] weights)
    {
        int total = 0;
        foreach (int w in weights) total += w;
        int r = Random.Range(0, total);
        int cumulative = 0;
        for (int i = 0; i < weights.Length; i++)
        {
            cumulative += weights[i];
            if (r < cumulative) return i;
        }
        return weights.Length - 1;
    }
}
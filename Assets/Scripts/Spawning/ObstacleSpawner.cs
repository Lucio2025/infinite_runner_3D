using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public Transform player;

    private const float SEGMENT_LEN = 20f;
    private const int SEGMENTS_AHEAD = 6;
    private const float DESPAWN_BEHIND = 30f;
    private const int CLEAR_START_SEGS = 2;

    private class Segment
    {
        public float startZ;
        public float endZ;
        public GameObject ground;
        public readonly List<GameObject> objects = new();
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
            SpawnCoins(seg, startZ);
        }

        _active.Add(seg);
    }

    // Pesos: 0=Ninguno 1=Normal 2=Doble 3=BarreraTotal 4=BarreraParcial 5=ComboMusicoMuro
    private void SpawnObstacleRow(Segment seg, float z)
    {
        int roll = WeightedRandom(new[] { 20, 28, 18, 14, 12, 8 });

        switch (roll)
        {
            case 0: break;

            case 1: // Obstáculo normal — 1 carril
                {
                    int lane = Random.Range(0, 3);
                    var obs = SpawnPool.Instance.GetNormalObstacle();
                    obs.transform.position = new Vector3(SpawnPool.Lanes[lane], 1.1f, z);
                    seg.objects.Add(obs);
                    break;
                }

            case 2: // Obstáculo doble — 2 carriles naranja
                {
                    var pair = SpawnPool.DoubleLanePairs[Random.Range(0, SpawnPool.DoubleLanePairs.Length)];
                    var obs = SpawnPool.Instance.GetDoubleObstacle(pair.laneA, pair.laneB);
                    obs.transform.position = new Vector3(0f, 1.1f, z);
                    seg.objects.Add(obs);
                    break;
                }

            case 3: // Barrera musical total — 3 carriles
                {
                    int track = Random.Range(0, 3);
                    var bar = SpawnPool.Instance.GetFullBarrier();
                    bar.transform.position = new Vector3(0f, 1.4f, z);
                    bar.GetComponent<MusicBarrierFull>().Configure(track);
                    seg.objects.Add(bar);
                    break;
                }

            case 4: // Barrera musical parcial — 1 carril libre
                {
                    int lane = Random.Range(0, 3);
                    int track = Random.Range(0, 3);
                    var bar = SpawnPool.Instance.GetPartialBarrier();
                    bar.transform.position = new Vector3(SpawnPool.Lanes[lane], 1.4f, z);
                    bar.GetComponent<MusicBarrierPartial>().Configure(track);
                    seg.objects.Add(bar);
                    break;
                }

            case 5: // COMBO — barrera musical en 1 carril + muros normales en los otros 2
                {
                    // Elegimos en qué carril va la barrera musical
                    int musicLane = Random.Range(0, 3);
                    int track = Random.Range(0, 3);

                    var bar = SpawnPool.Instance.GetPartialBarrier();
                    bar.transform.position = new Vector3(SpawnPool.Lanes[musicLane], 1.4f, z);
                    bar.GetComponent<MusicBarrierPartial>().Configure(track);
                    seg.objects.Add(bar);

                    // Los otros dos carriles se tapan con obstáculos normales
                    for (int lane = 0; lane < 3; lane++)
                    {
                        if (lane == musicLane) continue;
                        var wall = SpawnPool.Instance.GetNormalObstacle();
                        wall.transform.position = new Vector3(SpawnPool.Lanes[lane], 1.1f, z);
                        seg.objects.Add(wall);
                    }

                    Debug.Log($"[Spawner] Combo generado — barrera musical en carril {musicLane}, muros en los demás");
                    break;
                }
        }
    }

    private void SpawnCoins(Segment seg, float startZ)
    {
        float[] coinZ = { startZ + SEGMENT_LEN * 0.3f, startZ + SEGMENT_LEN * 0.7f };
        foreach (float cz in coinZ)
        {
            if (Random.value < 0.5f) continue;
            int laneCount = Random.Range(1, 4);
            int[] lanes = ShuffleLanes();
            for (int i = 0; i < laneCount; i++)
            {
                var coin = SpawnPool.Instance.GetCoin();
                coin.transform.position = new Vector3(SpawnPool.Lanes[lanes[i]], 1f, cz);
                seg.objects.Add(coin);
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

        foreach (var obj in seg.objects)
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
            else if (obj.TryGetComponent<Coin>(out _))
                SpawnPool.Instance.ReturnCoin(obj);
        }
        seg.objects.Clear();
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

    private static int[] ShuffleLanes()
    {
        int[] arr = { 0, 1, 2 };
        for (int i = arr.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }
        return arr;
    }
}
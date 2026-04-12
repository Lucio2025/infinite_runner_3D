using System.Collections.Generic;
using UnityEngine;

public class SpawnPool : MonoBehaviour
{
    public static SpawnPool Instance { get; private set; }

    public Material GroundMaterial => _groundMat;

    private readonly Queue<GameObject> _groundPool = new();
    private readonly Queue<GameObject> _normalObsPool = new();
    private readonly Queue<GameObject> _doubleObsPool = new();
    private readonly Queue<GameObject> _fullBarrierPool = new();
    private readonly Queue<GameObject> _partialBarrierPool = new();
    private readonly Queue<GameObject> _coinPool = new();

    private Material _groundMat;
    private Material _normalObsMat;
    private Material _doubleObsMat;
    private Material _fullBarrierMat;
    private Material _partialBarrierMat;
    private Material _coinMat;

    private const int PREWARM = 8;

    public static readonly float[] Lanes = { -2.5f, 0f, 2.5f };

    public static readonly (int laneA, int laneB)[] DoubleLanePairs =
    {
        (0, 1),
        (1, 2),
        (0, 2)
    };

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildMaterials();
        for (int i = 0; i < PREWARM; i++)
        {
            ReturnGround(CreateGround());
            ReturnNormalObstacle(CreateNormalObstacle());
            ReturnDoubleObstacle(CreateDoubleObstacle());
            ReturnFullBarrier(CreateFullBarrier());
            ReturnPartialBarrier(CreatePartialBarrier());
            ReturnCoin(CreateCoin());
        }
    }

    private void BuildMaterials()
    {
        // Usamos siempre URP/Lit para los objetos del juego
        // El shader Painterly tiene propiedades distintas que rompen la asignacion de colores
        Shader s = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

        _groundMat = new Material(s) { color = new Color(0.28f, 0.28f, 0.32f) };
        _normalObsMat = new Material(s) { color = new Color(0.75f, 0.08f, 0.08f) };
        _doubleObsMat = new Material(s) { color = new Color(0.85f, 0.45f, 0.05f) };
        _fullBarrierMat = new Material(s) { color = Color.white };
        _partialBarrierMat = new Material(s) { color = Color.white };
        _coinMat = new Material(s) { color = new Color(1f, 0.88f, 0.1f) };
        _coinMat.EnableKeyword("_EMISSION");
        _coinMat.SetColor("_EmissionColor", new Color(1f, 0.7f, 0f) * 0.6f);
    }

    private static Material CloneMat(Material src) => new Material(src);

    // ── GROUND ────────────────────────────────────────────
    private GameObject CreateGround()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "GroundTile";
        go.transform.localScale = new Vector3(8f, 0.2f, 20f);
        go.GetComponent<Renderer>().sharedMaterial = _groundMat;
        go.SetActive(false);
        go.transform.SetParent(transform);
        return go;
    }
    public GameObject GetGround() => Fetch(_groundPool, CreateGround);
    public void ReturnGround(GameObject go) => Release(_groundPool, go);

    // ── NORMAL OBSTACLE ───────────────────────────────────
    private GameObject CreateNormalObstacle()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "NormalObstacle";
        go.transform.localScale = new Vector3(1.8f, 2.2f, 1f);
        go.GetComponent<Renderer>().sharedMaterial = _normalObsMat;
        go.GetComponent<BoxCollider>().isTrigger = true;
        go.AddComponent<NormalObstacle>();
        go.SetActive(false);
        go.transform.SetParent(transform);
        return go;
    }
    public GameObject GetNormalObstacle() => Fetch(_normalObsPool, CreateNormalObstacle);
    public void ReturnNormalObstacle(GameObject go) => Release(_normalObsPool, go);

    // ── DOUBLE OBSTACLE ───────────────────────────────────
    private GameObject CreateDoubleObstacle()
    {
        var root = new GameObject("DoubleObstacle");
        root.AddComponent<NormalObstacle>();
        for (int i = 0; i < 2; i++)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = $"Block{i}";
            cube.transform.SetParent(root.transform, false);
            cube.transform.localScale = new Vector3(1.8f, 2.2f, 1f);
            cube.GetComponent<Renderer>().sharedMaterial = _doubleObsMat;
            cube.GetComponent<BoxCollider>().isTrigger = true;
        }
        root.SetActive(false);
        root.transform.SetParent(transform);
        return root;
    }
    public GameObject GetDoubleObstacle(int laneA, int laneB)
    {
        var go = Fetch(_doubleObsPool, CreateDoubleObstacle);
        var blocks = new List<Transform>();
        foreach (Transform child in go.transform) blocks.Add(child);
        if (blocks.Count >= 2)
        {
            blocks[0].localPosition = new Vector3(Lanes[laneA], 0f, 0f);
            blocks[1].localPosition = new Vector3(Lanes[laneB], 0f, 0f);
        }
        return go;
    }
    public void ReturnDoubleObstacle(GameObject go) => Release(_doubleObsPool, go);

    // ── FULL BARRIER ──────────────────────────────────────
    private GameObject CreateFullBarrier()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "FullBarrier";
        go.transform.localScale = new Vector3(7.5f, 2.8f, 0.5f);
        go.GetComponent<Renderer>().material = CloneMat(_fullBarrierMat);
        go.GetComponent<BoxCollider>().isTrigger = true;
        go.AddComponent<MusicBarrierFull>();
        go.SetActive(false);
        go.transform.SetParent(transform);
        return go;
    }
    public GameObject GetFullBarrier() => Fetch(_fullBarrierPool, CreateFullBarrier);
    public void ReturnFullBarrier(GameObject go) => Release(_fullBarrierPool, go);

    // ── PARTIAL BARRIER ───────────────────────────────────
    private GameObject CreatePartialBarrier()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "PartialBarrier";
        go.transform.localScale = new Vector3(1.8f, 2.8f, 0.5f);
        go.GetComponent<Renderer>().material = CloneMat(_partialBarrierMat);
        go.GetComponent<BoxCollider>().isTrigger = true;
        go.AddComponent<MusicBarrierPartial>();
        go.SetActive(false);
        go.transform.SetParent(transform);
        return go;
    }
    public GameObject GetPartialBarrier() => Fetch(_partialBarrierPool, CreatePartialBarrier);
    public void ReturnPartialBarrier(GameObject go) => Release(_partialBarrierPool, go);

    // ── COIN ──────────────────────────────────────────────
    private GameObject CreateCoin()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "Coin";
        go.transform.localScale = Vector3.one * 0.5f;
        go.GetComponent<Renderer>().sharedMaterial = _coinMat;
        go.GetComponent<SphereCollider>().isTrigger = true;
        go.AddComponent<Coin>();
        go.SetActive(false);
        go.transform.SetParent(transform);
        return go;
    }
    public GameObject GetCoin() => Fetch(_coinPool, CreateCoin);
    public void ReturnCoin(GameObject go) => Release(_coinPool, go);

    // ── Pool helpers ──────────────────────────────────────
    private static GameObject Fetch(Queue<GameObject> pool, System.Func<GameObject> factory)
    {
        var go = pool.Count > 0 ? pool.Dequeue() : factory();
        go.transform.SetParent(null);
        go.SetActive(true);
        return go;
    }
    private void Release(Queue<GameObject> pool, GameObject go)
    {
        if (go == null) return;
        go.SetActive(false);
        go.transform.SetParent(transform);
        pool.Enqueue(go);
    }
}
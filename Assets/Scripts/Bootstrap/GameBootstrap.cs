using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attach this to ONE empty GameObject in SampleScene and press Play.
/// GameBootstrap constructs the entire game scene in code:
/// lighting, managers, player, camera, UI, and spawner.
/// No prefabs or manual scene setup required.
/// </summary>
[DefaultExecutionOrder(-100)]
public class GameBootstrap : MonoBehaviour
{
    private void Awake()
    {
        BuildLighting();
        BuildGameManager();
        BuildMusicManager();
        BuildSpawnPool();
        GameObject player = BuildPlayer();
        BuildCamera(player.transform);
        BuildUI();
        BuildSpawner(player.transform);
    }

    // ─────────────────────────────────────────────────────
    // LIGHTING
    // ─────────────────────────────────────────────────────
    private void BuildLighting()
    {
        // Reuse existing directional light or create one
        Light existing = FindAnyObjectByType<Light>();
        Light light = existing ?? new GameObject("Directional Light").AddComponent<Light>();
        light.type      = LightType.Directional;
        light.color     = new Color(0.98f, 0.92f, 0.82f);
        light.intensity = 1.3f;
        light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        RenderSettings.ambientLight = new Color(0.15f, 0.15f, 0.22f);
    }

    // ─────────────────────────────────────────────────────
    // MANAGERS
    // ─────────────────────────────────────────────────────
    private void BuildGameManager()
    {
        if (GameManager.Instance != null) return;
        new GameObject("GameManager").AddComponent<GameManager>();
    }

    private void BuildMusicManager()
    {
        if (MusicLayerManager.Instance != null) return;
        new GameObject("MusicLayerManager").AddComponent<MusicLayerManager>();
    }

    private void BuildSpawnPool()
    {
        if (SpawnPool.Instance != null) return;
        new GameObject("SpawnPool").AddComponent<SpawnPool>();
    }

    // ─────────────────────────────────────────────────────
    // PLAYER
    // ─────────────────────────────────────────────────────
    private GameObject BuildPlayer()
    {
        // Root (physics)
        var player = new GameObject("Player");
        player.tag = "Player";

        var rb = player.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity  = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        var col = player.AddComponent<CapsuleCollider>();
        col.height = 2f;
        col.radius = 0.45f;
        col.center = Vector3.zero;
        col.isTrigger = false; // solid collider so trigger obstacles fire

        player.AddComponent<PlayerController>();
        player.AddComponent<PlayerHealth>();
        player.transform.position = new Vector3(0f, 1f, 0f);

        // Visual child (capsule without its own collider)
        var vis = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        vis.name = "Visual";
        Destroy(vis.GetComponent<Collider>());
        vis.transform.SetParent(player.transform, false);
        vis.transform.localPosition = Vector3.zero;

        ApplyMaterial(vis, GetShader(), new Color(0.2f, 0.6f, 1f));

        // Lane indicator (small sphere on top, colour-coded for debug)
        var indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        indicator.name = "LaneIndicator";
        Destroy(indicator.GetComponent<Collider>());
        indicator.transform.SetParent(player.transform, false);
        indicator.transform.localPosition = new Vector3(0f, 1.35f, 0f);
        indicator.transform.localScale    = Vector3.one * 0.3f;
        ApplyMaterial(indicator, GetShader(), new Color(0.9f, 0.9f, 0.1f));

        return player;
    }

    // ─────────────────────────────────────────────────────
    // CAMERA
    // ─────────────────────────────────────────────────────
    private void BuildCamera(Transform playerTransform)
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            cam = camGO.AddComponent<Camera>();
            camGO.AddComponent<AudioListener>();
        }

        cam.clearFlags       = CameraClearFlags.SolidColor;
        cam.backgroundColor  = new Color(0.07f, 0.07f, 0.12f);
        cam.fieldOfView      = 65f;
        cam.nearClipPlane    = 0.1f;
        cam.farClipPlane     = 200f;

        var follow = cam.gameObject.GetComponent<CameraFollow>()
                  ?? cam.gameObject.AddComponent<CameraFollow>();
        follow.target     = playerTransform;
        follow.offset     = new Vector3(0f, 5f, -8f);
        follow.smoothSpeed = 12f;
    }

    // ─────────────────────────────────────────────────────
    // UI
    // ─────────────────────────────────────────────────────
    private void BuildUI()
    {
        // Canvas
        var canvasGO = new GameObject("Canvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight  = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // ── HUD texts ──
        var lives  = MakeLabel(canvasGO.transform, "LivesText",  "❤ Vidas: 3",
                               new Vector2(0f,1f), new Vector2(0f,1f),
                               new Vector2(15f,-15f), new Vector2(320f,48f),
                               TextAlignmentOptions.Left);

        var coins  = MakeLabel(canvasGO.transform, "CoinsText",  "● Monedas: 0",
                               new Vector2(1f,1f), new Vector2(1f,1f),
                               new Vector2(-15f,-15f), new Vector2(320f,48f),
                               TextAlignmentOptions.Right);
        coins.GetComponent<RectTransform>().pivot = new Vector2(1f,1f);

        var music  = MakeLabel(canvasGO.transform, "MusicText",  "♪ Música: 1  [1/2/3]",
                               new Vector2(.5f,1f), new Vector2(.5f,1f),
                               new Vector2(0f,-15f), new Vector2(480f,48f),
                               TextAlignmentOptions.Center);
        music.GetComponent<RectTransform>().pivot = new Vector2(0.5f,1f);

        // ── Game Over Panel ──
        var panel    = MakePanel(canvasGO.transform);
        var btnComp  = panel.transform.Find("RestartButton")?.GetComponent<Button>();

        // ── Attach controller ──
        canvasGO.AddComponent<GameUI>();
    }

    private GameObject MakeLabel(Transform parent, string id, string text,
        Vector2 ancMin, Vector2 ancMax, Vector2 ancPos, Vector2 size,
        TextAlignmentOptions alignment)
    {
        var go = new GameObject(id);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin       = ancMin;
        rt.anchorMax       = ancMax;
        rt.anchoredPosition= ancPos;
        rt.sizeDelta       = size;
        rt.pivot           = new Vector2(0f,1f);

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = 28f;
        tmp.color     = Color.white;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = alignment;

        // Subtle dark background panel for readability
        var bg = new GameObject("BG");
        bg.transform.SetParent(go.transform, false);
        var bgRT = bg.AddComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = new Vector2(-6f,-4f); bgRT.offsetMax = new Vector2(6f,4f);
        var img = bg.AddComponent<Image>();
        img.color = new Color(0f,0f,0f,0.45f);
        bg.transform.SetAsFirstSibling();

        return go;
    }

    private GameObject MakePanel(Transform parent)
    {
        // Dark semi-transparent panel
        var panel = new GameObject("GameOverPanel");
        panel.transform.SetParent(parent, false);
        var rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f,0.5f);
        rt.sizeDelta = new Vector2(480f, 280f);
        rt.anchoredPosition = Vector2.zero;
        var img = panel.AddComponent<Image>();
        img.color = new Color(0.03f, 0.03f, 0.08f, 0.92f);

        // Title label
        var titleGO = new GameObject("Title");
        titleGO.transform.SetParent(panel.transform, false);
        var titleRT = titleGO.AddComponent<RectTransform>();
        titleRT.anchorMin = titleRT.anchorMax = titleRT.pivot = new Vector2(0.5f, 0.5f);
        titleRT.anchoredPosition = new Vector2(0f, 80f);
        titleRT.sizeDelta = new Vector2(440f, 80f);
        var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.text      = "GAME OVER";
        titleTMP.fontSize  = 56f;
        titleTMP.color     = new Color(1f, 0.25f, 0.25f);
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Center;

        // Restart button
        var btnGO = new GameObject("RestartButton");
        btnGO.transform.SetParent(panel.transform, false);
        var btnRT = btnGO.AddComponent<RectTransform>();
        btnRT.anchorMin = btnRT.anchorMax = btnRT.pivot = new Vector2(0.5f,0.5f);
        btnRT.anchoredPosition = new Vector2(0f, -50f);
        btnRT.sizeDelta = new Vector2(220f, 64f);
        var btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0.18f, 0.75f, 0.38f);
        var btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = btnImg;

        var btnTextGO = new GameObject("Label");
        btnTextGO.transform.SetParent(btnGO.transform, false);
        var btnTextRT = btnTextGO.AddComponent<RectTransform>();
        btnTextRT.anchorMin = Vector2.zero; btnTextRT.anchorMax = Vector2.one;
        btnTextRT.offsetMin = btnTextRT.offsetMax = Vector2.zero;
        var btnTMP = btnTextGO.AddComponent<TextMeshProUGUI>();
        btnTMP.text      = "REINICIAR";
        btnTMP.fontSize  = 26f;
        btnTMP.color     = Color.white;
        btnTMP.fontStyle = FontStyles.Bold;
        btnTMP.alignment = TextAlignmentOptions.Center;

        panel.SetActive(false);
        return panel;
    }

    // ─────────────────────────────────────────────────────
    // SPAWNER
    // ─────────────────────────────────────────────────────
    private void BuildSpawner(Transform playerTransform)
    {
        var go      = new GameObject("ObstacleSpawner");
        var spawner = go.AddComponent<ObstacleSpawner>();
        spawner.player = playerTransform;
    }

    // ─────────────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────────────
    private static Shader GetShader()
    {
        Shader s = Shader.Find("Universal Render Pipeline/Lit");
        return s != null ? s : Shader.Find("Standard");
    }

    private static void ApplyMaterial(GameObject go, Shader shader, Color color)
    {
        var mat = new Material(shader) { color = color };
        go.GetComponent<Renderer>().material = mat;
    }
}

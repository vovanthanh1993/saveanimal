using UnityEngine;

/// <summary>
/// Quản lý Fog of War - che phủ đường đi chưa được khám phá
/// </summary>
public class FogOfWarManager : MonoBehaviour
{
    public static FogOfWarManager Instance { get; private set; }

    [Header("Fog of War Settings")]
    [Tooltip("Material sử dụng shader FogOfWar")]
    public Material fogMaterial;
    
    [Tooltip("Kích thước texture mask (độ phân giải)")]
    public int textureSize = 512;
    
    [Tooltip("Kích thước thế giới mà fog bao phủ (World Units)")]
    public Vector2 worldSize = new Vector2(50f, 50f);
    
    [Header("Reveal Settings")]
    [Tooltip("Bán kính vùng sáng xung quanh player (World Units)")]
    public float revealRadius = 5f;
    
    [Tooltip("Độ mềm của viền vùng sáng (0-1)")]
    [Range(0f, 1f)]
    public float revealSoftness = 0.3f;
    
    [Tooltip("Tốc độ fade khi reveal (0 = tức thì, 1 = rất chậm)")]
    [Range(0f, 1f)]
    public float revealSpeed = 0.1f;
    
    [Header("Player Reference")]
    [Tooltip("Transform của player (tự động tìm nếu để trống)")]
    public Transform playerTransform;
    
    [Header("Debug")]
    [Tooltip("Hiển thị texture mask trong Inspector")]
    public bool showDebugTexture = false;

    private RenderTexture maskTexture;
    private Texture2D revealTexture;
    private Material revealMaterial;
    private Camera revealCamera;
    private GameObject revealCameraObject;
    
    private Vector2 lastPlayerPosition;
    private bool isInitialized = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        InitializeFogOfWar();
    }

    private void Update()
    {
        if (!isInitialized) return;

        UpdateFogOfWar();
    }

    /// <summary>
    /// Khởi tạo hệ thống Fog of War
    /// </summary>
    private void InitializeFogOfWar()
    {
        // Tìm player nếu chưa được gán
        if (playerTransform == null)
        {
            if (PlayerController.Instance != null)
            {
                playerTransform = PlayerController.Instance.transform;
            }
            else
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerTransform = player.transform;
                }
            }
        }

        if (playerTransform == null)
        {
            Debug.LogWarning("FogOfWarManager: Không tìm thấy Player! Vui lòng gán playerTransform.");
            return;
        }

        // Tạo RenderTexture cho mask
        maskTexture = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.R8);
        maskTexture.name = "FogOfWarMask";
        
        // Khởi tạo texture với màu đen (che phủ hoàn toàn)
        Graphics.Blit(Texture2D.blackTexture, maskTexture);

        // Tạo camera để vẽ reveal
        CreateRevealCamera();

        // Tạo texture và material để vẽ reveal
        CreateRevealTexture();

        // Áp dụng mask texture vào material
        if (fogMaterial != null)
        {
            fogMaterial.SetTexture("_Mask", maskTexture);
        }
        else
        {
            Debug.LogWarning("FogOfWarManager: Fog Material chưa được gán!");
        }

        // Lưu vị trí ban đầu của player
        lastPlayerPosition = WorldToTextureCoord(playerTransform.position);

        isInitialized = true;
    }

    /// <summary>
    /// Tạo camera để vẽ các vùng reveal
    /// </summary>
    private void CreateRevealCamera()
    {
        revealCameraObject = new GameObject("FogOfWarRevealCamera");
        revealCameraObject.transform.SetParent(transform);
        revealCameraObject.transform.position = new Vector3(0, 10, 0);
        revealCameraObject.transform.rotation = Quaternion.Euler(90, 0, 0);

        revealCamera = revealCameraObject.AddComponent<Camera>();
        revealCamera.orthographic = true;
        revealCamera.orthographicSize = Mathf.Max(worldSize.x, worldSize.y) / 2f;
        revealCamera.nearClipPlane = 0.1f;
        revealCamera.farClipPlane = 20f;
        revealCamera.clearFlags = CameraClearFlags.SolidColor;
        revealCamera.backgroundColor = Color.black;
        revealCamera.cullingMask = 0; // Không render gì cả
        revealCamera.enabled = false;
    }

    /// <summary>
    /// Tạo texture để vẽ reveal (gradient tròn)
    /// </summary>
    private void CreateRevealTexture()
    {
        int size = 128;
        revealTexture = new Texture2D(size, size, TextureFormat.R8, false);
        revealTexture.name = "RevealTexture";

        float center = size / 2f;
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float normalizedDist = dist / radius;
                
                // Tạo gradient mềm
                float value = 1f - Mathf.Clamp01((normalizedDist - (1f - revealSoftness)) / revealSoftness);
                revealTexture.SetPixel(x, y, new Color(value, value, value, value));
            }
        }

        revealTexture.Apply();

        // Tạo material để blend reveal texture
        revealMaterial = new Material(Shader.Find("Hidden/Internal-GUITexture"));
    }

    /// <summary>
    /// Cập nhật Fog of War mỗi frame
    /// </summary>
    private void UpdateFogOfWar()
    {
        if (playerTransform == null) return;

        Vector2 currentPos = WorldToTextureCoord(playerTransform.position);
        
        // Kiểm tra xem player có di chuyển đủ xa không
        float moveDistance = Vector2.Distance(currentPos, lastPlayerPosition);
        float threshold = revealRadius / Mathf.Max(worldSize.x, worldSize.y) * 0.1f;
        
        if (moveDistance > threshold)
        {
            RevealArea(playerTransform.position, revealRadius);
            lastPlayerPosition = currentPos;
        }
    }

    /// <summary>
    /// Reveal một vùng xung quanh vị trí
    /// </summary>
    public void RevealArea(Vector3 worldPosition, float radius)
    {
        if (!isInitialized || maskTexture == null || revealTexture == null) return;

        Vector2 texCoord = WorldToTextureCoord(worldPosition);
        float radiusInTex = (radius / Mathf.Max(worldSize.x, worldSize.y)) * textureSize;

        // Đọc mask texture hiện tại
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = maskTexture;
        Texture2D currentMask = new Texture2D(textureSize, textureSize, TextureFormat.R8, false);
        currentMask.ReadPixels(new Rect(0, 0, textureSize, textureSize), 0, 0);
        currentMask.Apply();
        RenderTexture.active = previous;

        // Tính toán vùng cần vẽ
        int centerX = Mathf.RoundToInt(texCoord.x * textureSize);
        int centerY = Mathf.RoundToInt((1f - texCoord.y) * textureSize);
        int revealSize = Mathf.RoundToInt(radiusInTex * 2f);
        int halfRevealSize = revealSize / 2;

        // Vẽ reveal lên mask
        for (int y = 0; y < revealSize; y++)
        {
            for (int x = 0; x < revealSize; x++)
            {
                int maskX = centerX - halfRevealSize + x;
                int maskY = centerY - halfRevealSize + y;

                if (maskX < 0 || maskX >= textureSize || maskY < 0 || maskY >= textureSize)
                    continue;

                // Lấy giá trị từ reveal texture
                float revealValue = revealTexture.GetPixel(
                    Mathf.RoundToInt((float)x / revealSize * revealTexture.width),
                    Mathf.RoundToInt((float)y / revealSize * revealTexture.height)
                ).r;

                // Lấy giá trị hiện tại từ mask
                float currentValue = currentMask.GetPixel(maskX, maskY).r;

                // Blend: lấy giá trị lớn hơn (giữ lại vùng sáng nhất)
                float newValue = Mathf.Max(currentValue, revealValue);
                currentMask.SetPixel(maskX, maskY, new Color(newValue, newValue, newValue, 1f));
            }
        }

        currentMask.Apply();

        // Ghi lại vào RenderTexture
        RenderTexture.active = maskTexture;
        Graphics.Blit(currentMask, maskTexture);
        RenderTexture.active = previous;

        DestroyImmediate(currentMask);
    }

    /// <summary>
    /// Chuyển đổi tọa độ thế giới sang tọa độ texture (0-1)
    /// </summary>
    private Vector2 WorldToTextureCoord(Vector3 worldPos)
    {
        // Giả sử fog plane ở vị trí (0, 0, 0) và kéo dài theo X và Z
        float x = (worldPos.x / worldSize.x) + 0.5f;
        float z = (worldPos.z / worldSize.y) + 0.5f;
        return new Vector2(Mathf.Clamp01(x), Mathf.Clamp01(z));
    }

    /// <summary>
    /// Reveal toàn bộ map (dùng cho debug hoặc cheat)
    /// </summary>
    [ContextMenu("Reveal All")]
    public void RevealAll()
    {
        if (!isInitialized || maskTexture == null) return;

        RenderTexture.active = maskTexture;
        GL.Clear(true, true, Color.white);
        RenderTexture.active = null;
    }

    /// <summary>
    /// Reset fog (che phủ lại toàn bộ)
    /// </summary>
    [ContextMenu("Reset Fog")]
    public void ResetFog()
    {
        if (!isInitialized || maskTexture == null) return;

        Graphics.Blit(Texture2D.blackTexture, maskTexture);
        lastPlayerPosition = WorldToTextureCoord(playerTransform.position);
    }

    private void OnDestroy()
    {
        if (maskTexture != null)
        {
            maskTexture.Release();
            DestroyImmediate(maskTexture);
        }

        if (revealTexture != null)
        {
            DestroyImmediate(revealTexture);
        }

        if (revealMaterial != null)
        {
            DestroyImmediate(revealMaterial);
        }

        if (revealCameraObject != null)
        {
            DestroyImmediate(revealCameraObject);
        }
    }

    private void OnGUI()
    {
        if (showDebugTexture && maskTexture != null)
        {
            GUI.DrawTexture(new Rect(10, 10, 256, 256), maskTexture);
        }
    }
}


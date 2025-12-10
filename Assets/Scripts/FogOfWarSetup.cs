using UnityEngine;

/// <summary>
/// Script helper để tạo Material và Plane cho Fog of War
/// Sử dụng script này để setup nhanh hoặc làm mẫu để tạo thủ công
/// </summary>
[System.Serializable]
public class FogOfWarSetup : MonoBehaviour
{
    [Header("Setup Settings")]
    [Tooltip("Shader FogOfWar (tự động tìm nếu để trống)")]
    public Shader fogOfWarShader;
    
    [Tooltip("Material đã tạo (hiển thị để tham khảo)")]
    public Material createdMaterial;
    
    [Tooltip("Plane đã tạo (hiển thị để tham khảo)")]
    public GameObject createdPlane;

    [ContextMenu("Create Material")]
    public void CreateMaterial()
    {
        // Tìm shader nếu chưa được gán
        if (fogOfWarShader == null)
        {
            fogOfWarShader = Shader.Find("Custom/FogOfWar");
            if (fogOfWarShader == null)
            {
                Debug.LogError("Không tìm thấy shader 'Custom/FogOfWar'! Vui lòng đảm bảo shader đã được import.");
                return;
            }
        }

        // Tạo material mới
        Material newMaterial = new Material(fogOfWarShader);
        newMaterial.name = "FogOfWar_Material";

        // Thiết lập các giá trị mặc định
        if (newMaterial.HasProperty("_FogColor"))
        {
            newMaterial.SetColor("_FogColor", new Color(0, 0, 0, 1));
        }
        
        if (newMaterial.HasProperty("_FogIntensity"))
        {
            newMaterial.SetFloat("_FogIntensity", 0.8f);
        }
        
        if (newMaterial.HasProperty("_RevealedIntensity"))
        {
            newMaterial.SetFloat("_RevealedIntensity", 0.3f);
        }
        
        if (newMaterial.HasProperty("_FadeDistance"))
        {
            newMaterial.SetFloat("_FadeDistance", 0.2f);
        }

        // Tạo texture mask mặc định (màu đen = che phủ)
        Texture2D defaultMask = new Texture2D(512, 512, TextureFormat.R8, false);
        Color[] blackPixels = new Color[512 * 512];
        for (int i = 0; i < blackPixels.Length; i++)
        {
            blackPixels[i] = Color.black;
        }
        defaultMask.SetPixels(blackPixels);
        defaultMask.Apply();

        if (newMaterial.HasProperty("_Mask"))
        {
            newMaterial.SetTexture("_Mask", defaultMask);
        }

        createdMaterial = newMaterial;
        Debug.Log($"Đã tạo Material: {newMaterial.name}");
        Debug.Log($"Material được lưu tại: {gameObject.name}");
    }

    [ContextMenu("Create Plane")]
    public void CreatePlane()
    {
        if (createdMaterial == null)
        {
            Debug.LogWarning("Chưa có Material! Vui lòng tạo Material trước hoặc gán material vào field 'Created Material'.");
            return;
        }

        // Tạo GameObject với Plane mesh
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.name = "FogOfWar_Plane";
        plane.transform.SetParent(transform);
        plane.transform.localPosition = Vector3.zero;
        plane.transform.localRotation = Quaternion.identity;
        plane.transform.localScale = Vector3.one;

        // Áp dụng material
        Renderer renderer = plane.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = createdMaterial;
        }

        // Tắt collider (không cần thiết cho fog)
        Collider col = plane.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        createdPlane = plane;
        Debug.Log($"Đã tạo Plane: {plane.name}");
    }

    [ContextMenu("Create Material and Plane")]
    public void CreateMaterialAndPlane()
    {
        CreateMaterial();
        CreatePlane();
    }

    /// <summary>
    /// Hướng dẫn tạo thủ công (hiển thị trong Console)
    /// </summary>
    [ContextMenu("Show Manual Setup Instructions")]
    public void ShowManualSetupInstructions()
    {
        string instructions = @"
=== HƯỚNG DẪN TẠO FOG OF WAR THỦ CÔNG ===

BƯỚC 1: TẠO MATERIAL
1. Trong Project window, click chuột phải → Create → Material
2. Đặt tên: FogOfWar_Material
3. Trong Inspector, chọn Shader: Custom/FogOfWar
4. Thiết lập các thuộc tính:
   - Fog Color: Màu sương mù (mặc định: đen)
   - Fog Intensity: 0.8
   - Revealed Intensity: 0.3
   - Fade Distance: 0.2
   - Mask: Sẽ được gán tự động bởi FogOfWarManager

BƯỚC 2: TẠO PLANE
1. Trong Hierarchy, click chuột phải → 3D Object → Plane
2. Đặt tên: FogOfWar_Plane
3. Điều chỉnh Transform:
   - Position: (0, 0.1, 0) - độ cao phù hợp
   - Scale: Điều chỉnh theo kích thước map
4. Kéo Material FogOfWar_Material vào Mesh Renderer

BƯỚC 3: SETUP FOGOFWAR MANAGER
1. Tạo GameObject trống, đặt tên: FogOfWarManager
2. Add Component → FogOfWarManager
3. Gán Material FogOfWar_Material vào field 'Fog Material'
4. Điều chỉnh World Size theo kích thước plane
5. Điều chỉnh Reveal Radius theo nhu cầu

LƯU Ý:
- Plane nên ở độ cao nhỏ hơn player một chút (ví dụ: 0.1)
- World Size phải khớp với kích thước thực tế của plane
- Mask texture sẽ được tạo tự động bởi FogOfWarManager
";

        Debug.Log(instructions);
    }
}


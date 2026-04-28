using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager instance;

    [Header("=== GRID SETTINGS ===")]
    public float nodeRadius = 0.45f; // Bán kính quả bóng
    public float xSpacing = 0.9f;    // Khoảng cách ngang giữa 2 tâm bóng
    public float ySpacing = 0.78f;   // Khoảng cách dọc (Gợi ý: xSpacing * 0.866 để khít nhất)

    void Awake()
    {
        if (instance == null) instance = this;
    }

    /// <summary>
    /// Tìm vị trí lưới gần nhất dựa trên tọa độ va chạm (Snap to Grid)
    /// </summary>
    public Vector3 GetNearestGridPos(Vector3 hitPos)
    {
        // 1. Tính toán hàng (Row)
        // Lưu ý: Dùng Mathf.Abs để tránh lỗi khi y âm nếu cần
        int row = Mathf.RoundToInt(hitPos.y / ySpacing);

        // 2. Tính toán độ lệch ngang (Offset)
        // Hàng lẻ sẽ lệch đi một nửa khoảng cách ngang
        float xOffset = (Mathf.Abs(row) % 2 != 0) ? xSpacing / 2f : 0f;

        // 3. Tính toán cột (Column)
        int col = Mathf.RoundToInt((hitPos.x - xOffset) / xSpacing);

        // 4. Trả về tọa độ thế giới đã được căn chỉnh (Snap)
        float finalX = col * xSpacing + xOffset;
        float finalY = row * ySpacing;

        return new Vector3(finalX, finalY, 0);
    }

    // Vẽ các điểm lưới trong Scene để dễ căn chỉnh
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        // Vẽ thử một vùng lưới rộng hơn để bạn dễ quan sát
        for (int r = -2; r < 12; r++)
        {
            float xOffset = (Mathf.Abs(r) % 2 != 0) ? xSpacing / 2f : 0f;
            for (int c = -6; c < 6; c++)
            {
                Vector3 pos = new Vector3(c * xSpacing + xOffset, r * ySpacing, 0);
                Gizmos.DrawWireSphere(pos, 0.1f);
            }
        }
    }
}
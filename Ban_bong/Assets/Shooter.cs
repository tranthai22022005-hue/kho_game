using UnityEngine;

public class Shooter : MonoBehaviour
{
    [Header("=== BALL PREFABS ===")]
    public GameObject Ball_R;
    public GameObject Ball_B;
    public GameObject Ball_Y;

    [Header("=== SETTINGS ===")]
    public Transform firePoint;
    public float shootForce = 500f;

    private Camera mainCam;
    private GameObject currentBall;
    private bool canShoot = false;

    void Start()
    {
        mainCam = Camera.main;
        CreateNextBall();
        // Gọi EnableShoot để cho phép bắn sau khi game bắt đầu
        Invoke(nameof(EnableShoot), 0.2f);
    }

    void Update()
    {
        AimAtMouse();
        if (Input.GetMouseButtonDown(0) && canShoot && currentBall != null)
        {
            Shoot();
        }
    }

    private void AimAtMouse()
    {
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        Vector3 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void Shoot()
    {
        if (currentBall == null || firePoint == null) return;

        canShoot = false; // Khóa bắn ngay lập tức
        Rigidbody2D rb = currentBall.GetComponent<Rigidbody2D>();
        Ball ballScript = currentBall.GetComponent<Ball>();

        if (ballScript != null) ballScript.isShot = true;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0;
        rb.linearVelocity = firePoint.up * (shootForce / 10f);
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        currentBall = null;

        Invoke(nameof(CreateNextBall), 0.2f);
        Invoke(nameof(EnableShoot), 0.4f); // Đợi bóng mới sẵn sàng mới cho bắn tiếp
    }

    private void CreateNextBall()
    {
        if (firePoint == null) return;
        GameObject prefab = GetRandomBall();
        currentBall = Instantiate(prefab, firePoint.position, Quaternion.identity);
        currentBall.transform.localScale = Vector3.one * 0.95f;

        Rigidbody2D rb = currentBall.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        Ball ballScript = currentBall.GetComponent<Ball>();
        if (ballScript != null) ballScript.isShot = false;
    }

    // ĐÂY LÀ HÀM BẠN ĐANG THIẾU DẪN ĐẾN LỖI ĐỎ
    private void EnableShoot()
    {
        canShoot = true;
    }

    private GameObject GetRandomBall()
    {
        int r = Random.Range(0, 3);
        return r == 0 ? Ball_R : r == 1 ? Ball_B : Ball_Y;
    }

    
}
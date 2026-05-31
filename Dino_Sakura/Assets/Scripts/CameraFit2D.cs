using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFit2D : MonoBehaviour
{
    [SerializeField] private float orthographicSize = 5f;
    [SerializeField] private Color backgroundColor = new Color(0.55f, 0.86f, 1f, 1f);

    private void Awake()
    {
        Camera cam = GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = orthographicSize;
        cam.backgroundColor = backgroundColor;
    }
}
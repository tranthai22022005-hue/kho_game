using UnityEngine;

public class DayNightController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private SpriteRenderer skyRenderer;

    [Header("Colors")]
    [SerializeField] private Color dayColor = new Color(0.55f, 0.86f, 1f, 1f);
    [SerializeField] private Color sunsetColor = new Color(1f, 0.52f, 0.66f, 1f);
    [SerializeField] private Color nightColor = new Color(0.12f, 0.15f, 0.35f, 1f);

    [Header("Cycle")]
    [SerializeField] private float cycleDuration = 60f;

    private float timer;

    private void Update()
    {
        if (skyRenderer == null) return;
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Idle) return;

        timer += Time.deltaTime;
        float t = (timer % cycleDuration) / cycleDuration;

        if (t < 0.5f)
        {
            skyRenderer.color = Color.Lerp(dayColor, sunsetColor, t / 0.5f);
        }
        else
        {
            skyRenderer.color = Color.Lerp(sunsetColor, nightColor, (t - 0.5f) / 0.5f);
        }
    }
}
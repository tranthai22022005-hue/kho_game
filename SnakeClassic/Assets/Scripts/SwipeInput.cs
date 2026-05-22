using UnityEngine;

public class SwipeInput : MonoBehaviour
{
    [SerializeField] private float minSwipeDistance = 40f;

    private Vector2 touchStart;
    private bool isDragging;

    public Direction? ReadDirection()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            return Direction.Up;

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            return Direction.Down;

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            return Direction.Left;

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            return Direction.Right;
#endif

        if (Input.touchCount <= 0)
            return null;

        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            touchStart = touch.position;
            isDragging = true;
        }

        if ((touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) && isDragging)
        {
            isDragging = false;

            Vector2 delta = touch.position - touchStart;

            if (delta.magnitude < minSwipeDistance)
                return null;

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                return delta.x > 0 ? Direction.Right : Direction.Left;
            }

            return delta.y > 0 ? Direction.Up : Direction.Down;
        }

        return null;
    }
}
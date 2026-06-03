using UnityEngine;
using UnityEngine.EventSystems;

public class MobileInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public enum ActionType
    {
        MoveLeft,
        MoveRight,
        Rotate,
        SoftDrop,
        HardDrop,
        Hold,
        Pause
    }

    [SerializeField] private Board board;
    [SerializeField] private Piece activePiece;
    [SerializeField] private ActionType actionType;
    [SerializeField] private float repeatDelay = 0.13f;
    [SerializeField] private float swipeThreshold = 70f;

    private bool holding;
    private float nextRepeatTime;
    private Vector2 startPosition;

    private void Update()
    {
        if (!holding || activePiece == null) return;

        if (actionType == ActionType.MoveLeft || actionType == ActionType.MoveRight || actionType == ActionType.SoftDrop)
        {
            if (Time.unscaledTime >= nextRepeatTime)
            {
                Execute();
                nextRepeatTime = Time.unscaledTime + repeatDelay;
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        holding = true;
        startPosition = eventData.position;
        nextRepeatTime = Time.unscaledTime;
        Execute();
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 delta = eventData.position - startPosition;
        if (delta.magnitude < swipeThreshold) return;

        if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
        {
            if (delta.y > 0)
                ExecuteSpecific(ActionType.HardDrop);
            else
                ExecuteSpecific(ActionType.SoftDrop);
        }
        else
        {
            if (delta.x < 0)
                ExecuteSpecific(ActionType.MoveLeft);
            else
                ExecuteSpecific(ActionType.MoveRight);
        }

        startPosition = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        holding = false;
    }

    private void Execute()
    {
        ExecuteSpecific(actionType);
    }

    private void ExecuteSpecific(ActionType type)
    {
        if (board == null) return;

        switch (type)
        {
            case ActionType.MoveLeft:
                if (activePiece != null) activePiece.Move(Vector2Int.left);
                break;
            case ActionType.MoveRight:
                if (activePiece != null) activePiece.Move(Vector2Int.right);
                break;
            case ActionType.Rotate:
                if (activePiece != null) activePiece.Rotate(1);
                break;
            case ActionType.SoftDrop:
                if (activePiece != null) activePiece.Step(true);
                break;
            case ActionType.HardDrop:
                if (activePiece != null) activePiece.HardDrop();
                break;
            case ActionType.Hold:
                board.Hold();
                break;
            case ActionType.Pause:
                board.TogglePause();
                break;
        }
    }
}

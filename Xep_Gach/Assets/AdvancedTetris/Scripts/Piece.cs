using UnityEngine;
using UnityEngine.Tilemaps;

public class Piece : MonoBehaviour
{
    public Board board { get; private set; }
    public TetrominoData data { get; private set; }
    public Vector3Int position { get; private set; }
    public Vector3Int[] cells { get; private set; }
    public int rotationIndex { get; private set; }

    private float stepDelay;
    private float lockDelay;
    private float stepTime;
    private float lockTime;
    private bool lastMoveWasRotation;

    public bool LastMoveWasRotation => lastMoveWasRotation;

    public void Initialize(Board board, Vector3Int position, TetrominoData data, float stepDelay, float lockDelay)
    {
        this.board = board;
        this.position = position;
        this.data = data;
        this.stepDelay = stepDelay;
        this.lockDelay = lockDelay;
        rotationIndex = 0;
        stepTime = Time.time + stepDelay;
        lockTime = 0f;
        lastMoveWasRotation = false;

        if (cells == null || cells.Length != data.cells.Length)
            cells = new Vector3Int[data.cells.Length];

        for (int i = 0; i < cells.Length; i++)
            cells[i] = (Vector3Int)data.cells[i];
    }

    private void Update()
    {
        if (board.State != GameState.Playing) return;

        board.Clear(this);
        lockTime += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Q)) Rotate(-1);
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.UpArrow)) Rotate(1);
        if (Input.GetKeyDown(KeyCode.Space)) HardDrop();
        if (Input.GetKeyDown(KeyCode.C)) board.Hold();
        if (Input.GetKeyDown(KeyCode.LeftArrow)) Move(Vector2Int.left);
        if (Input.GetKeyDown(KeyCode.RightArrow)) Move(Vector2Int.right);
        if (Input.GetKey(KeyCode.DownArrow))
        {
            if (Time.time >= stepTime) Step(true);
        }

        if (Time.time >= stepTime)
            Step(false);

        board.Set(this);
        board.DrawGhost(this);
    }

    public bool Move(Vector2Int translation)
    {
        Vector3Int newPosition = position;
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        bool valid = board.IsValidPosition(this, newPosition);
        if (valid)
        {
            position = newPosition;
            lockTime = 0f;
            lastMoveWasRotation = false;
            if (translation.y < 0) board.AddSoftDropScore(1);
            board.PlayMoveSfx();
        }
        return valid;
    }

    public void Step(bool softDrop)
    {
        stepTime = Time.time + (softDrop ? board.SoftDropStepDelay : stepDelay);
        Move(Vector2Int.down);

        if (lockTime >= lockDelay)
            Lock();
    }

    public void HardDrop()
    {
        int cellsDropped = 0;
        while (Move(Vector2Int.down))
            cellsDropped++;

        board.AddHardDropScore(cellsDropped);
        board.PlayHardDropSfx();
        Lock();
    }

    public void Lock()
    {
        board.Set(this);
        board.Lock(this);
    }

    public void Rotate(int direction)
    {
        int originalRotation = rotationIndex;
        rotationIndex = Wrap(rotationIndex + direction, 0, 4);
        ApplyRotationMatrix(direction);

        if (!TestWallKicks(rotationIndex, direction))
        {
            rotationIndex = originalRotation;
            ApplyRotationMatrix(-direction);
        }
        else
        {
            lockTime = 0f;
            lastMoveWasRotation = true;
            board.PlayRotateSfx();
        }
    }

    private void ApplyRotationMatrix(int direction)
    {
        for (int i = 0; i < cells.Length; i++)
        {
            Vector3 cell = cells[i];

            int x;
            int y;

            if (data.tetromino == Tetromino.I || data.tetromino == Tetromino.O)
            {
                cell.x -= 0.5f;
                cell.y -= 0.5f;
                x = Mathf.CeilToInt((cell.x * Data.Cos * direction) - (cell.y * Data.Sin * direction));
                y = Mathf.CeilToInt((cell.x * Data.Sin * direction) + (cell.y * Data.Cos * direction));
            }
            else
            {
                x = Mathf.RoundToInt((cell.x * Data.Cos * direction) - (cell.y * Data.Sin * direction));
                y = Mathf.RoundToInt((cell.x * Data.Sin * direction) + (cell.y * Data.Cos * direction));
            }

            cells[i] = new Vector3Int(x, y, 0);
        }
    }

    private bool TestWallKicks(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = GetWallKickIndex(rotationIndex, rotationDirection);

        for (int i = 0; i < data.wallKicks.GetLength(1); i++)
        {
            Vector2Int translation = data.wallKicks[wallKickIndex, i];

            if (Move(translation))
                return true;
        }

        return false;
    }

    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        int index = rotationIndex * 2;
        if (rotationDirection < 0)
            index--;
        return Wrap(index, 0, data.wallKicks.GetLength(0));
    }

    private int Wrap(int input, int min, int max)
    {
        if (input < min) return max - (min - input) % (max - min);
        return min + (input - min) % (max - min);
    }
}

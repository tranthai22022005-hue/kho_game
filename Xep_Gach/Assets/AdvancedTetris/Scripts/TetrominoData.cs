using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public struct TetrominoData
{
    public Tetromino tetromino;
    public TileBase tile;
    public Vector2Int[] cells;
    public Vector2Int[,] wallKicks;

    public void Initialize()
    {
        cells = Data.Cells[tetromino];
        wallKicks = Data.WallKicks[tetromino];
    }
}

public static class Data
{
    public static readonly float Cos = Mathf.Cos(Mathf.PI / 2f);
    public static readonly float Sin = Mathf.Sin(Mathf.PI / 2f);

    public static readonly Vector2Int[] ICells =
    {
        new Vector2Int(-1, 1),
        new Vector2Int(0, 1),
        new Vector2Int(1, 1),
        new Vector2Int(2, 1)
    };

    public static readonly Vector2Int[] OCells =
    {
        new Vector2Int(0, 1),
        new Vector2Int(1, 1),
        new Vector2Int(0, 0),
        new Vector2Int(1, 0)
    };

    public static readonly Vector2Int[] TCells =
    {
        new Vector2Int(-1, 0),
        new Vector2Int(0, 0),
        new Vector2Int(1, 0),
        new Vector2Int(0, 1)
    };

    public static readonly Vector2Int[] SCells =
    {
        new Vector2Int(0, 0),
        new Vector2Int(1, 0),
        new Vector2Int(-1, 1),
        new Vector2Int(0, 1)
    };

    public static readonly Vector2Int[] ZCells =
    {
        new Vector2Int(-1, 0),
        new Vector2Int(0, 0),
        new Vector2Int(0, 1),
        new Vector2Int(1, 1)
    };

    public static readonly Vector2Int[] JCells =
    {
        new Vector2Int(-1, 0),
        new Vector2Int(0, 0),
        new Vector2Int(1, 0),
        new Vector2Int(-1, 1)
    };

    public static readonly Vector2Int[] LCells =
    {
        new Vector2Int(-1, 0),
        new Vector2Int(0, 0),
        new Vector2Int(1, 0),
        new Vector2Int(1, 1)
    };

    public static readonly System.Collections.Generic.Dictionary<Tetromino, Vector2Int[]> Cells =
        new System.Collections.Generic.Dictionary<Tetromino, Vector2Int[]>()
        {
            { Tetromino.I, ICells },
            { Tetromino.O, OCells },
            { Tetromino.T, TCells },
            { Tetromino.S, SCells },
            { Tetromino.Z, ZCells },
            { Tetromino.J, JCells },
            { Tetromino.L, LCells },
        };

    private static readonly Vector2Int[,] WallKicksJLSTZ = new Vector2Int[,]
    {
        { new Vector2Int(0,0), new Vector2Int(-1,0), new Vector2Int(-1,1), new Vector2Int(0,-2), new Vector2Int(-1,-2) },
        { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(1,-1), new Vector2Int(0,2), new Vector2Int(1,2) },
        { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(1,1), new Vector2Int(0,-2), new Vector2Int(1,-2) },
        { new Vector2Int(0,0), new Vector2Int(-1,0), new Vector2Int(-1,-1), new Vector2Int(0,2), new Vector2Int(-1,2) },
        { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(1,1), new Vector2Int(0,-2), new Vector2Int(1,-2) },
        { new Vector2Int(0,0), new Vector2Int(-1,0), new Vector2Int(-1,-1), new Vector2Int(0,2), new Vector2Int(-1,2) },
        { new Vector2Int(0,0), new Vector2Int(-1,0), new Vector2Int(-1,1), new Vector2Int(0,-2), new Vector2Int(-1,-2) },
        { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(1,-1), new Vector2Int(0,2), new Vector2Int(1,2) },
    };

    private static readonly Vector2Int[,] WallKicksI = new Vector2Int[,]
    {
        { new Vector2Int(0,0), new Vector2Int(-2,0), new Vector2Int(1,0), new Vector2Int(-2,-1), new Vector2Int(1,2) },
        { new Vector2Int(0,0), new Vector2Int(2,0), new Vector2Int(-1,0), new Vector2Int(2,1), new Vector2Int(-1,-2) },
        { new Vector2Int(0,0), new Vector2Int(-1,0), new Vector2Int(2,0), new Vector2Int(-1,2), new Vector2Int(2,-1) },
        { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(-2,0), new Vector2Int(1,-2), new Vector2Int(-2,1) },
        { new Vector2Int(0,0), new Vector2Int(2,0), new Vector2Int(-1,0), new Vector2Int(2,1), new Vector2Int(-1,-2) },
        { new Vector2Int(0,0), new Vector2Int(-2,0), new Vector2Int(1,0), new Vector2Int(-2,-1), new Vector2Int(1,2) },
        { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(-2,0), new Vector2Int(1,-2), new Vector2Int(-2,1) },
        { new Vector2Int(0,0), new Vector2Int(-1,0), new Vector2Int(2,0), new Vector2Int(-1,2), new Vector2Int(2,-1) },
    };

    private static readonly Vector2Int[,] WallKicksO = new Vector2Int[,]
    {
        { Vector2Int.zero, Vector2Int.zero, Vector2Int.zero, Vector2Int.zero, Vector2Int.zero },
        { Vector2Int.zero, Vector2Int.zero, Vector2Int.zero, Vector2Int.zero, Vector2Int.zero },
        { Vector2Int.zero, Vector2Int.zero, Vector2Int.zero, Vector2Int.zero, Vector2Int.zero },
        { Vector2Int.zero, Vector2Int.zero, Vector2Int.zero, Vector2Int.zero, Vector2Int.zero },
        { Vector2Int.zero, Vector2Int.zero, Vector2Int.zero, Vector2Int.zero, Vector2Int.zero },
        { Vector2Int.zero, Vector2Int.zero, Vector2Int.zero, Vector2Int.zero, Vector2Int.zero },
        { Vector2Int.zero, Vector2Int.zero, Vector2Int.zero, Vector2Int.zero, Vector2Int.zero },
        { Vector2Int.zero, Vector2Int.zero, Vector2Int.zero, Vector2Int.zero, Vector2Int.zero },
    };

    public static readonly System.Collections.Generic.Dictionary<Tetromino, Vector2Int[,]> WallKicks =
        new System.Collections.Generic.Dictionary<Tetromino, Vector2Int[,]>()
        {
            { Tetromino.I, WallKicksI },
            { Tetromino.O, WallKicksO },
            { Tetromino.T, WallKicksJLSTZ },
            { Tetromino.S, WallKicksJLSTZ },
            { Tetromino.Z, WallKicksJLSTZ },
            { Tetromino.J, WallKicksJLSTZ },
            { Tetromino.L, WallKicksJLSTZ },
        };
}
using System.Collections.Generic;
using UnityEngine;

public class TetrisBag
{
    private readonly List<Tetromino> bag = new List<Tetromino>(7);

    public Tetromino Next()
    {
        if (bag.Count == 0)
        {
            bag.Add(Tetromino.I);
            bag.Add(Tetromino.O);
            bag.Add(Tetromino.T);
            bag.Add(Tetromino.S);
            bag.Add(Tetromino.Z);
            bag.Add(Tetromino.J);
            bag.Add(Tetromino.L);

            for (int i = 0; i < bag.Count; i++)
            {
                int randomIndex = Random.Range(i, bag.Count);
                Tetromino temp = bag[i];
                bag[i] = bag[randomIndex];
                bag[randomIndex] = temp;
            }
        }

        Tetromino value = bag[0];
        bag.RemoveAt(0);
        return value;
    }
}

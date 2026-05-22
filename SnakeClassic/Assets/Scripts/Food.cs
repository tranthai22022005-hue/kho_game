using UnityEngine;

public class Food : MonoBehaviour
{
    [SerializeField] private FoodType foodType = FoodType.Normal;

    public FoodType Type => foodType;

    public void Configure(FoodType type)
    {
        foodType = type;
    }
}
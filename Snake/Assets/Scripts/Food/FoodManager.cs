using UnityEngine;

public class FoodManager : MonoBehaviour
{
    public SnakeController snake;
    public GameObject foodPrefab;
    public int mapSize = 9;

    private GameObject currentFood;

    void Start()
    {
        SpawnFood();
    }

    public void SpawnFood()
    {
        // nếu còn food cũ thì xóa
        if (currentFood != null)
        {
            Destroy(currentFood);
        }

        Vector2 pos;

        do
        {
            int x = Random.Range(-mapSize, mapSize);
            int y = Random.Range(-mapSize, mapSize);
            pos = new Vector2(x, y);

        } while (IsOnSnake(pos)); // 🔥 kiểm tra trùng

        currentFood = Instantiate(foodPrefab, pos, Quaternion.identity);
    }

    bool IsOnSnake(Vector2 pos)
    {
        // check đầu
        if ((Vector2)snake.transform.position == pos)
            return true;

        // check thân
        foreach (Transform part in snake.GetBody())
        {
            if ((Vector2)part.position == pos)
                return true;
        }

        return false;
    }
}
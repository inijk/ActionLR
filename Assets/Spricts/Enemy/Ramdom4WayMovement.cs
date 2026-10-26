using UnityEngine;

[CreateAssetMenu(fileName = "Random4WayMovement", menuName = "EnemyAI/Movement/Random 4-Way")]
public class Random4WayMovement : EnemyMovementPattern
{
    private Vector2 currentDirection = Vector2.zero;

    public override Vector2 GetNextDirection(Transform enemyTransform, EntityStats stats, ref float timer, float interval)
    {
        timer += Time.deltaTime;

        if (timer >= interval)
        {
            timer = 0f;
            currentDirection = GetRandom4WayDirection();
        }

        return currentDirection;
    }

    private Vector2 GetRandom4WayDirection()
    {
        int dirIndex = Random.Range(0, 4);
        switch (dirIndex)
        {
            case 0: return Vector2.up;
            case 1: return Vector2.down;
            case 2: return Vector2.left;
            case 3: return Vector2.right;
            default: return Vector2.zero;
        }
    }
}
using UnityEngine;

public abstract class EnemyMovementPattern : ScriptableObject
{
    // 敵の Transform や Rigidbody2D、EntityStats などの参照を受け取り、次の移動方向（Vector2）を計算して返す
    public abstract Vector2 GetNextDirection(Transform enemyTransform, EntityStats stats, ref float timer, float interval);
}
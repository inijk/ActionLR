using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Enemy_", menuName = "Combat/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("― 基本情報 ―")]
    public string enemyName = "敵の名前";
    [TextArea] public string description;
    public Sprite icon;

    [Header("― 討伐報酬 ―")]
    public float dropExp = 25f; // 倒したときに得られるEXP
    // public List<ItemData> dropItems; // 将来的にアイテムドロップを追加する場合

    [Header("― AI・行動パターン ―")]
    public EnemyMovementPattern movementPattern; // 以前設計した移動パターンアセット
    public float moveInterval = 2f;             // 移動切り替え間隔
    public float moveSpeed = 3f;                // 移動速度

    private void OnValidate()
    {
        if (!string.IsNullOrEmpty(name) && string.IsNullOrEmpty(enemyName))
        {
            enemyName = name;
        }
    }
}
using UnityEngine;

public class EnemyExpDrop : MonoBehaviour
{
    [Header("― 討伐報酬 ―")]
    [SerializeField] private float dropExp = 25f;

    private void Start()
    {
        // 敵の EntityStats の OnDeath イベントに紐付ける例
        if (TryGetComponent<EntityStats>(out var stats))
        {
            stats.OnDeath += HandleDeath;
        }
    }

    private void OnDestroy()
    {
        if (TryGetComponent<EntityStats>(out var stats))
        {
            stats.OnDeath -= HandleDeath;
        }
    }

    private void HandleDeath()
    {
        // プレイヤーをシーンから探してEXPを渡す
        PlayerExpController playerExp = FindObjectOfType<PlayerExpController>();
        if (playerExp != null)
        {
            playerExp.GainExp(dropExp);
        }
    }
}
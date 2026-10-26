using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    [Header("敵プレハブリスト")]
    [SerializeField] private List<GameObject> enemyPrefabs; // 出現させたい敵のプレハブ

    private List<EnemySpawner> spawners = new List<EnemySpawner>();

    private void Awake()
    {
        // 部屋の中にある全 EnemySpawner を自動取得
        spawners.AddRange(GetComponentsInChildren<EnemySpawner>());
    }

    // 第1ウェーブ（事前出現）の敵を生成するメソッド
    public void SpawnInitialEnemies(RoomData roomData)
    {
        // すでにクリア済みなら敵は生成しない
        if (roomData.isCleared) return;

        // 【パターンA】一度訪問したことがあり、残存敵データがある場合 ➔ 状態を復元
        if (roomData.isVisited)
        {
            foreach (var enemyData in roomData.remainingEnemies)
            {
                // 敵プレハブをロード/参照して保存された位置・HPで生成
                GameObject enemyPrefab = GetEnemyPrefabByName(enemyData.enemyPrefabName);
                if (enemyPrefab != null)
                {
                    GameObject spawnedEnemy = Instantiate(enemyPrefab, enemyData.position, Quaternion.identity, transform);
                    
                    // HPの復元
                    if (spawnedEnemy.TryGetComponent<EntityStats>(out var stats))
                    {
                        // 保存されているHPまで減算/調整
                        float damageToApply = stats.HP.MaxStat.Value - enemyData.currentHP;
                        stats.HP.Consume(damageToApply);
                    }
                }
            }
        }
        // 【パターンB】初めて訪れる部屋の場合 ➔ 初期スポナーから出現させる
        else
        {
            foreach (var spawner in spawners)
            {
                if (spawner.isInitialWave)
                {
                    GameObject selectedPrefab = GetRandomEnemyPrefab();
                    if (selectedPrefab != null)
                    {
                        spawner.Spawn(selectedPrefab);
                    }
                }
            }
        }
    }

    // 敵プレハブをランダムに1つ選ぶ
    private GameObject GetRandomEnemyPrefab()
    {
        if (enemyPrefabs == null || enemyPrefabs.Count == 0) return null;
        int randomIndex = Random.Range(0, enemyPrefabs.Count);
        return enemyPrefabs[randomIndex];
    }

    /// <summary>
    /// プレハブ名（文字列）から一致する敵プレハブを取得する
    /// </summary>
    private GameObject GetEnemyPrefabByName(string prefabName)
    {
        if (enemyPrefabs == null || enemyPrefabs.Count == 0) return null;

        foreach (var prefab in enemyPrefabs)
        {
            if (prefab != null && prefab.name == prefabName)
            {
                return prefab;
            }
        }

        Debug.LogWarning($"[Room] 指定された敵プレハブ '{prefabName}' が enemyPrefabs に見つかりません。");
        return null;
    }
}
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("スポーン設定")]
    [Tooltip("第1ウェーブ（事前出現・未発見）用か、増援用か")]
    public bool isInitialWave = true;

    // スポーン処理
    public GameObject Spawn(GameObject enemyPrefab)
    {
        if (enemyPrefab == null) return null;

        // このスポーンポイントの位置・回転で敵を生成
        // 【修正点】 Instantiate の第4引数に transform (このSpawnerのTransform) を指定
        // これにより、敵は Spawner（＝部屋プレハブの子）として生成されます
        GameObject enemyObj = Instantiate(enemyPrefab, transform.position, transform.rotation, transform);
        
        return enemyObj;
    }

    // エディタ上で位置を見やすくするためのギズモ描画
    private void OnDrawGizmos()
    {
        Gizmos.color = isInitialWave ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.4f);
    }
}
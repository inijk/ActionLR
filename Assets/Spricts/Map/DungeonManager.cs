using System.Collections.Generic;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance { get; private set; }

    [Header("ダンジョン生成設定")]
    [SerializeField] private int mainRouteLength = 5; // メイン経路の長さ（部屋数）
    [Range(0f, 1f)]
    [SerializeField] private float sideRoomSpawnChance = 0.6f; // サイドルームが生える確率

    [Header("部屋のプレハブ群")]
    [SerializeField] private GameObject startRoomPrefab;          // スタート部屋用プレハブ
    [SerializeField] private GameObject bossRoomPrefab;           // ボス部屋用プレハブ
    [SerializeField] private List<GameObject> mainRoomPrefabs;    // メインルームのプレハブ一覧
    [SerializeField] private List<GameObject> sideRoomPrefabs;    // （必要に応じて）サイドルーム用一覧
    public Transform playerTransform;

    // マップデータ（座標ごとのRoomDataを保持）
    private Dictionary<Vector2Int, RoomData> mapData = new Dictionary<Vector2Int, RoomData>();
    private Vector2Int currentGridCoord = Vector2Int.zero;
    private GameObject currentRoomInstance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        GenerateDungeonMap();
        SpawnRoom(currentGridCoord);
    }

    private void GenerateDungeonMap()
    {
        mapData.Clear();
        Vector2Int currentCoord = Vector2Int.zero;

        // 1. スタート部屋の生成
        mapData.Add(currentCoord, new RoomData { roomType = RoomType.Start });
        List<Vector2Int> mainRouteCoords = new List<Vector2Int> { currentCoord };

        // 2. メイン経路（スタート → ボス）の作成
        for (int i = 0; i < mainRouteLength; i++)
        {
            Vector2Int nextCoord = GetRandomUnusedNeighbor(currentCoord);
            
            if (nextCoord != currentCoord)
            {
                currentCoord = nextCoord;
                mainRouteCoords.Add(currentCoord);
                
                // デフォルトは通常の敵部屋
                RoomType type = RoomType.Enemy;
                
                // 最後の部屋はボス部屋
                if (i == mainRouteLength - 1) 
                {
                    type = RoomType.Boss | RoomType.Enemy;
                }
                
                mapData.Add(currentCoord, new RoomData { roomType = type });
            }
            else
            {
                break; // 行き止まりになったら終了
            }
        }

        // 3. サイドルーム（枝分かれ）の自動生成
        List<Vector2Int> sideRoomCandidates = new List<Vector2Int>(mainRouteCoords);
        List<Vector2Int> newlyAddedSideRooms = new List<Vector2Int>();

        foreach (var coord in sideRoomCandidates)
        {
            // スタート部屋やボス部屋以外から一定確率でサイドルームを生やす
            if (coord != Vector2Int.zero && Random.value < sideRoomSpawnChance)
            {
                Vector2Int sideCoord = GetRandomUnusedNeighbor(coord);
                
                if (sideCoord != coord && !mapData.ContainsKey(sideCoord))
                {
                    // サイドルームのタイプを決定（例: 宝箱部屋、ショップ部屋、または敵なしの休憩部屋など）
                    RoomType sideType = GetRandomSideRoomType();
                    
                    mapData.Add(sideCoord, new RoomData { roomType = sideType });
                    newlyAddedSideRooms.Add(sideCoord);
                }
            }
        }

        // （発展）サイドルームからさらに枝分かれを1段階増やす場合
        foreach (var coord in newlyAddedSideRooms)
        {
            if (Random.value < 0.3f) // 30%の確率でさらに生える
            {
                Vector2Int subSideCoord = GetRandomUnusedNeighbor(coord);
                if (subSideCoord != coord && !mapData.ContainsKey(subSideCoord))
                {
                    RoomType subSideType = RoomType.Treasure; // 奥の行き止まりは宝箱部屋など
                    mapData.Add(subSideCoord, new RoomData { roomType = subSideType });
                }
            }
        }

        Debug.Log($"ダンジョン生成完了！ 総部屋数: {mapData.Count}");
    }

    // 指定した座標の周囲（上下左右）から、まだ部屋が置かれていないランダムな座標を1つ返す
    private Vector2Int GetRandomUnusedNeighbor(Vector2Int center)
    {
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        List<Vector2Int> availableDirs = new List<Vector2Int>();

        foreach (var dir in directions)
        {
            Vector2Int neighbor = center + dir;
            if (!mapData.ContainsKey(neighbor))
            {
                availableDirs.Add(neighbor);
            }
        }

        if (availableDirs.Count > 0)
        {
            // ランダムに未開通の方向を一つ選ぶ
            int randomIndex = Random.Range(0, availableDirs.Count);
            return availableDirs[randomIndex];
        }

        // 周囲がすべて埋まっている場合
        return center;
    }

    private void SpawnRoom(Vector2Int coord)
    {
        if (currentRoomInstance != null) Destroy(currentRoomInstance);

        RoomData currentRoomData = mapData[coord];

        // 【ポイント】部屋のタイプ（RoomType）に応じてどのプレハブを使うか決定する
        GameObject prefabToSpawn = GetPrefabByRoomType(currentRoomData.roomType);

        if (prefabToSpawn == null)
        {
            Debug.LogError($"[DungeonManager] 座標 {coord} のルームタイプ ({currentRoomData.roomType}) に対応するプレハブが見つかりません！");
            return;
        }

        // 選択されたプレハブを生成
        currentRoomInstance = Instantiate(prefabToSpawn, Vector3.zero, Quaternion.identity);

        SetupDoors(coord);

        // 敵の事前スポーン/状態復元処理
        if (currentRoomInstance.TryGetComponent<Room>(out var room))
        {
            room.SpawnInitialEnemies(currentRoomData);
        }

        // 部屋内オブジェクト（宝箱など）の状態復元
        InteractableObject[] interactables = currentRoomInstance.GetComponentsInChildren<InteractableObject>();
        foreach (var obj in interactables)
        {
            bool isOpened = currentRoomData.openedChestIDs.Contains(obj.objectID);
            obj.Setup(currentRoomData.isCleared, isOpened);
        }

        CheckRoomClearCondition();
    }

    // 部屋タイプを判定して適切なプレハブを返すヘルパーメソッド
    private GameObject GetPrefabByRoomType(RoomType roomType)
    {
        // スタート部屋
        if (roomType.HasFlag(RoomType.Start) && startRoomPrefab != null)
            return startRoomPrefab;
        // ボス部屋
        if (roomType.HasFlag(RoomType.Boss) && bossRoomPrefab != null)
            return bossRoomPrefab;

        // 宝箱やショップなど「サブルーム的要素」を持ち、かつボスではない場合
        bool isSideRoom = roomType.HasFlag(RoomType.Treasure) || roomType.HasFlag(RoomType.Shop);
        if (isSideRoom && sideRoomPrefabs != null && sideRoomPrefabs.Count > 0)
        {
            int randomIndex = Random.Range(0, sideRoomPrefabs.Count);
            return sideRoomPrefabs[randomIndex];
        }
        
        // メインルーム（複数の候補からランダムで1つ選ぶ）
        if (mainRoomPrefabs != null && mainRoomPrefabs.Count > 0)
        {
            int randomIndex = Random.Range(0, mainRoomPrefabs.Count);
            return mainRoomPrefabs[randomIndex];
        }

        // フォールバック（リストが空のときの保険）
        return null;
    }

    // ランダムなサブルームのタイプを返すヘルパー
    private RoomType GetRandomSideRoomType()
    {
        float rand = Random.value;
        if (rand < 0.4f)
        {
            return RoomType.Treasure; // 宝箱部屋（敵なし）
        }
        else if (rand < 0.7f)
        {
            return RoomType.Enemy; // 小規模な戦闘部屋
        }
        else
        {
            return RoomType.Shop | RoomType.Treasure; // ショップかつ宝箱がある部屋など（重複のテスト）
        }
    }
    
    // 部屋内の敵が全滅した時に呼び出すメソッド
    public void OnEnemyDefeated()
    {
        // 部屋の中に残っている敵（"Enemy"タグなど）をチェック
        GameObject[] remainingEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        // 最後の1体を倒した瞬間（Destroyされる前なので1以下で判定、あるいは敵側で数値を管理）
        if (remainingEnemies.Length <= 1) 
        {
            SetRoomCleared();
        }
    }

    // 部屋クリア処理
    public void SetRoomCleared()
    {
        RoomData data = mapData[currentGridCoord];
        if (!data.isCleared)
        {
            data.isCleared = true;
            Debug.Log($"部屋 {currentGridCoord} の敵を全滅させた！オブジェクトのロックが解除されます。");

            // 現在の部屋にある全オブジェクトのロックを解除
            InteractableObject[] interactables = currentRoomInstance.GetComponentsInChildren<InteractableObject>();
            foreach (var obj in interactables)
            {
                obj.OnRoomCleared();
            }
        }
    }

    // 宝箱が開けられたことを記録
    public void RecordChestOpened(int objectID)
    {
        RoomData data = mapData[currentGridCoord];
        if (!data.openedChestIDs.Contains(objectID))
        {
            data.openedChestIDs.Add(objectID);
        }
    }

    private void CheckRoomClearCondition()
    {
        // 敵タグのオブジェクトが無ければ自動的にクリア扱いにする
        if (GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
        {
            SetRoomCleared();
        }
    }
    // マップデータに沿って不要なドアを非アクティブ化
    // マップデータに沿ってドアの状態（開・閉）を更新
    private void SetupDoors(Vector2Int coord)
    {
        Door[] doors = currentRoomInstance.GetComponentsInChildren<Door>();

        foreach (var door in doors)
        {
            Vector2Int targetCoord = coord + GetDirectionOffset(door.direction);
            
            // 隣の座標に部屋が存在すれば「開く」、存在しなければ「閉じる（塗りつぶし＋壁化）」
            bool isOpen = mapData.ContainsKey(targetCoord);
            
            door.SetDoorState(isOpen);
        }
    }

    // ドアに触れたときの処理
    public void OnPlayerEnterDoor(Direction enteredDirection)
    {
        Vector2Int nextCoord = currentGridCoord + GetDirectionOffset(enteredDirection);

        if (mapData.ContainsKey(nextCoord))
        {
            // 【最重要追加】移動して旧部屋が消える前に、現在いる部屋の敵・状態を保存する
            SaveCurrentRoomState();

            // 座標を移動先に更新
            currentGridCoord = nextCoord;

            // 新しい部屋を生成（旧部屋はここで Destroy される）
            SpawnRoom(currentGridCoord);

            // プレイヤーの位置を反対側のドア付近へ移動
            RelocatePlayer(enteredDirection);
        }
    }

    // 部屋切り替え時に呼び出す処理
    private void SaveCurrentRoomState()
    {
        if (mapData.TryGetValue(currentGridCoord, out RoomData currentRoomData))
        {
            // 訪問済みフラグをオン
            currentRoomData.isVisited = true;

            // まだクリアしていない部屋の場合、生存している敵の情報を記録
            if (!currentRoomData.isCleared)
            {
                currentRoomData.remainingEnemies.Clear();

                // シーン上の敵を取得（"Enemy" タグなどで検索）
                GameObject[] activeEnemies = GameObject.FindGameObjectsWithTag("Enemy");
                foreach (var enemyObj in activeEnemies)
                {
                    if (enemyObj.TryGetComponent<EntityStats>(out var stats))
                    {
                        // 倒されていない（HP > 0）敵の情報を保存
                        if (!stats.IsDead)
                        {
                            EnemySaveData enemyData = new EnemySaveData
                            {
                                enemyPrefabName = enemyObj.name.Replace("(Clone)", "").Trim(), // プレハブ名
                                position = enemyObj.transform.position,
                                currentHP = stats.HP.CurrentValue
                            };
                            currentRoomData.remainingEnemies.Add(enemyData);
                        }
                    }
                }
            }
        }
    }


    // 入ってきた方向の「反対側」にプレイヤーを配置
    private void RelocatePlayer(Direction enteredDirection)
    {
        // 入ってきたドアの反対側のドアを探す（東から入ったなら西ドア）
        Direction spawnDoorDir = GetOppositeDirection(enteredDirection);
        Door[] doors = currentRoomInstance.GetComponentsInChildren<Door>();

        foreach (var door in doors)
        {
            if (door.direction == spawnDoorDir)
            {
                // ドアの方向（部屋の中心から外側へ向かうベクトル）
                Vector2Int dirVector = GetDirectionOffset(spawnDoorDir);
                Vector3 outerVector = new Vector3(dirVector.x, dirVector.y, 0f);

                // ドアの位置から「部屋の内側（outerVectorの逆方向）」へ 1.5 ユニット移動した位置に配置
                float pushInDistance = 1.5f; // プレイヤーがすぐドアに再接触しない距離（適宜調整）
                Vector3 spawnPos = door.transform.position - (outerVector * pushInDistance);

                playerTransform.position = spawnPos;
                break;
            }
        }
    }

    // 方向のベクトル変換
    private Vector2Int GetDirectionOffset(Direction dir)
    {
        switch (dir)
        {
            case Direction.North: return new Vector2Int(0, 1);
            case Direction.South: return new Vector2Int(0, -1);
            case Direction.East:  return new Vector2Int(1, 0);
            case Direction.West:  return new Vector2Int(-1, 0);
            default: return Vector2Int.zero;
        }
    }

    // 反対方向の取得
    private Direction GetOppositeDirection(Direction dir)
    {
        switch (dir)
        {
            case Direction.North: return Direction.South;
            case Direction.South: return Direction.North;
            case Direction.East:  return Direction.West;
            case Direction.West:  return Direction.East;
            default: return Direction.North;
        }
    }
}
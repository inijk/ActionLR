using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniMapUI : MonoBehaviour
{
    [Header("ミニマップ設定")]
    [SerializeField] private GameObject mapCellPrefab; // 単色のUI Imageプレハブ
    [SerializeField] private int mapSize = 5;          // グリッドのサイズ（例: 5x5）
    [SerializeField] private Vector2Int gridCenterOffset = new Vector2Int(2, 2); // (0,0)を中央に置くためのオフセット

    [Header("― 色のカスタマイズ設定 ―")]
    [SerializeField] private Color colorUnvisited = Color.black;                // 未到達
    [SerializeField] private Color colorCurrent = Color.green;                  // 現在地（緑）
    [SerializeField] private Color colorMainVisited = Color.white;              // 到達済み（メイン）
    [SerializeField] private Color colorSideVisited = new Color(0.7f, 0.7f, 0.7f); // 到達済み（サブルーム＝薄い灰色）

    // 生成したセルのUIを座標で管理するDictionary
    private Dictionary<Vector2Int, Image> cellImages = new Dictionary<Vector2Int, Image>();

    private void Start()
    {
        InitializeMiniMap();
    }

    private void Update()
    {
        // 毎フレーム（または部屋移動時のみでも可）マップの状態を更新
        UpdateMiniMapDisplay();
    }

    // NxN のグリッドマスを生成する
    private void InitializeMiniMap()
    {
        // 既存の子要素をクリア
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        cellImages.Clear();

        // 上から下、左から右へグリッドを生成
        for (int y = mapSize - 1; y >= 0; y--)
        {
            for (int x = 0; x < mapSize; x++)
            {
                // 座標系を変換（例: (0,0)中心にする場合はオフセット調整）
                Vector2Int coord = new Vector2Int(x - gridCenterOffset.x, y - gridCenterOffset.y);

                GameObject cellObj = Instantiate(mapCellPrefab, transform);
                cellObj.name = $"Cell_{coord.x}_{coord.y}";

                Image cellImage = cellObj.GetComponent<Image>();
                cellImage.color = colorUnvisited; // 初期状態はすべて黒

                cellImages.Add(coord, cellImage);
            }
        }
    }

    // マップデータに基づいてセルの色を更新
    private void UpdateMiniMapDisplay()
    {
        if (DungeonManager.Instance == null) return;

        // DungeonManager から現在のマップデータを取得するパブリックメソッドがあると仮定
        // ※ DungeonManager側に `public Dictionary<Vector2Int, RoomData> GetMapData() { return mapData; }` を追加してください
        Dictionary<Vector2Int, RoomData> mapData = DungeonManager.Instance.GetMapData();
        // 【重要】DungeonManager から「現在プレイヤーがいる部屋の座標」を取得できるパブリック変数またはメソッドが必要です
        // 例: DungeonManager.Instance.CurrentGridCoord など
        Vector2Int currentCoord = DungeonManager.Instance.GetCurrentGridCoord();

        foreach (var kvp in cellImages)
        {
            Vector2Int coord = kvp.Key;
            Image cellImage = kvp.Value;

            if (mapData.TryGetValue(coord, out RoomData roomData))
            {
                if (coord == currentCoord)
                {
                    cellImage.color = colorCurrent; // 現在地 = 緑
                }
                else if (roomData.isVisited)// 到達しているか？ (isVisited が true、または現在地)
                {
                    // サブルームかどうか（RoomTypeにTreasureやShopが含まれている、または判定フラグがある場合）
                    // ※ ここはプロジェクトの RoomType の実装に合わせて条件を調整してください
                    bool isSideRoom = roomData.roomType.HasFlag(RoomType.Treasure) || roomData.roomType.HasFlag(RoomType.Shop);

                    if (isSideRoom)
                    {
                        cellImage.color = colorSideVisited; // サブルーム = 薄い灰色
                    }
                    else
                    {
                        cellImage.color = colorMainVisited; // メインルーム = 白色
                    }
                }
                else
                {
                    cellImage.color = colorUnvisited; // 存在するが未到達 = 黒
                }
            }
            else
            {
                // マップデータ自体が存在しない場所
                cellImage.color = Color.clear; // 完全に見えないようにする等
            }
        }
    }
}
using System.Collections.Generic;
using System;
using UnityEngine;

// 【追加】部屋内に残っている敵の情報を保持するデータ構造
[System.Serializable]
public class EnemySaveData
{
    public string enemyPrefabName; // 敵の種類（プレハブ名）
    public Vector2 position;       // 座標
    public float currentHP;        // 現在HP
}

// 2の階乗（1, 2, 4, 8, 16...）で値を割り振ります
[Flags]
public enum RoomType
{
    None        = 0,
    Start       = 1 << 0, // 1  : 初期部屋
    Normal      = 1 << 1, // 2  : 通常部屋
    Enemy       = 1 << 2, // 4  : 敵が出現する部屋
    Treasure    = 1 << 3, // 8  : 宝箱がある部屋
    Shop        = 1 << 4, // 16 : 取引ができる部屋（ショップ）
    Rest        = 1 << 5, // 32 : 休憩・回復部屋
    Boss        = 1 << 6  // 64 : ボス部屋
}

[System.Serializable]
public class RoomData
{
    public RoomType roomType;          // 部屋の種類（Start, Normal, Boss など）
    public bool isCleared = false;   // 敵全滅フラグ（初期値は未クリア）
    public List<int> openedChestIDs = new List<int>(); // 開封済み宝箱のIDリスト

    // 【追加】その部屋で生存している敵のリスト
    public List<EnemySaveData> remainingEnemies = new List<EnemySaveData>();
    public bool isVisited = false; // 一度でも訪問したことがあるか

    // 特定のタイプが含まれているかを判定するヘルパーメソッド
    public bool HasType(RoomType type)
    {
        return (roomType & type) == type;
    }
}
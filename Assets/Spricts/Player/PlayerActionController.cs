using UnityEngine;

public class PlayerActionController : MonoBehaviour
{
    [Header("― ポーズ中やメニュー展開中に無効化するスクリプト一覧 ―")]
    [SerializeField] private MonoBehaviour[] restrictionComponents; 
    // 例：PlayerMovement, PlayerAttack, PlayerSkillExecutor などをアタッチ

    /// <summary>
    /// プレイヤーの行動（移動・攻撃など）をロック／解除する
    /// </summary>
    /// <param name="isLocked">trueなら行動禁止（コンポーネントOFF）、falseなら許可（コンポーネントON）</param>
    public void SetActionLock(bool isLocked)
    {
        foreach (var comp in restrictionComponents)
        {
            if (comp != null)
            {
                // ロック時はコンポーネントを無効化し、解除時は有効に戻す
                comp.enabled = !isLocked;
            }
        }
    }
}
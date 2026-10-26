[System.Serializable]
public class ActiveEffect
{
    public StatusEffectData data;
    public int currentLevel; // 1 ~ 5 (Lv.I ~ V)
    public float duration;   // 残り時間

    public ActiveEffect(StatusEffectData data, int level, float duration)
    {
        this.data = data;
        this.currentLevel = UnityEngine.Mathf.Clamp(level, 1, data.maxLevel);
        this.duration = duration;
    }

    // Lvに応じた効果量を取得
    public float GetCurrentValue()
    {
        int index = UnityEngine.Mathf.Clamp(currentLevel - 1, 0, data.valuePerLevel.Length - 1);
        return data.valuePerLevel[index];
    }
}
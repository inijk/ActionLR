using UnityEngine;

public class PlayerExpController : MonoBehaviour
{
    [SerializeField] private PlayerExpManager expManager;
    public PlayerExpManager ExpManager => expManager;

    private void Awake()
    {
        if (expManager == null)
        {
            expManager = new PlayerExpManager();
        }
        expManager.Initialize();
    }

    /// <summary>
    /// 敵撃破時に呼び出す
    /// </summary>
    public void GainExp(float amount)
    {
        expManager.AddExp(amount);
    }
}
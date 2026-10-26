using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("― Input System 設定 ―")]
    [SerializeField] private InputActionReference fireAction; // Inspectorで "Fire1" を割り当て[cite: 2]

    [Header("― 参照設定 ―")]
    [SerializeField] private GameObject attackHitboxPrefab; // 斬撃のプレハブ[cite: 3]
    [SerializeField] private Transform attackSpawnPoint;     // 生成位置 (Playerの子要素)[cite: 3]
    
    [Header("― 攻撃パラメータ ―")]
    [SerializeField] private float attackCooldown = 0.3f;   // 攻撃のクールタイム
    private float lastAttackTime;
    
    private EntityStats myStats; //[cite: 3]

    private void Awake()
    {
        myStats = GetComponent<EntityStats>(); //[cite: 3]
    }

    private void OnEnable()
    {
        if (fireAction != null)
        {
            fireAction.action.Enable(); // アクションの有効化[cite: 2]
        }
    }

    private void OnDisable()
    {
        if (fireAction != null)
        {
            fireAction.action.Disable(); // アクションの無効化[cite: 2]
        }
    }

    private void Update()
    {
        // PlayerInput の "Fire1" アクションが押されたか判定[cite: 2]
        if (fireAction != null && fireAction.action.WasPressedThisFrame()) //[cite: 2]
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                PerformAttack();
                lastAttackTime = Time.time;
            }
        }
    }

    private void PerformAttack()
    {
        // 攻撃判定の生成[cite: 3]
        if (attackHitboxPrefab == null || attackSpawnPoint == null) return;

        // 【変更点】 Instantiate の第4引数に attackSpawnPoint を指定
        // これにより、生成された斬撃オブジェクトは attackSpawnPoint の子要素になります
        GameObject hitboxObj = Instantiate(
            attackHitboxPrefab, 
            attackSpawnPoint.position, 
            attackSpawnPoint.rotation, 
            attackSpawnPoint // ← 親要素として設定
        );
        
        if (hitboxObj.TryGetComponent<MeleeHitbox>(out var meleeHitbox))
        {
            meleeHitbox.Setup(myStats);
        }
    }
}
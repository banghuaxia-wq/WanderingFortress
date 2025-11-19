using UnityEngine;

/// <summary>
/// 使用 ScriptableObject 定义枪械的属性，方便在编辑器中创建和配置不同类型的枪械。
/// </summary>
[CreateAssetMenu(fileName = "NewGunDefinition", menuName = "Weapon/Gun Definition")]
public class SOGun : ScriptableObject
{
    [Header("Firing Properties")]
    /// <summary>
    /// 每次射击之间的间隔时间（秒）。
    /// </summary>
    [Tooltip("每次射击之间的间隔时间（秒）。")]
    public float FireRate = 0.15f; 
    
    /// <summary>
    /// 每次射击发射的子弹数量（用于霰弹枪）。
    /// </summary>
    [Tooltip("每次射击发射的子弹数量（用于霰弹枪）。")]
    public int BulletsPerShot = 1;

    /// <summary>
    /// 子弹的散射角度（0为完美精确，越大越扩散）。
    /// </summary>
    [Tooltip("子弹的散射角度（0为完美精确，越大越扩散）。")]
    [Range(0f, 45f)]
    public float SpreadAngle = 0f;

    [Header("Ammunition")]
    /// <summary>
    /// 该枪械使用的子弹类型（引用 SOBullet 资产）。
    /// </summary>
    [Tooltip("该枪械使用的子弹类型（引用 SOBullet 资产）。")]
    public SOBullet AmmoType; // 引用类型已更新
    
    [Header("Damage Override")]
    /// <summary>
    /// 如果勾选，将使用下面的伤害和眩晕值，覆盖 AmmoType 中的默认值。
    /// </summary>
    [Tooltip("如果勾选，将使用下面的伤害和眩晕值，覆盖 AmmoType 中的默认值。")]
    public bool UseGunDamageOverride = true; 
    
    /// <summary>
    /// 枪械设定的最终伤害值。
    /// </summary>
    [Tooltip("枪械设定的最终伤害值。")]
    public float DamageOverride = 30f; 
    
    /// <summary>
    /// 枪械设定的最终眩晕值。
    /// </summary>
    [Tooltip("枪械设定的最终眩晕值。")]
    public float StunOverride = 5f;
}
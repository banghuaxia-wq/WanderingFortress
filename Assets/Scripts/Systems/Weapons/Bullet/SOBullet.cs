using UnityEngine;
using WF.Gameplay.Core.Data;

namespace WF.Gameplay.Systems.Weapons.Bullet
{
[CreateAssetMenu(fileName = "NewBulletDefinition", menuName = "Weapon/Bullet Definition")]
public class SOBullet : ScriptableObject
{
    [Header("Projectile Properties")]
    /// <summary>
    /// 子弹对生命值造成的【基础】伤害量。
    /// </summary>
    [Tooltip("子弹对生命值造成的【基础】伤害量。")]
    public float DamageAmount = 10f;
    
    /// <summary>
    /// 子弹对眩晕条造成的【基础】眩晕度增长。
    /// </summary>
    [Tooltip("子弹对眩晕条造成的【基础】眩晕度增长。")]
    public float StunGainAmount = 5f;

    /// <summary>
    /// 子弹的移动速度。
    /// </summary>
    [Tooltip("子弹的移动速度。")]
    public float Speed = 50f;

    /// <summary>
    /// 子弹在场景中存在的最大时间。
    /// </summary>
    [Tooltip("子弹在场景中存在的最大时间。")]
    public float Lifetime = 3f;

    /// <summary>
    /// 子弹的预制体（GameObject），用于实例化。
    /// </summary>
    [Tooltip("子弹的预制体（GameObject），用于实例化。")]
    public GameObject Prefab; 

    [Header("Stun Settings")]
    [Tooltip("用于延迟眩晕的麻药 Buff 资产（需要包含 SedativeStun 模块）")]
    public BuffData SedativeBuffData;
}
}

using UnityEngine;

/// <summary>
/// 使用 ScriptableObject 定义子弹的基础属性，方便在编辑器中创建和配置不同类型的子弹。
/// </summary>
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
}
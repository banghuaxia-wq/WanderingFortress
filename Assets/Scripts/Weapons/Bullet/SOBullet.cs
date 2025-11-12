using UnityEngine;

// 文件名和菜单名已更新
[CreateAssetMenu(fileName = "NewBulletDefinition", menuName = "Weapon/Bullet Definition")]
public class SOBullet : ScriptableObject
{
    [Header("Projectile Properties")]
    // 注意：基础伤害/眩晕字段可以保留，但我们主要依赖 SOGun 来确定最终伤害
    [Tooltip("子弹对生命值造成的【基础】伤害量。")]
    public float DamageAmount = 10f;
    
    [Tooltip("子弹对眩晕条造成的【基础】眩晕度增长。")]
    public float StunGainAmount = 5f;

    [Tooltip("子弹的移动速度。")]
    public float Speed = 50f;

    [Tooltip("子弹在场景中存在的最大时间。")]
    public float Lifetime = 3f;

    [Tooltip("子弹的预制体（GameObject），用于实例化。")]
    public GameObject Prefab; 
}
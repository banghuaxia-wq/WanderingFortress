using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("跟随目标")]
    [Tooltip("请将玩家对象拖拽到这里")]
    public Transform target; 

    [Header("跟随参数")]
    [Tooltip("相机平滑跟随的速度，值越大跟得越紧，建议 5-10")]
    public float smoothSpeed = 10f;
    
    [Tooltip("相机相对于玩家的固定偏移量，建议在场景中摆好相机位置后，运行游戏自动计算")]
    public Vector3 offset;

    void Start()
    {
        // 如果在面板里没有手动设置偏移量，就自动计算当前相机和玩家的距离作为固定偏移
        // 这样你只需要在编辑模式先把相机摆到一个舒服的角度即可
        if (offset == Vector3.zero && target != null)
        {
            offset = transform.position - target.position;
        }
        else if (target == null)
        {
            Debug.LogWarning("CameraFollow: 请将玩家 Transform 赋值给 Target 变量！");
        }
    }

    // 使用 LateUpdate 确保在玩家移动完成后相机才移动，防止画面抖动
    void LateUpdate()
    {
        if (target == null) return;

        // 计算目标位置：玩家当前位置 + 固定的偏移量
        Vector3 desiredPosition = target.position + offset;

        // 使用 Lerp 进行平滑插值移动，让跟随看起来有“弹簧”感，更自然
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // 更新相机位置
        transform.position = smoothedPosition;

        // 可选：如果你希望相机在跟随位置的同时，始终旋转视角看向玩家，取消下面这行的注释
        // transform.LookAt(target); 
    }
}
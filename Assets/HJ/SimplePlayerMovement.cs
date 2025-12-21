using UnityEngine;

public class SimplePlayerMovement : MonoBehaviour
{
    [Header("移动设置")]
    [Tooltip("模拟人类快走的速度，建议范围 3.0 - 6.0")]
    public float walkSpeed = 5.0f;

    void Update()
    {
        // 1. 获取键盘输入
        // Horizontal 对应 A/D 或 左/右箭头 (返回 -1 到 1)
        // Vertical 对应 W/S 或 上/下箭头 (返回 -1 到 1)
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // 2. 组装移动方向向量
        // 我们在 X 和 Z 轴上移动，Y 轴保持 0 (确保不飞天)
        Vector3 moveDirection = new Vector3(moveX, 0f, moveZ);

        // 3. 归一化向量
        // 重要：防止斜着走(例如同时按W+D)时速度变快(变成根号2倍)。
        // 如果输入向量长度大于1，将其长度缩放到1，保持方向不变。
        if (moveDirection.magnitude > 1f)
        {
            moveDirection.Normalize();
        }

        // 4. 应用移动
        // 使用 Space.World 确保 W 永远指向世界坐标的 Z 正方向，A 永远指向世界坐标的 X 负方向，与玩家自身的旋转无关。
        // 乘以 Time.deltaTime 确保移动速度与帧率无关，保证在不同性能的机器上移动距离一致。
        transform.Translate(moveDirection * walkSpeed * Time.deltaTime, Space.World);
    }
}

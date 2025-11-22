using UnityEngine;
using WF.Gameplay;
using System.Text;

public class MovementSpeedDebugger : MonoBehaviour
{
    [SerializeField] private PlayerMove playerMove;
    [SerializeField] private PlayerStateManager stateManager;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float logInterval = 0.5f;
    [SerializeField] private bool enabledLogging = true;
    private float _timer;

    private void Awake()
    {
        if (playerMove == null) playerMove = FindObjectOfType<PlayerMove>();
        if (stateManager == null) stateManager = PlayerStateManager.Instance ?? FindObjectOfType<PlayerStateManager>();
        if (rb == null && playerMove != null) rb = playerMove.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (!enabledLogging || playerMove == null || rb == null) return;
        _timer += Time.deltaTime;
        if (_timer < logInterval) return;
        _timer = 0f;

        Vector3 hv = rb.velocity; hv.y = 0f;
        float currentSpeed = hv.magnitude;
        float mul = stateManager != null ? stateManager.MovementSpeedMultiplier : 1f;
        float slowPct = 1f - mul;

        var sb = new StringBuilder();
        sb.AppendFormat("[MoveDebug] speed={0:F2} mul={1:F3} slow={2:P1}", currentSpeed, mul, slowPct);
        if (stateManager != null)
        {
            var kvs = stateManager.GetMovementSpeedMultipliersSnapshot();
            if (kvs != null && kvs.Count > 0)
            {
                sb.Append(" keys=");
                for (int i = 0; i < kvs.Count; i++)
                {
                    var kv = kvs[i];
                    sb.AppendFormat("{0}:{1:F3}", kv.Key, kv.Value);
                    if (i < kvs.Count - 1) sb.Append(", ");
                }
            }
        }

        Debug.Log(sb.ToString(), this);
    }
}


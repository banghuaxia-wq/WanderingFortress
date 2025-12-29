using UnityEngine;
using UnityEngine.AI;

namespace WF.Gameplay.Systems.Animation
{
    [RequireComponent(typeof(Animator))]
    public class EnemyMovementAnimatorDriver : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private NavMeshAgent _navMeshAgent;
        [SerializeField] private Rigidbody _rigidbodySource;
        [SerializeField] private string _movementSpeedParameter = "MovementSpeed";
        [SerializeField] private float _speedDampTime = 0.1f;

        private int _movementSpeedHash;

        private void Awake()
        {
            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }

            if (_navMeshAgent == null)
            {
                _navMeshAgent = GetComponent<NavMeshAgent>();
            }

            if (_rigidbodySource == null)
            {
                _rigidbodySource = GetComponent<Rigidbody>();
            }

            _movementSpeedHash = Animator.StringToHash(_movementSpeedParameter);
        }

        private void Update()
        {
            if (_animator == null) return;

            float speed = GetPlanarSpeed();

            if (_speedDampTime > 0f)
            {
                _animator.SetFloat(_movementSpeedHash, speed, _speedDampTime, Time.deltaTime);
            }
            else
            {
                _animator.SetFloat(_movementSpeedHash, speed);
            }
        }

        private float GetPlanarSpeed()
        {
            if (_navMeshAgent != null && _navMeshAgent.enabled && _navMeshAgent.isOnNavMesh)
            {
                Vector3 vel = _navMeshAgent.velocity;
                vel.y = 0f;
                return vel.magnitude;
            }

            if (_rigidbodySource != null)
            {
                Vector3 vel = _rigidbodySource.velocity;
                vel.y = 0f;
                return vel.magnitude;
            }

            return 0f;
        }
    }
}

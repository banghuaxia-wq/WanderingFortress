using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Core.Interfaces;
using WF.Gameplay.Systems.Camera;
using WF.Gameplay.Systems.Combat;
using WF.Gameplay.UI.HUD;
using UCamera = UnityEngine.Camera;

namespace WF.Gameplay.Systems.Player
{
    public class PlayerCombat : MonoBehaviour, IPlayerCombatState
    {
        private const string FireInputName = "Fire1";
        private const float MinAimDirectionSqr = 0.0001f;

        [Tooltip("子弹生成的起始位置")]
        [SerializeField] private Transform shootOrigin;
        [Tooltip("用于射线检测以确定瞄准点的层")]
        [SerializeField] private LayerMask aimLayerMask;
        
        private UCamera _gameplayCamera;
        private float _verticalRecoil;
        private float _horizontalRecoil;
        private Vector2 _axisVertical;
        private Vector2 _axisHorizontal;
        
        private IWeaponItem _currentWeapon;
        private Vector2 _currentCrosshairOffset;
        private bool _inputEnabled = true;
        private PlayerMove _playerMove; // 玩家移动组件引用（用于查询/控制冲刺状态）（中文注释）

        public Vector2 CrosshairOffset => _currentCrosshairOffset;

        private void Awake()
        {
            if (shootOrigin == null)
            {
                shootOrigin = transform;
            }

            _playerMove = GetComponent<PlayerMove>();
            AcquireGameplayCamera();
        }

        private void OnEnable()
        {
            WF.Gameplay.Core.Events.EventBus.Subscribe<WF.Gameplay.Core.Events.InputStateChangedEvent>(OnInputStateChanged);
        }

        private void OnDisable()
        {
            WF.Gameplay.Core.Events.EventBus.Unsubscribe<WF.Gameplay.Core.Events.InputStateChangedEvent>(OnInputStateChanged);
        }

        private void OnInputStateChanged(WF.Gameplay.Core.Events.InputStateChangedEvent e)
        {
            _inputEnabled = e.InputEnabled;
        }

        private void Update()
        {
            if (!_inputEnabled) return;

            // Resolve weapon from WeaponManager
            if (WeaponManager.Instance != null)
            {
                _currentWeapon = WeaponManager.Instance.GetCurrentWeapon();
            }
            
            if (_currentWeapon == null || _currentWeapon.AttackData == null)
            {
                return;
            }

            ApplyRecoilDecay();
            UpdateCrosshairOffset();

            bool wantsAttack = WantsAttackInput(_currentWeapon.AttackData);
            if (!wantsAttack) return;

            if (_playerMove != null && _playerMove.IsSprinting && !CanAttackWhileSprinting(_currentWeapon.AttackData))
            {
                _playerMove.ForceStopSprint();
            }

            // AttackManager handles cooldowns now
            // We just try to attack every frame the button is held
            // If the weapon is semi-auto, AttackManager or Behavior needs to handle it (or Input.GetButtonDown)
            // For now, assuming auto-fire or cooldown management by AttackManager.
            
            FireWeapon();
        }

        private static bool WantsAttackInput(AttackData data) // 根据攻击类型决定使用按住还是单击（中文注释）
        {
            if (data == null) return false;
            if (data.Type == AttackType.Ranged)
            {
                return Input.GetButton(FireInputName) || Input.GetMouseButton(0);
            }
            return Input.GetButtonDown(FireInputName) || Input.GetMouseButtonDown(0);
        }

        private static bool CanAttackWhileSprinting(AttackData data) // 冲刺期间是否允许攻击（中文注释）
        {
            if (data == null) return false;
            return data.Type == AttackType.Ranged;
        }

        private void FireWeapon()
        {
            if (_gameplayCamera == null)
            {
                AcquireGameplayCamera();
            }

            Vector3 aimDirection = GetAimDirection();
            // We pass the raw aim direction (modified by recoil in GetAimDirection? No, GetAimDirection reads mouse and applies recoil to the ray)
            // Wait, GetAimDirection applies recoil to the *mouse position* before raycasting.
            // So the returned direction is already "recoiled".
            // Correct.

            if (aimDirection.sqrMagnitude < MinAimDirectionSqr)
            {
                return;
            }

            if (AttackManager.Instance != null)
            {
                bool attacked = AttackManager.Instance.TryAttack(gameObject, _currentWeapon, aimDirection, shootOrigin.position);
                if (attacked)
                {
                    ApplyRecoilKick();
                }
            }
        }

        private Vector3 GetAimDirection()
        {
            if (_gameplayCamera == null)
            {
                return Vector3.zero;
            }

            Vector3 playerScreen = _gameplayCamera.WorldToScreenPoint(shootOrigin.position);
            Vector2 toMouse = new Vector2(Input.mousePosition.x - playerScreen.x, Input.mousePosition.y - playerScreen.y);
            if (toMouse.sqrMagnitude < MinAimDirectionSqr) return Vector3.zero;
            toMouse.Normalize();
            Vector2 perp = new Vector2(-toMouse.y, toMouse.x);
            _axisVertical = toMouse;
            _axisHorizontal = perp;
            
            // Apply visual recoil to the aim point
            Vector2 offset = _axisVertical * _verticalRecoil + _axisHorizontal * _horizontalRecoil;
            Vector3 mouse = new Vector3(Input.mousePosition.x + offset.x, Input.mousePosition.y + offset.y, 0f);
            
            Ray aimRay = _gameplayCamera.ScreenPointToRay(mouse);

            if (Physics.Raycast(aimRay, out RaycastHit hitInfo, Mathf.Infinity, aimLayerMask))
            {
                Vector3 directionToHit = hitInfo.point - shootOrigin.position;
                directionToHit.y = 0f;
                directionToHit.Normalize();
                return directionToHit;
            }

            Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, shootOrigin.position.y, 0f));
            if (groundPlane.Raycast(aimRay, out float distance))
            {
                Vector3 planePoint = aimRay.GetPoint(distance);
                Vector3 direction = planePoint - shootOrigin.position;
                direction.y = 0f;
                if (direction.sqrMagnitude > MinAimDirectionSqr)
                {
                    direction.Normalize();
                    return direction;
                }
            }

            return Vector3.zero;
        }

        private void AcquireGameplayCamera()
        {
            if (GameplayCameraProvider.TryGetGameplayCamera(out UCamera camera))
            {
                _gameplayCamera = camera;
            }
            else
            {
                _gameplayCamera = UCamera.main;
            }
        }

        private void ApplyRecoilKick()
        {
            if (_currentWeapon == null || _currentWeapon.AttackData == null) return;
            
            // Recoil data is now in AttackData
            float v = _currentWeapon.AttackData.VerticalRecoil;
            // Assuming random direction for horizontal recoil for now, or add a flag in AttackData later
            // Legacy had 'RandomizeHorizontalDirection'. For now, just alternate or random.
            float h = _currentWeapon.AttackData.HorizontalRecoil * (Random.value < 0.5f ? -1f : 1f);
            
            // Max recoil? Legacy had 'VerticalRecoilMax'.
            // For now, let's just clamp to some reasonable multiplier of the recoil per shot, or add MaxRecoil to AttackData.
            // Simplification: Clamp to 5x the per-shot recoil or a hardcoded value if not in data.
            float maxV = v * 10f; 
            float maxH = Mathf.Abs(h) * 10f;

            _verticalRecoil = Mathf.Clamp(_verticalRecoil + v, -maxV, maxV);
            _horizontalRecoil = Mathf.Clamp(_horizontalRecoil + h, -maxH, maxH);
        }

        private void ApplyRecoilDecay()
        {
            if (_currentWeapon == null || _currentWeapon.AttackData == null) return;
            float decay = Mathf.Max(0f, _currentWeapon.AttackData.RecoilDecay) * Time.deltaTime;
            _verticalRecoil = Mathf.MoveTowards(_verticalRecoil, 0f, decay);
            _horizontalRecoil = Mathf.MoveTowards(_horizontalRecoil, 0f, decay);
        }

        private void UpdateCrosshairOffset()
        {
            if (_gameplayCamera == null) return;
            Vector3 playerScreen = _gameplayCamera.WorldToScreenPoint(shootOrigin.position);
            Vector2 toMouse = new Vector2(Input.mousePosition.x - playerScreen.x, Input.mousePosition.y - playerScreen.y);
            if (toMouse.sqrMagnitude < MinAimDirectionSqr)
            {
                _currentCrosshairOffset = Vector2.zero;
                return;
            }
            toMouse.Normalize();
            Vector2 perp = new Vector2(-toMouse.y, toMouse.x);
            _currentCrosshairOffset = toMouse * _verticalRecoil + perp * _horizontalRecoil;
        }
    }
}

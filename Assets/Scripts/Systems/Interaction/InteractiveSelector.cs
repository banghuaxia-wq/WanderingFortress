using UnityEngine;
using WF.Gameplay.Systems.ContainerSystem;

namespace WF.Gameplay.Systems.Interaction
{
    public class InteractiveSelector : MonoBehaviour
    {
        [SerializeField] private float radius = 3f;
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        private ContainerActor _current;
        private void Update()
        {
            Select();
            if (_current != null && Input.GetKeyDown(interactKey))
            {
                var cm = ContainerManager.Instance; if (cm != null) cm.Open(_current.ContainerId);
            }
        }
        private void Select()
        {
            ContainerActor best = null; float bestDist = float.MaxValue;
            var list = ContainerActor.Registry;
            for (int i = 0; i < list.Count; i++)
            {
                var a = list[i]; if (a == null) continue;
                float d = Vector3.Distance(transform.position, a.transform.position);
                if (d <= radius && d < bestDist) { best = a; bestDist = d; }
            }
            if (_current == best) return;
            if (_current != null) _current.SetOutlined(false);
            _current = best;
            if (_current != null) _current.SetOutlined(true);
        }
    }
}

using UnityEngine;
using WF.Gameplay.Core.Data;

namespace WF.Gameplay.Systems.ContainerSystem
{
    public class ContainerActor : MonoBehaviour
    {
        public static System.Collections.Generic.List<ContainerActor> Registry = new System.Collections.Generic.List<ContainerActor>();
        [SerializeField] private string containerId;
        [SerializeField] private ContainerType type = ContainerType.NormalBox;
        [SerializeField] private int slotLimit = 8;
        [SerializeField] private Material outlineMaterial;
        private Renderer[] _renderers;
        private bool _outlined;
        public string ContainerId => string.IsNullOrEmpty(containerId) ? gameObject.name : containerId;
        public ContainerType Type => type;
        public int SlotLimit => slotLimit;
        private void Awake(){ _renderers = GetComponentsInChildren<Renderer>(true); }
        private void OnEnable(){ if(!Registry.Contains(this)) Registry.Add(this); EnsureRegistered(); }
        private void OnDisable(){ Registry.Remove(this); SetOutlined(false); }
        public void SetOutlined(bool enabled)
        {
            if (_outlined == enabled) return; _outlined = enabled;
            if (_renderers == null) return;
            for (int i = 0; i < _renderers.Length; i++)
            {
                var r = _renderers[i];
                if (r == null) continue;
                var mats = r.sharedMaterials;
                if (enabled)
                {
                    if (outlineMaterial != null)
                    {
                        var arr = new Material[mats.Length + 1];
                        for (int j = 0; j < mats.Length; j++) arr[j] = mats[j];
                        arr[mats.Length] = outlineMaterial;
                        r.materials = arr;
                    }
                    else
                    {
                        for (int j = 0; j < mats.Length; j++) r.material.SetFloat("_OutlineThickness", 0.02f);
                    }
                }
                else
                {
                    if (outlineMaterial != null)
                    {
                        int idx = System.Array.IndexOf(r.sharedMaterials, outlineMaterial);
                        if (idx >= 0)
                        {
                            var list = new System.Collections.Generic.List<Material>(r.sharedMaterials);
                            list.RemoveAt(idx);
                            r.materials = list.ToArray();
                        }
                    }
                    else
                    {
                        for (int j = 0; j < mats.Length; j++) r.material.SetFloat("_OutlineThickness", 0f);
                    }
                }
            }
        }
        private void EnsureRegistered()
        {
            var cm = ContainerManager.Instance; if (cm == null) return;
            var d = cm.Get(ContainerId);
            if (d == null)
            {
                d = new ContainerData { Id = ContainerId, Type = type, SlotLimit = slotLimit };
                cm.Register(d);
            }
        }
    }
}

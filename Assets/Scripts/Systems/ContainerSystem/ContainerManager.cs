using System.Collections.Generic;
using UnityEngine;
using WF.Gameplay.Core.Data;
using WF.Gameplay.Systems.EventSystem;

namespace WF.Gameplay.Systems.ContainerSystem
{
    public class ContainerManager : MonoBehaviour
    {
        public static ContainerManager Instance { get; private set; }
        private readonly Dictionary<string, ContainerData> _containers = new Dictionary<string, ContainerData>();
        // 当前已打开的容器数据（供UI在面板启用后立即渲染）
        public ContainerData CurrentOpened { get; private set; }
        private void Awake() { if (Instance != null && Instance != this) { Destroy(gameObject); return; } Instance = this; }
        public void Register(ContainerData data) { if (data == null || string.IsNullOrEmpty(data.Id)) return; _containers[data.Id] = data; }
        public ContainerData Get(string id) { if (string.IsNullOrEmpty(id)) return null; _containers.TryGetValue(id, out var d); return d; }
        public void Open(string id)
        {
            var d = Get(id);
            if (d == null)
            {
                var actor = FindActor(id);
                if (actor != null)
                {
                    d = new ContainerData { Id = actor.ContainerId, Type = actor.Type, SlotLimit = actor.SlotLimit };
                    Register(d);
                }
            }
            if (d != null)
            {
                CurrentOpened = d;
                GameEvents.RaiseContainerOpened(d);
            }
        }
        public void Close(string id) { var d = Get(id); if (d != null) { GameEvents.RaiseContainerClosed(d); if (CurrentOpened == d) CurrentOpened = null; } }
        public void AddItem(string id, ItemStack item)
        {
            if (string.IsNullOrEmpty(id) || item == null) return;
            var d = Get(id);
            if (d == null)
            {
                // Try register from scene actor
                var actor = FindActor(id);
                if (actor != null)
                {
                    d = new ContainerData { Id = actor.ContainerId, Type = actor.Type, SlotLimit = actor.SlotLimit };
                    Register(d);
                }
                else
                {
                    d = new ContainerData { Id = id, Type = ContainerType.NormalBox, SlotLimit = 8 };
                    Register(d);
                }
            }
            d.Items.Add(item);
            GameEvents.RaiseContainerUpdated(d);
        }

        private ContainerActor FindActor(string id)
        {
            for (int i = 0; i < ContainerActor.Registry.Count; i++)
            {
                var a = ContainerActor.Registry[i]; if (a == null) continue; if (a.ContainerId == id) return a;
            }
            return null;
        }
        public void RemoveItem(string id, int index)
        {
            var d = Get(id);
            if (d == null) return;
            if (index >= 0 && index < d.Items.Count)
            {
                d.Items.RemoveAt(index);
                GameEvents.RaiseContainerUpdated(d);
            }
        }
    }
}

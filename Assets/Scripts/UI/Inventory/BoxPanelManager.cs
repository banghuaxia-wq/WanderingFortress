using UnityEngine;
using WF.Gameplay.Systems.ContainerSystem;
using WF.Gameplay.Core.Events;
using WF.Gameplay.Core.Data;

namespace WF.Gameplay.UI.Inventory
{
    public class BoxPanelManager : MonoBehaviour
    {
        [SerializeField] private Transform content;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private UISlotPoolManager slotPool;
        [SerializeField] private string currentContainerId;
        
        private void OnEnable()
        {
            EventBus.Subscribe<ContainerOpenedEvent>(OnOpened);
            EventBus.Subscribe<ContainerUpdatedEvent>(OnUpdated);
            var cm = ContainerManager.Instance;
            if (cm != null && cm.CurrentOpened != null)
            {
                currentContainerId = cm.CurrentOpened.Id;
                Render(cm.CurrentOpened);
            }
        }
        
        private void OnDisable() 
        { 
            EventBus.Unsubscribe<ContainerOpenedEvent>(OnOpened); 
            EventBus.Unsubscribe<ContainerUpdatedEvent>(OnUpdated); 
        }
        
        public void Open(string id) { currentContainerId = id; var d = ContainerManager.Instance?.Get(id); if (d != null) Render(d); }
        
        private void OnOpened(ContainerOpenedEvent e) { var d = e.Data; if (d == null) return; currentContainerId = d.Id; Render(d); }
        private void OnUpdated(ContainerUpdatedEvent e) { var d = e.Data; if (d == null) return; if (d.Id == currentContainerId) Render(d); }
        
        private void Render(ContainerData d)
        {
            if (content == null || slotPrefab == null || slotPool == null) return;
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                var go = content.GetChild(i).gameObject;
                var po = go.GetComponent<WF.Gameplay.Core.Utilities.Pooling.PooledObject>();
                if (po != null) slotPool.Release(go); else GameObject.Destroy(go);
            }

            int slotCount = d.SlotLimit > 0 ? d.SlotLimit : d.Items.Count;
            for (int i = 0; i < slotCount; i++)
            {
                var slot = slotPool.Get(slotPrefab, content);
                slot.SetSlotType(UISlotType.Package);
                slot.SetMeta(TransferSource.Box, currentContainerId, i);
                var item = (d.Items != null && i >= 0 && i < d.Items.Count) ? d.Items[i] : null;
                slot.Bind(item);
            }
        }
    }
}

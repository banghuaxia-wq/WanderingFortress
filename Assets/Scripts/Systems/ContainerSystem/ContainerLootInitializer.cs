using UnityEngine;
using WF.Gameplay.Core.Data;

namespace WF.Gameplay.Systems.ContainerSystem
{
    public class ContainerLootInitializer : MonoBehaviour
    {
        [SerializeField] private ContainerLootConfigSO containerLootConfig;
        [SerializeField] private float luck = 0f;
        [SerializeField] private float luckSoftCap = 0.75f;
        [SerializeField] private int globalSeed = 0;
        [SerializeField] private bool fillOnlyEmpty = true;

        private bool _hasRun;

        private void Start()
        {
            GenerateOnce();
        }

        public void GenerateOnce()
        {
            if (_hasRun) return;
            _hasRun = true;

            var manager = ContainerManager.Instance;
            if (manager == null) return;
            if (containerLootConfig == null) return;

            for (int i = 0; i < ContainerActor.Registry.Count; i++)
            {
                var actor = ContainerActor.Registry[i];
                if (actor == null) continue;

                var containerId = actor.ContainerId;
                if (string.IsNullOrEmpty(containerId)) continue;

                var data = manager.Get(containerId);
                if (data == null)
                {
                    data = new ContainerData { Id = containerId, Type = actor.Type, SlotLimit = actor.SlotLimit };
                    manager.Register(data);
                }

                if (data.HasGeneratedLoot) continue;
                if (fillOnlyEmpty && data.Items != null && data.Items.Count > 0) { data.HasGeneratedLoot = true; continue; }

                var table = containerLootConfig.GetLootTable(actor.Type);
                if (table == null) { data.HasGeneratedLoot = true; continue; }

                int seed = ComputeContainerSeed(containerId);
                ContainerGenerator.FillContainerWithLoot(data, table, luck, seed, luckSoftCap);
                data.HasGeneratedLoot = true;
            }
        }

        private int ComputeContainerSeed(string containerId)
        {
            int idHash = StableStringHash(containerId);
            return globalSeed == 0 ? idHash : (globalSeed ^ idHash);
        }

        private static int StableStringHash(string s)
        {
            if (string.IsNullOrEmpty(s)) return 0;
            unchecked
            {
                int hash = 23;
                for (int i = 0; i < s.Length; i++)
                {
                    hash = (hash * 31) + s[i];
                }
                return hash;
            }
        }
    }
}

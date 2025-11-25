using UnityEngine;
using System.Collections.Generic;

namespace WF.Gameplay
{
    public enum BuffUpdateEnum { AddTime, RefreshTime, RefreshAndAddStack, AddStackOnly, Replace }
    public enum BuffRemoveEnum { RemoveAll, ReduceOneStack }

    [CreateAssetMenu(menuName = "WF/Buffs/BuffData")]
    public class BuffData : ScriptableObject
    {
        public string Id;
        public string BuffName;
        public string Desc;
        public Sprite Icon;
        public int MaxStack = 1;
        public float Duration = 5f;
        public bool IsForever = false;
        public BuffUpdateEnum UpdateStrategy = BuffUpdateEnum.RefreshAndAddStack;
        public BuffRemoveEnum RemoveStrategy = BuffRemoveEnum.RemoveAll;
        public List<BuffModule> BuffModules = new List<BuffModule>();
    }
}

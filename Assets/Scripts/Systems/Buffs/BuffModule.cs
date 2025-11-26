using UnityEngine;

using WF.Gameplay.Core.Data;

namespace WF.Gameplay.Systems.Buffs
{
    public enum BuffCallback { OnCreate, OnRemove, OnAddStack, OnReduceStack, OnTick }
    public abstract class BuffModule : ScriptableObject
    {
        public BuffCallback callback;
        public abstract void Execute(BuffRunTimeInfo info, BuffManager manager);
    }
}

using UnityEngine;

namespace WF.Gameplay
{
    public enum BuffCallback { OnCreate, OnRemove, OnAddStack, OnReduceStack, OnTick }
    public abstract class BuffModule : ScriptableObject
    {
        public BuffCallback callback;
        public abstract void Execute(BuffRunTimeInfo info, BuffManager manager);
    }
}


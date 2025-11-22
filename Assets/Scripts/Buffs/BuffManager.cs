using System.Collections.Generic;
using UnityEngine;

namespace WF.Gameplay
{
    public class BuffManager : MonoBehaviour
    {
        public List<BuffRunTimeInfo> buffs = new List<BuffRunTimeInfo>();
        [SerializeField] private bool enableDebugLogs = true;

        public BuffRunTimeInfo AddBuff(BuffData data, GameObject creator)
        {
            BuffRunTimeInfo existing = buffs.Find(b => b.BuffData != null && b.BuffData.Id == data.Id);
            if (existing == null)
            {
                var runtime = new BuffRunTimeInfo(data, creator, gameObject);
                buffs.Add(runtime);
                InvokeModules(BuffCallback.OnCreate, runtime);
                if (enableDebugLogs) Debug.Log($"施加成功：现在 1 层 '{GetBuffLabel(data)}'", this);
                return runtime;
            }
            switch (data.UpdateStrategy)
            {
                case BuffUpdateEnum.AddTime:
                    if (!existing.BuffData.IsForever) existing.DurationTimer += data.Duration;
                    if (enableDebugLogs) Debug.Log($"延长持续时间 '{GetBuffLabel(data)}' 至 {existing.DurationTimer:F2}s", this);
                    break;
                case BuffUpdateEnum.RefreshTime:
                    if (!existing.BuffData.IsForever) existing.DurationTimer = data.Duration;
                    existing.TickTimer = data.TickInterval;
                    if (enableDebugLogs) Debug.Log($"刷新持续时间 '{GetBuffLabel(data)}' 为 {data.Duration:F2}s", this);
                    break;
                case BuffUpdateEnum.RefreshAndAddStack:
                    if (!existing.BuffData.IsForever) existing.DurationTimer = data.Duration;
                    existing.TickTimer = data.TickInterval;
                    {
                        int oldStack = existing.CurStack;
                        existing.CurStack = Mathf.Clamp(existing.CurStack + 1, 1, data.MaxStack);
                        if (existing.CurStack > oldStack) InvokeModules(BuffCallback.OnAddStack, existing);
                        if (enableDebugLogs) Debug.Log($"叠加成功：现在 {existing.CurStack} 层 '{GetBuffLabel(data)}'", this);
                    }
                    break;
                case BuffUpdateEnum.AddStackOnly:
                    {
                        int oldStack = existing.CurStack;
                        existing.CurStack = Mathf.Clamp(existing.CurStack + 1, 1, data.MaxStack);
                        if (existing.CurStack > oldStack) InvokeModules(BuffCallback.OnAddStack, existing);
                        if (enableDebugLogs) Debug.Log($"叠加成功：现在 {existing.CurStack} 层 '{GetBuffLabel(data)}'", this);
                    }
                    break;
                case BuffUpdateEnum.Replace:
                    existing.BuffData = data;
                    existing.CurStack = 1;
                    existing.DurationTimer = data.IsForever ? float.PositiveInfinity : data.Duration;
                    existing.TickTimer = data.TickInterval;
                    InvokeModules(BuffCallback.OnCreate, existing);
                    if (enableDebugLogs) Debug.Log($"替换 Buff：现在 1 层 '{GetBuffLabel(data)}'", this);
                    break;
            }
            return existing;
        }

        public void RemoveBuff(string id)
        {
            int idx = buffs.FindIndex(b => b.BuffData != null && b.BuffData.Id == id);
            if (idx >= 0)
            {
                var info = buffs[idx];
                InvokeModules(BuffCallback.OnRemove, info);
                if (enableDebugLogs) Debug.Log($"手动移除 '{GetBuffLabel(info.BuffData)}'", this);
                buffs.RemoveAt(idx);
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            for (int i = buffs.Count - 1; i >= 0; i--)
            {
                var info = buffs[i];
                if (info.BuffData == null) { buffs.RemoveAt(i); continue; }
                if (!info.BuffData.IsForever)
                {
                    info.DurationTimer -= dt;
                    if (info.DurationTimer <= 0f)
                    {
                        if (info.BuffData.RemoveStrategy == BuffRemoveEnum.RemoveAll || info.CurStack <= 1)
                        {
                            InvokeModules(BuffCallback.OnRemove, info);
                            if (enableDebugLogs) Debug.Log($"效果结束：移除 '{GetBuffLabel(info.BuffData)}'", this);
                            buffs.RemoveAt(i);
                            continue;
                        }
                        info.CurStack = Mathf.Max(1, info.CurStack - 1);
                        InvokeModules(BuffCallback.OnReduceStack, info);
                        if (enableDebugLogs) Debug.Log($"层数减少：现在 {info.CurStack} 层 '{GetBuffLabel(info.BuffData)}'", this);
                        info.DurationTimer = info.BuffData.Duration;
                        info.TickTimer = info.BuffData.TickInterval;
                    }
                }
                if (info.BuffData.TickInterval > 0f)
                {
                    info.TickTimer -= dt;
                    if (info.TickTimer <= 0f)
                    {
                        float interval = Mathf.Max(0.001f, info.BuffData.TickInterval);
                        info.TickTimer += interval;
                        InvokeModules(BuffCallback.OnTick, info);
                    }
                }
            }
        }

        private void InvokeModules(BuffCallback cb, BuffRunTimeInfo info)
        {
            var modules = info.BuffData.BuffModules;
            if (modules == null) return;
            for (int m = 0; m < modules.Count; m++)
            {
                var mod = modules[m];
                if (mod == null) continue;
                if (mod.callback == cb) mod.Execute(info, this);
            }
        }

        private string GetBuffLabel(BuffData data)
        {
            if (data == null) return "Buff";
            if (!string.IsNullOrEmpty(data.BuffName)) return data.BuffName;
            if (!string.IsNullOrEmpty(data.Id)) return data.Id;
            return data.name;
        }
    }
}

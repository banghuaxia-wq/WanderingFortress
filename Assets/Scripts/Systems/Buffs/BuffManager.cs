using System.Collections.Generic;
using UnityEngine;
using WF.Gameplay.Core.Data;

namespace WF.Gameplay.Systems.Buffs
{
    public class BuffManager : MonoBehaviour
    {
        public List<BuffRunTimeInfo> buffs = new List<BuffRunTimeInfo>();
        [SerializeField] private bool enableDebugLogs = true;
        public bool IsLogEnabled => enableDebugLogs;
        private readonly Dictionary<BuffRunTimeInfo, Coroutine> _tickRoutines = new Dictionary<BuffRunTimeInfo, Coroutine>();

        public BuffRunTimeInfo AddBuff(BuffData data, GameObject creator)
        {
            BuffRunTimeInfo existing = buffs.Find(b => b.BuffData != null && b.BuffData.Id == data.Id);
            if (existing == null)
            {
                var runtime = new BuffRunTimeInfo(data, creator, gameObject);
                buffs.Add(runtime);
                InvokeModules(BuffCallback.OnCreate, runtime);
                StartTickRoutine(runtime);
                runtime.ElapsedSeconds++;
                if (enableDebugLogs)
                {
                    Debug.Log($"[BuffTick] start sec={runtime.ElapsedSeconds} buff={GetBuffLabel(runtime.BuffData)}", this);
                }
                InvokeModules(BuffCallback.OnTick, runtime);
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
                    if (enableDebugLogs) Debug.Log($"刷新持续时间 '{GetBuffLabel(data)}' 为 {data.Duration:F2}s", this);
                    break;
                case BuffUpdateEnum.RefreshAndAddStack:
                    if (!existing.BuffData.IsForever) existing.DurationTimer = data.Duration;
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
                    InvokeModules(BuffCallback.OnCreate, existing);
                    RestartTickRoutine(existing);
                    if (enableDebugLogs) Debug.Log($"替换 Buff：现在 1 层 '{GetBuffLabel(data)}'", this);
                    break;
            }
            return existing;
        }

        public BuffRunTimeInfo AddBuff(BuffData data, GameObject creator, float extraValue)
        {
            BuffRunTimeInfo info = AddBuff(data, creator);
            if (info == null) return null;
            if (info.CurStack <= 1 && info.ExtraValue == 0f)
            {
                info.ExtraValue = extraValue;
            }
            else
            {
                switch (data.UpdateStrategy)
                {
                    case BuffUpdateEnum.AddTime:
                    case BuffUpdateEnum.RefreshTime:
                        info.ExtraValue = extraValue; // 替换为最新值
                        break;
                    case BuffUpdateEnum.RefreshAndAddStack:
                    case BuffUpdateEnum.AddStackOnly:
                        info.ExtraValue += extraValue; // 按堆叠累加
                        break;
                    case BuffUpdateEnum.Replace:
                        info.ExtraValue = extraValue;
                        break;
                }
            }
            return info;
        }

        public void RemoveBuff(string id)
        {
            int idx = buffs.FindIndex(b => b.BuffData != null && b.BuffData.Id == id);
            if (idx >= 0)
            {
                var info = buffs[idx];
                InvokeModules(BuffCallback.OnRemove, info);
                if (enableDebugLogs) Debug.Log($"手动移除 '{GetBuffLabel(info.BuffData)}'", this);
                StopTickRoutine(info);
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
                            StopTickRoutine(info);
                            buffs.RemoveAt(i);
                            continue;
                        }
                        info.CurStack = Mathf.Max(1, info.CurStack - 1);
                        InvokeModules(BuffCallback.OnReduceStack, info);
                        if (enableDebugLogs) Debug.Log($"层数减少：现在 {info.CurStack} 层 '{GetBuffLabel(info.BuffData)}'", this);
                        info.DurationTimer = info.BuffData.Duration;
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

        private void StartTickRoutine(BuffRunTimeInfo info)
        {
            if (info == null) return;
            if (_tickRoutines.ContainsKey(info)) return;
            var co = StartCoroutine(TickRoutine(info));
            _tickRoutines[info] = co;
        }

        private void RestartTickRoutine(BuffRunTimeInfo info)
        {
            StopTickRoutine(info);
            StartTickRoutine(info);
        }

        private void StopTickRoutine(BuffRunTimeInfo info)
        {
            if (info == null) return;
            if (_tickRoutines.TryGetValue(info, out var co) && co != null)
            {
                StopCoroutine(co);
            }
            _tickRoutines.Remove(info);
        }

        private System.Collections.IEnumerator TickRoutine(BuffRunTimeInfo info)
        {
            var wait = new WaitForSeconds(1f);
            while (buffs.Contains(info))
            {
                yield return wait;
                if (!buffs.Contains(info)) break;
                info.ElapsedSeconds++;
                if (enableDebugLogs)
                {
                    Debug.Log($"[BuffTick] sec={info.ElapsedSeconds} buff={GetBuffLabel(info.BuffData)}", this);
                }
                InvokeModules(BuffCallback.OnTick, info);
            }
        }
    }
}

using UnityEditor;
using UnityEngine;

/// <summary>
/// 简易项目工具：在 Unity 菜单中提供“重新生成 C# 项目文件”。
/// 适用于编辑器或外部 IDE 无法自动刷新 .sln/.csproj 时手动触发。
/// </summary>
public static class ProjectTools
{
    [MenuItem("Tools/Rebuild C# Project Files", priority = 10)]
    public static void RebuildCsproj()
    {
        try
        {
            EditorUtility.DisplayProgressBar("Regenerate .csproj", "Refreshing assets...", 0.25f);
            AssetDatabase.Refresh();

            // 优先使用较新的 API（Unity 2020+），否则回退到 SyncVS。
            // 某些版本没有 CodeEditor API 或 SyncVS 的可见性不同，故使用反射以提升兼容性。
            EditorUtility.DisplayProgressBar("Regenerate .csproj", "Syncing solution...", 0.6f);

            var codeEditorType = System.Type.GetType("Unity.CodeEditor.CodeEditor, UnityEditor.CoreModule");
            if (codeEditorType != null)
            {
                var currentProp = codeEditorType.GetProperty("Current", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                var current = currentProp?.GetValue(null);
                var syncAll = current?.GetType().GetMethod("SyncAll", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                syncAll?.Invoke(current, null);
            }
            else
            {
                var syncVsType = System.Type.GetType("UnityEditor.SyncVS, UnityEditor");
                var syncSolution = syncVsType?.GetMethod("SyncSolution", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                syncSolution?.Invoke(null, null);
            }

            Debug.Log("[Tools] Regenerated .sln/.csproj and synced solution.");
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[Tools] Regenerate failed: {ex.Message}\n尝试手动执行：Assets > Open C# Project 或 Reimport All。");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }
}


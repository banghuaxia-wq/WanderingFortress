using UnityEditor;
using UnityEngine;
using System.IO;

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

    [MenuItem("Tools/Normalize Line Endings (Windows)", priority = 11)]
    public static void NormalizeSelectedFilesToWindows()
    {
        var guids = Selection.assetGUIDs;
        if (guids == null || guids.Length == 0)
        {
            Debug.LogWarning("[Tools] No assets selected. Select text assets (e.g., .shader) to normalize.");
            return;
        }
        try
        {
            EditorUtility.DisplayProgressBar("Normalize Line Endings", "Processing selected assets...", 0.2f);
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path)) continue;
                var ext = Path.GetExtension(path).ToLowerInvariant();
                // Process common text-based assets
                if (ext == ".shader" || ext == ".cginc" || ext == ".txt" || ext == ".cs" || ext == ".json" || ext == ".xml" || ext == ".uxml" || ext == ".uss")
                {
                    var fullPath = Path.Combine(Directory.GetCurrentDirectory(), path.Replace('/', Path.DirectorySeparatorChar));
                    if (!File.Exists(fullPath)) continue;
                    var content = File.ReadAllText(fullPath);
                    // Normalize to Windows CRLF
                    content = content.Replace("\r\n", "\n");
                    content = content.Replace("\r", "\n");
                    content = content.Replace("\n", "\r\n");
                    File.WriteAllText(fullPath, content);
                    Debug.Log($"[Tools] Normalized line endings: {path}");
                }
            }
            AssetDatabase.Refresh();
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }
}

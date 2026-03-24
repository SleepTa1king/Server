using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class UIAssetBundleBuilder
{
    private const string BundleName = "ui_bundle.ab";
    private const string OutputFolder = "AssetBundles/UI";

    [MenuItem("Tools/UI/AssetBundles/Build Selected UI Bundle")]
    private static void BuildSelectedUIBundle()
    {
        var prefabPaths = CollectSelectedPrefabPaths();
        if (prefabPaths.Count == 0)
        {
            Debug.LogWarning("[UIAssetBundleBuilder] No prefab assets were found in the current selection.");
            return;
        }

        var duplicateNames = prefabPaths
            .Select(path => Path.GetFileNameWithoutExtension(path))
            .GroupBy(name => name)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicateNames.Length > 0)
        {
            Debug.LogError("[UIAssetBundleBuilder] Duplicate prefab names found in selection: " + string.Join(", ", duplicateNames));
            Debug.LogError("[UIAssetBundleBuilder] Please make prefab names unique, otherwise AssetBundle assetName lookups will be ambiguous.");
            return;
        }

        var outputPath = Path.Combine(Application.streamingAssetsPath, OutputFolder);
        Directory.CreateDirectory(outputPath);

        var build = new AssetBundleBuild
        {
            assetBundleName = BundleName,
            assetNames = prefabPaths.ToArray(),
            addressableNames = prefabPaths
                .Select(path => Path.GetFileNameWithoutExtension(path))
                .ToArray()
        };

        BuildPipeline.BuildAssetBundles(
            outputPath,
            new[] { build },
            BuildAssetBundleOptions.ChunkBasedCompression,
            EditorUserBuildSettings.activeBuildTarget);

        AssetDatabase.Refresh();
        Debug.Log($"[UIAssetBundleBuilder] Built '{BundleName}' with {prefabPaths.Count} prefabs to: {outputPath}");
    }

    [MenuItem("Tools/UI/AssetBundles/Build Selected UI Bundle", true)]
    private static bool ValidateBuildSelectedUIBundle()
    {
        return CollectSelectedPrefabPaths().Count > 0;
    }

    [MenuItem("Tools/UI/AssetBundles/Log Selected UI Asset Names")]
    private static void LogSelectedUIAssetNames()
    {
        var prefabPaths = CollectSelectedPrefabPaths();
        if (prefabPaths.Count == 0)
        {
            Debug.LogWarning("[UIAssetBundleBuilder] No prefab assets were found in the current selection.");
            return;
        }

        foreach (var prefabPath in prefabPaths)
        {
            Debug.Log($"[UIAssetBundleBuilder] assetName = {Path.GetFileNameWithoutExtension(prefabPath)}, path = {prefabPath}");
        }
    }

    private static List<string> CollectSelectedPrefabPaths()
    {
        var prefabPaths = new HashSet<string>();
        foreach (var guid in Selection.assetGUIDs)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                continue;
            }

            if (AssetDatabase.IsValidFolder(assetPath))
            {
                var nestedPrefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { assetPath });
                foreach (var prefabGuid in nestedPrefabGuids)
                {
                    var nestedPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
                    if (!string.IsNullOrWhiteSpace(nestedPath))
                    {
                        prefabPaths.Add(nestedPath);
                    }
                }
                continue;
            }

            if (Path.GetExtension(assetPath).Equals(".prefab", System.StringComparison.OrdinalIgnoreCase))
            {
                prefabPaths.Add(assetPath);
            }
        }

        return prefabPaths.OrderBy(path => path).ToList();
    }
}

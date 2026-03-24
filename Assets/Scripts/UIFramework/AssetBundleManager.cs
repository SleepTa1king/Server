using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UIFramework
{
    /// <summary>
    /// 运行时 AssetBundle 管理器，负责缓存和释放。
    /// </summary>
    public static class AssetBundleManager
    {
        private sealed class BundleHandle
        {
            public string FullPath;
            public AssetBundle Bundle;
            public int RefCount;
        }

        private static readonly Dictionary<string, BundleHandle> LoadedBundles = new Dictionary<string, BundleHandle>();

        public static bool TryLoadBundle(string fullPath, out AssetBundle bundle)
        {
            bundle = null;

            if (string.IsNullOrWhiteSpace(fullPath))
            {
                Debug.LogWarning("[AssetBundleManager] Bundle path is empty.");
                return false;
            }

            var normalizedPath = NormalizePath(fullPath);
            if (LoadedBundles.TryGetValue(normalizedPath, out var existingHandle))
            {
                existingHandle.RefCount++;
                bundle = existingHandle.Bundle;
                return bundle != null;
            }

            if (!File.Exists(normalizedPath))
            {
                Debug.LogWarning($"[AssetBundleManager] Bundle file not found: {normalizedPath}");
                return false;
            }

            bundle = AssetBundle.LoadFromFile(normalizedPath);
            if (bundle == null)
            {
                Debug.LogError($"[AssetBundleManager] Failed to load AssetBundle from '{normalizedPath}'.");
                return false;
            }

            LoadedBundles[normalizedPath] = new BundleHandle
            {
                FullPath = normalizedPath,
                Bundle = bundle,
                RefCount = 1
            };
            return true;
        }

        public static bool TryLoadAsset<T>(string fullPath, string assetName, out T asset) where T : UnityEngine.Object
        {
            asset = null;
            if (string.IsNullOrWhiteSpace(assetName))
            {
                Debug.LogWarning("[AssetBundleManager] Asset name is empty.");
                return false;
            }

            if (!TryLoadBundle(fullPath, out var bundle))
            {
                return false;
            }

            asset = bundle.LoadAsset<T>(assetName);
            if (asset == null)
            {
                Debug.LogError($"[AssetBundleManager] Asset '{assetName}' was not found in bundle '{fullPath}'.");
                ReleaseBundle(fullPath);
                return false;
            }

            return true;
        }

        public static bool IsBundleLoaded(string fullPath)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
            {
                return false;
            }

            return LoadedBundles.ContainsKey(NormalizePath(fullPath));
        }

        public static void ReleaseBundle(string fullPath, bool unloadAllLoadedObjects = false)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
            {
                return;
            }

            var normalizedPath = NormalizePath(fullPath);
            if (!LoadedBundles.TryGetValue(normalizedPath, out var handle))
            {
                return;
            }

            handle.RefCount--;
            if (handle.RefCount > 0)
            {
                return;
            }

            try
            {
                handle.Bundle?.Unload(unloadAllLoadedObjects);
            }
            finally
            {
                LoadedBundles.Remove(normalizedPath);
            }
        }

        public static void ReleaseAll(bool unloadAllLoadedObjects = false)
        {
            foreach (var pair in LoadedBundles)
            {
                pair.Value.Bundle?.Unload(unloadAllLoadedObjects);
            }

            LoadedBundles.Clear();
        }

        private static string NormalizePath(string fullPath)
        {
            return Path.GetFullPath(fullPath).Replace('\\', '/');
        }
    }
}

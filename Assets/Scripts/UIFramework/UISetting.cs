using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UIFramework.Core;

namespace UIFramework
{
    /// <summary>
    /// UI的模板
    /// </summary>
    [CreateAssetMenu(fileName = "UISetting", menuName = ("UI/UI Settings"))]
    public class UISetting : ScriptableObject
    {
        [Serializable]
        private class AssetBundleScreenReference
        {
            [Tooltip("注册到 UIFrame 时使用的 ScreenId，留空则使用 Asset Name")]
            public string screenId;

            [Tooltip("AssetBundle 中预制体的资源名")]
            public string assetName;
        }

        [Tooltip("UI Frame的预制体")]
        [SerializeField]
        private UIFrame templateUIPrefab = null;

        [Tooltip("界面的预制体(直接引用，用于Editor或不热更的UI)")]
        [SerializeField]
        private List<GameObject> screenToRegister = new List<GameObject>();

        [Header("AssetBundle UI")]
        [Tooltip("是否启用 AssetBundle 中的 UI 预制体注册")]
        [SerializeField]
        private bool useAssetBundleScreens = false;

        [Tooltip("AssetBundle 文件名，例如 ui_bundle.ab")]
        [SerializeField]
        private string assetBundleFileName = "ui_bundle.ab";

        [Tooltip("相对 StreamingAssets 的子目录，留空表示直接放在根目录")]
        [SerializeField]
        private string assetBundleDirectory = string.Empty;

        [Tooltip("通过 AssetBundle 加载的界面列表")]
        [SerializeField]
        private List<AssetBundleScreenReference> assetBundleScreens = new List<AssetBundleScreenReference>();

        [SerializeField, HideInInspector]
        private List<string> screensToLoadViaAB = new List<string>();

        [Tooltip("创建完成后是否立即释放本次加载用到的 AssetBundle 句柄")]
        [SerializeField]
        private bool releaseAssetBundleAfterRegister = false;

        [Tooltip("实例化时是否停用")]
        [SerializeField]
        private bool deactivateScreenGOs = true;

        /// <summary>
        /// 创建一个UI Frame对象
        /// </summary>
        public UIFrame CreateUIInstance(bool instanceAndRegisterScreens = true)
        {
            var newUI = Instantiate(templateUIPrefab);
            if (!newUI.gameObject.activeSelf)
            {
                newUI.gameObject.SetActive(true);
            }
            newUI.Initialize();

            if (!instanceAndRegisterScreens)
            {
                return newUI;
            }

            foreach (var screen in screenToRegister)
            {
                if (screen == null)
                {
                    continue;
                }

                InstantiateAndRegister(newUI, screen);
            }

            if (useAssetBundleScreens && assetBundleScreens.Count > 0)
            {
                LoadScreensFromAssetBundle(newUI);
            }

            return newUI;
        }

        private void InstantiateAndRegister(UIFrame newUI, GameObject prefab, string screenIdOverride = null)
        {
            var screenId = string.IsNullOrWhiteSpace(screenIdOverride) ? prefab.name : screenIdOverride;
            if (newUI.IsScreenRegistered(screenId))
            {
                Debug.LogWarning($"[UISetting] ScreenId '{screenId}' is already registered. Skipping duplicate prefab '{prefab.name}'.");
                return;
            }

            var screenInstance = Instantiate(prefab);
            var screenController = screenInstance.GetComponent<IScreenController>();

            if (screenController == null)
            {
                Debug.LogError($"[UISetting] Screen prefab '{prefab.name}' does not contain a ScreenController component.");
                Destroy(screenInstance);
                return;
            }

            newUI.RegisterScreen(screenId, screenController, screenInstance.transform);
            if (deactivateScreenGOs && screenInstance.activeSelf)
            {
                screenInstance.SetActive(false);
            }
        }

        private void LoadScreensFromAssetBundle(UIFrame newUI)
        {
            if (string.IsNullOrWhiteSpace(assetBundleFileName))
            {
                Debug.LogWarning($"[UISetting] AssetBundle file name is empty on '{name}'.");
                return;
            }

            var assetBundlePath = GetAssetBundlePath();
            var acquiredBundle = false;

            try
            {
                if (!AssetBundleManager.TryLoadBundle(assetBundlePath, out var assetBundle))
                {
                    return;
                }
                acquiredBundle = true;

                foreach (var screenRef in assetBundleScreens)
                {
                    if (screenRef == null || string.IsNullOrWhiteSpace(screenRef.assetName))
                    {
                        Debug.LogWarning($"[UISetting] Found an empty AssetBundle UI entry on '{name}', skipping.");
                        continue;
                    }

                    var prefab = assetBundle.LoadAsset<GameObject>(screenRef.assetName);
                    if (prefab == null)
                    {
                        Debug.LogError($"[UISetting] Asset '{screenRef.assetName}' was not found in AssetBundle '{assetBundleFileName}'.");
                        continue;
                    }

                    InstantiateAndRegister(newUI, prefab, screenRef.screenId);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[UISetting] Failed to load AssetBundle UI from '{assetBundlePath}'. {e}");
            }
            finally
            {
                if (acquiredBundle && releaseAssetBundleAfterRegister)
                {
                    AssetBundleManager.ReleaseBundle(assetBundlePath);
                }
            }
        }

        private string GetAssetBundlePath()
        {
            if (string.IsNullOrWhiteSpace(assetBundleDirectory))
            {
                return Path.Combine(Application.streamingAssetsPath, assetBundleFileName);
            }

            return Path.Combine(Application.streamingAssetsPath, assetBundleDirectory, assetBundleFileName);
        }

        private void OnValidate()
        {
            var objectsToRemove = new List<GameObject>();
            for (int i = 0; i < screenToRegister.Count; i++)
            {
                if (screenToRegister[i] == null)
                {
                    continue;
                }

                var screenCtl = screenToRegister[i].GetComponent<IScreenController>();
                if (screenCtl == null)
                {
                    objectsToRemove.Add(screenToRegister[i]);
                }
            }

            if (objectsToRemove.Count > 0)
            {
                foreach (var obj in objectsToRemove)
                {
                    Debug.LogError($"[UISettings] Removed {obj.name} from {name} as it has no Screen Controller");
                    screenToRegister.Remove(obj);
                }
            }

            if (string.IsNullOrWhiteSpace(assetBundleFileName))
            {
                useAssetBundleScreens = false;
            }

            if (assetBundleScreens.Count == 0 && screensToLoadViaAB.Count > 0)
            {
                foreach (var legacyAssetName in screensToLoadViaAB)
                {
                    if (string.IsNullOrWhiteSpace(legacyAssetName))
                    {
                        continue;
                    }

                    assetBundleScreens.Add(new AssetBundleScreenReference
                    {
                        assetName = legacyAssetName,
                        screenId = legacyAssetName
                    });
                }
            }

            foreach (var screenRef in assetBundleScreens)
            {
                if (screenRef == null || string.IsNullOrWhiteSpace(screenRef.assetName))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(screenRef.screenId))
                {
                    screenRef.screenId = screenRef.assetName;
                }
            }
        }
    }
}

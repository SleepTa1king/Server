using UnityEngine;
using UIFramework;

namespace Main
{
    /// <summary>
    /// UI 启动器：在场景启动时根据 UISetting 创建 UIFrame。
    /// </summary>
    public class UIBoot : MonoBehaviour
    {
        public static UIBoot Instance { get; private set; }

        [SerializeField]
        private UISetting uiSetting = null;

        [Tooltip("创建后自动显示的首个界面，留空则不自动打开")]
        [SerializeField]
        private string initialScreenId = string.Empty;

        [Tooltip("是否在切场景时保留这套 UI")]
        [SerializeField]
        private bool dontDestroyOnLoad = true;

        private UIFrame uiFrameInstance;

        public UIFrame UIFrameInstance => uiFrameInstance;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            Boot();
        }

        public void Boot()
        {
            if (uiFrameInstance != null)
            {
                return;
            }

            if (uiSetting == null)
            {
                Debug.LogError("[UIBoot] UISetting is not assigned.");
                return;
            }

            uiFrameInstance = uiSetting.CreateUIInstance();
            if (uiFrameInstance == null)
            {
                Debug.LogError("[UIBoot] Failed to create UIFrame from UISetting.");
                return;
            }

            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(uiFrameInstance.gameObject);
            }

            if (!string.IsNullOrWhiteSpace(initialScreenId))
            {
                uiFrameInstance.ShowScreen(initialScreenId);
            }
        }

        public void ShowScreen(string screenId)
        {
            if (uiFrameInstance == null)
            {
                Debug.LogWarning("[UIBoot] UIFrame has not been created yet.");
                return;
            }

            uiFrameInstance.ShowScreen(screenId);
        }
    }
}

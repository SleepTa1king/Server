using UnityEngine;

public class PlayerVisibilityDebugger : MonoBehaviour
{
    private bool _hasReportedRendererCount = false;

    void Start()
    {
        Debug.Log($"<color=cyan>[Visibility Debug]</color> 初始检查: {gameObject.name}");
        CheckVisibility();
    }

    void Update()
    {
        // 每秒检查一次，防止漏掉运行时消失的情况
        if (Time.frameCount % 60 == 0)
        {
            CheckVisibility();
        }
    }

    void CheckVisibility()
    {
        var renderers = GetComponentsInChildren<Renderer>(true);
        
        if (renderers.Length == 0)
        {
            Debug.LogError($"[Visibility Debug] {Time.time:F2}s: ！！！严重警告！！！场景中找不到任何 Renderer 了。它们是被 Destroy 了吗？");
        }
        else if (!_hasReportedRendererCount || renderers.Length > 1)
        {
            foreach (var r in renderers)
            {
                // 忽略 Trail 渲染器，我们找核心模型
                if (r.gameObject.name == "Trail") continue;

                Debug.Log($"[Visibility Debug] {Time.time:F2}s: 发现 Renderer: {r.gameObject.name} " +
                          $"| 启用: {r.enabled} " +
                          $"| 激活: {r.gameObject.activeInHierarchy} " +
                          $"| Layer: {LayerMask.LayerToName(r.gameObject.layer)}");
                
                if (!r.enabled || !r.gameObject.activeInHierarchy)
                {
                    Debug.LogWarning($"[Visibility Debug] {r.gameObject.name} 被禁用了！请检查是谁调用的 SetActive(false) 或 .enabled = false");
                }
            }
            _hasReportedRendererCount = true;
        }

        var p = GetComponent<Player>();
        if (p != null && p.skin != null)
        {
            if (p.skin.localScale.sqrMagnitude < 0.001f)
            {
                Debug.LogError($"[Visibility Debug] {Time.time:F2}s: Skin {p.skin.name} 的缩放变为 0 了！");
            }
        }
    }
}

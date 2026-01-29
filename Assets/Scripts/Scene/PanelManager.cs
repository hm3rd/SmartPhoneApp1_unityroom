using UnityEngine;

public class PanelManager : MonoBehaviour
{
    public GameObject targetPanel; // Inspectorで表示/非表示したいパネルをアサイン

    private static readonly System.Collections.Generic.List<GameObject> registeredPanels = new System.Collections.Generic.List<GameObject>();

    void Awake()
    {
        if (targetPanel != null && !registeredPanels.Contains(targetPanel))
            registeredPanels.Add(targetPanel);
    }

    void Start()
    {
        if (targetPanel != null)
            targetPanel.SetActive(false); // 初期状態ではパネルを非表示にする
    }

    void OnDestroy()
    {
        registeredPanels.Remove(targetPanel);
    }
    // 表示ボタンのOnClickに登録
    public void OpenPanel()
    {
        if (targetPanel != null)
            targetPanel.SetActive(true);
    }

    // 非表示ボタンのOnClickに登録
    public void ClosePanel()
    {
        if (targetPanel != null)
            targetPanel.SetActive(false);
    }

    // すべてのパネルをまとめて非表示にするためのボタン用ハンドラー
    public void CloseAllPanels()
    {
        for (int i = registeredPanels.Count - 1; i >= 0; i--)
        {
            var panel = registeredPanels[i];
            if (panel != null && panel.activeSelf)
                panel.SetActive(false);
        }
    }
}

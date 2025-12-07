using UnityEngine;

public class PanelManager : MonoBehaviour
{
    public GameObject targetPanel; // Inspectorで表示/非表示したいパネルをアサイン

    void Start()
    {
        if (targetPanel != null)
            targetPanel.SetActive(false); // 初期状態ではパネルを非表示にする
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
}

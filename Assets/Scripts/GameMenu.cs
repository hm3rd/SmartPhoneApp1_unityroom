using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenu : MonoBehaviour
{
    public GameObject menuPanel; // InspectorでPanelをアサイン
    public GameObject gameOverPanel; // Inspectorでパネルをアサイン

    public void Start()
    {
        if (menuPanel != null)
            menuPanel.SetActive(false); // 初期状態ではメニューを非表示にする
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false); // 初期状態ではゲームオーバーパネルを非表示にする
    }

    // メニューを開く
    public void OpenMenu()
    {
        if (menuPanel != null)
            menuPanel.SetActive(true);
        Time.timeScale = 0f; // ゲームを停止
    }

    // メニューを閉じる
    public void CloseMenu()
    {
        if (menuPanel != null)
            menuPanel.SetActive(false);
        Time.timeScale = 1f; // ゲームを再開
    }

    // ゲームオーバーを表示
    public void ShowGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        Time.timeScale = 0f; // ゲーム停止（必要なら）
    }

}

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// シンプルなシーン遷移スクリプト
/// UIボタンから直接呼び出して使用します
/// </summary>
public class LoadScene : MonoBehaviour
{
    [Header("移動先シーン名")]
    [Tooltip("このボタンで移動するシーン名を設定")]
    public string targetSceneName = "";

    [Header("ステージ選択用（任意）")]
    [Tooltip("ステージ番号を保存する場合のみ設定（-1で無効）")]
    public int stageIndex = -1;

    /// <summary>
    /// シーンを読み込む（UIボタンのOnClickから呼ぶ）
    /// </summary>
    public void LoadTargetScene()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogWarning("targetSceneNameが設定されていません");
            return;
        }

        if (SceneManager.GetActiveScene().name == "GameScene" && targetSceneName == "HomeScene")
        {
            PlayerPrefs.DeleteKey("ShowPreparationPanel");
            PlayerPrefs.DeleteKey("ReturnSceneName");
            PlayerPrefs.DeleteKey("CurrentSelectingSlot");
            PlayerPrefs.DeleteKey("PreparingStageIndex");
            PlayerPrefs.Save();
        }

        // ステージ番号が指定されていれば保存
        if (stageIndex >= 0)
        {
            PlayerPrefs.SetInt("SelectedStageIndex", stageIndex);
            PlayerPrefs.Save();
        }

        // 時間を戻す（ポーズ解除）
        Time.timeScale = 1f;

        // シーン遷移
        SceneManager.LoadScene(targetSceneName);
    }
}

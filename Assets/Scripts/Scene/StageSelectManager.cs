using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ステージ選択と準備画面を管理
/// </summary>
public class StageSelectManager : MonoBehaviour
{
    private const string PreparingStageIndexKey = "PreparingStageIndex";

    [Header("準備画面")]
    [Tooltip("ステージ選択後に表示するパネル")]
    public GameObject preparationPanel;
    
    [Tooltip("準備完了ボタン")]
    public Button readyButton;
    
    [Tooltip("ステージ情報表示テキスト（任意）")]
    public Text stageInfoText;
    
    // 現在選択されているステージ番号
    private int selectedStageIndex = -1;
    
    void Start()
    {
        bool isReturningFromCharacterSelect = HomeScenePanelState.HasSavedState;

        if (isReturningFromCharacterSelect &&
            HomeScenePanelState.IsPreparationPanelActive &&
            preparationPanel != null)
        {
            preparationPanel.SetActive(true);
        }

        if (isReturningFromCharacterSelect)
        {
            selectedStageIndex = PlayerPrefs.GetInt(PreparingStageIndexKey, -1);
            UpdateStageInfo();
        }
        
    }
    
    /// <summary>
    /// ステージボタンが押された時（UIボタンのOnClickから呼ぶ）
    /// </summary>
    public void SelectStage(int stageNumber)
    {
        selectedStageIndex = stageNumber;
        PlayerPrefs.SetInt(PreparingStageIndexKey, selectedStageIndex);
        PlayerPrefs.Save();
        Debug.Log($"ステージ {selectedStageIndex} を選択しました");
        
        // 準備画面を表示
        if (preparationPanel != null)
        {
            preparationPanel.SetActive(true);
        }
        
        // ステージ情報を表示
        UpdateStageInfo();
    }
    
    /// <summary>
    /// 準備完了ボタンが押された時
    /// </summary>
    void OnReadyButtonClicked()
    {
        if (selectedStageIndex < 0)
        {
            Debug.LogWarning("ステージが選択されていません");
            return;
        }
        
        Debug.Log($"ステージ {selectedStageIndex} で出撃開始");
        
        // PlayerPrefs に保存（NewStageManager で読み込む）
        PlayerPrefs.SetInt("SelectedStageIndex", selectedStageIndex);
        PlayerPrefs.DeleteKey(PreparingStageIndexKey);
        PlayerPrefs.Save();
        
        // ゲームシーンへ遷移
        LoadScene gameSceneLoader = gameObject.AddComponent<LoadScene>();
        gameSceneLoader.targetSceneName = "GameScene";
        gameSceneLoader.stageIndex = selectedStageIndex;
        gameSceneLoader.LoadTargetScene();
    }

    private void UpdateStageInfo()
    {
        if (stageInfoText != null && selectedStageIndex >= 0)
        {
            stageInfoText.text = $"ステージ {selectedStageIndex + 1}";
        }
    }
}

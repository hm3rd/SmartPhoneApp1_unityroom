using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    // ボタンから呼び出す
    public void GoToHomeScene()
    {
        // "GameScene" を実際のゲームシーン名に変更してください
        SceneManager.LoadScene("HomeScene");
    }

    public void GoToGameScene()
    {
        // "GameScene" を実際のゲームシーン名に変更してください
        SceneManager.LoadScene("GameScene");
        Time.timeScale = 1f; //ゲームオーバーから再開する場合、時間を進める
    }

    // プレイシーンの名前（NewStageManager が存在するシーン）
    [SerializeField] private string playSceneName = "PlayScene";

    // 選択したステージ番号を保持する（他スクリプトと共有するために保存）
    private int selectedStageIndex;

    // 任意のステージを選択（ボタンから呼ばれる）
    public void SelectStage(int index)
    {
        selectedStageIndex = index;

        // ステージ番号を保存（NewStageManager が Start() で取得できるように）
        PlayerPrefs.SetInt("SelectedStageIndex", selectedStageIndex);
        PlayerPrefs.Save();

        // ステージ実行用のシーンに遷移
        SceneManager.LoadScene(playSceneName);
    }

}
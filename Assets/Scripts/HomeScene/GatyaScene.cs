using UnityEngine;
using UnityEngine.UI;

public class GatyaScene : MonoBehaviour
{
    public GameObject gatyaPanel;      // InspectorでGatyaPanelをアサイン
    public Text resultText;            // ガチャ結果表示用Text

    void Start()
    {
        if (gatyaPanel != null)
            gatyaPanel.SetActive(false); // 初期状態ではガチャパネルを非表示にする
        if (resultText != null)
            resultText.text = ""; // 初期状態では結果テキストを空にする
    }

    // ガチャパネルを開く
    public void OpenGatya()
    {
        gatyaPanel.SetActive(true);
        if (resultText != null)
            resultText.text = "";
    }

    // ガチャパネルを閉じてホーム状態に戻る
    public void CloseGatya()
    {
        gatyaPanel.SetActive(false);
    }

    // ガチャを回す
    public void RollGatya()
    {
        int result = Random.Range(1, 4);
        if (resultText != null)
        {
            if (result == 1) resultText.text = "★レアキャラGET！";
            else if (result == 2) resultText.text = "★アイテムGET！";
            else resultText.text = "★はずれ…";
        }
    }

    // 10連ガチャを回す
    public void RollGatya10()
    {
        string result = "10連ガチャ結果：\n";
        for (int i = 0; i < 10; i++)
        {
            int num;
            int rand = Random.Range(0, 100); // 0～99

            if (rand < 16) num = 1;
            else if (rand < 32) num = 2;
            else if (rand < 48) num = 3;
            else if (rand < 64) num = 4;
            else if (rand < 80) num = 5;
            else if (rand < 96) num = 6;
            else num = 7; // 96～99（4%）

            result += num.ToString();
            if (i < 9) result += ", ";
        }
        if (resultText != null)
            resultText.text = result;
    }
}

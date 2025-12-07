//ItemPanelでのキャラ、武器、装備のパネルの切り替えのためのスクリプト

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class PanelBinding
{
    public string panelName;           // パネル名（ヒエラルキー上の名前）
    public Button triggerButton;       // 対応ボタン
    public bool showOnStart = false;   // 最初から表示するかどうか
}


public class MainPanelManager : MonoBehaviour
{
    public List<PanelBinding> panelBindings = new List<PanelBinding>();
    private Dictionary<string, GameObject> panelLookup = new Dictionary<string, GameObject>();

    void Start()
    {
        // 全オブジェクトの登録
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (var obj in allObjects)
        {
            panelLookup[obj.name] = obj;
        }

        // 各バインディングに対して初期化とリスナー登録
        foreach (var binding in panelBindings)
        {
            if (binding.triggerButton != null && panelLookup.ContainsKey(binding.panelName))
            {
                var panelObj = panelLookup[binding.panelName];

                // 初期表示設定
                panelObj.SetActive(binding.showOnStart);

                binding.triggerButton.onClick.AddListener(() =>
                {
                    // 表示されていない場合だけ表示
                    if (!panelObj.activeSelf)
                    {
                        panelObj.SetActive(true);
                    }

                    // 常に前面に出す
                    panelObj.transform.SetAsLastSibling();
                });
            }
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 石の現在数をUGUI TextまたはTMPへ表示する。
/// </summary>
public class StoneAmountDisplay : MonoBehaviour
{
    [SerializeField] private Text legacyText;
    [SerializeField] private TMP_Text tmpText;
    [SerializeField] private string prefix = "";

    private void Awake()
    {
        if (legacyText == null) legacyText = GetComponent<Text>();
        if (tmpText == null) tmpText = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        PlayerStoneWallet.AmountChanged += Refresh;
        Refresh(PlayerStoneWallet.Amount);
    }

    private void OnDisable()
    {
        PlayerStoneWallet.AmountChanged -= Refresh;
    }

    public void Refresh(int amount)
    {
        string value = $"{prefix}{amount}";
        if (legacyText != null) legacyText.text = value;
        if (tmpText != null) tmpText.text = value;
    }
}

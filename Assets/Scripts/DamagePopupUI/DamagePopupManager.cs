using UnityEngine;
using TMPro;

public class DamagePopupManager : MonoBehaviour
{
    [Header("プレハブ設定")]
    public GameObject damagePopupPrefab; // DamagePopup スクリプトを持つUI Prefab

    [Header("Canvas設定")]
    public Canvas targetCanvas; // ダメージ表示用Canvas（World Space推奨）

    [Header("表示設定")]
    public Color normalDamageColor = Color.red;
    public Color criticalDamageColor = Color.yellow;
    public Vector3 offsetPosition = new Vector3(0, 0.5f, 0); // 敵の位置からのオフセット

    private static DamagePopupManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Canvasが未設定の場合、自動的に作成
        if (targetCanvas == null)
        {
            targetCanvas = GetComponentInChildren<Canvas>();
        }
    }

    public static void ShowDamage(int damage, Vector3 worldPosition, bool isCritical = false)
    {
        if (instance == null)
        {
            Debug.LogWarning("DamagePopupManager instance not found!");
            return;
        }

        instance.CreatePopup(damage, worldPosition, isCritical);
    }

    private void CreatePopup(int damage, Vector3 worldPosition, bool isCritical)
    {
        if (damagePopupPrefab == null)
        {
            Debug.LogWarning("DamagePopup prefab not assigned!");
            return;
        }

        if (targetCanvas == null)
        {
            Debug.LogWarning("Target canvas not assigned!");
            return;
        }

        // プレハブを生成
        GameObject popupObj = Instantiate(damagePopupPrefab, targetCanvas.transform);
        
        // ワールド座標にオフセットを加える
        Vector3 spawnPosition = worldPosition + offsetPosition;

        // Canvas の Render Mode に応じて座標変換
        if (targetCanvas.renderMode == RenderMode.WorldSpace)
        {
            popupObj.transform.position = spawnPosition;
        }
        else if (targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // ワールド座標をスクリーン座標に変換
            Vector3 screenPos = Camera.main.WorldToScreenPoint(spawnPosition);
            popupObj.transform.position = screenPos;
        }
        else if (targetCanvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(spawnPosition);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                targetCanvas.transform as RectTransform,
                screenPos,
                targetCanvas.worldCamera,
                out Vector2 localPos
            );
            popupObj.transform.localPosition = localPos;
        }

        // DamagePopup の初期化
        DamagePopup popup = popupObj.GetComponent<DamagePopup>();
        if (popup != null)
        {
            Color damageColor = isCritical ? criticalDamageColor : normalDamageColor;
            popup.Initialize(damage, popupObj.transform.position, damageColor);
        }
    }
}

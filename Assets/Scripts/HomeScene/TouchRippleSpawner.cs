using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 画面タッチ/クリック位置にUIリップル(円形エフェクト)を生成するスポナー。
/// Canvas(スクリーンスペース)配下で使う想定。
/// </summary>
public class TouchRippleSpawner : MonoBehaviour
{
    [Header("生成先(通常はCanvasやその子のRectTransform)")]
    public RectTransform spawnRoot;

    [Header("リップルの描画レイヤー(任意)")]
    [Tooltip("ここに指定があれば、このRectTransform配下に生成します。パネルより前面に置いた空のオブジェクトを割り当てると確実に前面に出せます。")]
    public RectTransform rippleLayer;

    [Header("リップルPrefab (UI Image など)")]
    public GameObject ripplePrefab;

    [Header("UIカメラ (Screen Space - Camera の場合のみ設定)")]
    public Camera uiCamera;

    [Header("連打防止間隔(秒)")]
    public float minInterval = 0.03f;

    private float _cooldown = 0f;

    [Header("デバッグ")]
    public bool debugLogPositions = false;

    void Awake()
    {
        if (spawnRoot == null)
        {
            // 自身にRectTransformがあればそれを使う
            spawnRoot = GetComponent<RectTransform>();
        }

        // Canvas/カメラを自動検出
        var canvas = spawnRoot != null ? spawnRoot.GetComponentInParent<Canvas>() : null;
        if (canvas != null)
        {
            switch (canvas.renderMode)
            {
                case RenderMode.ScreenSpaceOverlay:
                    uiCamera = null; // Overlayは常にnull
                    break;
                case RenderMode.ScreenSpaceCamera:
                case RenderMode.WorldSpace:
                    // 基本はCanvasに設定されたカメラを使う
                    uiCamera = canvas.worldCamera;
                    if (uiCamera == null)
                    {
                        // 未設定の場合はMainを試す（正確ではないことを警告）
                        uiCamera = Camera.main;
                        if (debugLogPositions)
                        {
                            Debug.LogWarning("[TouchRippleSpawner] Canvas.worldCamera が未設定です。Camera.mainを使用します。");
                        }
                    }
                    break;
            }
        }
    }

    void Update()
    {
        if (_cooldown > 0f) _cooldown -= Time.deltaTime;

        // タッチ入力（複数対応）
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                var t = Input.GetTouch(i);
                if (t.phase == TouchPhase.Began)
                {
                    TrySpawnAtScreenPosition(t.position);
                }
            }
        }
        else
        {
            // マウス/エディタ用
            if (Input.GetMouseButtonDown(0))
            {
                TrySpawnAtScreenPosition(Input.mousePosition);
            }
        }
    }

    private void TrySpawnAtScreenPosition(Vector2 screenPos)
    {
        if (_cooldown > 0f) return;
        if (ripplePrefab == null || spawnRoot == null) return;

        // まずはローカル座標への変換（Overlay/ScreenSpaceCamera/WorldSpaceで有効）
        // 生成先の親（rippleLayer優先）
        var parentForSpawn = (rippleLayer != null) ? rippleLayer : spawnRoot;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentForSpawn, screenPos, uiCamera, out var localPos))
        {
            var go = Instantiate(ripplePrefab, parentForSpawn);
            var rt = go.transform as RectTransform;
            if (rt != null)
            {
                rt.anchoredPosition = localPos;
                rt.localScale = Vector3.one; // 見た目の拡大はRippleEffect側で制御
                // パネルより前面に出す
                go.transform.SetAsLastSibling();
                if (debugLogPositions)
                {
                    Debug.Log($"[TouchRippleSpawner] Local spawn pos: {localPos}");
                }
            }
            else if (debugLogPositions)
            {
                Debug.LogWarning("[TouchRippleSpawner] ripplePrefabにRectTransformがありません（UIプレハブを使用してください）。");
            }
            MakeNonBlocking(go);
            _cooldown = minInterval;
            return;
        }

        // フォールバック: ワールド座標への変換（World Space Canvas等）
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(parentForSpawn, screenPos, uiCamera, out var worldPos))
        {
            var go = Instantiate(ripplePrefab, parentForSpawn);
            var rt = go.transform as RectTransform;
            if (rt != null)
            {
                rt.position = worldPos;
                rt.localScale = Vector3.one;
                go.transform.SetAsLastSibling();
                if (debugLogPositions)
                {
                    Debug.Log($"[TouchRippleSpawner] World spawn pos (RectTransform): {worldPos}");
                }
            }
            else
            {
                // 非UIプレハブの場合でも位置を反映
                go.transform.position = worldPos;
                go.transform.localScale = Vector3.one;
                go.transform.SetAsLastSibling();
                if (debugLogPositions)
                {
                    Debug.Log($"[TouchRippleSpawner] World spawn pos (Transform): {worldPos}");
                }
            }
            MakeNonBlocking(go);
            _cooldown = minInterval;
            return;
        }

        if (debugLogPositions)
        {
            Debug.LogWarning("[TouchRippleSpawner] 位置変換に失敗しました。Canvas/Camera 設定とspawnRootを確認してください。");
        }
    }

    // 生成したリップルがUI入力をブロックしないようにする
    private void MakeNonBlocking(GameObject go)
    {
        if (go == null) return;
        var graphics = go.GetComponentsInChildren<UnityEngine.UI.Graphic>(true);
        foreach (var g in graphics)
        {
            g.raycastTarget = false;
        }
        var cg = go.GetComponent<UnityEngine.CanvasGroup>();
        if (cg == null) cg = go.AddComponent<UnityEngine.CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.interactable = false;
    }
}

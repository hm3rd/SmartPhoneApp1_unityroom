using UnityEngine;

public class MoveUI : MonoBehaviour
{
    public GameObject ringPrefab; // 円環のプレハブ
    public GameObject knobPrefab; // ノブ（つまみ）のプレハブ
    public float fixedScale = 1.0f; // 円環の固定スケール
    public float knobRange = 1.0f;  // ノブが動ける最大距離

    private Camera mainCamera;
    private GameObject ringInstance;
    private GameObject knobInstance;
    private Vector2 startTouchPos;
    public Vector2 inputDir; // 移動入力（-1～1）

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // 画面の左半分のみUIを表示
            if (touch.position.x < Screen.width / 2)
            {
                if (touch.phase == TouchPhase.Began && ringInstance == null)
                {
                    // タッチ開始位置に円環とノブを生成
                    startTouchPos = mainCamera.ScreenToWorldPoint(touch.position);
                    ringInstance = Instantiate(ringPrefab, startTouchPos, Quaternion.identity, this.transform);
                    ringInstance.transform.localScale = new Vector2(fixedScale, fixedScale);

                    knobInstance = Instantiate(knobPrefab, startTouchPos, Quaternion.identity, this.transform);
                    knobInstance.transform.localScale = new Vector2(fixedScale * 1f, fixedScale * 1f);//fixedScale：ノブのサイズ
                }
                else if ((touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary) && ringInstance != null)
                {
                    // 指の移動方向・距離を計算
                    Vector2 nowPos = mainCamera.ScreenToWorldPoint(touch.position);
                    Vector2 diff = nowPos - startTouchPos;
                    float len = Mathf.Min(diff.magnitude, knobRange);
                    Vector2 dir = diff.normalized * len;

                    // ノブを動かす
                    knobInstance.transform.position = (Vector2)ringInstance.transform.position + dir;

                    // 入力値（-1～1）を計算
                    inputDir = dir / knobRange;
                }
                else if ((touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) && ringInstance != null)
                {
                    // 終了時に削除
                    Destroy(ringInstance);
                    Destroy(knobInstance);
                    ringInstance = null;
                    knobInstance = null;
                    inputDir = Vector2.zero;
                }
            }
            else
            {
                // 右半分をタッチしたらUIを消す
                if (ringInstance != null)
                {
                    Destroy(ringInstance);
                    Destroy(knobInstance);
                    ringInstance = null;
                    knobInstance = null;
                    inputDir = Vector2.zero;
                }
            }
        }
        else
        {
            // タッチがない場合もUIを消す
            if (ringInstance != null)
            {
                Destroy(ringInstance);
                Destroy(knobInstance);
                ringInstance = null;
                knobInstance = null;
                inputDir = Vector2.zero;
            }
        }
    }
}
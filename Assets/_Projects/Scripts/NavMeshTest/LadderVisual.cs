using Unity.AI.Navigation;
using UnityEngine;

/// <summary>
/// 梯子の見た目を管理するクラス。
/// </summary>
public class LadderVisual : MonoBehaviour
{
    // 元のローカルスケール
    private Vector3 initialScale;

    // 元のワールド空間での高さ
    private float initialWorldHeight;

    // Rendererの元の下端位置
    private float initialBottomY;

    // Rendererを取得しているか
    private bool isInitialized;

    private NavMeshLink navMeshLink;

    // 初期状態を保存する。
    private void Awake()
    {
        Initialize();

        navMeshLink = GetComponent<NavMeshLink>();

        if (navMeshLink != null)
        {
            navMeshLink.enabled = false;
        }
    }

    // 元のスケールと高さを保存する。
    private void Initialize()
    {
        if (isInitialized) return;

        // 梯子の元のスケールを保存する
        initialScale = transform.localScale;

        // 子オブジェクトも含めてRendererを取得する
        Renderer renderer = GetComponentInChildren<Renderer>();

        if (renderer == null)
        {
            Debug.LogError($"{name}: 梯子にRendererがありません。", this);

            return;
        }

        // 元のワールド空間での高さを保存する
        initialWorldHeight = renderer.bounds.size.y;

        // 元の下端位置を保存する
        initialBottomY = renderer.bounds.min.y;

        if (initialWorldHeight <= 0.0001f)
        {
            Debug.LogError($"{name}: 梯子の元の高さが0に近すぎます。", this);

            return;
        }

        isInitialized = true;
    }


    /// <summary>
    /// 梯子の高さをワールド単位で設定する。
    /// </summary>
    /// <param name="targetHeight">目標とする高さ</param>
    public void SetHeight(float targetHeight)
    {
        // 初期化に失敗している場合は処理しない
        if (!isInitialized) return;

        // 高さがマイナスにならないようにする
        targetHeight = Mathf.Max(0f, targetHeight);

        // 元の高さに対する倍率を計算する
        float scaleMultiplier = targetHeight / initialWorldHeight;

        // 毎回、元のスケールを基準に計算する
        Vector3 newScale = initialScale;

        newScale.y = initialScale.y * scaleMultiplier;

        // 梯子のスケールを変更する
        transform.localScale = newScale;

        // スケール変更後のRendererを取得する
        Renderer renderer = GetComponentInChildren<Renderer>();

        if (renderer == null) return;

        // 下端の位置を確認する
        float currentBottomY = renderer.bounds.min.y;

        // スケール変更によって動いた下端を元の位置に戻す
        float bottomOffset = initialBottomY - currentBottomY;

        transform.position += Vector3.up * bottomOffset;

        // NavMeshLinkの終点を更新する
        UpdateNavMeshLink(targetHeight);
    }

    /// <summary>
    /// はしごの高さに合わせてNavMeshLinkを更新する(Y座標だけ)
    /// </summary>
    public void UpdateNavMeshLink(float height)
    {
        if (navMeshLink == null) return;

        // 現在の始点・終点を取得する
        Vector3 start = navMeshLink.startPoint;
        Vector3 end = navMeshLink.endPoint;

        // Y座標だけ更新する
        start.y = 0f;
        end.y = height;

        // 更新した座標を設定する
        navMeshLink.startPoint = start;
        navMeshLink.endPoint = end;
    }

    /// <summary>
    /// はしご完成時にリンクを有効化する
    /// </summary>
    public void CompleteLadder()
    {
        if (navMeshLink == null) return;

        navMeshLink.enabled = true;
    }
}

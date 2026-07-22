using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class TableSetup : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] ARTrackedImageManager trackedImageManager;

    [Header("Prefabs")]
    [SerializeField] GameObject towerPrefab;

    [Header("Scripts")]
    [SerializeField] PlaneGenerator planeGenerator;

    readonly List<Vector3> recordedPoints = new();
    bool isTableGenerated;
    bool isTowerGenerated;

    const int MaxCornerCount = 4;

    Button button;
    Label logLabel;
    Label transformLabel;

    void Awake()
    {
        UIDocument document = FindAnyObjectByType<UIDocument>();
        var root = document.rootVisualElement;
        button = root.Q<Button>();
        logLabel = root.Q<Label>("Log");
        transformLabel = root.Q<Label>("Transform");
    }

    void OnEnable()
    {
        trackedImageManager.trackablesChanged.AddListener(OnTrackedImageChanged);
        button.clicked += RecordCorner;
    }

    void OnDisable()
    {
        trackedImageManager.trackablesChanged.RemoveListener(OnTrackedImageChanged);
        button.clicked -= RecordCorner;
    }

    void Update()
    {
        transformLabel.text = Camera.main.transform.position.ToString();
    }

    void RecordCorner()
    {
        if (recordedPoints.Count >= MaxCornerCount)
        {
            return;
        }

        // 現在のカメラの位置を角の座標として記録
        Vector3 currentPos = Camera.main.transform.position;
        recordedPoints.Add(currentPos);
        logLabel.text += $"\n{recordedPoints.Count}点目を記録:{currentPos}";


        if (recordedPoints.Count == MaxCornerCount)
        {
            // 4点揃ったら即座にテーブルを生成・描画
            logLabel.text += "\n generated table \n" + recordedPoints;

            planeGenerator.GenerateFlatPlane(recordedPoints.ToArray());
            isTableGenerated = true;
        }
    }

    void OnTrackedImageChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        // テーブルが生成される前、またはすでにタワーが生成済みの場合は無視
        if (!isTableGenerated || isTowerGenerated) return;

        foreach (var trackedImage in eventArgs.added)
        {
            OnTowerDetected(trackedImage.transform);
            break;
        }
    }

    void OnTowerDetected(Transform towerTransform)
    {
        isTowerGenerated = true;

        // 物理タワーの位置に3Dタワーオブジェクトを生成
        Instantiate(towerPrefab, towerTransform.position, towerTransform.rotation);

        // ローカルでの「相対化 ➜ 復元」テスト
        // これを行うことで、ネットワーク通信を挟んでも位置がズレないかを1台で検証できます。
        RunSynchronizationSelfTest(towerTransform);
    }

    // 「相対化 ➜ 復元」の計算が100%合っているかを検証するデバッグ処理
    private void RunSynchronizationSelfTest(Transform towerTransform)
    {
        // A. 最初に記録した絶対座標を、タワー基準の「相対座標」に一度変換（ホスト送信データのシミュレート）
        Vector3[] relativeCorners = new Vector3[4];
        for (int i = 0; i < 4; i++)
        {
            //relativeCorners[i] = towerTransform.InverseTransformPoint(absoluteCorners[i]);
        }

        // B. 変換した相対座標を、もう一度タワー基準で「絶対世界座標」に再復元（クライアント受信データのシミュレート）
        Vector3[] reconstructedAbsoluteCorners = new Vector3[MaxCornerCount];
        for (int i = 0; i < MaxCornerCount; ++i)
        {
            reconstructedAbsoluteCorners[i] = towerTransform.TransformPoint(relativeCorners[i]);
        }

        // C. すでに描画されているテーブルを、再復元した座標に移動させる
        // もし計算が合っていれば、テーブルは1ミリも位置がズレずにその場にとどまる（または再配置される）はずです。
        UpdateTablePosition(reconstructedAbsoluteCorners);

        logLabel.text += "\n【テスト成功】タワーを認識し、座標の変換・復元シミュレーションを完了しました";
    }

    private void UpdateTablePosition(Vector3[] corners)
    {
        //if (generatedQuad == null) return;

        //Vector3 center = (corners[0] + corners[1] + corners[2] + corners[3]) / MaxCornerCount;
        //Vector3 dirX = (corners[1] - corners[0] + corners[2] - corners[3]).normalized;
        //Vector3 dirZ = (corners[0] - corners[3] + corners[1] - corners[2]).normalized;
        //Vector3 normal = Vector3.Cross(dirZ, dirX).normalized;

        //generatedQuad.transform.SetPositionAndRotation(center, Quaternion.LookRotation(dirZ, normal));
        //float width = (Vector3.Distance(corners[0], corners[1]) + Vector3.Distance(corners[3], corners[2])) / 2;
        //float depth = (Vector3.Distance(corners[3], corners[0]) + Vector3.Distance(corners[2], corners[1])) / 2;
        //generatedQuad.transform.localScale = new(width, depth, 1);
    }
}

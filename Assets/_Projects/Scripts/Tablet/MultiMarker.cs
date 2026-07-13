using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class MultiMarker : MonoBehaviour
{
    [SerializeField] GameObject[] prefabs;
    [SerializeField] ARTrackedImageManager imageManager;

    readonly Dictionary<string, GameObject> markerAndPrefabDict = new();
    readonly Dictionary<TrackableId, GameObject> spawnedObjects = new();

    Label label;

    void OnEnable()
    {
        imageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
    }

    void OnDisable()
    {
        imageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
    }

    void Awake()
    {
        if (imageManager.referenceLibrary.count != prefabs.Length)
        {
            Debug.LogError(
                $"ReferenceImage数({imageManager.referenceLibrary.count}) と " +
                $"Prefab数({prefabs.Length}) が一致していません"
            );
        }
        for (int i = 0; i < prefabs.Length; ++i)
        {
            string markerName = imageManager.referenceLibrary[i].name;

            if (!markerAndPrefabDict.TryAdd(markerName, prefabs[i]))
            {
                Debug.LogWarning($"重複したマーカー名です : {markerName}");
            }
        }

        UIDocument ui = FindAnyObjectByType<UIDocument>();
        var root = ui.rootVisualElement;
        label = root.Q<Label>();

        label.style.display = DisplayStyle.None;
    }

    // 画像マーカーの状態が変化したときに自動で実行されるメソッドです
    void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        // 新しくマーカー画像がカメラに映ったとき
        foreach (var newImage in eventArgs.added)
        {
            // 追加処理
            SpawnImage(newImage);
        }

        // マーカー画像が移動したとき、または状態が変わったとき
        foreach (var updatedImage in eventArgs.updated)
        {
            // 更新処理
            UpdateImage(updatedImage);
        }

        // マーカー画像が完全に画面から見失われた（削除された）とき
        foreach (var removedImage in eventArgs.removed)
        {
            // 削除処理
            RemoveImage(removedImage.Key);
        }
    }

    void SpawnImage(ARTrackedImage trackedImage)
    {
        string markerName = trackedImage.referenceImage.name;
        TrackableId id = trackedImage.trackableId;

        // 認識した画像マーカーの名前を使って辞書から任意のオブジェクトを引っ張り出す
        if (markerAndPrefabDict.TryGetValue(markerName, out GameObject arObject))
        {
            // 位置合わせの後、マーカーの子オブジェクトとして生成
            GameObject spawned = Instantiate(arObject, trackedImage.transform);

            // 後で表示/非表示を切り替えられるように辞書に保存しておきます
            spawnedObjects.Add(id, spawned);
        }
    }

    void UpdateImage(ARTrackedImage trackedImage)
    {
        TrackableId id = trackedImage.trackableId;

        if (spawnedObjects.TryGetValue(id, out GameObject spawnedObject))
        {
            // マーカーがカメラにしっかり映っている時（Tracking）だけ表示します
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                spawnedObject.SetActive(true);
                label.style.display = DisplayStyle.Flex;
            }
            else
            {
                // カメラから外れたり、手で隠れた時は非表示にします
                spawnedObject.SetActive(false);
                label.style.display = DisplayStyle.None;
            }
        }
    }

    void RemoveImage(TrackableId trackedImageId)
    {
        if (spawnedObjects.TryGetValue(trackedImageId, out GameObject spawnedObject))
        {
            Destroy(spawnedObject);
            spawnedObjects.Remove(trackedImageId);
        }
    }
}

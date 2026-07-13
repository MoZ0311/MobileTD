using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARTrackedImageManager))]

public class ARImagePrefabSpawner : MonoBehaviour
{
    

    // 画像の名前と、それに対応させたい3Dオブジェクトのペアを定義する構造体です
    [System.Serializable]
    public struct ImagePrefabPair
    {
        [Tooltip("画像ライブラリで設定した画像の名前")]
        public string imageName;
        [Tooltip("この画像が見つかった時に出現させたい3Dオブジェクトのプレハブ")]
        public GameObject prefabToSpawn;
    }

    [Tooltip("マーカー画像の名前と、出現させたいプレハブのペアのリスト")]
    public List<ImagePrefabPair> imagePrefabPairs;

    private ARTrackedImageManager trackedImageManager;

    // すでに生成されたオブジェクトを管理するための辞書（ディクショナリ）
    private readonly Dictionary<string, GameObject> spawnedPrefabs = new();

    void Awake()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();

        
    }

    void OnEnable()
    {
        // 画面内の画像マーカーが「追加」「更新」「削除」されたときに呼ばれるイベントを登録します
        trackedImageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
    }

    void OnDisable()
    {
        // スクリプトが無効になったときはイベントの登録を解除します
        trackedImageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
    }

    // 画像マーカーの状態が変化したときに自動で実行されるメソッドです
    void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        // 1. 新しくマーカー画像がカメラに映ったとき
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            UpdateImage(trackedImage);
        }

        // 2. マーカー画像が移動したとき、または状態が変わったとき
        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            UpdateImage(trackedImage);
        }

        // 3. マーカー画像が完全に画面から見失われた（削除された）とき
        foreach (var removed in eventArgs.removed)
        {
            TrackableId removedImageTrackableId = removed.Key;
            ARTrackedImage removedImage = removed.Value;
        }
    }

    private void UpdateImage(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;

        // まだその画像用のオブジェクトが作られていない場合のみ、新しく作ります
        if (!spawnedPrefabs.ContainsKey(imageName))
        {
            foreach (var pair in imagePrefabPairs)
            {
                if (pair.imageName == imageName && pair.prefabToSpawn != null)
                {
                    // マーカーと同じ位置、同じ回転でオブジェクトを生成します
                    GameObject newObject = Instantiate(pair.prefabToSpawn, trackedImage.transform.position, trackedImage.transform.rotation);

                    // 生成したオブジェクトをマーカーの「子（Parent）」に設定することで、自動的にマーカーの動きに追従させます
                    newObject.transform.SetParent(trackedImage.transform);

                    spawnedPrefabs.Add(imageName, newObject);
                    break;
                }
            }
        }

        // マーカーの追跡状態に合わせて、表示・非表示を切り替えます
        if (spawnedPrefabs.ContainsKey(imageName))
        {
            GameObject spawnedObject = spawnedPrefabs[imageName];

            // マーカーがしっかりと見えている（Tracking）ときだけ表示します
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                spawnedObject.SetActive(true);
                
            }
            else
            {
                // 手で隠れたり、見失ったりしているときは非表示にして消します
                spawnedObject.SetActive(false);
                
            }
        }
    }
}

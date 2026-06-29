using UnityEngine;
using UnityEngine.UIElements;

public class PhoneUIController : MonoBehaviour
{
    [SerializeField] private SiegePlayer siegePlayer;

    private Label speedLabel;
    private Label headingLabel;

    private void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;    // UI ドキュメントのルート要素取得

        speedLabel = root.Q<Label>("SpeedLabel");
        headingLabel = root.Q<Label>("HeadingLabel");
    }

    private void Update()
    {
        speedLabel.text = $"速度 : {siegePlayer.CurrentSpeed:F2}";

        headingLabel.text = $"方角 : {siegePlayer.CurrentHeading:F0}";
    }
}

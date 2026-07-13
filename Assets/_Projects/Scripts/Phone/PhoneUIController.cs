using UnityEngine;
using UnityEngine.UIElements;

public class PhoneUIController : MonoBehaviour
{
    [SerializeField] private SiegePlayer siegePlayer;

    private Button clearButton;
    private Label logLabel;
    private Label maxSpeedLabel;
    private Label speedLabel;
    private Label headingLabel;
    private VisualElement gauge;

    private void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;    // UI ドキュメントのルート要素取得

        clearButton = root.Q<Button>("ClearButton");
        logLabel = root.Q<Label>("LogLabel");

        maxSpeedLabel = root.Q<Label>("MaxSpeedLabel");
        speedLabel = root.Q<Label>("SpeedLabel");
        headingLabel = root.Q<Label>("HeadingLabel");
        gauge = root.Q<VisualElement>("AccelerationGauge");

        clearButton.clicked += ClearLog;

        // 初期値
        gauge.style.flexGrow = 0;
    }

    private void Update()
    {
        maxSpeedLabel.text = "最大加速度 : " + siegePlayer.MaxAcceleration;

        speedLabel.text = "加速度 : " + siegePlayer.CurrentAcceleration;

        headingLabel.text = $"方角 : {siegePlayer.CurrentHeading:F0}";

        SetAcceleration();
    }

    public void SetAcceleration()
    {
        float rate = Mathf.Clamp01(siegePlayer.CurrentAcceleration / siegePlayer.crashAcceleration);

        gauge.style.flexGrow = rate;
    }

    // ログ追加
    public void AddLog(string message)
    {
        logLabel.text += message + "\n";
    }

    // ログリセット
    private void ClearLog()
    {
        siegePlayer.Respawn();
        logLabel.text = string.Empty;
    }
}

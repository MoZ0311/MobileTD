using UnityEngine;
using UnityEngine.UIElements;

public class TabletUIManager : MonoBehaviour
{
    [SerializeField] UIDocument tabletUI;
    Button leftButton;
    Button rightButton;
    const string LeftButton = "LeftButton";
    const string RightButton = "RightButton";

    const string isAttacking = "攻撃中";

    void Awake()
    {
        VisualElement root = tabletUI.rootVisualElement;
        leftButton = root.Q<Button>(LeftButton);
        rightButton = root.Q<Button>(RightButton);

        RegisterEvents();
    }

    void RegisterEvents()
    {
        leftButton.RegisterCallback<PointerDownEvent>(
            e => OnButtonDown(e, leftButton), CallbackOptions.TrickleDown
        );

        leftButton.RegisterCallback<PointerUpEvent>(
            e => OnButtonUp(e, leftButton), CallbackOptions.TrickleDown
        );

        rightButton.RegisterCallback<PointerDownEvent>(
            e => OnButtonDown(e, rightButton), CallbackOptions.TrickleDown
        );

        rightButton.RegisterCallback<PointerUpEvent>(
            e => OnButtonUp(e, rightButton), CallbackOptions.TrickleDown
        );
    }

    void OnDisable()
    {
        leftButton.UnregisterAllRemovableCallbacks();
        rightButton.UnregisterAllRemovableCallbacks();
    }

    void OnButtonDown(PointerDownEvent e, Button button)
    {
        button.text = isAttacking;
    }

    void OnButtonUp(PointerUpEvent e, Button button)
    {
        button.text = string.Empty;
    }
}

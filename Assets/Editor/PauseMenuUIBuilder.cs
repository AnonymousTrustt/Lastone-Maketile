using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class PauseMenuUIBuilder
{
    private const string RootName = "PauseMenuSystem";
    private const string GameSceneName = "SampleScene";
    private const string MainMenuSceneName = "MainMenu";

    [MenuItem("Tools/Threshold City 17/Create Pause Menu UI")]
    public static void CreatePauseMenuUI()
    {
        string activeSceneName = EditorSceneManager.GetActiveScene().name;

        if (activeSceneName == MainMenuSceneName)
        {
            Debug.LogWarning("Pause Menu UI belongs in the actual game scene, not MainMenu. Open Assets/Scenes/SampleScene.unity, then run this tool again.");
            return;
        }

        if (activeSceneName != GameSceneName)
        {
            Debug.LogWarning($"Pause Menu UI is intended for {GameSceneName}. Current scene is '{activeSceneName}'. Open Assets/Scenes/{GameSceneName}.unity before creating it.");
            return;
        }

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();

        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        EnsureEventSystem();

        Transform oldRoot = canvas.transform.Find(RootName);
        if (oldRoot != null)
        {
            Undo.DestroyObjectImmediate(oldRoot.gameObject);
        }

        Transform oldPauseButton = canvas.transform.Find("PauseButton");
        if (oldPauseButton != null)
        {
            Undo.DestroyObjectImmediate(oldPauseButton.gameObject);
        }

        GameObject root = CreateUIObject(RootName, canvas.transform);
        PauseMenuManager pauseMenuManager = root.AddComponent<PauseMenuManager>();

        Button pauseButton = CreateButton("PauseButton", canvas.transform, "II", new Vector2(84f, 64f), 24f);
        RectTransform pauseButtonRect = pauseButton.GetComponent<RectTransform>();
        pauseButtonRect.anchorMin = new Vector2(0f, 1f);
        pauseButtonRect.anchorMax = new Vector2(0f, 1f);
        pauseButtonRect.pivot = new Vector2(0f, 1f);
        pauseButtonRect.anchoredPosition = new Vector2(24f, -24f);

        GameObject pausePanel = CreatePanel("PausePanel", root.transform, new Color(0f, 0f, 0f, 0.72f));
        RectTransform pausePanelRect = pausePanel.GetComponent<RectTransform>();
        StretchToFill(pausePanelRect);

        TextMeshProUGUI titleText = CreateText("TitleText", pausePanel.transform, "Paused", 52f, TextAlignmentOptions.Center);
        RectTransform titleRect = titleText.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.sizeDelta = new Vector2(700f, 80f);
        titleRect.anchoredPosition = new Vector2(0f, -150f);

        GameObject buttonStack = CreateUIObject("ButtonStack", pausePanel.transform);
        RectTransform stackRect = buttonStack.GetComponent<RectTransform>();
        stackRect.anchorMin = new Vector2(0.5f, 0.5f);
        stackRect.anchorMax = new Vector2(0.5f, 0.5f);
        stackRect.pivot = new Vector2(0.5f, 0.5f);
        stackRect.sizeDelta = new Vector2(420f, 430f);
        stackRect.anchoredPosition = new Vector2(0f, -10f);

        VerticalLayoutGroup layout = buttonStack.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 16f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        Button resumeButton = CreateButton("ResumeButton", buttonStack.transform, "Resume", new Vector2(380f, 56f), 28f);
        Button audioToggleButton = CreateButton("AudioToggleButton", buttonStack.transform, "Audio: ON", new Vector2(380f, 56f), 28f);
        Button saveGameButton = CreateButton("SaveGameButton", buttonStack.transform, "Save Game", new Vector2(380f, 56f), 28f);
        Button restartButton = CreateButton("RestartButton", buttonStack.transform, "Restart", new Vector2(380f, 56f), 28f);
        Button quitToMenuButton = CreateButton("QuitToMenuButton", buttonStack.transform, "Quit To Menu", new Vector2(380f, 56f), 28f);

        TextMeshProUGUI saveConfirmationText = CreateText("SaveConfirmationText", pausePanel.transform, string.Empty, 24f, TextAlignmentOptions.Center);
        RectTransform saveConfirmationRect = saveConfirmationText.GetComponent<RectTransform>();
        saveConfirmationRect.anchorMin = new Vector2(0.5f, 0.5f);
        saveConfirmationRect.anchorMax = new Vector2(0.5f, 0.5f);
        saveConfirmationRect.pivot = new Vector2(0.5f, 0.5f);
        saveConfirmationRect.sizeDelta = new Vector2(500f, 44f);
        saveConfirmationRect.anchoredPosition = new Vector2(0f, -285f);
        saveConfirmationText.gameObject.SetActive(false);

        GameObject warningPanel = CreatePanel("WarningPanel", pausePanel.transform, new Color(0.03f, 0.05f, 0.08f, 0.96f));
        RectTransform warningRect = warningPanel.GetComponent<RectTransform>();
        warningRect.anchorMin = new Vector2(0.5f, 0.5f);
        warningRect.anchorMax = new Vector2(0.5f, 0.5f);
        warningRect.pivot = new Vector2(0.5f, 0.5f);
        warningRect.sizeDelta = new Vector2(720f, 290f);
        warningRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI warningText = CreateText("WarningText", warningPanel.transform, "Warning", 28f, TextAlignmentOptions.Center);
        RectTransform warningTextRect = warningText.GetComponent<RectTransform>();
        warningTextRect.anchorMin = new Vector2(0.5f, 1f);
        warningTextRect.anchorMax = new Vector2(0.5f, 1f);
        warningTextRect.pivot = new Vector2(0.5f, 1f);
        warningTextRect.sizeDelta = new Vector2(620f, 110f);
        warningTextRect.anchoredPosition = new Vector2(0f, -46f);

        Button confirmButton = CreateButton("ConfirmButton", warningPanel.transform, "Confirm", new Vector2(220f, 56f), 24f);
        RectTransform confirmRect = confirmButton.GetComponent<RectTransform>();
        confirmRect.anchorMin = new Vector2(0.5f, 0f);
        confirmRect.anchorMax = new Vector2(0.5f, 0f);
        confirmRect.pivot = new Vector2(0.5f, 0f);
        confirmRect.anchoredPosition = new Vector2(-130f, 44f);

        Button cancelButton = CreateButton("CancelButton", warningPanel.transform, "Cancel", new Vector2(220f, 56f), 24f);
        RectTransform cancelRect = cancelButton.GetComponent<RectTransform>();
        cancelRect.anchorMin = new Vector2(0.5f, 0f);
        cancelRect.anchorMax = new Vector2(0.5f, 0f);
        cancelRect.pivot = new Vector2(0.5f, 0f);
        cancelRect.anchoredPosition = new Vector2(130f, 44f);

        warningPanel.SetActive(false);
        pausePanel.SetActive(false);

        AssignReference(pauseMenuManager, "pauseButton", pauseButton);
        AssignReference(pauseMenuManager, "pausePanel", pausePanel);
        AssignReference(pauseMenuManager, "titleText", titleText);
        AssignReference(pauseMenuManager, "resumeButton", resumeButton);
        AssignReference(pauseMenuManager, "audioToggleButton", audioToggleButton);
        AssignReference(pauseMenuManager, "audioToggleButtonText", audioToggleButton.GetComponentInChildren<TextMeshProUGUI>());
        AssignReference(pauseMenuManager, "saveGameButton", saveGameButton);
        AssignReference(pauseMenuManager, "saveConfirmationText", saveConfirmationText);
        AssignReference(pauseMenuManager, "restartButton", restartButton);
        AssignReference(pauseMenuManager, "quitToMenuButton", quitToMenuButton);
        AssignReference(pauseMenuManager, "warningPanel", warningPanel);
        AssignReference(pauseMenuManager, "warningText", warningText);
        AssignReference(pauseMenuManager, "confirmButton", confirmButton);
        AssignReference(pauseMenuManager, "cancelButton", cancelButton);
        AssignReference(pauseMenuManager, "saveLoadManager", Object.FindFirstObjectByType<SaveLoadManager>());
        AssignReference(pauseMenuManager, "turnManager", Object.FindFirstObjectByType<TurnManager>());
        AssignReference(pauseMenuManager, "buildingPlacementManager", Object.FindFirstObjectByType<BuildingPlacementManager>());
        AssignReference(pauseMenuManager, "backgroundAudioManager", Object.FindFirstObjectByType<BackgroundAudioManager>());

        Selection.activeGameObject = root;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Created Threshold: City 17 Pause Menu UI. Press Play and click the top-left pause button to test it.");
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem));
        Undo.RegisterCreatedObjectUndo(eventSystem, "Create EventSystem");
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(gameObject, $"Create {name}");
        gameObject.transform.SetParent(parent, false);
        return gameObject;
    }

    private static GameObject CreatePanel(string name, Transform parent, Color color)
    {
        GameObject panel = CreateUIObject(name, parent);
        Image image = panel.AddComponent<Image>();
        image.color = color;
        return panel;
    }

    private static Button CreateButton(string name, Transform parent, string label, Vector2 size, float fontSize)
    {
        GameObject buttonObject = CreatePanel(name, parent, new Color(0.02f, 0.03f, 0.05f, 0.95f));
        RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = size;

        LayoutElement layoutElement = buttonObject.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = size.x;
        layoutElement.preferredHeight = size.y;

        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.02f, 0.03f, 0.05f, 0.95f);
        colors.highlightedColor = new Color(0.12f, 0.18f, 0.25f, 0.98f);
        colors.pressedColor = new Color(0.18f, 0.28f, 0.38f, 1f);
        button.colors = colors;

        TextMeshProUGUI text = CreateText("Text", buttonObject.transform, label, fontSize, TextAlignmentOptions.Center);
        StretchToFill(text.GetComponent<RectTransform>());

        return button;
    }

    private static TextMeshProUGUI CreateText(string name, Transform parent, string text, float fontSize, TextAlignmentOptions alignment)
    {
        GameObject textObject = CreateUIObject(name, parent);
        TextMeshProUGUI textComponent = textObject.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.alignment = alignment;
        textComponent.color = Color.white;
        textComponent.enableWordWrapping = true;
        textComponent.overflowMode = TextOverflowModes.Ellipsis;
        textComponent.raycastTarget = false;
        return textComponent;
    }

    private static void StretchToFill(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    private static void AssignReference(Object target, string propertyName, Object value)
    {
        if (target == null)
        {
            return;
        }

        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);

        if (property == null)
        {
            Debug.LogWarning($"Could not assign '{propertyName}' on {target.name}.");
            return;
        }

        property.objectReferenceValue = value;
        serializedObject.ApplyModifiedProperties();
    }
}

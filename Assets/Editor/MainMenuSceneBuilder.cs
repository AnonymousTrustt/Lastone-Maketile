using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MainMenuSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/MainMenu.unity";
    private const string GameScenePath = "Assets/Scenes/SampleScene.unity";
    private const string FontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

    [MenuItem("Tools/Threshold City/Create Main Menu Scene")]
    public static void CreateMainMenuScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "MainMenu";

        TMP_FontAsset font = LoadUIFont();

        Camera camera = CreateCamera();
        Canvas canvas = CreateCanvas(camera);
        MainMenuPlaceholderActions actions = CreateActionController();

        CreateBackground(canvas.transform);
        CreateTitle(canvas.transform, font);
        CreateButtons(canvas.transform, font, actions);
        CreateEventSystem();

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuildSettings();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);

        Debug.Log($"Main menu scene created at {ScenePath}");
    }

    private static Camera CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Menu Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.08f, 0.12f, 0.18f);
        camera.orthographic = true;
        camera.orthographicSize = 5f;
        camera.tag = "MainCamera";
        return camera;
    }

    private static Canvas CreateCanvas(Camera camera)
    {
        GameObject canvasObject = new GameObject("Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private static MainMenuPlaceholderActions CreateActionController()
    {
        GameObject controllerObject = new GameObject("MainMenuController");
        return controllerObject.AddComponent<MainMenuPlaceholderActions>();
    }

    private static void CreateBackground(Transform parent)
    {
        GameObject backgroundObject = new GameObject("Background", typeof(RectTransform), typeof(Image));
        backgroundObject.transform.SetParent(parent, false);

        RectTransform rect = backgroundObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = backgroundObject.GetComponent<Image>();
        image.color = new Color(0.06f, 0.1f, 0.16f, 1f);
    }

    private static void CreateTitle(Transform parent, TMP_FontAsset font)
    {
        GameObject titleObject = new GameObject("GameTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObject.transform.SetParent(parent, false);

        RectTransform rect = titleObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -120f);
        rect.sizeDelta = new Vector2(1200f, 140f);

        TextMeshProUGUI title = titleObject.GetComponent<TextMeshProUGUI>();
        title.text = "Threshold: City 17";
        title.font = font;
        title.fontSize = 72f;
        title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.Center;
        title.color = Color.white;
        title.enableWordWrapping = false;
    }

    private static void CreateButtons(Transform parent, TMP_FontAsset font, MainMenuPlaceholderActions actions)
    {
        GameObject containerObject = new GameObject("MenuButtons", typeof(RectTransform), typeof(VerticalLayoutGroup));
        containerObject.transform.SetParent(parent, false);

        RectTransform rect = containerObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, -40f);
        rect.sizeDelta = new Vector2(460f, 310f);

        VerticalLayoutGroup layout = containerObject.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 28f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        CreateButton(containerObject.transform, "Start New Game", font, actions.StartNewGame);
        CreateButton(containerObject.transform, "Load Save", font, actions.LoadSave);
        CreateButton(containerObject.transform, "Quit", font, actions.QuitGame);
    }

    private static void CreateButton(Transform parent, string label, TMP_FontAsset font, UnityEngine.Events.UnityAction clickAction)
    {
        GameObject buttonObject = new GameObject(label + " Button", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(420f, 76f);

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = new Color(0.02f, 0.04f, 0.07f, 0.92f);

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = buttonImage;
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.02f, 0.04f, 0.07f, 0.92f);
        colors.highlightedColor = new Color(0.12f, 0.22f, 0.32f, 0.96f);
        colors.pressedColor = new Color(0.18f, 0.32f, 0.44f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;
        UnityEventTools.AddPersistentListener(button.onClick, clickAction);

        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(buttonObject.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(16f, 6f);
        textRect.offsetMax = new Vector2(-16f, -6f);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = label;
        text.font = font;
        text.fontSize = 30f;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.enableWordWrapping = false;
    }

    private static void CreateEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<InputSystemUIInputModule>();
    }

    private static TMP_FontAsset LoadUIFont()
    {
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        if (font != null)
        {
            return font;
        }

        string[] guids = AssetDatabase.FindAssets("LiberationSans SDF t:TMP_FontAsset");
        if (guids.Length == 0)
        {
            return null;
        }

        return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
    }

    private static void AddSceneToBuildSettings()
    {
        string[] wantedPaths = { ScenePath, GameScenePath };
        System.Collections.Generic.List<EditorBuildSettingsScene> scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>();

        foreach (string path in wantedPaths)
        {
            if (System.Array.Exists(EditorBuildSettings.scenes, scene => scene.path == path))
            {
                continue;
            }

            scenes.Add(new EditorBuildSettingsScene(path, true));
        }

        foreach (EditorBuildSettingsScene existingScene in EditorBuildSettings.scenes)
        {
            if (existingScene.path != ScenePath && existingScene.path != GameScenePath)
            {
                scenes.Add(existingScene);
            }
        }

        EditorBuildSettings.scenes = scenes.ToArray();
    }
}

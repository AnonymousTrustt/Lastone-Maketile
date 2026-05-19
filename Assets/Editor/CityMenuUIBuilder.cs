using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class CityMenuUIBuilder
{
    private const string RootName = "CityMenuUIRoot";
    private const string PanelSpritePath = "Assets/Sprites/UI/Bottom and top Panel.png";
    private const string TooltipSpritePath = "Assets/Sprites/UI/Popup ToolTips.png";
    private const string UpArrowSpritePath = "Assets/Sprites/UI/Up Arrow from Shivang.png";
    private const string DownArrowSpritePath = "Assets/Sprites/UI/Down Arrow from Shivang.png";
    private const string UIFontAssetPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
    private static readonly Color PanelColor = new Color(1f, 1f, 1f, 0.96f);
    private static readonly Color ButtonColor = new Color(0.16f, 0.2f, 0.22f, 1f);
    private static readonly Color AccentColor = new Color(0.18f, 0.54f, 0.48f, 1f);
    private static readonly Color TextColor = new Color(0.95f, 0.97f, 0.94f, 1f);
    private static readonly Color TooltipColor = new Color(0.04f, 0.05f, 0.06f, 1f);

    [MenuItem("Tools/Threshold City 17/Create Bottom City Menu UI")]
    public static void CreateBottomCityMenuUI()
    {
        Canvas canvas = FindOrCreateCanvas();
        EnsureEventSystem();

        DestroyObjectsNamed(RootName);
        DestroyObjectsNamed("TooltipPanel");
        DestroyObjectsNamed("TooltipSystem");

        DisableOldBuildButtonPanels();

        GameObject root = CreateUIObject(RootName, canvas.transform);
        Stretch(root.GetComponent<RectTransform>());

        CityMenuUI cityMenuUI = root.AddComponent<CityMenuUI>();

        PrepareUISprite(PanelSpritePath, new Vector4(28f, 28f, 28f, 28f));
        PrepareUISprite(TooltipSpritePath, new Vector4(18f, 18f, 18f, 18f));

        Button cityMenuButton = CreateButton("CityMenuButton", root.transform, string.Empty, 44f, 32f, 1);
        RectTransform cityButtonRect = cityMenuButton.GetComponent<RectTransform>();
        cityButtonRect.anchorMin = new Vector2(0.5f, 0f);
        cityButtonRect.anchorMax = new Vector2(0.5f, 0f);
        cityButtonRect.pivot = new Vector2(0.5f, 0.5f);
        cityButtonRect.anchoredPosition = new Vector2(0f, 18f);
        Image cityButtonImage = cityMenuButton.GetComponent<Image>();
        cityButtonImage.sprite = null;
        cityButtonImage.color = new Color(0f, 0f, 0f, 0f);
        Image arrowImage = CreateImage("ArrowIcon", cityMenuButton.transform, LoadSprite(UpArrowSpritePath), false);
        RectTransform arrowRect = arrowImage.rectTransform;
        arrowRect.anchorMin = new Vector2(0.5f, 0.5f);
        arrowRect.anchorMax = new Vector2(0.5f, 0.5f);
        arrowRect.pivot = new Vector2(0.5f, 0.5f);
        arrowRect.sizeDelta = new Vector2(36f, 26f);
        arrowRect.anchoredPosition = Vector2.zero;
        arrowImage.preserveAspect = true;
        arrowImage.transform.SetAsLastSibling();

        GameObject popup = CreatePanel("BottomPopupPanel", root.transform, PanelColor);
        Image popupImage = popup.GetComponent<Image>();
        popupImage.sprite = LoadSprite(PanelSpritePath);
        popupImage.type = Image.Type.Sliced;
        RectTransform popupRect = popup.GetComponent<RectTransform>();
        popupRect.anchorMin = new Vector2(0.15f, 0f);
        popupRect.anchorMax = new Vector2(0.85f, 0f);
        popupRect.pivot = new Vector2(0.5f, 0f);
        popupRect.sizeDelta = new Vector2(0f, 126f);
        popupRect.anchoredPosition = new Vector2(0f, 8f);
        cityMenuButton.transform.SetAsLastSibling();

        GameObject mainMenuView = CreateView("MainMenuView", popup.transform);
        GameObject buildingsView = CreateView("BuildingsView", popup.transform);
        GameObject objectivesView = CreateView("ObjectivesView", popup.transform);
        GameObject cityStatusView = CreateView("CityStatusView", popup.transform);

        Button buildingsButton = CreateButton("BuildingsButton", mainMenuView.transform, "Buildings", 104f, 30f, 12);
        Button objectivesButton = CreateButton("ObjectivesButton", mainMenuView.transform, "Objectives", 104f, 30f, 12);
        Button cityStatusButton = CreateButton("CityStatusButton", mainMenuView.transform, "City Status", 112f, 30f, 12);
        AddTooltipTrigger(buildingsButton.gameObject, "Buildings", "Choose a building category, then select a building to place in the city.");
        AddTooltipTrigger(objectivesButton.gameObject, "Objectives", "View current city goals and stage requirements.");
        AddTooltipTrigger(cityStatusButton.gameObject, "City Status", "View current resources, pollution limit, month, and city stage.");
        AddHorizontalLayout(mainMenuView, 8f, TextAnchor.MiddleCenter);

        TextMeshProUGUI buildingsTitle = CreateLabel("BuildingsTitleText", buildingsView.transform, "Residential", 16, FontStyles.Bold);
        Button buildingsBackButton = CreateButton("BuildingsBackButton", buildingsView.transform, "Back", 58f, 26f, 10);
        AddTooltipTrigger(buildingsBackButton.gameObject, "Back", "Return to the main City Menu options.");
        GameObject categoryParent = CreateUIObject("CategoryButtonParent", buildingsView.transform);
        GameObject buildingParent = CreateUIObject("BuildingButtonParent", buildingsView.transform);
        Button categoryTemplate = CreateButton("CategoryButtonTemplate", buildingsView.transform, "Category", 92f, 26f, 11);
        BuildingMenuButton buildingTemplate = CreateBuildingButtonTemplate(buildingsView.transform);
        categoryTemplate.gameObject.SetActive(false);
        buildingTemplate.gameObject.SetActive(false);
        ConfigureBuildingsView(buildingsView, buildingsTitle, buildingsBackButton, categoryParent, buildingParent);

        TextMeshProUGUI objectiveTitle = CreateLabel("ObjectiveTitleText", objectivesView.transform, "Objective", 13, FontStyles.Bold);
        TextMeshProUGUI objectiveRequirements = CreateLabel("RequirementsText", objectivesView.transform, "Requirements", 10, FontStyles.Normal);
        TextMeshProUGUI objectiveProgress = CreateLabel("ProgressText", objectivesView.transform, "Progress", 10, FontStyles.Normal);
        TextMeshProUGUI objectiveStage = CreateLabel("ObjectiveStageText", objectivesView.transform, "Current Stage:", 10, FontStyles.Normal);
        TextMeshProUGUI objectivePollutionLimit = CreateLabel("ObjectivePollutionLimitText", objectivesView.transform, "Pollution Limit:", 10, FontStyles.Normal);
        TextMeshProUGUI objectiveDeadline = CreateLabel("ObjectiveDeadlineText", objectivesView.transform, "Deadline Month:", 10, FontStyles.Normal);
        Button objectivesBackButton = CreateButton("ObjectivesBackButton", objectivesView.transform, "Back", 58f, 26f, 10);
        AddTooltipTrigger(objectivesBackButton.gameObject, "Back", "Return to the main City Menu options.");
        ConfigureInfoView(objectivesView, objectivesBackButton);

        TextMeshProUGUI moneyText = CreateLabel("MoneyText", cityStatusView.transform, "Money:", 10, FontStyles.Normal);
        TextMeshProUGUI energyText = CreateLabel("EnergyText", cityStatusView.transform, "Energy:", 10, FontStyles.Normal);
        TextMeshProUGUI pollutionText = CreateLabel("PollutionText", cityStatusView.transform, "Pollution:", 10, FontStyles.Normal);
        TextMeshProUGUI happinessText = CreateLabel("HappinessText", cityStatusView.transform, "Happiness:", 10, FontStyles.Normal);
        TextMeshProUGUI monthText = CreateLabel("MonthText", cityStatusView.transform, "Month:", 10, FontStyles.Normal);
        TextMeshProUGUI stageText = CreateLabel("StageText", cityStatusView.transform, "Stage:", 10, FontStyles.Normal);
        Button cityStatusBackButton = CreateButton("CityStatusBackButton", cityStatusView.transform, "Back", 58f, 26f, 10);
        AddTooltipTrigger(cityStatusBackButton.gameObject, "Back", "Return to the main City Menu options.");
        ConfigureInfoView(cityStatusView, cityStatusBackButton);

        CreateTooltipPanel(canvas.transform);

        AssignCityMenuReferences(
            cityMenuUI,
            cityMenuButton,
            popupRect,
            mainMenuView,
            buildingsView,
            objectivesView,
            cityStatusView,
            buildingsButton,
            objectivesButton,
            cityStatusButton,
            buildingsBackButton,
            objectivesBackButton,
            cityStatusBackButton,
            categoryParent.transform,
            buildingParent.transform,
            categoryTemplate,
            buildingTemplate,
            buildingsTitle,
            objectiveTitle,
            objectiveRequirements,
            objectiveProgress,
            objectiveStage,
            objectivePollutionLimit,
            objectiveDeadline,
            moneyText,
            energyText,
            pollutionText,
            happinessText,
            monthText,
            stageText);

        mainMenuView.SetActive(true);
        buildingsView.SetActive(false);
        objectivesView.SetActive(false);
        cityStatusView.SetActive(false);
        popup.SetActive(false);

        EditorUtility.SetDirty(root);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("Created Threshold: City 17 bottom City Menu UI and saved the active scene.");
    }

    private static Canvas FindOrCreateCanvas()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            return canvas;
        }

        GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform));
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

    private static void PrepareUISprite(string path, Vector4 border)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100f;
        importer.spriteBorder = border;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();
    }

    private static Sprite LoadSprite(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static TMP_FontAsset LoadUIFont()
    {
        return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(UIFontAssetPath);
    }

    private static Image CreateImage(string name, Transform parent, Sprite sprite, bool stretch)
    {
        GameObject imageObject = CreateUIObject(name, parent);
        Image image = imageObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = Color.white;
        image.type = Image.Type.Simple;
        image.raycastTarget = false;

        if (stretch)
        {
            Stretch(image.rectTransform);
        }

        return image;
    }

    private static GameObject CreateView(string name, Transform parent)
    {
        GameObject view = CreateUIObject(name, parent);
        RectTransform rect = view.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(18f, 14f);
        rect.offsetMax = new Vector2(-18f, -14f);
        return view;
    }

    private static Button CreateButton(string name, Transform parent, string text, float width, float height, int fontSize)
    {
        GameObject buttonObject = CreateUIObject(name, parent);
        Image image = buttonObject.AddComponent<Image>();
        image.color = ButtonColor;

        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = ButtonColor;
        colors.highlightedColor = AccentColor;
        colors.pressedColor = new Color(0.1f, 0.33f, 0.3f, 1f);
        colors.selectedColor = AccentColor;
        button.colors = colors;

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, height);

        TextMeshProUGUI label = CreateLabel("Label", buttonObject.transform, text, fontSize, FontStyles.Bold);
        Stretch(label.rectTransform);
        label.alignment = TextAlignmentOptions.Center;
        label.margin = new Vector4(4f, 1f, 4f, 1f);
        label.enableAutoSizing = true;
        label.fontSizeMin = Mathf.Max(8f, fontSize - 2f);
        label.fontSizeMax = Mathf.Max(1f, fontSize);

        return button;
    }

    private static BuildingMenuButton CreateBuildingButtonTemplate(Transform parent)
    {
        Button button = CreateButton("BuildingButtonTemplate", parent, "Building\n$0", 110f, 44f, 10);
        TooltipTrigger tooltipTrigger = button.gameObject.AddComponent<TooltipTrigger>();
        BuildingMenuButton menuButton = button.gameObject.AddComponent<BuildingMenuButton>();
        SerializedObject serializedButton = new SerializedObject(menuButton);
        serializedButton.FindProperty("labelText").objectReferenceValue = button.GetComponentInChildren<TextMeshProUGUI>();
        serializedButton.FindProperty("tooltipTrigger").objectReferenceValue = tooltipTrigger;
        serializedButton.ApplyModifiedPropertiesWithoutUndo();
        return menuButton;
    }

    private static TooltipTrigger AddTooltipTrigger(GameObject target, string title, string description)
    {
        TooltipTrigger trigger = target.GetComponent<TooltipTrigger>();
        if (trigger == null)
        {
            trigger = target.AddComponent<TooltipTrigger>();
        }

        SerializedObject serializedTrigger = new SerializedObject(trigger);
        serializedTrigger.FindProperty("title").stringValue = title;
        serializedTrigger.FindProperty("description").stringValue = description;
        serializedTrigger.ApplyModifiedPropertiesWithoutUndo();
        return trigger;
    }

    private static TooltipUI CreateTooltipPanel(Transform parent)
    {
        GameObject tooltipSystem = CreateUIObject("TooltipSystem", parent);
        Stretch(tooltipSystem.GetComponent<RectTransform>());

        GameObject panel = CreatePanel("TooltipPanel", tooltipSystem.transform, TooltipColor);
        Image panelImage = panel.GetComponent<Image>();
        panelImage.sprite = LoadSprite(TooltipSpritePath);
        panelImage.type = Image.Type.Sliced;
        panelImage.color = new Color(1f, 1f, 1f, 0.96f);
        panelImage.raycastTarget = false;
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0f, 1f);
        panelRect.sizeDelta = new Vector2(320f, 168f);
        panelRect.anchoredPosition = Vector2.zero;

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(14, 14, 10, 12);
        layout.spacing = 6f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        TextMeshProUGUI title = CreateLabel("TooltipTitleText", panel.transform, "Tooltip", 17, FontStyles.Bold);
        title.alignment = TextAlignmentOptions.Left;
        title.raycastTarget = false;
        LayoutElement titleLayout = title.gameObject.AddComponent<LayoutElement>();
        titleLayout.preferredHeight = 26f;

        TextMeshProUGUI body = CreateLabel("TooltipBodyText", panel.transform, "Tooltip body", 14, FontStyles.Normal);
        body.alignment = TextAlignmentOptions.TopLeft;
        body.raycastTarget = false;
        LayoutElement bodyLayout = body.gameObject.AddComponent<LayoutElement>();
        bodyLayout.preferredHeight = 110f;

        TooltipUI tooltipUI = tooltipSystem.AddComponent<TooltipUI>();
        SerializedObject serializedTooltip = new SerializedObject(tooltipUI);
        Set(serializedTooltip, "tooltipPanel", panelRect);
        Set(serializedTooltip, "titleText", title);
        Set(serializedTooltip, "bodyText", body);
        Set(serializedTooltip, "canvas", parent.GetComponent<Canvas>());
        serializedTooltip.FindProperty("followMouse").boolValue = true;
        serializedTooltip.FindProperty("mouseOffset").vector2Value = new Vector2(18f, -18f);
        serializedTooltip.FindProperty("screenPadding").floatValue = 12f;
        serializedTooltip.ApplyModifiedPropertiesWithoutUndo();

        panel.SetActive(false);
        tooltipSystem.transform.SetAsLastSibling();
        return tooltipUI;
    }

    private static TextMeshProUGUI CreateLabel(string name, Transform parent, string text, int fontSize, FontStyles style)
    {
        GameObject textObject = CreateUIObject(name, parent);
        TextMeshProUGUI label = textObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = style;
        label.color = TextColor;
        TMP_FontAsset font = LoadUIFont();
        if (font != null)
        {
            label.font = font;
        }

        label.alignment = TextAlignmentOptions.MidlineLeft;
        label.enableWordWrapping = true;
        label.overflowMode = TextOverflowModes.Overflow;
        label.enableAutoSizing = true;
        label.fontSizeMin = Mathf.Max(8f, fontSize - 3f);
        label.fontSizeMax = Mathf.Max(1f, fontSize);
        return label;
    }

    private static void ConfigureBuildingsView(GameObject view, TextMeshProUGUI title, Button backButton, GameObject categoryParent, GameObject buildingParent)
    {
        RectTransform titleRect = title.rectTransform;
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(0f, 1f);
        titleRect.pivot = new Vector2(0f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, 0f);
        titleRect.sizeDelta = new Vector2(260f, 30f);

        RectTransform backRect = backButton.GetComponent<RectTransform>();
        LayoutElement backLayout = backButton.gameObject.AddComponent<LayoutElement>();
        backLayout.ignoreLayout = true;
        backRect.anchorMin = new Vector2(1f, 1f);
        backRect.anchorMax = new Vector2(1f, 1f);
        backRect.pivot = new Vector2(1f, 1f);
        backRect.anchoredPosition = new Vector2(0f, 0f);
        backButton.transform.SetAsLastSibling();

        RectTransform categoryRect = categoryParent.GetComponent<RectTransform>();
        categoryRect.anchorMin = new Vector2(0f, 0.56f);
        categoryRect.anchorMax = new Vector2(1f, 0.56f);
        categoryRect.pivot = new Vector2(0.5f, 0.5f);
        categoryRect.sizeDelta = new Vector2(0f, 28f);
        categoryRect.anchoredPosition = new Vector2(0f, 0f);
        AddHorizontalLayout(categoryParent, 6f, TextAnchor.MiddleLeft);

        RectTransform buildingRect = buildingParent.GetComponent<RectTransform>();
        buildingRect.anchorMin = new Vector2(0f, 0f);
        buildingRect.anchorMax = new Vector2(1f, 0.44f);
        buildingRect.offsetMin = new Vector2(0f, 0f);
        buildingRect.offsetMax = new Vector2(0f, 0f);
        AddHorizontalLayout(buildingParent, 6f, TextAnchor.MiddleLeft);
    }

    private static void ConfigureInfoView(GameObject view, Button backButton)
    {
        RectTransform backRect = backButton.GetComponent<RectTransform>();
        LayoutElement backLayout = backButton.gameObject.AddComponent<LayoutElement>();
        backLayout.ignoreLayout = true;
        backRect.anchorMin = new Vector2(1f, 1f);
        backRect.anchorMax = new Vector2(1f, 1f);
        backRect.pivot = new Vector2(1f, 1f);
        backRect.anchoredPosition = new Vector2(0f, 0f);
        backButton.transform.SetAsLastSibling();

        GridLayoutGroup grid = view.AddComponent<GridLayoutGroup>();
        grid.padding = new RectOffset(0, 120, 4, 0);
        grid.cellSize = new Vector2(220f, 28f);
        grid.spacing = new Vector2(8f, 4f);
        grid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
        grid.constraintCount = 3;
        grid.childAlignment = TextAnchor.UpperLeft;
    }

    private static void AddHorizontalLayout(GameObject gameObject, float spacing, TextAnchor alignment)
    {
        HorizontalLayoutGroup layout = gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = alignment;
        layout.spacing = spacing;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
    }

    private static void Stretch(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    private static void AssignCityMenuReferences(
        CityMenuUI cityMenuUI,
        Button cityMenuButton,
        RectTransform popup,
        GameObject mainMenuView,
        GameObject buildingsView,
        GameObject objectivesView,
        GameObject cityStatusView,
        Button buildingsButton,
        Button objectivesButton,
        Button cityStatusButton,
        Button buildingsBackButton,
        Button objectivesBackButton,
        Button cityStatusBackButton,
        Transform categoryParent,
        Transform buildingParent,
        Button categoryTemplate,
        BuildingMenuButton buildingTemplate,
        TextMeshProUGUI buildingsTitle,
        TextMeshProUGUI objectiveTitle,
        TextMeshProUGUI objectiveRequirements,
        TextMeshProUGUI objectiveProgress,
        TextMeshProUGUI objectiveStage,
        TextMeshProUGUI objectivePollutionLimit,
        TextMeshProUGUI objectiveDeadline,
        TextMeshProUGUI moneyText,
        TextMeshProUGUI energyText,
        TextMeshProUGUI pollutionText,
        TextMeshProUGUI happinessText,
        TextMeshProUGUI monthText,
        TextMeshProUGUI stageText)
    {
        SerializedObject serializedUI = new SerializedObject(cityMenuUI);
        Set(serializedUI, "buildingPlacementManager", Object.FindFirstObjectByType<BuildingPlacementManager>());
        Set(serializedUI, "resourceManager", Object.FindFirstObjectByType<ResourceManager>());
        Set(serializedUI, "turnManager", Object.FindFirstObjectByType<TurnManager>());
        Set(serializedUI, "townStageManager", Object.FindFirstObjectByType<TownStageManager>());
        Set(serializedUI, "objectiveManager", Object.FindFirstObjectByType<ObjectiveManager>());
        Set(serializedUI, "cityMenuButton", cityMenuButton);
        Set(serializedUI, "cityMenuButtonImage", cityMenuButton.GetComponent<Image>());
        Set(serializedUI, "cityMenuArrowImage", cityMenuButton.transform.Find("ArrowIcon").GetComponent<Image>());
        Set(serializedUI, "closedButtonSprite", LoadSprite(UpArrowSpritePath));
        Set(serializedUI, "openButtonSprite", LoadSprite(DownArrowSpritePath));
        Set(serializedUI, "bottomPopupPanel", popup);
        serializedUI.FindProperty("slideDuration").floatValue = 0.2f;
        serializedUI.FindProperty("closePanelAfterBuildingSelected").boolValue = true;
        Set(serializedUI, "mainMenuView", mainMenuView);
        Set(serializedUI, "buildingsView", buildingsView);
        Set(serializedUI, "objectivesView", objectivesView);
        Set(serializedUI, "cityStatusView", cityStatusView);
        Set(serializedUI, "buildingsButton", buildingsButton);
        Set(serializedUI, "objectivesButton", objectivesButton);
        Set(serializedUI, "cityStatusButton", cityStatusButton);
        Set(serializedUI, "buildingsBackButton", buildingsBackButton);
        Set(serializedUI, "objectivesBackButton", objectivesBackButton);
        Set(serializedUI, "cityStatusBackButton", cityStatusBackButton);
        Set(serializedUI, "categoryButtonParent", categoryParent);
        Set(serializedUI, "buildingButtonParent", buildingParent);
        Set(serializedUI, "categoryButtonPrefab", categoryTemplate);
        Set(serializedUI, "buildingButtonPrefab", buildingTemplate);
        Set(serializedUI, "buildingsTitleText", buildingsTitle);
        Set(serializedUI, "objectiveTitleText", objectiveTitle);
        Set(serializedUI, "objectiveRequirementsText", objectiveRequirements);
        Set(serializedUI, "objectiveProgressText", objectiveProgress);
        Set(serializedUI, "objectiveStageText", objectiveStage);
        Set(serializedUI, "objectivePollutionLimitText", objectivePollutionLimit);
        Set(serializedUI, "objectiveDeadlineText", objectiveDeadline);
        Set(serializedUI, "moneyText", moneyText);
        Set(serializedUI, "energyText", energyText);
        Set(serializedUI, "pollutionText", pollutionText);
        Set(serializedUI, "happinessText", happinessText);
        Set(serializedUI, "monthText", monthText);
        Set(serializedUI, "stageText", stageText);
        AssignBuildingCategories(serializedUI);
        serializedUI.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void AssignBuildingCategories(SerializedObject serializedUI)
    {
        Dictionary<string, List<BuildingDefinition>> categories = new Dictionary<string, List<BuildingDefinition>>
        {
            { "Residential", new List<BuildingDefinition>() },
            { "Commercial", new List<BuildingDefinition>() },
            { "Agriculture", new List<BuildingDefinition>() },
            { "Energy", new List<BuildingDefinition>() },
            { "Industry", new List<BuildingDefinition>() },
            { "Environment", new List<BuildingDefinition>() }
        };

        string[] guids = AssetDatabase.FindAssets("t:BuildingDefinition");
        foreach (string guid in guids)
        {
            BuildingDefinition definition = AssetDatabase.LoadAssetAtPath<BuildingDefinition>(AssetDatabase.GUIDToAssetPath(guid));
            if (definition == null)
            {
                continue;
            }

            categories[GuessCategory(definition)].Add(definition);
        }

        SerializedProperty categoryList = serializedUI.FindProperty("buildingCategories");
        categoryList.arraySize = categories.Count;

        int index = 0;
        foreach (KeyValuePair<string, List<BuildingDefinition>> pair in categories)
        {
            SerializedProperty category = categoryList.GetArrayElementAtIndex(index);
            category.FindPropertyRelative("categoryName").stringValue = pair.Key;
            SerializedProperty buildings = category.FindPropertyRelative("buildings");
            buildings.arraySize = pair.Value.Count;

            for (int buildingIndex = 0; buildingIndex < pair.Value.Count; buildingIndex++)
            {
                buildings.GetArrayElementAtIndex(buildingIndex).objectReferenceValue = pair.Value[buildingIndex];
            }

            index++;
        }
    }

    private static string GuessCategory(BuildingDefinition definition)
    {
        string name = $"{definition.name} {definition.buildingName} {definition.buildingId}".ToLowerInvariant();

        if (name.Contains("house") || name.Contains("apartment") || name.Contains("residential"))
        {
            return "Residential";
        }

        if (name.Contains("market") || name.Contains("commercial"))
        {
            return "Commercial";
        }

        if (name.Contains("farm") || name.Contains("agric"))
        {
            return "Agriculture";
        }

        if (name.Contains("solar") || name.Contains("wind") || name.Contains("hydro"))
        {
            return "Energy";
        }

        if (name.Contains("coal") || name.Contains("petroleum") || name.Contains("factory") || definition.sectorType == BuildingSector.Industry)
        {
            return "Industry";
        }

        if (name.Contains("recycling") || name.Contains("water") || name.Contains("treatment") || definition.sectorType == BuildingSector.EnvironmentalMitigation)
        {
            return "Environment";
        }

        return definition.sectorType == BuildingSector.Energy ? "Energy" : "Commercial";
    }

    private static void DisableOldBuildButtonPanels()
    {
        foreach (Button button in Object.FindObjectsByType<Button>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            string lowerName = button.gameObject.name.ToLowerInvariant();
            if (!lowerName.Contains("build") && !lowerName.Contains("house") && !lowerName.Contains("market") &&
                !lowerName.Contains("factory") && !lowerName.Contains("farm") && !lowerName.Contains("solar") &&
                !lowerName.Contains("wind") && !lowerName.Contains("recycling") && !lowerName.Contains("water"))
            {
                continue;
            }

            Transform target = button.transform;
            while (target.parent != null && target.parent.GetComponent<Canvas>() == null)
            {
                string parentName = target.parent.name.ToLowerInvariant();
                if (parentName.Contains("panel") || parentName.Contains("menu") || parentName.Contains("buttons"))
                {
                    target = target.parent;
                    break;
                }

                target = target.parent;
            }

            if (target.name != RootName)
            {
                target.gameObject.SetActive(false);
            }
        }
    }

    private static void DestroyObjectsNamed(string objectName)
    {
        foreach (GameObject gameObject in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (gameObject.name == objectName)
            {
                Object.DestroyImmediate(gameObject);
            }
        }
    }

    private static void Set(SerializedObject serializedObject, string propertyName, Object value)
    {
        serializedObject.FindProperty(propertyName).objectReferenceValue = value;
    }
}

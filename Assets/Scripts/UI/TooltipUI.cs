using TMPro;
using UnityEngine;

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private RectTransform tooltipPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private Canvas canvas;

    [Header("Position")]
    [SerializeField] private bool followMouse = true;
    [SerializeField] private Vector2 mouseOffset = new Vector2(18f, -18f);
    [SerializeField] private float screenPadding = 12f;

    private RectTransform canvasRect;
    private bool isVisible;
    private Vector2 lastScreenPosition;

    private void Awake()
    {
        Instance = this;

        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }

        if (canvas != null)
        {
            canvasRect = canvas.GetComponent<RectTransform>();
        }

        Hide();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (isVisible && followMouse)
        {
            MoveToScreenPosition(lastScreenPosition);
        }
    }

    public void Show(string title, string body, Vector2 screenPosition)
    {
        if (tooltipPanel == null)
        {
            return;
        }

        if (titleText != null)
        {
            titleText.text = title;
        }

        if (bodyText != null)
        {
            bodyText.text = body;
        }

        tooltipPanel.gameObject.SetActive(true);
        tooltipPanel.SetAsLastSibling();
        isVisible = true;
        lastScreenPosition = screenPosition;
        MoveToScreenPosition(screenPosition);
    }

    public void Hide()
    {
        isVisible = false;

        if (tooltipPanel != null)
        {
            tooltipPanel.gameObject.SetActive(false);
        }
    }

    private void MoveToScreenPosition(Vector2 screenPosition)
    {
        if (tooltipPanel == null || canvasRect == null)
        {
            return;
        }

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition + mouseOffset, null, out localPoint);
        tooltipPanel.anchoredPosition = localPoint;

        Canvas.ForceUpdateCanvases();
        ClampInsideCanvas();
    }

    private void ClampInsideCanvas()
    {
        Vector2 position = tooltipPanel.anchoredPosition;
        Vector2 panelSize = tooltipPanel.rect.size;
        Vector2 canvasSize = canvasRect.rect.size;

        float minX = -canvasSize.x * 0.5f + screenPadding;
        float maxX = canvasSize.x * 0.5f - panelSize.x - screenPadding;
        float minY = -canvasSize.y * 0.5f + panelSize.y + screenPadding;
        float maxY = canvasSize.y * 0.5f - screenPadding;

        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);
        tooltipPanel.anchoredPosition = position;
    }
}

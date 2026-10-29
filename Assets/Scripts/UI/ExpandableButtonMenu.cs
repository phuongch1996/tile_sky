using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ExpandableButtonMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button mainButton;
    [SerializeField] private RectTransform panel;
    [SerializeField] private CanvasGroup panelCanvasGroup;
    [SerializeField] private Button[] childButtons;

    [Header("Animation")]
    [SerializeField] private bool openOnStart = false;
    [SerializeField] private bool closeOnChildClick = true;
    [SerializeField] private float animationDuration = 0.25f;
    [SerializeField] private float childStagger = 0.04f;
    [SerializeField] private Ease openEase = Ease.OutBack;
    [SerializeField] private Ease closeEase = Ease.InBack;
    [SerializeField] private Vector2 closedScale = new Vector2(1f, 0f);
    [SerializeField] private Vector2 openedScale = Vector2.one;

    [Header("Layout")]
    [SerializeField] private bool autoFitPanelHeight = true;
    [SerializeField] private float buttonSpacing = 12f;
    [SerializeField] private float panelTopPadding = 18f;
    [SerializeField] private float panelBottomPadding = 12f;

    [Header("Events")]
    public UnityEvent onOpened;
    public UnityEvent onClosed;

    private bool isOpen;
    private Sequence sequence;
    private bool mainButtonInsidePanel;
    private Graphic panelGraphic;
    private RectTransform[] childRects;
    private CanvasGroup[] childCanvasGroups;
    private Vector2[] childOpenPositions;
    private Vector3[] childOpenScales;
    private bool[] childAvailable;

    public bool IsOpen
    {
        get { return isOpen; }
    }

    private void Reset()
    {
        mainButton = GetComponentInChildren<Button>();
        panel = transform as RectTransform;
        panelCanvasGroup = GetComponentInChildren<CanvasGroup>();
    }

    private void Awake()
    {
        mainButtonInsidePanel = mainButton != null && panel != null && mainButton.transform.IsChildOf(panel);
        if (panel != null)
        {
            panelGraphic = panel.GetComponent<Graphic>();
        }

        CacheChildButtonStates();

        if (!mainButtonInsidePanel && panelCanvasGroup == null && panel != null)
            panelCanvasGroup = panel.GetComponent<CanvasGroup>();

        if (!mainButtonInsidePanel && panelCanvasGroup == null && panel != null)
            panelCanvasGroup = panel.gameObject.AddComponent<CanvasGroup>();

        if (mainButton != null)
            mainButton.onClick.AddListener(Toggle);

        if (closeOnChildClick && childButtons != null)
        {
            foreach (Button childButton in childButtons)
            {
                if (childButton != null)
                    childButton.onClick.AddListener(Close);
            }
        }
    }

    private void Start()
    {
        SetOpenImmediate(openOnStart);
    }

    private void OnDestroy()
    {
        if (sequence != null)
            sequence.Kill();

        if (mainButton != null)
            mainButton.onClick.RemoveListener(Toggle);

        if (childButtons != null)
        {
            foreach (Button childButton in childButtons)
            {
                if (childButton != null)
                    childButton.onClick.RemoveListener(Close);
            }
        }
    }

    public void Toggle()
    {
        if (isOpen)
            Close();
        else
            Open();
    }

    public void Open()
    {
        SetOpen(true);
    }

    public void Close()
    {
        SetOpen(false);
    }

    public void SetOpenImmediate(bool open)
    {
        isOpen = open;

        if (sequence != null)
            sequence.Kill();

        if (panel != null)
        {
            panel.gameObject.SetActive(open || mainButtonInsidePanel);
            panel.localScale = open || mainButtonInsidePanel ? openedScale : closedScale;
        }

        if (mainButtonInsidePanel)
        {
            SetPanelGraphicVisible(open);
            SetChildButtonsImmediate(open);
        }
        else if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = open ? 1f : 0f;
            panelCanvasGroup.interactable = open;
            panelCanvasGroup.blocksRaycasts = open;
        }

        SetChildButtonsInteractable(open);
    }

    public void RefreshChildButtonAvailability()
    {
        if (childButtons == null)
            return;

        if (childAvailable == null || childAvailable.Length != childButtons.Length)
            childAvailable = new bool[childButtons.Length];

        for (int i = 0; i < childButtons.Length; i++)
            childAvailable[i] = childButtons[i] != null && childButtons[i].gameObject.activeSelf;

        ApplyAvailableLayout();

        if (!isOpen && mainButtonInsidePanel)
            SetChildButtonsImmediate(false);
    }

    private void SetOpen(bool open)
    {
        if (isOpen == open)
            return;

        isOpen = open;

        if (sequence != null)
            sequence.Kill();

        if (panel == null)
            return;

        panel.gameObject.SetActive(true);
        SetChildButtonsInteractable(open);

        if (mainButtonInsidePanel)
        {
            SetOpenWithMainButtonInsidePanel(open);
            return;
        }

        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.interactable = open;
            panelCanvasGroup.blocksRaycasts = open;
        }

        sequence = DOTween.Sequence();
        sequence.Join(panel.DOScale(open ? openedScale : closedScale, animationDuration).SetEase(open ? openEase : closeEase));

        if (panelCanvasGroup != null)
            sequence.Join(panelCanvasGroup.DOFade(open ? 1f : 0f, animationDuration));

        sequence.OnComplete(() =>
        {
            if (open)
            {
                onOpened.Invoke();
            }
            else
            {
                panel.gameObject.SetActive(false);
                onClosed.Invoke();
            }
        });
    }

    private void SetChildButtonsInteractable(bool interactable)
    {
        if (childButtons == null)
            return;

        for (int i = 0; i < childButtons.Length; i++)
        {
            Button childButton = childButtons[i];
            if (childButton != null)
            {
                bool available = childAvailable == null || childAvailable.Length <= i || childAvailable[i];
                childButton.interactable = interactable && available;
            }
        }
    }

    private void SetOpenWithMainButtonInsidePanel(bool open)
    {
        panel.localScale = openedScale;

        if (open)
            SetChildButtonsReadyForOpen();

        sequence = DOTween.Sequence();
        if (panelGraphic != null)
        {
            float panelDuration = animationDuration * 0.75f;
            sequence.Join(panelGraphic.DOFade(open ? 1f : 0f, panelDuration).SetEase(open ? Ease.OutQuad : Ease.InQuad));
        }

        if (childRects != null)
        {
            for (int i = 0; i < childRects.Length; i++)
            {
                RectTransform childRect = childRects[i];
                if (childRect == null)
                    continue;

                bool available = childAvailable == null || childAvailable.Length <= i || childAvailable[i];
                if (!available)
                    continue;

                childRect.DOKill();

                float delay = open ? i * childStagger : (childRects.Length - 1 - i) * childStagger;
                Vector2 targetPosition = open ? childOpenPositions[i] : GetChildClosedPosition();
                Vector3 targetScale = open ? childOpenScales[i] : Vector3.zero;

                sequence.Insert(delay, childRect.DOAnchorPos(targetPosition, animationDuration).SetEase(open ? Ease.OutCubic : Ease.InCubic));
                sequence.Insert(delay, childRect.DOScale(targetScale, animationDuration).SetEase(open ? openEase : closeEase));

                if (childCanvasGroups != null && childCanvasGroups[i] != null)
                    sequence.Insert(delay, childCanvasGroups[i].DOFade(open ? 1f : 0f, animationDuration * 0.65f));
            }
        }

        sequence.OnComplete(() =>
        {
            if (open)
            {
                onOpened.Invoke();
            }
            else
            {
                SetPanelGraphicVisible(false);
                SetChildButtonsImmediate(false);
                onClosed.Invoke();
            }
        });
    }

    private void CacheChildButtonStates()
    {
        if (childButtons == null)
            return;

        childRects = new RectTransform[childButtons.Length];
        childCanvasGroups = new CanvasGroup[childButtons.Length];
        childOpenPositions = new Vector2[childButtons.Length];
        childOpenScales = new Vector3[childButtons.Length];
        childAvailable = new bool[childButtons.Length];

        for (int i = 0; i < childButtons.Length; i++)
        {
            Button childButton = childButtons[i];
            if (childButton == null)
                continue;

            childAvailable[i] = childButton.gameObject.activeSelf;

            RectTransform childRect = childButton.transform as RectTransform;
            childRects[i] = childRect;

            if (childRect != null)
            {
                childOpenPositions[i] = childRect.anchoredPosition;
                childOpenScales[i] = childRect.localScale;
            }

            CanvasGroup canvasGroup = childButton.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = childButton.gameObject.AddComponent<CanvasGroup>();

            childCanvasGroups[i] = canvasGroup;
        }

        ApplyAvailableLayout();
    }

    private void ApplyAvailableLayout()
    {
        if (!autoFitPanelHeight || panel == null || childRects == null)
            return;

        int availableCount = GetAvailableChildCount();
        RectTransform mainRect = mainButton != null ? mainButton.transform as RectTransform : null;
        float mainHeight = mainRect != null ? mainRect.rect.height : 0f;
        float childHeight = GetMaxAvailableChildHeight();
        float targetHeight = mainHeight * 0.5f + panelBottomPadding + panelTopPadding;

        if (availableCount > 0)
            targetHeight += availableCount * childHeight + Mathf.Max(0, availableCount - 1) * buttonSpacing;

        panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);

        if (mainRect != null)
        {
            mainRect.anchorMin = new Vector2(0.5f, 0f);
            mainRect.anchorMax = new Vector2(0.5f, 0f);
            mainRect.anchoredPosition = Vector2.zero;
        }

        int availableIndex = 0;
        for (int i = 0; i < childRects.Length; i++)
        {
            RectTransform childRect = childRects[i];
            if (childRect == null)
                continue;

            bool available = childAvailable == null || childAvailable.Length <= i || childAvailable[i];
            if (!available)
                continue;

            childRect.anchorMin = new Vector2(0.5f, 0f);
            childRect.anchorMax = new Vector2(0.5f, 0f);

            float y = mainHeight * 0.5f + panelBottomPadding + childHeight * 0.5f;
            y += (availableCount - 1 - availableIndex) * (childHeight + buttonSpacing);
            childOpenPositions[i] = new Vector2(0f, y);
            childRect.anchoredPosition = childOpenPositions[i];
            availableIndex++;
        }
    }

    private void SetChildButtonsReadyForOpen()
    {
        if (childRects == null)
            return;

        Vector2 closedPosition = GetChildClosedPosition();
        for (int i = 0; i < childRects.Length; i++)
        {
            RectTransform childRect = childRects[i];
            if (childRect == null)
                continue;

            bool available = childAvailable == null || childAvailable.Length <= i || childAvailable[i];
            childRect.gameObject.SetActive(available);
            if (!available)
                continue;

            childRect.anchoredPosition = closedPosition;
            childRect.localScale = Vector3.zero;

            if (childCanvasGroups != null && childCanvasGroups[i] != null)
                childCanvasGroups[i].alpha = 0f;
        }
    }

    private void SetPanelGraphicVisible(bool visible)
    {
        if (panelGraphic == null)
            return;

        Color color = panelGraphic.color;
        color.a = visible ? 1f : 0f;
        panelGraphic.color = color;
        panelGraphic.raycastTarget = visible;
    }

    private void SetChildButtonsImmediate(bool active)
    {
        if (childRects == null)
            return;

        Vector2 closedPosition = GetChildClosedPosition();
        for (int i = 0; i < childRects.Length; i++)
        {
            RectTransform childRect = childRects[i];
            if (childRect == null)
                continue;

            bool available = childAvailable == null || childAvailable.Length <= i || childAvailable[i];
            childRect.gameObject.SetActive(active && available);
            childRect.anchoredPosition = active ? childOpenPositions[i] : closedPosition;
            childRect.localScale = active ? childOpenScales[i] : Vector3.zero;

            if (childCanvasGroups != null && childCanvasGroups[i] != null)
                childCanvasGroups[i].alpha = active && available ? 1f : 0f;
        }
    }

    private Vector2 GetChildClosedPosition()
    {
        RectTransform mainRect = mainButton != null ? mainButton.transform as RectTransform : null;
        if (mainRect != null)
            return mainRect.anchoredPosition;

        return Vector2.zero;
    }

    private int GetAvailableChildCount()
    {
        if (childButtons == null)
            return 0;

        int count = 0;
        for (int i = 0; i < childButtons.Length; i++)
        {
            if (childButtons[i] != null && (childAvailable == null || childAvailable.Length <= i || childAvailable[i]))
                count++;
        }

        return count;
    }

    private float GetMaxAvailableChildHeight()
    {
        float height = 0f;
        if (childRects == null)
            return height;

        for (int i = 0; i < childRects.Length; i++)
        {
            RectTransform childRect = childRects[i];
            if (childRect == null)
                continue;

            bool available = childAvailable == null || childAvailable.Length <= i || childAvailable[i];
            if (available)
                height = Mathf.Max(height, childRect.rect.height);
        }

        return height;
    }
}

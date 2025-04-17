using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DialogController : MonoBehaviour
{
    public Button m_OpenButton;
    public Button m_CloseButton;
    public float m_AppearDuration = 0.2f;
    public float m_DisappearDuration = 0.2f;
    public AnimationCurve m_AnimationCurve;
    public Color m_OverlayColor = new Color(0, 0, 0, 0.5f);
    public GameObject m_OverlayBackground;
    
    private CanvasGroup m_CanvasGroup;
    private RectTransform m_RectTransform;
    private Coroutine m_CurrentAnimation;
    private Image m_OverlayImage;
    
    void Awake()
    {
        m_CanvasGroup = GetComponent<CanvasGroup>();
        m_RectTransform = GetComponent<RectTransform>();
        m_OpenButton.onClick.AddListener(ShowDialog);
        m_CloseButton.onClick.AddListener(HideDialog);
        
        // Set up the overlay background if not assigned
        if (m_OverlayBackground == null)
        {
            SetupOverlayBackground();
        }
        else
        {
            m_OverlayImage = m_OverlayBackground.GetComponent<Image>();
            if (m_OverlayImage == null)
                m_OverlayImage = m_OverlayBackground.AddComponent<Image>();
        }
        
        // Configure overlay
        m_OverlayImage.color = Color.clear;
        m_OverlayBackground.SetActive(false);
        
        // Set default animation curve if none is provided
        if (m_AnimationCurve.keys.Length == 0)
        {
            m_AnimationCurve = new AnimationCurve(
                new Keyframe(0, 0, 0, 2),
                new Keyframe(1, 1, 0, 0)
            );
        }
        
        // Initialize as hidden
        m_CanvasGroup.alpha = 0;
        m_CanvasGroup.interactable = false;
        m_CanvasGroup.blocksRaycasts = false;
    }

    private void SetupOverlayBackground()
    {
        // Create overlay background GameObject if not provided
        m_OverlayBackground = new GameObject("DialogOverlay");
        m_OverlayBackground.transform.SetParent(transform.parent);
        
        // Ensure dialog is displayed on top of overlay
        m_OverlayBackground.transform.SetSiblingIndex(transform.GetSiblingIndex());
        
        // Setup RectTransform to cover the entire screen
        RectTransform overlayRectTransform = m_OverlayBackground.AddComponent<RectTransform>();
        overlayRectTransform.anchorMin = Vector2.zero;
        overlayRectTransform.anchorMax = Vector2.one;
        overlayRectTransform.sizeDelta = Vector2.zero;
        overlayRectTransform.localPosition = Vector3.zero;
        
        // Add image component for the background color
        m_OverlayImage = m_OverlayBackground.AddComponent<Image>();
        m_OverlayImage.color = Color.clear;
    }

    private void ShowDialog()
    {
        // Stop any running animations
        if (m_CurrentAnimation != null)
            StopCoroutine(m_CurrentAnimation);
        
        // Activate overlay
        m_OverlayBackground.SetActive(true);
            
        // Set raycast blocking and interactable before animation starts
        m_CanvasGroup.interactable = true;
        m_CanvasGroup.blocksRaycasts = true;
        
        // Start appear animation
        m_CurrentAnimation = StartCoroutine(AnimateAppear());
    }

    private void HideDialog()
    {
        // Stop any running animations
        if (m_CurrentAnimation != null)
            StopCoroutine(m_CurrentAnimation);
            
        // Start disappear animation
        m_CurrentAnimation = StartCoroutine(AnimateDisappear());
    }
    
    private IEnumerator AnimateAppear()
    {
        float startScale = 0.8f;
        float targetScale = 1.0f;
        float startAlpha = 0f;
        float targetAlpha = 1f;
        float elapsedTime = 0f;
        Color startColor = Color.clear;
        Color targetColor = m_OverlayColor;
        
        // Set initial values
        m_RectTransform.localScale = Vector3.one * startScale;
        m_CanvasGroup.alpha = startAlpha;
        m_OverlayImage.color = startColor;
        
        while (elapsedTime < m_AppearDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / m_AppearDuration;
            float evaluatedTime = m_AnimationCurve.Evaluate(normalizedTime);
            
            // Animate scale
            float currentScale = Mathf.Lerp(startScale, targetScale, evaluatedTime);
            m_RectTransform.localScale = Vector3.one * currentScale;
            
            // Animate alpha
            m_CanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, evaluatedTime);
            
            // Animate overlay
            m_OverlayImage.color = Color.Lerp(startColor, targetColor, evaluatedTime);
            
            yield return null;
        }
        
        // Ensure end values are exactly what we want
        m_RectTransform.localScale = Vector3.one * targetScale;
        m_CanvasGroup.alpha = targetAlpha;
        m_OverlayImage.color = targetColor;
    }
    
    private IEnumerator AnimateDisappear()
    {
        float startScale = 1.0f;
        float targetScale = 0.8f;
        float startAlpha = 1f;
        float targetAlpha = 0f;
        float elapsedTime = 0f;
        Color startColor = m_OverlayColor;
        Color targetColor = Color.clear;
        
        // Set initial values
        m_RectTransform.localScale = Vector3.one * startScale;
        m_CanvasGroup.alpha = startAlpha;
        m_OverlayImage.color = startColor;
        
        while (elapsedTime < m_DisappearDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / m_DisappearDuration;
            float evaluatedTime = m_AnimationCurve.Evaluate(normalizedTime);
            
            // Animate scale
            float currentScale = Mathf.Lerp(startScale, targetScale, evaluatedTime);
            m_RectTransform.localScale = Vector3.one * currentScale;
            
            // Animate alpha
            m_CanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, evaluatedTime);
            
            // Animate overlay
            m_OverlayImage.color = Color.Lerp(startColor, targetColor, evaluatedTime);
            
            yield return null;
        }
        
        // Ensure end values are exactly what we want
        m_RectTransform.localScale = Vector3.one * targetScale;
        m_CanvasGroup.alpha = targetAlpha;
        m_OverlayImage.color = targetColor;
        
        // Disable interaction after animation completes
        m_CanvasGroup.interactable = false;
        m_CanvasGroup.blocksRaycasts = false;
        m_OverlayBackground.SetActive(false);
    }
}

using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using System.Collections;
using System;
using UnityEngine.Localization.Settings;

public class LoadingUIController : MonoBehaviour
{
    [SerializeField] LoadingStrategy m_LoadingStrategy;
    [SerializeField] VisualTreeAsset m_LoadingOverlay;
    [SerializeField] VisualTreeAsset m_FailureOverlay;
    [SerializeField] VisualTreeAsset m_SuccessOverlay;
    [SerializeField] string m_MessageLabelName = "MessageLabel";
    [SerializeField] string m_ReloadButtonName = "ReloadButton";
    [SerializeField] string m_ReturnButtonName = "ReturnButton";
    [SerializeField] string m_ProgressBarName = "ProgressBar";
    [SerializeField] float m_MinDuration = 3.0f;
    [SerializeField] float m_ProgressUpdateInterval = 0.05f;

    VisualElement m_RootElement;
    VisualElement m_LoadingOverlayElement;
    VisualElement m_FailureOverlayElement;
    VisualElement m_SuccessOverlayElement;

    ProgressBar m_ProgressBar;
    bool m_IsLoading = false;

    void Awake()
    {
        LocalizationSettings.InitializationOperation.WaitForCompletion();

        m_RootElement = GetComponent<UIDocument>().rootVisualElement;

        Assert.IsNotNull(m_RootElement);
        Assert.IsNotNull(m_LoadingStrategy);
        Assert.IsNotNull(m_LoadingOverlay);
        Assert.IsNotNull(m_FailureOverlay);

        m_LoadingOverlayElement = m_LoadingOverlay.CloneTree();
        m_FailureOverlayElement = m_FailureOverlay.CloneTree();

        if (m_SuccessOverlay != null)
        {
            m_SuccessOverlayElement = m_SuccessOverlay.CloneTree();
        }

        m_ProgressBar = m_LoadingOverlayElement.Q<ProgressBar>(m_ProgressBarName);

        Button reloadButton = m_FailureOverlayElement.Q<Button>(m_ReloadButtonName);
        if (reloadButton != null)
        {
            reloadButton.clicked += () => UIToolkitUtils.HideOverlay(m_FailureOverlayElement);
            reloadButton.clicked += async () => await HandleLoad();
        }

        Button returnButton = m_FailureOverlayElement.Q<Button>(m_ReturnButtonName);
        returnButton.clicked += SceneUtils.Return;

        UIToolkitUtils.ShowOverlay(m_RootElement, m_LoadingOverlayElement);

        _ = HandleLoad();
    }

    async Task HandleLoad()
    {
        if (m_IsLoading)
            return;

        m_IsLoading = true;
        
        if (m_ProgressBar != null)
        {
            m_ProgressBar.value = 0;
        }
        
        UIToolkitUtils.ShowOverlay(m_RootElement, m_LoadingOverlayElement);
        
        // Start the loading animation
        StartCoroutine(UpdateProgressBar());
        
        // Get the start time to track the minimum duration
        float startTime = Time.time;
        
        // Actually do the loading
        LoadingResult result = await m_LoadingStrategy.Load();
        
        // Wait for minimum duration if needed
        float elapsedTime = Time.time - startTime;
        if (elapsedTime < m_MinDuration)
        {
            await Task.Delay(TimeSpan.FromSeconds(m_MinDuration - elapsedTime));
        }
        
        // Stop animating the progress bar
        m_IsLoading = false;
        
        // Ensure progress bar reaches 100% at the end
        if (m_ProgressBar != null)
        {
            m_ProgressBar.value = 100;
        }
        
        // Small delay to show 100% complete before showing result
        await Task.Delay(200);

        if (!result.isSuccess)
        {
            Label messageLabel = m_FailureOverlayElement.Q<Label>(m_MessageLabelName);
            if (messageLabel != null)
            {
                messageLabel.text = result.exception.Message;
            }

            UIToolkitUtils.ShowOverlay(m_RootElement, m_FailureOverlayElement);
            return;
        }

        if (m_SuccessOverlayElement != null)
        {
            Label messageLabel = m_SuccessOverlayElement.Q<Label>(m_MessageLabelName);
            if (messageLabel != null && result.exception != null) 
            {
                messageLabel.text = result.exception.Message;
            }

            UIToolkitUtils.ShowOverlay(m_RootElement, m_SuccessOverlayElement);
        }
        else
        {
            UIToolkitUtils.HideOverlay(m_LoadingOverlayElement);
        }
    }
    
    IEnumerator UpdateProgressBar()
    {
        if (m_ProgressBar == null)
            yield break;
            
        // Simulate loading progress - start slower and speed up toward the end
        // but never quite reach 100% until explicitly set after loading completes
        float progress = 0;
        
        while (m_IsLoading && progress < 95)
        {
            // Slow down progress as it approaches 95%
            float speed = Mathf.Lerp(40f, 5f, progress / 95f);
            progress += speed * m_ProgressUpdateInterval;
            progress = Mathf.Min(progress, 95);
            
            m_ProgressBar.value = progress;
            
            yield return new WaitForSeconds(m_ProgressUpdateInterval);
        }
    }
}

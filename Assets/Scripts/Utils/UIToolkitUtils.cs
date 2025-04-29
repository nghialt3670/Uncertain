using System;
using UnityEngine;
using UnityEngine.UIElements;

public static class UIToolkitUtils
{
    public static void ShowOverlay(VisualElement rootElement, VisualElement overlayElement)
    {
        if (rootElement.Contains(overlayElement)) return;

        overlayElement.StretchToParentSize();
        rootElement.Add(overlayElement);
    }

    public static void HideOverlay(VisualElement overlay)
    {
        overlay.parent.Remove(overlay);
    }

    public static void MonitorElementEnablement(VisualElement element, Func<bool> condition, int frequency = 10)
    {
        IVisualElementScheduledItem scheduledItem = element.schedule
            .Execute(() => element.SetEnabled(condition()))
            .Every(frequency)
            .Until(() => element.panel == null);
    }

    public static void SmoothScrollToBottom(ScrollView scrollView, float duration = 0.5f)
    {
        float elapsed = 0f;
        float t = 0f;
        float startValue = scrollView.scrollOffset.y;
        float endValue = Mathf.Max(0, scrollView.contentContainer.layout.height);

        scrollView.schedule.Execute(() =>
        {
            elapsed += Time.deltaTime;
            t = Mathf.Clamp01(elapsed / duration);
            float newY = Mathf.Lerp(startValue, endValue, t);
            scrollView.scrollOffset = new Vector2(0, newY);
        }).Every(10).Until(() => t >= 1f);
    }
}

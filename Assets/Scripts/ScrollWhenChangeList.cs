using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScrollWhenChangeList : MonoBehaviour
{
    public float m_ScrollDuration = 0.3f;
    private int m_LastChildCount = 0;


    private void Start()
    {
        m_LastChildCount = transform.childCount;
    }

    private void Update()
    {
        int currentChildCount = transform.childCount;
        
        if (currentChildCount > m_LastChildCount)
        {
            m_LastChildCount = currentChildCount;
            ScrollToBottom();
        }
    }

    private void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        if (transform.parent != null)
        {
            ScrollRect scrollRect = transform.parent.GetComponentInParent<ScrollRect>();
            if (scrollRect != null)
            {
                StartCoroutine(SmoothScrollToBottom(scrollRect));
            }
        }
    }

    private IEnumerator SmoothScrollToBottom(ScrollRect scrollRect)
    {
        float elapsedTime = 0f;
        float startPosition = scrollRect.normalizedPosition.y;
        float targetPosition = 0f;

        while (elapsedTime < m_ScrollDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / m_ScrollDuration;
            
            // Use smoothstep for easing
            float easedTime = normalizedTime * normalizedTime * (3f - 2f * normalizedTime);
            
            float newY = Mathf.Lerp(startPosition, targetPosition, easedTime);
            scrollRect.normalizedPosition = new Vector2(scrollRect.normalizedPosition.x, newY);
            yield return null;
        }

        // Ensure we end up exactly at the target position
        scrollRect.normalizedPosition = new Vector2(scrollRect.normalizedPosition.x, targetPosition);
    }
}

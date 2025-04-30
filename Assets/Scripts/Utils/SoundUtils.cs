using UnityEngine;
using System.Collections;

public static class SoundUtils
{
    /// <summary>
    /// Plays a sound with optional volume and pitch adjustments
    /// </summary>
    public static AudioSource PlaySound(this AudioSource source, AudioClip clip, float volume = 1.0f, float pitch = 1.0f)
    {
        if (source == null || clip == null)
            return null;
            
        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.Play();
        return source;
    }
    
    /// <summary>
    /// Plays a sound at a specific position in 3D space
    /// </summary>
    public static AudioSource PlayClipAtPoint(AudioClip clip, Vector3 position, float volume = 1.0f)
    {
        if (clip == null)
            return null;
            
        GameObject tempGO = new GameObject("TempAudio");
        tempGO.transform.position = position;
        AudioSource audioSource = tempGO.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.spatialBlend = 1.0f;
        audioSource.Play();
        
        Object.Destroy(tempGO, clip.length);
        return audioSource;
    }
    
    /// <summary>
    /// Fades in audio source volume over time
    /// </summary>
    public static IEnumerator FadeIn(this AudioSource audioSource, float duration, float targetVolume = 1.0f)
    {
        float startVolume = audioSource.volume;
        float timer = 0;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, timer / duration);
            yield return null;
        }
        
        audioSource.volume = targetVolume;
    }
    
    /// <summary>
    /// Fades out audio source volume over time
    /// </summary>
    public static IEnumerator FadeOut(this AudioSource audioSource, float duration)
    {
        float startVolume = audioSource.volume;
        float timer = 0;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0, timer / duration);
            yield return null;
        }
        
        audioSource.volume = 0;
        audioSource.Stop();
    }
    
    /// <summary>
    /// Adjusts audio volume based on distance from listener
    /// </summary>
    public static float CalculateVolumeByDistance(Vector3 sourcePosition, Vector3 listenerPosition, float maxDistance, float minVolume = 0, float maxVolume = 1)
    {
        float distance = Vector3.Distance(sourcePosition, listenerPosition);
        return Mathf.Lerp(maxVolume, minVolume, distance / maxDistance);
    }
}

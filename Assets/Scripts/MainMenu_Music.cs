using UnityEngine;

public class MainMenu_Music : MonoBehaviour
{
    private static AudioSource source;

    void Awake()
    {
        if (source != null && source) return;

        AudioClip clip = Resources.Load<AudioClip>("MainMenuSound");
        if (clip == null) return;

        GameObject obj = new GameObject("MainMenu_Music_AudioSource");
        source = obj.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = true;
        source.volume = 0.6f;
        source.playOnAwake = false;
        source.Play();
    }

    public static void StopMusic()
    {
        if (source != null && source)
        {
            source.Stop();
            Destroy(source.gameObject);
            source = null;
        }
    }
}

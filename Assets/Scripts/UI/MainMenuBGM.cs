using UnityEngine;

public class MainMenuBGM : MonoBehaviour
{
    [SerializeField] private AudioClip bgmClip;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = 0.32f;
    }

    private void Start()
    {
        if (bgmClip != null)
        {
            audioSource.clip = bgmClip;
            audioSource.Play();
        }
    }
}

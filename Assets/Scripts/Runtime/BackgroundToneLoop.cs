using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class BackgroundToneLoop : MonoBehaviour
{
    [SerializeField] private float volume = 0.12f;

    private void Start()
    {
        AudioSource source = GetComponent<AudioSource>();
        source.clip = ToneAudio.CreateLoop("Generated Adventure Loop", 3f, volume);
        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.Play();
    }
}

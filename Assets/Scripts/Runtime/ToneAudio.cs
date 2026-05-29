using UnityEngine;

public static class ToneAudio
{
    private const int SampleRate = 44100;

    public static AudioClip CreateTone(string name, float frequency, float duration, float volume = 0.25f)
    {
        int sampleCount = Mathf.Max(1, Mathf.RoundToInt(SampleRate * duration));
        float[] data = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)SampleRate;
            float envelope = Mathf.Clamp01(i / 220f) * Mathf.Clamp01((sampleCount - i) / 220f);
            data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * volume * envelope;
        }

        AudioClip clip = AudioClip.Create(name, sampleCount, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    public static AudioClip CreateLoop(string name, float duration = 2f, float volume = 0.08f)
    {
        int sampleCount = Mathf.Max(1, Mathf.RoundToInt(SampleRate * duration));
        float[] data = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)SampleRate;
            float pad = Mathf.Sin(2f * Mathf.PI * 110f * t) * 0.55f;
            float pulse = Mathf.Sin(2f * Mathf.PI * 220f * t) * 0.25f;
            float shimmer = Mathf.Sin(2f * Mathf.PI * 440f * t) * 0.10f;
            data[i] = (pad + pulse + shimmer) * volume;
        }

        AudioClip clip = AudioClip.Create(name, sampleCount, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    public static void PlayTone(Vector3 position, float frequency, float duration, float volume = 0.25f)
    {
        AudioSource.PlayClipAtPoint(CreateTone("Generated Tone", frequency, duration, volume), position, 1f);
    }
}

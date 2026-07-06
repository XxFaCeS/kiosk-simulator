using System.Collections.Generic;
using UnityEngine;

namespace Kiosk.Audio
{
    public enum SoundId
    {
        DoorBell, Scanner, Register, Coins, CardPayment, PackageScan,
        Lotto, CustomerHappy, CustomerUnhappy, DaySummary, Click
    }

    /// <summary>
    /// AudioManager mit prozedural erzeugten Platzhalter-Sounds (Sinustoene).
    /// Echte Audiodateien koennen spaeter in Assets/Audio abgelegt und hier
    /// den SoundIds zugewiesen werden.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        AudioSource _source;
        readonly Dictionary<SoundId, AudioClip> _clips = new Dictionary<SoundId, AudioClip>();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.spatialBlend = 0f;
            GenerateClips();
        }

        void GenerateClips()
        {
            _clips[SoundId.DoorBell] = CreateTone(new float[] { 880f, 660f }, 0.35f);
            _clips[SoundId.Scanner] = CreateTone(new float[] { 1200f }, 0.12f);
            _clips[SoundId.Register] = CreateTone(new float[] { 500f, 700f }, 0.2f);
            _clips[SoundId.Coins] = CreateTone(new float[] { 1500f, 1800f, 2100f }, 0.25f);
            _clips[SoundId.CardPayment] = CreateTone(new float[] { 950f, 950f }, 0.3f);
            _clips[SoundId.PackageScan] = CreateTone(new float[] { 800f, 1100f }, 0.2f);
            _clips[SoundId.Lotto] = CreateTone(new float[] { 600f, 800f, 1000f }, 0.35f);
            _clips[SoundId.CustomerHappy] = CreateTone(new float[] { 523f, 659f, 784f }, 0.35f);
            _clips[SoundId.CustomerUnhappy] = CreateTone(new float[] { 400f, 300f }, 0.35f);
            _clips[SoundId.DaySummary] = CreateTone(new float[] { 523f, 659f, 784f, 1047f }, 0.6f);
            _clips[SoundId.Click] = CreateTone(new float[] { 2000f }, 0.05f);
        }

        /// <summary>Erzeugt einen kurzen Ton aus einer Frequenzfolge.</summary>
        static AudioClip CreateTone(float[] frequencies, float duration)
        {
            int sampleRate = 22050;
            int total = Mathf.CeilToInt(sampleRate * duration);
            var data = new float[total];
            int perSegment = total / frequencies.Length;
            for (int i = 0; i < total; i++)
            {
                int seg = Mathf.Min(i / Mathf.Max(1, perSegment), frequencies.Length - 1);
                float t = (float)i / sampleRate;
                float envelope = 1f - (float)i / total;
                data[i] = Mathf.Sin(2f * Mathf.PI * frequencies[seg] * t) * 0.25f * envelope;
            }
            var clip = AudioClip.Create("tone", total, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public void Play(SoundId id)
        {
            AudioClip clip;
            if (_source != null && _clips.TryGetValue(id, out clip) && clip != null)
                _source.PlayOneShot(clip);
        }
    }
}

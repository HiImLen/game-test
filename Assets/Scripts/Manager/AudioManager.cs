using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Sources")]
    public AudioSource EffectsSource;
    public AudioSource MusicSource;
    public bool IsMuted = false;

    [Header("Music")]
    [SerializeField] private AudioClip[] musicTracks;
    [SerializeField] private AudioClip burshHitEffect;
    [SerializeField] private AudioClip brushHitWallEffect;
    [SerializeField] private AudioClip winEffects;
    [SerializeField] private AudioClip loseEffects;
    private int _currentTrack = 0;

    [Header("Pitch")]
    public float LowPitchRange = .95f;
    public float HighPitchRange = 1.05f;

    void Update()
    {
        if (musicTracks == null) return;
        if (!MusicSource.isPlaying)
        {
            _currentTrack++;
            if (_currentTrack >= musicTracks.Length)
            {
                _currentTrack = 0;
            }
            MusicSource.clip = musicTracks[_currentTrack];
            MusicSource.Play();
        }
    }
    // Play a single clip through the sound effects source.
    public void PlayEffect(AudioClip clip)
    {
        EffectsSource.PlayOneShot(clip);
    }
    public void PlayBrushHitEffect()
    {
        EffectsSource.PlayOneShot(burshHitEffect);
    }
    public void PlayBrushHitWallEffect()
    {
        EffectsSource.PlayOneShot(brushHitWallEffect);
    }
    public void PlayWinEffect()
    {
        // Wait for 0.5s and play win effect
        StartCoroutine(PlayWinEffectCoroutine());
    }
    IEnumerator PlayWinEffectCoroutine()
    {
        yield return new WaitForSeconds(0.45f);
        EffectsSource.PlayOneShot(winEffects);
    }
    public void PlayLoseEffect()
    {
        EffectsSource.PlayOneShot(loseEffects);
    }
    // Play a single clip through the music source.
    public void PlayMusic(AudioClip[] clips)
    {
        musicTracks = clips;
        MusicSource.clip = musicTracks[_currentTrack];
        MusicSource.Play();
    }
    // Play a random clip from an array, and randomize the pitch slightly.
    public void RandomSoundEffect(params AudioClip[] clips)
    {
        int randomIndex = Random.Range(0, clips.Length);
        float randomPitch = Random.Range(LowPitchRange, HighPitchRange);
        EffectsSource.pitch = randomPitch;
        EffectsSource.clip = clips[randomIndex];
        EffectsSource.Play();
    }
    public void StopMusic()
    {
        musicTracks = null;
        MusicSource.Stop();
        _currentTrack = 0;
    }
    public void SetVolume(float volume)
    {
        EffectsSource.volume = volume;
        MusicSource.volume = volume;
    }

    public void ToggleMute()
    {
        IsMuted = !IsMuted;
        EffectsSource.mute = IsMuted;
        MusicSource.mute = IsMuted;
    }
}
using UnityEngine;
using System.Collections.Generic;

public enum SFXType
{
    ClickButton = 0,
    Equip = 1,
    Buy = 2,
    Popup = 3,
    GetCoin = 4,
    Trick = 5,
    StartCountDown = 6,
    EndCountDown = 7,
    RaceFinish = 8,
    Clap = 9,
    Flip = 10,
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("BGM Clip")]
    [SerializeField] private AudioClip bgmClip;

    [Header("SFX Clips (Index Based)")]
    [SerializeField] private List<AudioClip> sfxClips = new List<AudioClip>();

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 안전장치
        if (sfxSource == null) sfxSource = GetComponent<AudioSource>();
        if (bgmSource == null) bgmSource = GetComponent<AudioSource>();
    }

    void Start() {
        PlayBGM();
    }

    public void PlayBGM() {
        if (bgmSource != null && bgmClip != null) {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }
    public void PlaySFX(int index) {
        if (index < 0 || index >= sfxClips.Count || sfxClips[index] == null) return;
        sfxSource.PlayOneShot(sfxClips[index]);
    }
    public void PlaySFX(SFXType type) {
        PlaySFX((int)type);
    }
}

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 사운드 시스템 싱글톤 (기획서 17장).
/// v2: AI 생성 에셋 적용 + 볼륨 밸런스 + 제외 사운드 처리
///
/// 변경점:
/// - 제외된 사운드(BossAppear, MeteorRain, WaveStart, StageClear)는 프로시저럴도 생성 안 함
/// - 크리티컬 히트 볼륨 낮춤
/// - 전체 sfxVolume 기본값 0.45로 하향
/// - 개별 볼륨 스케일 세분화
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    // ─── BGM ────────────────────────────────────────────
    [Header("BGM")]
    [SerializeField] private AudioClip bgmBattle;
    [SerializeField] private AudioClip bgmBoss;
    [SerializeField] private float bgmVolume = 0.4f;

    // ─── Combat SFX ─────────────────────────────────────
    [Header("Combat SFX")]
    [SerializeField] private AudioClip sfxPlayerAttack;
    [SerializeField] private AudioClip sfxEnemyHit;
    [SerializeField] private AudioClip sfxEnemyDeath;
    [SerializeField] private AudioClip sfxCriticalHit;
    [SerializeField] private AudioClip sfxBossAppear;
    [SerializeField] private AudioClip sfxBossDefeat;

    // ─── Skill SFX ──────────────────────────────────────
    [Header("Skill SFX")]
    [SerializeField] private AudioClip sfxStaffRush;
    [SerializeField] private AudioClip sfxMeteorRain;
    [SerializeField] private AudioClip sfxMeteorExplosion;
    [SerializeField] private AudioClip sfxChainLightning;
    [SerializeField] private AudioClip sfxBerserkerDash;
    [SerializeField] private AudioClip sfxManualAttack;
    [SerializeField] private AudioClip sfxBuffActivate;

    // ─── System SFX ─────────────────────────────────────
    [Header("System SFX")]
    [SerializeField] private AudioClip sfxGoldPickup;
    [SerializeField] private AudioClip sfxLevelUp;
    [SerializeField] private AudioClip sfxUpgradeSuccess;
    [SerializeField] private AudioClip sfxWaveStart;
    [SerializeField] private AudioClip sfxStageClear;
    [SerializeField] private AudioClip sfxOfflineReward;

    // ─── Settings ───────────────────────────────────────
    [Header("Settings")]
    [SerializeField] private float sfxVolume = 0.45f;   // v2: 0.7 → 0.45 (전체 소리 줄임)
    [SerializeField] private int sfxPoolSize = 8;

    private AudioSource bgmSource;
    private AudioSource[] sfxPool;
    private int sfxPoolIndex;

    // ─── SFX 빈도 제어 ─────────────────────────────────
    private Dictionary<string, float> lastPlayTime = new Dictionary<string, float>();

    // ================================================================
    //  Lifecycle
    // ================================================================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // BGM AudioSource
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.volume = bgmVolume;

        // SFX Pool
        sfxPool = new AudioSource[sfxPoolSize];
        for (int i = 0; i < sfxPoolSize; i++)
        {
            sfxPool[i] = gameObject.AddComponent<AudioSource>();
            sfxPool[i].playOnAwake = false;
            sfxPool[i].volume = sfxVolume;
        }

        // 빈 슬롯은 프로시저럴 사운드로 자동 채움 (제외 대상은 건너뜀)
        GenerateProceduralSounds();

        LoadVolumeSettings();
    }

    private void Start()
    {
        PlayBGM(bgmBattle);
    }

    // ================================================================
    //  BGM
    // ================================================================

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;
        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void PlayBossBGM()
    {
        // 통일: 전투 BGM 유지, 전환 안 함
    }

    public void PlayBattleBGM()
    {
        // 통일: 이미 재생 중이므로 다시 호출할 필요 없음
    }

    // ================================================================
    //  SFX — 내부 재생
    // ================================================================

    private void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;
        AudioSource source = sfxPool[sfxPoolIndex];
        sfxPoolIndex = (sfxPoolIndex + 1) % sfxPoolSize;
        source.pitch = Random.Range(0.93f, 1.07f);
        source.PlayOneShot(clip, sfxVolume * volumeScale);
    }

    private bool CanPlaySFX(string id, float minInterval)
    {
        float now = Time.unscaledTime;
        if (lastPlayTime.TryGetValue(id, out float last))
        {
            if (now - last < minInterval) return false;
        }
        lastPlayTime[id] = now;
        return true;
    }

    // ================================================================
    //  Combat SFX — v2 볼륨 밸런스 적용
    // ================================================================

    public void PlayPlayerAttack() => PlaySFX(sfxPlayerAttack, 0.7f);

    public void PlayEnemyHit()
    {
        if (CanPlaySFX("EnemyHit", 0.05f))
            PlaySFX(sfxEnemyHit, 0.45f);        // 가장 빈번 → 낮게
    }

    public void PlayEnemyDeath()
    {
        if (CanPlaySFX("EnemyDeath", 0.03f))
            PlaySFX(sfxEnemyDeath, 0.6f);
    }

    public void PlayCriticalHit()
    {
        // 제외 — 사용자 피드백
    }

    public void PlayBossAppear()
    {
        // v2: 제외 — 듣기 싫은 소리
    }

    public void PlayBossDefeat() => PlaySFX(sfxBossDefeat, 0.8f);

    // ================================================================
    //  Skill SFX — v2 볼륨 밸런스 적용
    // ================================================================

    public void PlayStaffRush()       => PlaySFX(sfxStaffRush, 0.6f);

    public void PlayMeteorRain()
    {
        // v2: 제외 — 불필요
    }

    public void PlayMeteorExplosion()
    {
        if (CanPlaySFX("MeteorExplosion", 0.08f))
            PlaySFX(sfxMeteorExplosion, 0.55f);
    }

    public void PlayChainLightning()  => PlaySFX(sfxChainLightning, 0.6f);
    public void PlayBerserkerDash()   => PlaySFX(sfxBerserkerDash, 0.65f);
    public void PlayManualAttack()    => PlaySFX(sfxManualAttack, 0.7f);
    public void PlayBuffActivate()    => PlaySFX(sfxBuffActivate, 0.6f);

    // ================================================================
    //  System SFX — v2 볼륨 밸런스 적용
    // ================================================================

    public void PlayGoldPickup()
    {
        if (CanPlaySFX("GoldPickup", 0.1f))
            PlaySFX(sfxGoldPickup, 0.3f);        // 가장 빈번한 시스템 소리 → 매우 낮게
    }

    public void PlayLevelUp()        => PlaySFX(sfxLevelUp, 0.75f);
    public void PlayUpgradeSuccess() => PlaySFX(sfxUpgradeSuccess, 0.7f);

    public void PlayWaveStart()
    {
        // v2: 제외
    }

    public void PlayStageClear()
    {
        // v2: 제외
    }

    public void PlayOfflineReward()  => PlaySFX(sfxOfflineReward, 0.7f);

    // ================================================================
    //  Volume Control
    // ================================================================

    public void SetBGMVolume(float vol)
    {
        bgmVolume = Mathf.Clamp01(vol);
        bgmSource.volume = bgmVolume;
        PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
    }

    public void SetSFXVolume(float vol)
    {
        sfxVolume = Mathf.Clamp01(vol);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
    }

    private void LoadVolumeSettings()
    {
        if (PlayerPrefs.HasKey("BGMVolume"))
            bgmVolume = PlayerPrefs.GetFloat("BGMVolume");
        if (PlayerPrefs.HasKey("SFXVolume"))
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume");
        bgmSource.volume = bgmVolume;
    }

    // ================================================================
    //  프로시저럴 사운드 — v2: 제외 대상은 생성하지 않음
    // ================================================================

    private void GenerateProceduralSounds()
    {
        // Combat
        if (sfxPlayerAttack   == null) sfxPlayerAttack   = ProceduralSoundLibrary.GenerateHitSound();
        if (sfxEnemyHit       == null) sfxEnemyHit       = ProceduralSoundLibrary.GenerateHitSound();
        if (sfxEnemyDeath     == null) sfxEnemyDeath     = ProceduralSoundLibrary.GenerateDeathSound();
        // sfxCriticalHit — 제외
        // sfxBossAppear — 제외
        if (sfxBossDefeat     == null) sfxBossDefeat     = ProceduralSoundLibrary.GenerateLevelUpSound();

        // Skills
        if (sfxStaffRush       == null) sfxStaffRush       = ProceduralSoundLibrary.GenerateSkillShootSound();
        // sfxMeteorRain — 제외
        if (sfxMeteorExplosion == null) sfxMeteorExplosion = ProceduralSoundLibrary.GenerateExplosionSound();
        if (sfxChainLightning  == null) sfxChainLightning  = ProceduralSoundLibrary.GenerateElectricSound();
        if (sfxBerserkerDash   == null) sfxBerserkerDash   = ProceduralSoundLibrary.GenerateDashSound();
        if (sfxManualAttack    == null) sfxManualAttack    = ProceduralSoundLibrary.GenerateExplosionSound();
        if (sfxBuffActivate    == null) sfxBuffActivate    = ProceduralSoundLibrary.GenerateBuffSound();

        // System
        if (sfxGoldPickup      == null) sfxGoldPickup      = ProceduralSoundLibrary.GenerateCoinSound();
        if (sfxLevelUp         == null) sfxLevelUp         = ProceduralSoundLibrary.GenerateLevelUpSound();
        if (sfxUpgradeSuccess  == null) sfxUpgradeSuccess  = ProceduralSoundLibrary.GenerateUpgradeSound();
        // sfxWaveStart — 제외
        // sfxStageClear — 제외
        if (sfxOfflineReward   == null) sfxOfflineReward   = ProceduralSoundLibrary.GenerateCoinSound();
    }
}

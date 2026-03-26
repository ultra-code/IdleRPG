using UnityEngine;

/// <summary>
/// 실제 사운드 에셋 없이 테스트 가능하도록
/// AudioClip.Create()로 기본 파형을 코드 생성하는 유틸리티.
/// 13종의 프로시저럴 사운드를 제공한다.
/// </summary>
public static class ProceduralSoundLibrary
{
    private const int SampleRate = 44100;

    // ================================================================
    // 타격음 — 짧은 노이즈 버스트 (0.08초)
    // ================================================================
    public static AudioClip GenerateHitSound()
    {
        int samples = (int)(SampleRate * 0.08f);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float envelope = 1f - t;
            data[i] = Random.Range(-1f, 1f) * envelope * 0.5f;
        }
        AudioClip clip = AudioClip.Create("Hit", samples, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    // ================================================================
    // 사망음 — 낮은 톤 노이즈 + 피치 하강 (0.15초)
    // ================================================================
    public static AudioClip GenerateDeathSound()
    {
        int samples = (int)(SampleRate * 0.15f);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float envelope = (1f - t) * (1f - t);
            float freq = 120f - t * 80f;
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.4f
                     + Random.Range(-0.15f, 0.15f) * envelope;
        }
        AudioClip clip = AudioClip.Create("Death", samples, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    // ================================================================
    // 크리티컬 — 높은 핑 (0.12초)
    // ================================================================
    public static AudioClip GenerateCriticalSound()
    {
        int samples = (int)(SampleRate * 0.12f);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float envelope = (1f - t);
            data[i] = Mathf.Sin(2f * Mathf.PI * 880f * t) * envelope * 0.3f
                     + Mathf.Sin(2f * Mathf.PI * 1320f * t) * envelope * 0.2f;
        }
        AudioClip clip = AudioClip.Create("Critical", samples, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    // ================================================================
    // 골드 획득 — 짧은 코인 소리 (0.06초)
    // ================================================================
    public static AudioClip GenerateCoinSound()
    {
        int samples = (int)(SampleRate * 0.06f);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float envelope = (1f - t);
            data[i] = Mathf.Sin(2f * Mathf.PI * 2200f * t) * envelope * 0.25f;
        }
        AudioClip clip = AudioClip.Create("Coin", samples, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    // ================================================================
    // 레벨업 — 상승 팡파르 (0.4초)
    // ================================================================
    public static AudioClip GenerateLevelUpSound()
    {
        int samples = (int)(SampleRate * 0.4f);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float envelope = t < 0.1f ? t / 0.1f : (1f - (t - 0.1f) / 0.9f);
            float freq = 440f + t * 440f;
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.3f
                     + Mathf.Sin(2f * Mathf.PI * freq * 1.5f * t) * envelope * 0.15f;
        }
        AudioClip clip = AudioClip.Create("LevelUp", samples, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    // ================================================================
    // 강화 성공 — 파워업 소리 (0.25초)
    // ================================================================
    public static AudioClip GenerateUpgradeSound()
    {
        int samples = (int)(SampleRate * 0.25f);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float envelope = t < 0.05f ? t / 0.05f : (1f - (t - 0.05f) / 0.95f);
            float freq = 660f + t * 330f;
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.3f;
        }
        AudioClip clip = AudioClip.Create("Upgrade", samples, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    // ================================================================
    // 스킬 발사 — 슈팅/관통 (0.2초)
    // ================================================================
    public static AudioClip GenerateSkillShootSound()
    {
        int samples = (int)(SampleRate * 0.2f);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float envelope = (1f - t) * (1f - t);
            float freq = 600f - t * 400f;
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.3f
                     + Random.Range(-0.1f, 0.1f) * envelope;
        }
        AudioClip clip = AudioClip.Create("SkillShoot", samples, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    // ================================================================
    // 폭발 — 유성 착탄 (0.3초)
    // ================================================================
    public static AudioClip GenerateExplosionSound()
    {
        int samples = (int)(SampleRate * 0.3f);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float envelope = t < 0.02f ? t / 0.02f : Mathf.Pow(1f - t, 2f);
            float low = Mathf.Sin(2f * Mathf.PI * 60f * t) * 0.4f;
            float noise = Random.Range(-1f, 1f) * 0.3f;
            data[i] = (low + noise) * envelope * 0.5f;
        }
        AudioClip clip = AudioClip.Create("Explosion", samples, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    // ================================================================
    // 전기 — 체인 라이트닝 (0.15초)
    // ================================================================
    public static AudioClip GenerateElectricSound()
    {
        int samples = (int)(SampleRate * 0.15f);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float envelope = (1f - t);
            float buzz = Mathf.Sin(2f * Mathf.PI * 180f * t) * 0.2f;
            float crackle = Random.Range(-1f, 1f) * (Random.value > 0.7f ? 0.5f : 0.1f);
            data[i] = (buzz + crackle) * envelope * 0.4f;
        }
        AudioClip clip = AudioClip.Create("Electric", samples, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    // ================================================================
    // 돌진 — 윙 소리 (0.3초)
    // ================================================================
    public static AudioClip GenerateDashSound()
    {
        int samples = (int)(SampleRate * 0.3f);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float envelope = Mathf.Sin(t * Mathf.PI);
            float freq = 200f + t * 300f;
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.15f
                     + Random.Range(-0.1f, 0.1f) * envelope;
        }
        AudioClip clip = AudioClip.Create("Dash", samples, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    // ================================================================
    // 버프 발동 — 파워 차지 (0.35초)
    // ================================================================
    public static AudioClip GenerateBuffSound()
    {
        int samples = (int)(SampleRate * 0.35f);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float envelope = t < 0.3f ? t / 0.3f : (1f - (t - 0.3f) / 0.7f);
            float freq = 330f + t * 550f;
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.25f
                     + Mathf.Sin(2f * Mathf.PI * freq * 2f * t) * envelope * 0.1f;
        }
        AudioClip clip = AudioClip.Create("Buff", samples, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    // ================================================================
    // 보스 등장 — 위압적인 저음 (0.6초)
    // ================================================================
    public static AudioClip GenerateBossAppearSound()
    {
        int samples = (int)(SampleRate * 0.6f);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float envelope = t < 0.1f ? t / 0.1f : (1f - (t - 0.1f) / 0.9f);
            data[i] = Mathf.Sin(2f * Mathf.PI * 55f * t) * envelope * 0.5f
                     + Mathf.Sin(2f * Mathf.PI * 82.5f * t) * envelope * 0.25f
                     + Random.Range(-0.05f, 0.05f) * envelope;
        }
        AudioClip clip = AudioClip.Create("BossAppear", samples, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    // ================================================================
    // 웨이브 시작 — 경고음 (0.15초)
    // ================================================================
    public static AudioClip GenerateWaveStartSound()
    {
        int samples = (int)(SampleRate * 0.15f);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float envelope = 1f - t;
            data[i] = Mathf.Sin(2f * Mathf.PI * 520f * t) * envelope * 0.2f;
        }
        AudioClip clip = AudioClip.Create("WaveStart", samples, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}

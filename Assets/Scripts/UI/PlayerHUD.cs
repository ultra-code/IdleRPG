using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private CharacterStats player;
    [SerializeField] private WaveSpawnManager waveManager;

    [Header("텍스트")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI atkText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI stageWaveText;

    [Header("바")]
    [SerializeField] private Image hpFill;
    [SerializeField] private Image expFill;

    private void Start()
    {
        if (player == null)
            player = FindAnyObjectByType<CharacterStats>();
        if (waveManager == null)
            waveManager = FindAnyObjectByType<WaveSpawnManager>();

        if (waveManager != null)
        {
            waveManager.OnStageChanged += _ => UpdateStageWave();
            waveManager.OnWaveStarted += _ => UpdateStageWave();
        }

        UpdateStageWave();
    }

    private void Update()
    {
        if (player == null) return;

        levelText.text = $"Lv.{player.Level}";
        hpText.text = $"{Mathf.Ceil(player.HP)} / {player.MaxHP}";
        atkText.text = $"ATK {player.ATK:F0}";
        goldText.text = $"{player.Gold:F0} G";

        hpFill.fillAmount = player.MaxHP > 0 ? player.HP / player.MaxHP : 0f;
        expFill.fillAmount = player.ExpToNextLevel > 0 ? player.Exp / player.ExpToNextLevel : 0f;
    }

    private void UpdateStageWave()
    {
        if (waveManager == null) return;
        stageWaveText.text = $"Stage {waveManager.CurrentStage} - Wave {waveManager.CurrentWave}/{waveManager.WavesPerStage}";
    }
}

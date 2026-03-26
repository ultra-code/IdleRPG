using UnityEngine;
using TMPro;

public class StageUI : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private WaveSpawnManager battleSystem;

    [Header("UI 요소")]
    [SerializeField] private TextMeshProUGUI stageText;

    private void Start()
    {
        if (battleSystem == null)
            battleSystem = FindAnyObjectByType<WaveSpawnManager>();

        battleSystem.OnStageChanged += HandleStageChanged;
        UpdateStageText(battleSystem.CurrentStage);
    }

    private void OnDestroy()
    {
        if (battleSystem != null)
            battleSystem.OnStageChanged -= HandleStageChanged;
    }

    private void HandleStageChanged(int stage)
    {
        UpdateStageText(stage);
    }

    private void UpdateStageText(int stage)
    {
        stageText.text = $"Stage {stage}";
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHPBar : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private AutoBattleSystem battleSystem;

    [Header("UI 요소")]
    [SerializeField] private Image hpFill;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI hpText;

    private Enemy currentEnemy;

    private void Start()
    {
        if (battleSystem == null)
            battleSystem = FindAnyObjectByType<AutoBattleSystem>();

        battleSystem.OnEnemySpawned += HandleEnemySpawned;
        SetVisible(false);
    }

    private void OnDestroy()
    {
        if (battleSystem != null)
            battleSystem.OnEnemySpawned -= HandleEnemySpawned;
    }

    private void HandleEnemySpawned(Enemy enemy)
    {
        currentEnemy = enemy;
        nameText.text = $"{enemy.EnemyName} Lv.{enemy.Level}";
        SetVisible(true);
    }

    private void Update()
    {
        if (currentEnemy == null || currentEnemy.IsDead)
        {
            SetVisible(false);
            return;
        }

        hpFill.fillAmount = currentEnemy.HPRatio;
        hpText.text = $"{Mathf.Ceil(currentEnemy.CurrentHP)} / {currentEnemy.MaxHP}";
    }

    private void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}

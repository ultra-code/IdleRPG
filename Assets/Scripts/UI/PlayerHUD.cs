using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private CharacterStats player;

    [Header("텍스트")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI atkText;
    [SerializeField] private TextMeshProUGUI defText;
    [SerializeField] private TextMeshProUGUI goldText;

    [Header("바")]
    [SerializeField] private Image hpFill;
    [SerializeField] private Image expFill;

    private void Start()
    {
        if (player == null)
            player = FindAnyObjectByType<CharacterStats>();
    }

    private void Update()
    {
        if (player == null) return;

        levelText.text = $"Lv.{player.Level}";
        hpText.text = $"{Mathf.Ceil(player.HP)} / {player.MaxHP}";
        atkText.text = $"ATK {player.ATK:F0}";
        defText.text = $"DEF {player.DEF:F0}";
        goldText.text = $"Gold {player.Gold:F0}";

        hpFill.fillAmount = player.MaxHP > 0 ? player.HP / player.MaxHP : 0f;
        expFill.fillAmount = player.ExpToNextLevel > 0 ? player.Exp / player.ExpToNextLevel : 0f;
    }
}

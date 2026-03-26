using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manual physical attack button — completely independent from auto-attack and skill systems.
/// Attach to a UI Button. Does NOT modify any existing scripts.
/// </summary>
public class ManualAttackButton : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private float damageMultiplier = 10.0f;
    [SerializeField] private float attackRange = 2.0f;

    [Header("Animation")]
    [SerializeField] private Animator heroAnimator;
    [SerializeField] private string attackTriggerName = "Attack";
    [SerializeField] private float fallbackAnimDuration = 0.5f;

    [Header("UI")]
    [SerializeField] private Button attackButton;
    [SerializeField] private Image buttonBackground;
    [SerializeField] private Color activeColor = new Color(0.24f, 0.13f, 0.32f, 1f);   // #3D2252
    [SerializeField] private Color disabledColor = new Color(0.35f, 0.35f, 0.35f, 1f);

    private CharacterStats player;
    private WaveSpawnManager waveManager;
    private bool isAttacking;

    private void Start()
    {
        player = FindAnyObjectByType<CharacterStats>();
        waveManager = FindAnyObjectByType<WaveSpawnManager>();

        if (heroAnimator == null && player != null)
            heroAnimator = player.GetComponent<Animator>();

        if (attackButton == null)
            attackButton = GetComponent<Button>();

        if (attackButton != null)
            attackButton.onClick.AddListener(OnAttackClicked);
    }

    private void OnAttackClicked()
    {
        if (isAttacking || player == null || player.IsDead) return;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayManualAttack();

        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        SetButtonInteractable(false);

        // Trigger animation
        float animDuration = fallbackAnimDuration;
        if (heroAnimator != null && HasParameter(heroAnimator, attackTriggerName))
        {
            heroAnimator.SetTrigger(attackTriggerName);

            // Wait one frame for state to transition
            yield return null;
            yield return null;

            AnimatorStateInfo state = heroAnimator.GetCurrentAnimatorStateInfo(0);
            animDuration = state.length > 0.05f ? state.length : fallbackAnimDuration;
        }
        else
        {
            Debug.LogWarning("ManualAttackButton: Animator or trigger not found, using fallback delay");
        }

        // Deal damage at ~30% into the animation (impact moment)
        yield return new WaitForSeconds(animDuration * 0.3f);
        DealDamage();

        // Wait for rest of animation
        yield return new WaitForSeconds(animDuration * 0.7f);

        isAttacking = false;
        SetButtonInteractable(true);
    }

    private void DealDamage()
    {
        if (player == null || waveManager == null) return;

        float damage = player.ATK * damageMultiplier;
        Vector2 playerPos = player.transform.position;
        int hitCount = 0;

        var snapshot = new System.Collections.Generic.List<Enemy>(waveManager.ActiveEnemies);
        foreach (Enemy enemy in snapshot)
        {
            if (enemy == null || enemy.IsDead) continue;
            float dist = Vector2.Distance(playerPos, (Vector2)enemy.transform.position);
            if (dist <= attackRange)
            {
                enemy.TakeDamage(damage, DamagePopupType.Skill);
                hitCount++;
            }
        }

        Debug.Log($"ManualAttack: {damage:F0} dmg to {hitCount} enemies");
    }

    private void SetButtonInteractable(bool interactable)
    {
        if (attackButton != null)
            attackButton.interactable = interactable;

        if (buttonBackground != null)
            buttonBackground.color = interactable ? activeColor : disabledColor;
    }

    private static bool HasParameter(Animator animator, string paramName)
    {
        foreach (var param in animator.parameters)
        {
            if (param.name == paramName) return true;
        }
        return false;
    }
}

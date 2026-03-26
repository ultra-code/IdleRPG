using UnityEngine;

public enum SkillType
{
    FanShape,
    Circle,
    Chain,
    Buff
}

[CreateAssetMenu(fileName = "NewSkillData", menuName = "IdleRPG/Skill Data")]
public class SkillData : ScriptableObject
{
    public string skillName = "Skill";
    public SkillType skillType = SkillType.FanShape;

    [Header("쿨타임")]
    public float cooldown = 4f;

    [Header("데미지")]
    public float damageMultiplier = 1.5f;

    [Header("범위")]
    public float range = 3f;
    public float angle = 60f;

    [Header("연쇄 (Chain)")]
    public int chainCount = 5;

    [Header("버프 (Buff)")]
    public float buffDuration = 8f;

    [Header("이펙트")]
    public GameObject effectPrefab;
    public AudioClip soundClip;
}

# 플레이 테스트 피드백 수정 작업 지시서

프로젝트: C:/UnityPhase1/IdleRPG
먼저 HANDOFF.md를 읽어서 현재 상태를 파악하고, 아래 작업을 순서대로 진행해줘.

---

## 작업 1: 보스 팝업 UI 한글 깨짐 수정 (5분)

BossUIController.cs에서 한글이 사용되는 텍스트를 전부 영문으로 변경해줘.
현재 프로젝트는 TMP 한글 폰트가 없어서 런타임 생성 텍스트에 한글을 쓰면 깨진다.

변경 예시:
- "보스" → "BOSS"
- "보스 등장!" → "BOSS!"
- "보스 처치!" → "BOSS DEFEATED!"
- "HP" → 그대로 유지
- 기타 한글 문자열 → 적절한 영문으로

PlayerHUD.cs, ShopUI.cs 등 다른 UI 스크립트에도 한글 런타임 텍스트가 있으면 같이 수정해줘.

---

## 작업 2: 몬스터 크기 차별화 (10분)

기획서에서 적 5종은 역할군이 다르다. 크기로 시각적 차별화를 줘야 한다.

### 2-1. EnemyData.cs에 Scale 필드 추가

```csharp
[Header("비주얼")]
public float SpriteScale = 1.0f;
```

### 2-2. Enemy.cs Initialize에서 Scale 적용

Initialize(EnemyData, StageData) 메서드와 Initialize(EnemyData, int level) 메서드 모두에서:
```csharp
transform.localScale = Vector3.one * data.SpriteScale;
```

보스는 추가로 1.5배 크게:
```csharp
// InitializeAsBoss 또는 ConfigureAsBoss에서:
transform.localScale *= 1.5f;
```

### 2-3. EnemyData 에셋 5개의 SpriteScale 값 설정

| 에셋 | SpriteScale | 근거 |
|------|------------|------|
| EnemyData_Slime (살점 슬라임) | 1.0 | 기본 크기 |
| EnemyData_Bat (눈알 스토커) | 0.85 | 돌격형, 작고 빠른 느낌 |
| EnemyData_Goblin (혈구 군체) | 0.75 | 군집형, 작지만 여럿 |
| EnemyData_SkeletonWarrior (스켈레톤 악마) | 1.4 | 탱커, 크고 묵직 |
| EnemyData_Golem (쌍두 변이체) | 1.6 | 엘리트, 가장 큼 |

unity-cli로 에셋 값 변경하거나, DesignSyncUtility에 추가해줘.

---

## 작업 3: 몬스터 스폰 수 증가 (5분)

현재 StageData의 MinEnemies/MaxEnemies 값을 전체적으로 상향 조정해줘.
"쓸어버리는 쾌감"을 위해 화면에 적이 더 많이 있어야 한다.

변경 기준:

| Stage | 현재 Min/Max | 변경 Min/Max |
|-------|------------|------------|
| 1~5 | 2~3 → 3~5 | 3~5 → 4~6 |
| 6~10 | 3~5 | 4~7 |
| 11~20 | 4~6 | 5~8 |
| 21~30 | 5~7 | 6~9 |

DesignSyncUtility.cs를 수정하고 다시 실행하거나, StageData 에셋을 직접 수정해줘.

---

## 작업 4: 플레이어 자동 이동 추가 (15분)

현재 플레이어는 제자리에서만 공격한다. 기획서 5장에도 "가장 가까운 적을 탐색 → 해당 적 방향으로 자동 이동 → attackRange 이내 도달 시 자동 공격"으로 정의되어 있다.

### 4-1. CharacterStats.cs 또는 새 PlayerMovement.cs에 이동 로직 추가

핵심 로직:
```csharp
// 매 FixedUpdate에서:
// 1. WaveSpawnManager의 ActiveEnemies에서 가장 가까운 적 찾기
// 2. 거리가 attackRange보다 멀면 적 방향으로 이동
// 3. attackRange 이내이면 이동 중지 (공격은 WaveSpawnManager가 처리)

[Header("이동")]
[SerializeField] private float moveSpeed = 3.0f;
[SerializeField] private float attackRange = 1.5f;

private WaveSpawnManager waveManager;
private Rigidbody2D rb;
private SpriteRenderer spriteRenderer;

private void FixedUpdate()
{
    if (IsDead) return;
    if (waveManager == null) waveManager = FindAnyObjectByType<WaveSpawnManager>();
    if (waveManager == null || waveManager.ActiveEnemies.Count == 0) return;

    // 가장 가까운 적 찾기
    Enemy closest = null;
    float closestDist = float.MaxValue;
    foreach (var enemy in waveManager.ActiveEnemies)
    {
        if (enemy == null || enemy.IsDead) continue;
        float dist = Vector2.Distance(transform.position, enemy.transform.position);
        if (dist < closestDist) { closestDist = dist; closest = enemy; }
    }

    if (closest == null) return;

    // attackRange 밖이면 이동
    if (closestDist > attackRange)
    {
        Vector2 dir = ((Vector2)closest.transform.position - (Vector2)transform.position).normalized;
        rb.MovePosition(rb.position + dir * moveSpeed * Time.fixedDeltaTime);

        // 방향 전환
        if (spriteRenderer != null)
            spriteRenderer.flipX = dir.x < 0;
    }
}
```

### 4-2. 주의사항
- Player에 Rigidbody2D가 없으면 추가 (gravityScale=0, freezeRotation=true)
- WaveSpawnManager의 attackRadius와 이 attackRange를 맞춰야 한다
- 이동 중에도 WaveSpawnManager의 자동 공격은 독립적으로 작동해야 한다

---

## 작업 5: 번개 돌진 스킬 (20분, 선택)

이것은 기획서에 없는 새 스킬이므로, 시간이 남으면 진행.
기존 스킬 4종에 5번째 스킬을 추가하는 것이 아니라, 플레이어 이동에 번개 이펙트를 입히는 방식이 더 자연스럽다.

### 5-1. 이동 시 잔상 이펙트 (간이 구현)

```csharp
// PlayerMovement에서 이동 중일 때:
// 0.1초 간격으로 현재 위치에 반투명 스프라이트 잔상 생성
// 잔상은 0.3초 후 페이드아웃되며 자동 삭제
```

### 5-2. 돌진 연출
- 마력 폭주(Buff) 활성화 시 moveSpeed를 2배로 올리고
- 이동 경로에 번개 색상(파란색) 잔상을 남기면 "번개 돌진" 느낌이 난다

이 작업은 코드만으로 가능하지만, 비주얼 품질은 스프라이트 리소스에 의존한다.

---

## 완료 후 확인

1. Play 모드에서 보스 팝업에 한글 깨짐이 없는지 확인
2. 몬스터 크기가 종류별로 다른지 확인
3. 화면에 적이 더 많이 나오는지 확인
4. 플레이어가 적을 향해 이동하는지 확인
5. 이동 후에도 자동 공격/스킬이 정상 작동하는지 확인

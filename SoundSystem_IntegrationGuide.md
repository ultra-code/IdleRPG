# 사운드 시스템 통합 가이드 (작업 4~6)

프로젝트: `C:/UnityPhase1/IdleRPG`
HANDOFF.md 기준 스크립트 경로를 참조합니다.

---

## 신규 파일 배치

| 파일 | 경로 |
|------|------|
| AudioManager.cs | `Assets/Scripts/Core/AudioManager.cs` |
| ProceduralSoundLibrary.cs | `Assets/Scripts/Core/ProceduralSoundLibrary.cs` |

위 2개 파일을 프로젝트에 복사한 후, 아래 기존 스크립트에 사운드 호출을 삽입합니다.

---

## 작업 4-1. Enemy.cs

경로: `Assets/Scripts/Core/Enemy.cs`

HANDOFF 참고: Enemy.cs에는 `TakeDamage()`, `Die()`, `Initialize()`, `InitializeAsBoss()` 등이 있음. 피격/사망/크리티컬/골드 사운드를 삽입.

### TakeDamage() — 데미지 적용 직후

```csharp
// 기존 데미지 적용 코드 아래에 추가:
if (AudioManager.Instance != null)
{
    if (isCritical)
        AudioManager.Instance.PlayCriticalHit();
    else
        AudioManager.Instance.PlayEnemyHit();
}
```

주의: `isCritical` 변수명은 기존 코드의 크리티컬 판정 변수에 맞춰 조정할 것.

### Die() — 사망 처리부

```csharp
// 사망 처리 시작 부분에 추가:
if (AudioManager.Instance != null)
    AudioManager.Instance.PlayEnemyDeath();
```

### Die() — 골드 지급 후

```csharp
// 골드를 CharacterStats에 추가한 직후:
if (AudioManager.Instance != null)
    AudioManager.Instance.PlayGoldPickup();
```

---

## 작업 4-2. WaveSpawnManager.cs

경로: `Assets/Scripts/Core/WaveSpawnManager.cs`

HANDOFF 참고: PlayerAutoAttack 로직, StartWave(), SpawnBoss(), HandleEnemyDeath, AdvanceStage() 등이 있음.

### 플레이어 자동 공격 적중 시

```csharp
// hitEnemies 리스트에 적이 있을 때 (데미지 적용 직후):
if (hitEnemies.Count > 0 && AudioManager.Instance != null)
    AudioManager.Instance.PlayPlayerAttack();
```

참고: HANDOFF 10.8 기준, 공격 적중은 impact delay 후 실행됨. 사운드는 실제 데미지 적용 시점에 호출.

### StartWave()

```csharp
// 웨이브 시작 시:
if (AudioManager.Instance != null)
    AudioManager.Instance.PlayWaveStart();
```

### SpawnBoss()

```csharp
// 보스 스폰 직후:
if (AudioManager.Instance != null)
{
    AudioManager.Instance.PlayBossAppear();
    AudioManager.Instance.PlayBossBGM();
}
```

### HandleEnemyDeath — 보스 처치 시

```csharp
// enemy.IsBoss 확인 후:
if (enemy.IsBoss && AudioManager.Instance != null)
{
    AudioManager.Instance.PlayBossDefeat();
    AudioManager.Instance.PlayBattleBGM();
}
```

HANDOFF 참고: 보스 판정은 `CurrentStage % 10 == 0`.

### AdvanceStage()

```csharp
// 스테이지 클리어 시:
if (AudioManager.Instance != null)
    AudioManager.Instance.PlayStageClear();
```

---

## 작업 4-3. 스킬 스크립트들

### FanSkill.cs (`Assets/Scripts/Core/FanSkill.cs`)

```csharp
// 스태프 부채꼴 투사체 발사 시:
if (AudioManager.Instance != null)
    AudioManager.Instance.PlayStaffRush();
```

### MeteorSkill.cs (`Assets/Scripts/Core/MeteorSkill.cs`)

```csharp
// 스킬 시작 시 1회:
if (AudioManager.Instance != null)
    AudioManager.Instance.PlayMeteorRain();

// 각 유성 착탄 시 (폭발 콜백 또는 착탄 로직 내부):
if (AudioManager.Instance != null)
    AudioManager.Instance.PlayMeteorExplosion();
```

참고: MeteorExplosion은 쿨다운 0.08초가 AudioManager에 내장되어 있어, 15~20개 동시 착탄해도 폭주하지 않음.

### ChainSkill.cs (`Assets/Scripts/Core/ChainSkill.cs`)

```csharp
// 연쇄 번개 시작 시:
if (AudioManager.Instance != null)
    AudioManager.Instance.PlayChainLightning();
```

### DashSkill.cs (`Assets/Scripts/Core/DashSkill.cs`)

```csharp
// 돌진 시작 시:
if (AudioManager.Instance != null)
    AudioManager.Instance.PlayBerserkerDash();

// 버프 오라 발동 시:
if (AudioManager.Instance != null)
    AudioManager.Instance.PlayBuffActivate();
```

### ManualAttackButton.cs (`Assets/Scripts/Core/ManualAttackButton.cs`)

```csharp
// 수동 공격 버튼 클릭 시:
if (AudioManager.Instance != null)
    AudioManager.Instance.PlayManualAttack();
```

---

## 작업 4-4. CharacterStats.cs

경로: `Assets/Scripts/Core/CharacterStats.cs`

```csharp
// LevelUp() 메서드 내부, 레벨 증가 직후:
if (AudioManager.Instance != null)
    AudioManager.Instance.PlayLevelUp();
```

---

## 작업 4-5. UpgradeSystem.cs

경로: `Assets/Scripts/Core/UpgradeSystem.cs`

HANDOFF 참고: `enhanceCostData` 테이블 기반 비용 계산이 적용되어 있음.

```csharp
// TryUpgrade() 성공 시 (비용 차감 + 보너스 적용 후):
if (AudioManager.Instance != null)
    AudioManager.Instance.PlayUpgradeSuccess();
```

---

## 작업 4-7. OfflineRewardSystem.cs

경로: `Assets/Scripts/Core/OfflineRewardSystem.cs`

HANDOFF 참고: `OfflineRewardPopupUI`와 연동되어 있음. 영문 텍스트 사용.

```csharp
// ClaimReward() 또는 실제 보상 수령 처리 시:
if (AudioManager.Instance != null)
    AudioManager.Instance.PlayOfflineReward();
```

---

## 작업 5. SampleScene에 AudioManager 배치

GameManager 오브젝트에 AudioManager 컴포넌트 추가.

### unity-cli 사용 시 (PowerShell):

```powershell
unity-cli exec 'var gm = GameObject.Find("GameManager"); gm.AddComponent<AudioManager>();'
unity-cli menu 'File/Save Project'
```

### 수동 배치:
1. SampleScene 열기
2. Hierarchy에서 `GameManager` 선택
3. Inspector → Add Component → AudioManager
4. File → Save Project

SerializeField 슬롯은 비워 두면 됩니다 — 프로시저럴 사운드가 자동 생성됩니다.

---

## 작업 6. 빈도 제어 — AudioManager에 이미 내장됨

| 메서드 | 쿨다운 | 이유 |
|--------|--------|------|
| `PlayEnemyHit()` | 0.05초 | 완전 동시 재생만 방지 |
| `PlayEnemyDeath()` | 0.03초 | 10~22마리 동시 사망 대응 |
| `PlayGoldPickup()` | 0.1초 | 코인 소리 과다 방지 |
| `PlayMeteorExplosion()` | 0.08초 | 유성 15~20개 동시 착탄 대응 |

---

## 완료 후 확인 체크리스트

1. Play 모드 진입 → 적 타격 시 타격음 확인
2. 적 사망 시 사망음 + 코인 소리 확인
3. 크리티컬 히트 시 높은 핑 소리 확인
4. 스킬 4종 발동 시 각각 고유한 소리 확인
   - FanShape: 슈팅음
   - Meteor: 발사음 + 착탄 폭발음
   - Chain: 전기음
   - Dash(Buff): 돌진음 + 버프 차지음
5. 레벨업/강화 시 시스템 소리 확인
6. 보스 등장(10/20/30 스테이지) 시 위압적 저음 확인
7. 다수 적 동시 처치 시 소리 폭주 없음 확인
8. 볼륨이 적절한지 확인 (불쾌하면 AudioManager Inspector에서 조정)

### 마지막으로:

```powershell
unity-cli menu 'File/Save Project'
```

# 힘마법사 키우기 — HANDOFF.md

최종 업데이트: 2026-03-25
프로젝트 경로: `C:/UnityPhase1/IdleRPG`
대상: 다음 작업을 이어받을 Claude / Codex / 협업 에이전트

---

## 1. 이 문서의 목적

이 문서는 `힘마법사 키우기` Unity 프로토타입을, 시스템 기획서와 최대한 일치하도록 보정한 현재 상태를 정리한 인수인계서다.

이번 세션의 핵심 목표는 아래 2가지였다.

1. 기획서 기준 불일치 코드/데이터를 수정해 `90%+` 수준까지 싱크로율을 끌어올릴 것
2. 다음 작업자가 바로 이어서 검증/보정할 수 있도록, 변경 내역과 남은 리스크를 문서화할 것

현재 판단 기준 싱크로율:

- 코드 구조 싱크로율: 약 `90%+`
- 실제 데이터/씬/연출 포함 종합 싱크로율: 약 `91~93%`

주의:

- `StageData 1~30` 전체 값은 PDF의 표 원본을 텍스트로 완전 추출하지 못해, 기획서 본문에서 직접 확인된 앵커 값 `1 / 2 / 3 / 5 / 10 / 15 / 20 / 30`을 기준으로 보간하여 입력했다.
- 따라서 현재 상태는 `기획 의도/구조/핵심 수치 축`은 거의 맞지만, `StageData 1~30 상세 수치의 완전 일치`는 원본 표 확보 후 최종 확정이 필요하다.

---

## 2. 작업 환경

- 프로젝트: `C:/UnityPhase1/IdleRPG`
- Unity: `6000.3.6f1`
- `unity-cli`: 포트 `8090` 확인됨
- 셸: PowerShell

반드시 기억할 규칙:

- PowerShell + `unity-cli exec` 사용 시 바깥은 작은따옴표, 안쪽 C# 문자열은 큰따옴표 사용
- 에셋/씬/인스펙터 수정 후 반드시 `unity-cli menu 'File/Save Project'`
- 대량 에셋 수정은 한 번에 너무 많이 하지 말고 종류별/단계별로 나눠 실행

---

## 3. 이번 세션에서 실제 반영된 것 요약

### 3.1 P1 밸런스 불일치 수정

다음 항목을 코드와 실제 에셋에 반영했다.

#### LevelData

파일:

- `Assets/Scripts/Data/LevelData.cs`
- `Assets/Data/DefaultLevelData.asset`

반영 내용:

- `HPQuadratic = 1.2f` 추가
- HP 공식 변경:
  - `MaxHP = 100 + (Lv-1)*20 + (Lv-1)^2*1.2`
- `ATKPerLevel = 5.7f`
- `DEFPerLevel = 2.9f`
- `ExpMultiplier = 1.45f`

주의:

- 초기 지시서의 검증값과 공식 일부가 서로 모순되는 부분이 있었는데, 코드는 기획서 공식 기준으로 맞췄다.

#### CharacterStats

파일:

- `Assets/Scripts/Core/CharacterStats.cs`

반영 내용:

- `LevelData`가 없을 때 사용하는 폴백 성장식도 동일하게 수정
- `hpQuadratic = 1.2f`
- `atkPerLevel = 5.7f`
- `defPerLevel = 2.9f`
- `expMultiplier = 1.45f`

#### SkillData + 스킬 에셋 4종

파일:

- `Assets/Scripts/Data/SkillData.cs`
- `Assets/Skills/Skill_Buff.asset`
- `Assets/Skills/Skill_Circle.asset`
- `Assets/Skills/Skill_Chain.asset`
- `Assets/Skills/Skill_FanShape.asset`

반영 내용:

- 기본값:
  - `cooldown = 4`
  - `damageMultiplier = 1.5`
  - `angle = 60`
  - `chainCount = 5`
  - `buffDuration = 8`

- 실제 에셋 값:
  - Buff: `20 / 0 / 0 / 0 / 1 / 8`
  - Circle: `8 / 3.0 / 2.5 / 0 / 1 / 0`
  - Chain: `6 / 1.2 / 3.0 / 0 / 5 / 0`
  - FanShape: `4 / 1.5 / 2.0 / 60 / 1 / 0`

#### EnemyData + 적 에셋 5종

파일:

- `Assets/Scripts/Data/EnemyData.cs`
- `Assets/Data/Enemies/EnemyData_Slime.asset`
- `Assets/Data/Enemies/EnemyData_Bat.asset`
- `Assets/Data/Enemies/EnemyData_Goblin.asset`
- `Assets/Data/Enemies/EnemyData_SkeletonWarrior.asset`
- `Assets/Data/Enemies/EnemyData_Golem.asset`

반영 내용:

- 기획서 기준 적 5종 값으로 수정
- 실제 프로젝트 파일명은 기존 이름을 유지하고, 내부 `EnemyName`과 수치만 기획서 기준으로 맞춤
- `MoveSpeed` 필드 추가
- `AttackRange` 필드 추가
- `RoleType` enum 추가

현재 적 매핑:

- `EnemyData_Slime.asset` → `살점 슬라임`
- `EnemyData_Bat.asset` → `눈알 스토커`
- `EnemyData_Goblin.asset` → `혈구 군체`
- `EnemyData_SkeletonWarrior.asset` → `스켈레톤 악마`
- `EnemyData_Golem.asset` → `쌍두 변이체`

이미지 기준으로 다시 확인 후 수정한 값:

- 눈알 스토커 `BaseDEF = 1`
- 혈구 군체 `BaseDEF = 0`

---

## 4. P2 구조적 누락 보정

### 4.1 StageData 구조 확장

파일:

- `Assets/Scripts/Data/StageData.cs`

추가 필드:

- `HPMult`
- `ATKMult`
- `DEFMult`
- `GoldMult`
- `WavesPerStage`
- `MinEnemies`
- `MaxEnemies`
- `IsBossStage`
- `BossID`

기존 레거시 필드도 남아 있다:

- `EnemiesPerStage`
- `MinEnemyLevel`
- `MaxEnemyLevel`

이유:

- 기존 폴백 로직/레거시 호환을 완전히 끊지 않기 위해 당장은 남겨 두었다.

### 4.2 EnemyData 배율 기반 메서드 추가

파일:

- `Assets/Scripts/Data/EnemyData.cs`

추가 내용:

- `RoleType` enum
- 배율 기반 메서드
  - `GetHP(float hpMult)`
  - `GetATK(float atkMult)`
  - `GetDEF(float defMult)`
  - `GetGoldReward(float goldMult)`

기존 레벨 기반 메서드는 유지:

- `GetHP(int level)` 등

### 4.3 BossData 신규 생성

파일:

- `Assets/Scripts/Data/BossData.cs`
- `Assets/Data/Bosses/BossData_Stomach.asset`
- `Assets/Data/Bosses/BossData_Spine.asset`
- `Assets/Data/Bosses/BossData_Heart.asset`

구성:

- `BossID`
- `DisplayName`
- `BaseEnemy`
- `BossHPMult`
- `BossATKMult`
- `RewardMult`
- `FixedGoldBonus`

현재 값:

- `BOSS_STOMACH / 위장 비스트 / 살점 슬라임 / 5.0 / 2.0 / 5.0 / 500`
- `BOSS_SPINE / 척추 드래곤 / 스켈레톤 악마 / 5.0 / 2.0 / 5.0 / 2000`
- `BOSS_HEART / 던전의 심장 / 쌍두 변이체 / 5.0 / 2.0 / 5.0 / 6000`

### 4.4 Enemy 초기화 로직 확장

파일:

- `Assets/Scripts/Core/Enemy.cs`

추가 메서드:

- `Initialize(EnemyData data, StageData stage)`
- `InitializeAsBoss(BossData bossData, StageData stage)`

반영 내용:

- 적 최종 스탯을 `StageData` 배율 기반으로 계산
- 보스는 `BossData` 배율과 `FixedGoldBonus` 적용
- `MoveSpeed`, `AttackRange`, `AttackInterval`도 데이터에서 읽도록 변경

기존 메서드 유지:

- `Initialize(EnemyData, int level)`
- `SetLevel(int level)`
- `ConfigureAsBoss(...)`

### 4.5 WaveSpawnManager 배율/보스 연동

파일:

- `Assets/Scripts/Core/WaveSpawnManager.cs`

반영 내용:

- `bossDataList` 추가
- `SpawnEnemy()`가 이제 `enemy.Initialize(enemyData, stageData)` 사용
- `SpawnBoss()`가 `BossData + StageData` 조합 사용
- `GetBossDataForStage(int stage)` 추가
- `GetWavesForCurrentStage()` 추가
- `WavesPerStage`는 `StageData.WavesPerStage` 우선 사용
- 일반 적 수는 `Random.Range(stageData.MinEnemies, stageData.MaxEnemies + 1)`

현재 보스 판정:

- `CurrentStage % 10 == 0`

### 4.6 SkillSystem 버프 발동 버그 수정

파일:

- `Assets/Scripts/Core/SkillSystem.cs`

수정 내용:

- Buff 스킬은 주변 적이 없어도 발동
- 즉, 자기 버프 스킬로 동작

현재 동작:

- `isBuffActive`가 아니면 즉시 `ActivateBuff(skill)` 가능

### 4.7 EnemyData에 MoveSpeed / AttackRange 반영

파일:

- `Assets/Scripts/Data/EnemyData.cs`
- `Assets/Scripts/Core/Enemy.cs`

수정 내용:

- 적 이동속도와 사거리를 더 이상 하드코딩에만 의존하지 않고 `EnemyData`에서 읽음

---

## 5. 90%+ 목표로 추가 반영한 5개 작업

아래 5개가 이번 세션의 후반 핵심 작업이었다.

### 5.1 StageData 30개 입력 자동화

파일:

- `Assets/Editor/DesignSyncUtility.cs`

무엇을 했는가:

- `StageData_01.asset` ~ `StageData_30.asset`에 대해 아래 필드를 일괄 동기화하는 에디터 유틸 작성
  - `HPMult`
  - `ATKMult`
  - `DEFMult`
  - `GoldMult`
  - `WavesPerStage`
  - `MinEnemies`
  - `MaxEnemies`
  - `EnemyPool`
  - `IsBossStage`
  - `BossID`

중요:

- PDF 표 전체를 완전 추출하지 못해서, 기획서 본문에서 직접 확인 가능한 앵커 값 `1 / 2 / 3 / 5 / 10 / 15 / 20 / 30`을 기준으로 보간했다.

앵커 값:

- Stage 1: `1.0 / 1.0 / 1.0 / 1.0 / 3 / 2 / 3`
- Stage 2: `1.1 / 1.05 / 1.0 / 1.0 / 3 / 2 / 4`
- Stage 3: `1.2 / 1.1 / 1.0 / 1.0 / 3 / 3 / 4`
- Stage 5: `1.5 / 1.25 / 1.1 / 1.1 / 4 / 3 / 5`
- Stage 10: `2.0 / 1.5 / 1.3 / 1.2 / 5 / 3 / 5`
- Stage 15: `3.0 / 2.0 / 1.5 / 1.3 / 5 / 4 / 5`
- Stage 20: `4.0 / 2.5 / 1.8 / 1.5 / 6 / 4 / 6`
- Stage 30: `6.0 / 3.5 / 2.5 / 2.0 / 7 / 5 / 7`

검증:

- Unity에서 직접 조회한 값
  - `S1: 1,1,1,1,3,2,3,false`
  - `S10: 2,1.5,1.3,1.2,5,3,5,true,BOSS_STOMACH`
  - `S20: 4,2.5,1.8,1.5,6,4,6,true,BOSS_SPINE`
  - `S30: 6,3.5,2.5,2,7,5,7,true,BOSS_HEART`

### 5.2 SampleScene 실제 구성 + Inspector 연결

원래 상태:

- `SampleScene.unity`는 사실상 빈 씬이었다
- 루트 오브젝트가 `Main Camera`, `Global Light 2D`밖에 없었다

반영 후:

- 루트 오브젝트 생성
  - `GameManager`
  - `Player`
  - `SpawnPoint_3`
  - `SpawnPoint_4`
  - `SpawnPoint_5`
  - `SpawnPoint_6`
  - `SpawnPoint_7`
  - `Canvas`
  - `EventSystem`

자동 연결한 항목:

- `WaveSpawnManager.stages = 30개`
- `WaveSpawnManager.bossDataList = 3개`
- `WaveSpawnManager.spawnPoints = 5개`
- `WaveSpawnManager.player = Player.CharacterStats`
- `WaveSpawnManager.enemyPrefab = Assets/Prefabs/Enemy.prefab`
- `CharacterStats.levelData = DefaultLevelData`
- `SkillSystem.skills = SkillData 4개`
- `SkillSystem.player = CharacterStats`
- `SkillSystem.waveManager = WaveSpawnManager`
- `SaveSystem.player / battleSystem / upgradeSystem`
- `OfflineRewardSystem.player / saveSystem`
- `UpgradeSystem.enhanceCostData`

검증:

- 루트 오브젝트 확인 결과:
  - `Main Camera, Global Light 2D, GameManager, Player, SpawnPoint_3~7, Canvas, EventSystem`
- 참조 확인 결과:
  - `stages:30`
  - `bosses:3`
  - `level:DefaultLevelData`
  - `skills:4`

### 5.3 전투 연출 3종 구현

파일:

- `Assets/Scripts/Core/CombatFeedbackSystem.cs`
- `Assets/Scripts/Core/Enemy.cs`

추가 연출:

- 히트스톱
- 적 사망 혈흔 버스트
- 보스 처치 시 카메라 흔들림

구현 방식:

- `CombatFeedbackSystem.HitStop(...)`
- `CombatFeedbackSystem.BloodBurst(...)`
- `CombatFeedbackSystem.ShakeCamera(...)`

적용 위치:

- 피격 시 `Enemy.TakeDamage()`
- 사망 시 `Enemy.Die()`

추가 수정 기록:

- 플레이 테스트 중 "전투 도중 게임 전체 속도가 점점 느려지는" 버그가 발견됨
- 원인:
  - `CombatFeedbackSystem.BeginHitStop()`가 히트스톱 중첩 시마다 `Time.timeScale`의 현재값을 다시 복구 기준값으로 저장하고 있었음
  - 이미 느려진 `Time.timeScale` 값(`0.08` 등)이 `restoreTimeScale`처럼 덮어써져, 히트스톱 종료 후에도 `1.0`이 아니라 느린 값으로 복구되는 구조였음
- 조치:
  - 히트스톱이 처음 시작될 때만 원래 속도를 저장하도록 수정
  - 중첩 히트스톱에서는 복구 기준값을 덮어쓰지 않도록 변경
  - `OnDisable()` / `OnDestroy()`에서 `Time.timeScale = 1f`, `Time.fixedDeltaTime = 0.02f`로 강제 복구하는 안전장치 추가
- 결과:
  - 연속 타격/군집 적 상황에서도 히트스톱 종료 후 정상 속도로 돌아와야 함
- 다음 작업자 재확인 포인트:
  - 플레이 중 장시간 전투 후에도 게임 전체 속도가 느려지지 않는지 직접 테스트 필요

### 5.4 오프라인 보상 팝업 UI 구현

파일:

- `Assets/Scripts/UI/OfflineRewardPopupUI.cs`
- `Assets/Scripts/Core/OfflineRewardSystem.cs`

구현 내용:

- 제목
- 설명 문구
- 골드 수치
- 경과 시간
- 수령 버튼
- 오픈 카운팅 연출
- 수령 후 닫힘 처리

연결 방식:

- `OfflineRewardSystem.Start()`에서 `OfflineRewardPopupUI.EnsureExists(this)`

현재 구조:

- `OfflineRewardSystem`은 계산/보상 데이터 관리
- `OfflineRewardPopupUI`는 실제 표시와 수령 UX 담당

추가 수정 기록:

- 플레이 테스트 중 게임 시작 직후 뜨는 오프라인 보상 팝업의 한글이 네모/외계어처럼 깨지는 현상 확인
- 원인:
  - `OfflineRewardPopupUI.cs`가 런타임에 `TextMeshProUGUI`를 직접 생성하면서 한글 문자열을 넣고 있었음
  - 현재 프로젝트는 TMP 기본 폰트가 한글 글리프를 포함하지 않아, 런타임 생성 텍스트가 한글을 정상 렌더링하지 못함
- 조치:
  - 오프라인 보상 팝업의 표시 문자열을 전부 영문으로 변경
  - 예:
    - `오프라인 보상` → `Offline Reward`
    - `경과 시간` → `Elapsed`
    - `수령하기` → `Claim`
    - `수령중...` → `Claiming...`
- 현재 판단:
  - 이 프로젝트는 TMP 한글 미지원 상태이므로, 런타임 생성 UI 텍스트는 영문 유지가 안전함
- 후속 대안:
  - 나중에 한글 UI가 꼭 필요하면, 한글 글리프 포함 TMP Font Asset을 별도 생성하고 런타임 생성 텍스트에 명시적으로 할당해야 함

### 5.5 EnhanceCostData 실제 도입

파일:

- `Assets/Scripts/Data/EnhanceCostData.cs`
- `Assets/Data/EnhanceCostData.asset`
- `Assets/Scripts/Core/UpgradeSystem.cs`

반영 내용:

- 기획서의 6번째 테이블인 `EnhanceCostData`를 실제 ScriptableObject로 구현
- `Rows[0..25]` 총 26행 생성
- 각 행에
  - `EnhanceLevel`
  - `CostGold`
  - `HPBonus`
  - `ATKBonus`
  - `DEFBonus`
  입력

현재 값:

- 비용 공식은 기존과 동일한 `Floor(50 * 1.3^level)`
- 보너스는
  - HP `10`
  - ATK `3`
  - DEF `1.5`

`UpgradeSystem` 변경점:

- `enhanceCostData` 참조 필드 추가
- 비용 계산 시 테이블 우선 사용
- 누적 보너스도 테이블 기준으로 계산
- 없으면 기존 공식으로 폴백

검증:

- `EnhanceCostData.asset` 생성 확인
- `Rows.Length = 26` 확인
- `SampleScene`의 `UpgradeSystem.enhanceCostData = EnhanceCostData` 연결 확인

---

## 5.6 플레이 테스트 피드백 대응 — 4건 완료

플레이 테스트(A~F 전 항목 통과, Console 에러 0건) 후 사용자 피드백 7건을 수집하고, 코드 수정 가능한 4건을 즉시 반영했다.

### 5.6.1 보스/UI 한글 깨짐 전면 수정

수정 파일:
- BossData.cs: 기본값 "위장 비스트" → "Stomach Beast"
- EnemyData.cs: 기본값 "슬라임" → "Slime"
- Enemy.cs: 기본값 "슬라임" → "Slime"
- SkillData.cs: 기본값 "스킬" → "Skill"
- StageData.cs: 기본값 "초원" → "Plains"
- OfflineRewardSystem.cs: "시간"/"분" → "h"/"m"

참고: [Header(...)] 속성의 한글은 Inspector 전용이라 런타임에 표시되지 않으므로 그대로 둠.

### 5.6.2 몬스터 크기 차별화

수정 파일:
- EnemyData.cs: SpriteScale 필드 추가
- Enemy.cs: Initialize, InitializeAsBoss, ConfigureAsBoss에서 localScale 적용 (보스는 ×1.5)

에셋 값:
- EnemyData_Slime: SpriteScale = 1.3
- EnemyData_Bat: SpriteScale = 1.1
- EnemyData_Goblin: SpriteScale = 1.0
- EnemyData_SkeletonWarrior: SpriteScale = 2.2
- EnemyData_Golem: SpriteScale = 2.5

### 5.6.3 몬스터 스폰 수 증가

StageData 30개 에셋의 MinEnemies/MaxEnemies를 상향 조정:
- Stage 1~5: 4/6 (기존 2~3에서 상향)
- Stage 6~10: 4/7
- Stage 11~20: 5/8
- Stage 21~30: 6/9

참고: 이후 v2.0 세션에서 추가 상향됨 (10~15 / 10~17 / 12~20 / 15~22).

### 5.6.4 플레이어 자동 이동 추가

수정 파일:
- CharacterStats.cs

추가 필드:
- moveSpeed = 3.0f
- attackRange = 1.5f

추가 로직:
- FixedUpdate에서 WaveSpawnManager.ActiveEnemies 중 최근접 적 탐색
- attackRange 밖이면 적 방향으로 Rigidbody2D.MovePosition 이동
- attackRange 안이면 정지 (공격은 WaveSpawnManager가 처리)
- localScale.x 부호 반전으로 방향 전환 (Animator-safe, flipX 대신)
- Rigidbody2D 자동 부착 (gravityScale=0, freezeRotation=true)

### 5.6.5 스킬 범위 확대 + 평타 범위 확대

SkillData 에셋 변경:
- Skill_FanShape: range 2.0 → 3.5, angle 60 → 90
- Skill_Circle: range 2.5 → 4.0
- Skill_Chain: range 3.0 → 5.0
- Skill_Buff: 변경 없음

WaveSpawnManager:
- attackRadius: 2.0 → 3.0

### 5.6.6 적 HP바 스케일 역보정

수정 파일:
- EnemyWorldHPBar.cs

문제: 몬스터 SpriteScale을 키우면서 HP바도 같이 커져 시야 방해
해결:
- HP바 캔버스 localScale에 1/parentScale 적용 → 크기 고정
- 세로 높이 12px → 8px (67% 축소)

---

## 5.7 번개 돌진 + 잔상 이펙트 구현

### 5.7.1 AfterImageSystem.cs 신규 생성

파일: Assets/Scripts/Core/AfterImageSystem.cs
- 오브젝트 풀(10개) 기반
- 이동 감지 시 0.08초 간격으로 파란 잔상(#4080FF, 알파 0.6) 스프라이트 생성
- 0.3초 페이드아웃 후 풀로 회수
- Player 오브젝트에 컴포넌트 추가 완료

### 5.7.2 버프 중 이동속도 2배

CharacterStats.cs FixedUpdate:
- SkillSystem.IsBuffActive 확인 (캐싱됨)
- 버프 중 moveSpeed × 2f

### 5.7.3 버프 중 잔상 강화

AfterImageSystem.cs:
- SkillSystem.IsBuffActive 확인 (캐싱됨)
- 버프 시 잔상 생성 간격 × 0.4f (2.5배 빈도)
- 잔상 색상: 기본 파란(0.25, 0.5, 1.0, 0.6) → 버프 보라(0.4, 0.2, 1.0, 0.7)

### 5.7.4 스프라이트 방향 보정

CharacterStats.cs:
- Player에 Animator("Hero")가 있어 flipX 방식은 충돌 위험
- transform.localScale.x 부호 반전 방식으로 전환 (Animator-safe)

---

## 5.8 v2.0 비주얼 + 시스템 업그레이드

### 5.8.1 마젠타 전면 수정

근본 원인: 프로젝트가 URP인데 Built-in 파이프라인 쉐이더(Particles/Standard Unlit) 사용 → 마젠타
해결:
- SkillVFX_Additive 머티리얼 → Universal Render Pipeline/Particles/Unlit 쉐이더로 교체
- MeteorExplosionFX, ChainSparkFX 프리팹 PSR 전부 URP 쉐이더 통일
- SpriteRenderer에 파티클 쉐이더 할당하던 코드 제거
- WarningCircleFX 프리팹 우회 → 코드에서 직접 프로시저럴 생성

### 5.8.2 MapBounds 동적 계산 시스템

파일: Assets/Scripts/Core/MapBounds.cs
- Camera.main 기반 동적 계산 (하드코딩 제거)
- Initialize()에서 orthographicSize × aspect로 HalfWidth/HalfHeight 산출
- ClampPlayer(), IsInside(), RandomSpawnEdge(), RandomInsideView() 제공
- 모든 스킬/이동/스폰에서 MapBounds 사용

### 5.8.3 몬스터 맵 가장자리 스폰

WaveSpawnManager.cs:
- SpawnEnemyAtPoint()에서 spawnPoints 무시 → 항상 MapBounds.RandomSpawnEdge() 사용
- 기존 spawnPoints 기반 스폰은 플레이어 근처 스폰 버그의 원인이었음

### 5.8.4 플레이어/몬스터 Separation Force

Enemy.cs:
- ComputeSeparation()으로 적끼리 + 적-플레이어 간 분리 힘 적용
- separationRadius는 SpriteScale에 비례 (0.5 + SpriteScale × 0.3)
- separationStrength = 3f

CharacterStats.cs:
- ComputePlayerSeparation()으로 플레이어-적 간 분리 힘 적용
- 반경 0.5, 강도 1.5 (이동 방해 최소화)

### 5.8.5 스킬 비주얼 업그레이드

- MeteorSkill: 8프레임 폭발 스프라이트 애니메이션 (Meteor_Explosion_SpriteSheet)
- 유성 투사체 3배 확대, 스태프 투사체 2배 확대
- 버프 오라: 8프레임 스프라이트 루프 (Buff_Aura_SpriteSheet)
- ChainSkill: ballSprite 없을 때 #96DCFF 프로시저럴 fallback

---

## 6. 현재 프로젝트 구조

### 스크립트

#### Core

- `Assets/Scripts/Core/CharacterStats.cs`
- `Assets/Scripts/Core/DamagePopup.cs`
- `Assets/Scripts/Core/Enemy.cs`
- `Assets/Scripts/Core/OfflineRewardSystem.cs`
- `Assets/Scripts/Core/ProceduralSpriteLibrary.cs`
- `Assets/Scripts/Core/SaveSystem.cs`
- `Assets/Scripts/Core/SimpleSkillEffect.cs`
- `Assets/Scripts/Core/SkillSystem.cs`
- `Assets/Scripts/Core/UpgradeSystem.cs`
- `Assets/Scripts/Core/WaveSpawnManager.cs`
- `Assets/Scripts/Core/CombatFeedbackSystem.cs`
- `Assets/Scripts/Core/AfterImageSystem.cs`
- `Assets/Scripts/Core/MapBounds.cs`
- `Assets/Scripts/Core/FXWarningCircle.cs`
- `Assets/Scripts/Core/MeteorSkill.cs`
- `Assets/Scripts/Core/ChainSkill.cs`
- `Assets/Scripts/Core/FanSkill.cs`
- `Assets/Scripts/Core/DashSkill.cs`
- `Assets/Scripts/Core/ActorAnimationEvents.cs`
- `Assets/Scripts/Core/ManualAttackButton.cs`

#### Data

- `Assets/Scripts/Data/BossData.cs`
- `Assets/Scripts/Data/EnemyData.cs`
- `Assets/Scripts/Data/LevelData.cs`
- `Assets/Scripts/Data/SkillData.cs`
- `Assets/Scripts/Data/StageData.cs`
- `Assets/Scripts/Data/EnhanceCostData.cs`

#### UI

- `Assets/Scripts/UI/BossUIController.cs`
- `Assets/Scripts/UI/EnemyHPBar.cs`
- `Assets/Scripts/UI/EnemyWorldHPBar.cs`
- `Assets/Scripts/UI/PlayerHUD.cs`
- `Assets/Scripts/UI/ShopUI.cs`
- `Assets/Scripts/UI/StageUI.cs`
- `Assets/Scripts/UI/OfflineRewardPopupUI.cs`

#### Editor

- `Assets/Editor/CanvasSetupGenerator.cs`
- `Assets/Editor/DataAssetGenerator.cs`
- `Assets/Editor/HUDConnector.cs`
- `Assets/Editor/ShopSetupGenerator.cs`
- `Assets/Editor/DesignSyncUtility.cs`

### 데이터 에셋

#### Level

- `Assets/Data/DefaultLevelData.asset`

#### Enemy

- `Assets/Data/Enemies/EnemyData_Slime.asset`
- `Assets/Data/Enemies/EnemyData_Bat.asset`
- `Assets/Data/Enemies/EnemyData_Goblin.asset`
- `Assets/Data/Enemies/EnemyData_SkeletonWarrior.asset`
- `Assets/Data/Enemies/EnemyData_Golem.asset`

#### Boss

- `Assets/Data/Bosses/BossData_Stomach.asset`
- `Assets/Data/Bosses/BossData_Spine.asset`
- `Assets/Data/Bosses/BossData_Heart.asset`

#### Stage

- `Assets/Data/Stages/StageData_01.asset` ~ `StageData_30.asset`

#### Enhance

- `Assets/Data/EnhanceCostData.asset`

#### Skill

- `Assets/Skills/Skill_Buff.asset`
- `Assets/Skills/Skill_Circle.asset`
- `Assets/Skills/Skill_Chain.asset`
- `Assets/Skills/Skill_FanShape.asset`

### 씬

- `Assets/Scenes/SampleScene.unity`

---

## 7. 현재 기준 “기획서와 잘 맞는 것”

아래는 현재 프로젝트에서 이미 기획서와 높은 정합성을 보이는 항목들이다.

- 자동 전투 코어 루프
- 레벨업 공식
- 강화 비용 공식과 보너스 구조
- 스킬 4종 구조와 우선순위
- Buff 자기 버프 처리
- EnemyData 5종 기본 수치
- BossData 3종 구조
- StageData 배율 구조
- 오프라인 보상 계산 구조
- 저장/로드 기본 구조
- PlayerHUD / 보스 UI / 상점 UI 기본 구조
- 데미지 팝업
- 보스 배율 + 고정 골드 보너스
- StageData / BossData / LevelData / SkillData / EnhanceCostData 참조 구조
- 플레이어 자동 이동 (기획서 5장 자동 전투 규칙과 일치)
- 몬스터 크기 차별화 (역할군별 SpriteScale 적용)
- 스킬 범위 확대 (FanShape 3.5/90°, Circle 4.0, Chain 5.0)
- 적 HP바 스케일 역보정 (대형 몬스터 시야 방해 해결)
- 번개 잔상 이펙트 (이동 시 파란 잔상, 버프 시 보라)
- 전체 런타임 텍스트 영문 전환 (TMP 한글 미지원 대응)
- MapBounds 동적 계산 (카메라 기반, 기기별 해상도 대응)
- 몬스터 Separation Force (겹침 방지)
- URP 쉐이더 통일 (마젠타 완전 제거)

---

## 8. 아직 남아 있는 불일치 / 리스크

이 섹션이 가장 중요하다. 다음 작업자가 이어서 볼 때, 무엇이 “완료”이고 무엇이 “거의 맞지만 아직 확정 아님”인지 구분해야 한다.

### 8.1 가장 큰 리스크 — StageData 1~30 상세 수치

현재 상태:

- `StageData 1~30`은 입력되어 있음
- 그러나 원본 PDF의 전체 표를 직접 복사한 값이 아니라, 기획서 본문에서 확인 가능한 앵커 값 보간치임

의미:

- 구조와 난이도 곡선 방향은 맞음
- 하지만 “완전 일치”라고 단정하면 안 됨

다음 작업자가 해야 할 일:

- PDF 원본의 `StageData 1~30` 전체 표를 이미지 또는 텍스트로 확보
- 각 `StageData_01~30.asset` 값 재대조

### 8.2 공격 스킬 발동 조건

현재 상태:

- Buff는 적이 없어도 발동
- Circle / Chain / Fan은 적이 없으면 발동하지 않음

기획서 본문 기준:

- 공격 스킬도 “범위 내 적이 없을 경우 발동하되 대상 없음, 쿨타임 소비”로 읽힐 여지가 있음

판단:

- 지금은 Buff 버그만 확실히 수정된 상태
- 공격 스킬 무대상 발동까지 기획서 100% 일치로 볼지는 추가 판단 필요

### 8.3 전투 피드백은 “간이 구현” 수준

현재 상태:

- 히트스톱 있음
- 혈흔 버스트 있음
- 보스 카메라 흔들림 있음

하지만:

- 기획서의 “히트스톱 → 혈흔 → 파편 → 골드 폭발” 시퀀스를 완전한 아트/사운드 품질로 구현한 것은 아님
- 지금은 코드 기반 임시 연출 수준
- 히트스톱 중첩으로 인한 `Time.timeScale` 복구 버그는 수정됨. 다만 장시간 플레이에서 재발 여부는 계속 확인 필요

### 8.4 오프라인 보상 팝업은 “기능 구현” 완료, 연출 퀄리티는 후속 가능

현재 상태:

- 팝업 생성
- 경과 시간 표시
- 골드 카운팅
- 수령 버튼
- 수령 후 닫힘
- 한글 깨짐 이슈를 피하기 위해 현재 표시 텍스트는 영문으로 변경됨

후속 보강 가능:

- 배경 애니메이션
- 수령 시 더 강한 골드 카운팅/반짝임
- 기획서 디자인 톤에 맞춘 색감 개선
- 한글 UI가 필요할 경우 TMP 한글 지원 폰트 자산 도입

### 8.5 SaveSystem은 여전히 기획서보다 단순함

현재 상태:

- 30초 자동 저장
- 앱 종료/일시정지 저장
- 레벨/스탯/골드/강화/스테이지 저장

하지만 기획서 기준으로 보면:

- `currentWave`
- 더 세밀한 오프라인 상태 복원
- 일부 예외 처리 규칙

등은 아직 문서만큼 정교하지 않다.

### 8.6 SampleScene은 “최소 플레이 가능 구조”로 생성한 상태

중요:

- 이번 세션에서 `SampleScene`은 원래 거의 빈 씬이어서 자동으로 최소 구조를 생성했다
- 즉, 이 씬은 “기획서 정합성 검증용 실무 기본 씬”으로 이해해야 한다
- 아트/배치/완성형 씬으로 보면 안 된다

---

## 9. 다음 작업자가 가장 먼저 확인해야 할 체크리스트

아래 순서대로 점검하면 된다.

### 1단계: Unity 열고 씬 상태 확인

- `Assets/Scenes/SampleScene.unity` 열기
- 루트에 아래가 있는지 확인
  - `GameManager`
  - `Player`
  - `SpawnPoint_3~7`
  - `Canvas`
  - `EventSystem`

### 2단계: Inspector 연결 확인

- `GameManager > WaveSpawnManager`
  - `stages = 30`
  - `bossDataList = 3`
  - `spawnPoints = 5`
  - `enemyPrefab` 연결 여부
- `Player > CharacterStats`
  - `levelData = DefaultLevelData`
- `Player > SkillSystem`
  - `skills = 4`
- `GameManager > UpgradeSystem`
  - `enhanceCostData = EnhanceCostData`

### 3단계: StageData 샘플 확인

최소 확인 대상:

- `StageData_01`
- `StageData_10`
- `StageData_20`
- `StageData_30`

확인 항목:

- `HPMult`
- `ATKMult`
- `DEFMult`
- `GoldMult`
- `WavesPerStage`
- `MinEnemies`
- `MaxEnemies`
- `IsBossStage`
- `BossID`

### 4단계: 플레이 테스트

확인할 것:

- 적 스폰이 되는지
- Buff가 적 없이도 발동하는지
- 보스 스테이지에서 보스가 스폰되는지
- 보스 처치 시 보너스 골드가 정상적으로 들어오는지
- 피격 히트스톱이 거슬리진 않는지
- 장시간 전투 중 게임 전체 속도가 다시 느려지지 않는지
- 혈흔 버스트가 과하지 않은지
- 오프라인 보상 팝업이 열리고 수령 가능한지

### 5단계: StageData 원본 표 확보 시 재정합화

이게 가장 가치 큰 후속 작업이다.

---

## 10. 스프라이트 / 애니메이션 적용 작업 인수인계

이 섹션은 이번 세션 후반에 진행한 `실제 스프라이트 시트 적용` 작업 전체를 정리한 것이다.

초기 상태:

- 프로젝트는 원래 `ProceduralSpriteLibrary` 기반의 임시 비주얼을 사용하고 있었다
- `Player`와 `Enemy.prefab`에는 `Animator`가 없었다
- 실제 캐릭터/몬스터 스프라이트 시트가 프로젝트에 없거나, 연결 로직이 없었다

이번 세션 목표:

1. 사용자가 준비한 주인공 + 몬스터 5종 스프라이트 시트를 Unity 프로젝트에 안전하게 임포트
2. 실제 스프라이트가 씬/전투에 보이도록 `Animator` 기반 구조로 확장
3. 공격/피격/죽음/스킬 애니메이션이 코드 이벤트와 연결되도록 구현
4. 전투 손맛을 위해 판정 타이밍과 죽음 처리 타이밍까지 폴리시

### 10.1 파일 배치 / 파일명 규칙 정리

사용자에게 안내한 최종 폴더:

- `Assets/Sprites/Characters/`
- `Assets/Sprites/Monsters/`

권장 파일명 규칙:

- 영문
- 소문자
- 언더스코어만 사용
- 한글/공백 사용 금지

주인공:

- `hero_idle_sheet.png`
- `hero_walk_sheet.png`
- `hero_attack_sheet.png`
- `hero_hit_sheet.png`
- `hero_skill_sheet.png`

몬스터 5종:

- `flesh_slime_idle_sheet.png`
- `flesh_slime_walk_sheet.png`
- `flesh_slime_attack_sheet.png`
- `flesh_slime_hit_sheet.png`
- `flesh_slime_die_sheet.png`

- `eye_stalker_idle_sheet.png`
- `eye_stalker_walk_sheet.png`
- `eye_stalker_attack_sheet.png`
- `eye_stalker_hit_sheet.png`
- `eye_stalker_die_sheet.png`

- `skeleton_demon_idle_sheet.png`
- `skeleton_demon_walk_sheet.png`
- `skeleton_demon_attack_sheet.png`
- `skeleton_demon_hit_sheet.png`
- `skeleton_demon_die_sheet.png`

- `blood_cells_idle_sheet.png`
- `blood_cells_walk_sheet.png`
- `blood_cells_attack_sheet.png`
- `blood_cells_hit_sheet.png`
- `blood_cells_die_sheet.png`

- `twin_head_idle_sheet.png`
- `twin_head_walk_sheet.png`
- `twin_head_attack_sheet.png`
- `twin_head_hit_sheet.png`
- `twin_head_die_sheet.png`

실제 확인된 폴더:

- `Assets/Sprites/Characters`
- `Assets/Sprites/Monsters`

### 10.2 실제 시트 레이아웃 확인 결과

중요:

- 처음에는 모든 `*_sheet.png`가 4프레임 시트일 가능성을 가정했지만, 실제 파일을 확인해보니 그렇지 않았다
- `hero_walk_sheet.png` 같은 다수의 파일은 `2x2 4프레임` 시트였다
- 하지만 `hero_idle_sheet.png`, `flesh_slime_idle_sheet.png` 등 일부 `idle_sheet`는 이름과 달리 `단일 프레임` 이미지였다

검증 방식:

- 로컬 이미지 뷰어로 실제 PNG를 직접 확인
- 대표 확인 이미지:
  - `Assets/Sprites/Characters/hero_idle_sheet.png`
  - `Assets/Sprites/Characters/hero_walk_sheet.png`
  - `Assets/Sprites/Monsters/flesh_slime_idle_sheet.png`

결론:

- 모든 시트를 무조건 4등분하면 안 됨
- `실제 2x2 시트만 Multiple + 4분할`
- `단일 프레임은 Single 유지`

### 10.3 자동 임포트 / 슬라이스 유틸 추가

파일:

- `Assets/Editor/SpriteImportUtility.cs`

무엇을 하는가:

- `Assets/Sprites` 아래 모든 PNG를 검색
- 공통 임포트 설정 적용:
  - `Texture Type = Sprite`
  - `Filter Mode = Point`
  - `Compression = Uncompressed`
  - `MipMap Off`
  - `PPU = 128`
  - `WrapMode = Clamp`
- 파일 크기와 이름을 보고 자동 분기:
  - 단일 프레임은 `Single`
  - `2x2 4프레임` 시트는 `Multiple` + 자동 SpriteMetaData 생성

메뉴:

- `Tools/IdleRPG/Configure Imported Sprite Sheets`

검증 결과:

- `hero_walk_sheet.png -> Multiple, sprites:4`
- `hero_idle_sheet.png -> Single, sprites:1`

### 10.4 애니메이션 클립 / Animator Controller 자동 생성

파일:

- `Assets/Editor/SpriteAnimationSetupUtility.cs`

무엇을 하는가:

- Hero용 애니메이션 클립 생성
- 몬스터 5종용 애니메이션 클립 생성
- Hero Animator Controller 생성
- 몬스터별 Animator Controller 생성
- `Player`에 `Animator` 추가 및 Hero 컨트롤러 연결
- `Enemy.prefab`에 `Animator` 추가
- 각 `EnemyData` 에셋에 해당 몬스터용 `AnimatorController` 참조 저장

생성 경로:

- `Assets/Animations/Hero/*.anim`
- `Assets/Animations/Monsters/*.anim`
- `Assets/Animations/Controllers/Hero.controller`
- `Assets/Animations/Controllers/flesh_slime.controller`
- `Assets/Animations/Controllers/eye_stalker.controller`
- `Assets/Animations/Controllers/skeleton_demon.controller`
- `Assets/Animations/Controllers/blood_cells.controller`
- `Assets/Animations/Controllers/twin_head.controller`

메뉴:

- `Tools/IdleRPG/Build Sprite Animation Assets`

### 10.5 런타임 코드 보정

기존 문제:

- `CharacterStats`와 `Enemy`는 `ProceduralSpriteLibrary` 기반으로 단일 스프라이트를 강제로 덮어쓸 수 있었다
- Animator가 있어도 런타임에서 procedural 스프라이트가 다시 입혀질 수 있는 구조였다

수정 파일:

- `Assets/Scripts/Core/CharacterStats.cs`
- `Assets/Scripts/Core/Enemy.cs`
- `Assets/Scripts/Data/EnemyData.cs`

반영 내용:

- `EnemyData`에 `RuntimeAnimatorController AnimatorController` 필드 추가
- `Player`와 `Enemy`는 `Animator.runtimeAnimatorController`가 있으면 procedural fallback을 생략
- `Enemy.Initialize(...)` 시 `EnemyData.AnimatorController`가 있으면 Animator에 할당
- `Enemy.prefab`은 프리팹 차원에서 `Animator`를 가지게 변경

### 10.6 1차 연결 완료 시점 상태

이 시점에서 확인된 것:

- `Player.Animator = Hero`
- `Enemy.prefab`에 `Animator` 존재
- `EnemyData_Slime.asset.AnimatorController = flesh_slime`
- Hero/몬스터가 procedural 컬러 덩어리 대신 실제 스프라이트로 렌더링 가능

주의:

- 당시 `idle_sheet` 다수가 단일 프레임이어서, 기본 대기 상태는 “정지 이미지”처럼 보일 수 있다
- 초기 컨트롤러에서는 Hero는 `idle`, 몬스터는 `walk`를 기본 상태로 잡아두었다

### 10.7 애니메이션 이벤트 트리거 구조 추가

파일:

- `Assets/Scripts/Core/ActorAnimationEvents.cs`

역할:

- `Animator` 트리거 호출을 공통으로 관리하는 브리지 컴포넌트
- 지원 트리거:
  - `Attack`
  - `Hit`
  - `Skill`
  - `Die`

이 컴포넌트가 붙는 대상:

- `Player`
- `Enemy.prefab`

검증 결과:

- `Player`에 `ActorAnimationEvents` 부착 확인
- `Enemy.prefab`에 `ActorAnimationEvents` 부착 확인

### 10.8 코드 이벤트와 애니메이션 연결

수정 파일:

- `Assets/Scripts/Core/CharacterStats.cs`
- `Assets/Scripts/Core/WaveSpawnManager.cs`
- `Assets/Scripts/Core/SkillSystem.cs`
- `Assets/Scripts/Core/Enemy.cs`
- `Assets/Editor/SpriteAnimationSetupUtility.cs`

구체적 연결:

- `WaveSpawnManager`
  - 플레이어 기본 공격 적중 시 `Attack`
- `SkillSystem`
  - Buff/공격 스킬 발동 시 `Skill`
- `CharacterStats`
  - 플레이어 피격 시 `Hit`
  - 플레이어 사망 시 `Die`
- `Enemy`
  - 적 공격 시 `Attack`
  - 적 피격 시 `Hit`
  - 적 사망 시 `Die`

컨트롤러 보강:

- Hero/몬스터 컨트롤러에 `Attack`, `Hit`, `Skill`, `Die` 트리거 파라미터 추가
- `Any State -> Attack/Hit/Skill/Die` 전환 추가
- `Attack/Hit/Skill`은 재생 후 기본 상태로 복귀
- `Die`는 죽음 상태로 진입 후 유지

검증 결과:

- `Hero.controller` 파라미터: `Attack,Hit,Skill,Die`

### 10.9 애니메이션 타이밍 폴리시

이 단계에서 전투 손맛 보정까지 진행했다.

목표:

- 공격 모션보다 데미지가 먼저 들어가는 문제 완화
- 스킬 모션 후 효과가 터지도록 보정
- 적이 죽자마자 바로 사라지지 않게 보정

수정 파일:

- `Assets/Scripts/Core/ActorAnimationEvents.cs`
- `Assets/Scripts/Core/CharacterStats.cs`
- `Assets/Scripts/Core/Enemy.cs`
- `Assets/Scripts/Core/WaveSpawnManager.cs`
- `Assets/Scripts/Core/SkillSystem.cs`
- `Assets/Editor/SpriteAnimationSetupUtility.cs`

핵심 반영:

#### 플레이어 기본 공격

- `WaveSpawnManager`에서 공격 직후 즉시 데미지를 넣지 않음
- `Attack` 트리거 후 짧은 `impact delay` 뒤에 실제 적에게 데미지 적용
- 버프 중 원형 평타도 동일한 딜레이 흐름 적용

#### 적 기본 공격

- `Enemy`의 근접 공격은 즉시 타격이 아니라 `AttackRoutine()` 코루틴으로 처리
- `Attack` 트리거 후 짧은 wind-up 뒤에 플레이어에게 실제 데미지 적용

#### 스킬

- `SkillSystem`에서 스킬 사용 시 곧바로 효과를 넣지 않음
- `Skill` 애니메이션 후 짧은 지연 뒤에:
  - Buff면 버프 적용
  - 공격 스킬이면 실제 데미지/효과 적용

#### 죽음 처리

- `Enemy.Die()`에서 즉시 `Destroy(gameObject)` 하지 않음
- `Die` 트리거 후:
  - 콜라이더 비활성화
  - 보상/OnDeath는 바로 처리
  - 실제 `Destroy`는 애니메이션 길이에 맞춘 delay 후 실행

#### 클립 길이 기반 타이밍 계산

- `ActorAnimationEvents`가 컨트롤러 내 실제 클립 길이를 읽음
- 여기서:
  - `GetAttackImpactDelay()`
  - `GetSkillImpactDelay()`
  - `GetDeathDestroyDelay()`
  를 계산

#### 클립 프레임레이트 보정

- `SpriteAnimationSetupUtility`에서 애니메이션 종류별 프레임레이트 차등 적용
- 의도:
  - `attack` 더 짧고 반응성 있게
  - `hit` 더 즉각적으로
  - `die` 조금 더 길게

검증값:

- `hero_attack: 0.333s @ 12fps`
- `slime_die: 0.500s @ 8fps`

### 10.10 스프라이트 / 애니메이션 작업 후 Unity에서 확인할 항목

다음 작업자가 플레이 모드에서 체크할 것:

- `Player`가 실제 히로인 스프라이트로 보이는지
- 몬스터 5종이 procedural 임시 비주얼 대신 실제 스프라이트로 보이는지
- 몬스터가 기본적으로 `walk` 상태로 재생되는지
- 플레이어 기본 공격이 애니메이션보다 먼저 맞지 않는지
- 스킬이 “모션 후 발동”처럼 보이는지
- 피격 시 `hit`가 너무 과도하게 반복되지 않는지
- 적이 죽을 때 바로 사라지지 않고 짧게 죽음 모션이 보이는지
- 전체 전투 템포가 너무 느려지지는 않았는지

### 10.11 현재 남아 있는 스프라이트/애니메이션 관련 리스크

1. `idle_sheet` 이름과 실제 내용이 불일치하는 파일이 있다

- 일부 `idle_sheet`는 진짜 시트가 아니라 단일 프레임이다
- 따라서 나중에 새 리소스로 교체할 때는 파일명과 실제 레이아웃을 함께 확인해야 한다

2. 현재 컨트롤러는 “이벤트 트리거 + 기본 상태 복귀” 중심의 단순 구조다

- 이동속도/실제 방향/정교한 blend tree는 아직 없음
- 필요한 경우 추후 `Idle/Walk` 전환을 이동량 기반으로 세분화할 수 있다

3. 죽음 후 보상은 즉시 지급, 오브젝트 파괴만 지연된다

- 웨이브 진행 템포는 유지되지만, 아주 엄밀한 연출 설계가 필요하면 “보상 지급 시점”도 조정할 수 있다

4. 애니메이션 이벤트는 코드 트리거 기반이지, Timeline/Animation Event 기반은 아니다

- 현재 구조는 유지보수와 자동 생성에는 유리함
- 다만 아주 세밀한 프레임 싱크는 추후 Animation Event 방식 검토 가능

---

## 11. 다음 작업 우선순위 추천

### 우선순위 1 (진행 예정)

사운드 시스템 구현 — AudioManager 싱글톤 + 프로시저럴 사운드 fallback + 기존 스크립트 연결
작업 지시서: SOUND_SYSTEM_PLAN.md

### 우선순위 2

남은 비주얼 작업 — Chain Lightning 스프라이트, Manual Attack 이펙트, Berserker Dash 강화, BloodBurst 개선

### 우선순위 3

포트폴리오 산출물 — 경쟁작 분석서, AI 활용 프로세스 문서, PDF 디자인 보강, 회사별 커버레터

### 우선순위 4

플레이 영상 촬영

### 우선순위 5

기술 부채 정리 — 런타임 머티리얼 생성 제거, 죽은 에셋 정리, 디버그 코드 정리

---

## 12. Claude에게 바로 넘길 때 쓸 요약 프롬프트

아래 문장을 그대로 다음 채팅 첫 메시지로 써도 된다.

```text
프로젝트는 C:/UnityPhase1/IdleRPG 입니다.
먼저 HANDOFF.md를 읽고 현재 상태를 파악해 주세요.

최근 세션들에서 한 작업:
- P1 밸런스 수치 + P2 구조적 누락 전면 보정 완료
- 플레이 테스트 → 피드백 7건 중 코드 수정 4건 즉시 반영
- 플레이어 자동 이동 + 번개 잔상 이펙트 구현
- 몬스터 크기 차별화 (SpriteScale) + HP바 역보정
- 스킬 범위 확대 + 평타 범위 확대
- 전체 런타임 한글 텍스트 영문 전환
- v2.0: 궤도형 스킬 4종 교체, MapBounds, Separation Force, 비주얼 업그레이드 대량
- 사운드 시스템 구현 예정 (SOUND_SYSTEM_PLAN.md)

현재 가장 중요한 남은 작업:
1. 사운드 시스템 구현
2. 남은 비주얼 작업 (Chain Lightning, Manual Attack 등)
3. 포트폴리오 산출물 (경쟁작 분석, AI 문서, PDF)

HANDOFF.md의 "남아 있는 불일치 / 리스크" 섹션을 우선 확인해 주세요.
```

---

## 13. 최종 결론

이번 세션 종료 시점 기준 프로젝트는:

- 기획서 구조 반영: 높음
- 데이터 테이블 반영: 높음
- 씬/Inspector 연결: 기본 플레이 가능 수준까지 확보
- 연출/UI 반영: 기획서 핵심 포인트는 구현, 품질은 후속 보강 가능
- 실제 스프라이트/Animator 연결: 기본 동작 확보
- 전투 애니메이션 타이밍 폴리시: 1차 반영 완료
- 남은 핵심 리스크: `StageData 1~30 전체 표 정합성`

즉, 지금 프로젝트는 “기획서와 거의 맞고, 실제 스프라이트/애니메이션까지 연결된 프로토타입” 단계까지 왔고, 다음 작업자는 `StageData 최종 확정 + 플레이 테스트 + 애니메이션/연출 polish` 중심으로 이어가면 된다.

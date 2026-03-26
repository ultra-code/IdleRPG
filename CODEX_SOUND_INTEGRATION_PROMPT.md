# Codex 작업 지시: 사운드 시스템 연결 (작업 4 + 작업 5)

프로젝트: C:/UnityPhase1/IdleRPG

## 사전 상태

- `Assets/Scripts/Core/AudioManager.cs` — 이미 배치 완료 (싱글톤, SFX 풀, 프로시저럴 자동 생성, 빈도 제어 내장)
- `Assets/Scripts/Core/ProceduralSoundLibrary.cs` — 이미 배치 완료
- `SoundSystem_IntegrationGuide.md` — 프로젝트 루트에 상세 가이드 있음

먼저 `HANDOFF.md`를 읽어서 프로젝트 구조를 파악하고, `SoundSystem_IntegrationGuide.md`를 읽어서 삽입할 코드 스니펫과 위치를 확인해줘.

## 해야 할 작업

아래 7개 스크립트에 AudioManager 사운드 호출을 삽입해줘. 모든 호출은 `if (AudioManager.Instance != null)` null 체크로 감싸야 한다.

### 1. Enemy.cs (`Assets/Scripts/Core/Enemy.cs`)

- `TakeDamage()` — 데미지 적용 직후: 크리티컬이면 `PlayCriticalHit()`, 아니면 `PlayEnemyHit()`
- `Die()` — 사망 처리 시작: `PlayEnemyDeath()`
- `Die()` — 골드 지급 직후: `PlayGoldPickup()`

### 2. WaveSpawnManager.cs (`Assets/Scripts/Core/WaveSpawnManager.cs`)

- 플레이어 자동 공격 적중 시 (hitEnemies가 있을 때): `PlayPlayerAttack()`
- `StartWave()` 또는 웨이브 시작 로직: `PlayWaveStart()`
- `SpawnBoss()` 또는 보스 스폰 로직: `PlayBossAppear()` + `PlayBossBGM()`
- 보스 처치 시 (HandleEnemyDeath에서 enemy.IsBoss 확인): `PlayBossDefeat()` + `PlayBattleBGM()`
- `AdvanceStage()` 또는 스테이지 클리어 로직: `PlayStageClear()`

### 3. FanSkill.cs (`Assets/Scripts/Core/FanSkill.cs`)

- 투사체 발사 시: `PlayStaffRush()`

### 4. MeteorSkill.cs (`Assets/Scripts/Core/MeteorSkill.cs`)

- 스킬 시작 시 1회: `PlayMeteorRain()`
- 각 유성 착탄/폭발 시: `PlayMeteorExplosion()`

### 5. ChainSkill.cs (`Assets/Scripts/Core/ChainSkill.cs`)

- 연쇄 번개 시작 시: `PlayChainLightning()`

### 6. DashSkill.cs (`Assets/Scripts/Core/DashSkill.cs`)

- 돌진 시작 시: `PlayBerserkerDash()`
- 버프 오라 발동 시: `PlayBuffActivate()`

### 7. ManualAttackButton.cs (`Assets/Scripts/Core/ManualAttackButton.cs`)

- 수동 공격 버튼 클릭 시: `PlayManualAttack()`

### 8. CharacterStats.cs (`Assets/Scripts/Core/CharacterStats.cs`)

- `LevelUp()` 내부, 레벨 증가 직후: `PlayLevelUp()`

### 9. UpgradeSystem.cs (`Assets/Scripts/Core/UpgradeSystem.cs`)

- `TryUpgrade()` 성공 시 (비용 차감 + 보너스 적용 후): `PlayUpgradeSuccess()`

### 10. OfflineRewardSystem.cs (`Assets/Scripts/Core/OfflineRewardSystem.cs`)

- 보상 수령 처리 시: `PlayOfflineReward()`

## 작업 5: SampleScene에 AudioManager 배치

unity-cli로 GameManager에 AudioManager 컴포넌트 추가:

```
unity-cli exec 'var gm = GameObject.Find("GameManager"); gm.AddComponent<AudioManager>();'
```

## 주의사항

- 기존 코드 로직을 변경하지 말 것 — 사운드 호출만 추가
- 각 파일의 실제 메서드명/변수명을 먼저 확인하고, 적절한 위치에 삽입
- PowerShell + unity-cli 사용 시 바깥은 작은따옴표, 안쪽 C# 문자열은 큰따옴표
- 작업 완료 후 `unity-cli menu 'File/Save Project'` 실행

## 완료 확인

모든 삽입 완료 후, 수정한 파일 목록과 각 파일에서 삽입한 위치를 정리해서 보고해줘.

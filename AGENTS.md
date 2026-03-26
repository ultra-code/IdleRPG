# 힘마법사 키우기 — AGENTS.md

## 환경
- 프로젝트: C:/UnityPhase1/IdleRPG
- Unity: 6000.3.6f1
- unity-cli: v0.2.27 / 포트 8090 (가끔 8092로 튐 → status 확인)

## PowerShell exec 규칙 (절대 준수)
바깥은 작은따옴표, 안쪽 C#은 큰따옴표
예: unity-cli exec 'var go = new GameObject("Name"); return "done";'

## 저장 규칙
에셋 생성/수정 후 반드시: unity-cli menu "File/Save Project"

## 에셋 생성 규칙
대량 생성 시 종류별로 나눠서 실행 (한번에 많으면 타임아웃)

## 의존성 순서 (절대 준수)
EnemyData → LevelData → StageData

## 현재 상태 (Day 1-3 완료)
- 컴파일 에러 0개 유지
- WaveSpawnManager, SkillSystem, SaveSystem, DamagePopup 작동 확인
- 스프라이트 없음 (Day 6 예정)

## Day 4 목표
Phase A: ScriptableObject 에셋 생성 (EnemyData → LevelData → StageData)
Phase B: Canvas + PlayerHUD 최소 연결
Phase C: 스킬 발동 + 밸런스 테스트
Phase D: 저장/오프라인 보상 테스트

## 파일 구조
Assets/Scripts/Core/ → CharacterStats, DamagePopup, Enemy, SaveSystem, SkillSystem, WaveSpawnManager
Assets/Scripts/Data/ → EnemyData, LevelData, SkillData, StageData (ScriptableObject)
Assets/Scripts/UI/   → EnemyHPBar, PlayerHUD, StageUI
Assets/Prefabs/      → Enemy.prefab
Assets/Skills/       → Skill_Buff, Skill_Chain, Skill_Circle, Skill_FanShape (.asset)

## 씬 오브젝트 구성
- GameManager: WaveSpawnManager, SaveSystem, OfflineRewardSystem
- Player: CharacterStats, SpriteRenderer, SkillSystem / 위치 (-2, 0, 0)
- SpawnPoint_3~7: WaveSpawnManager에 연결됨
- DamagePopupPool: DamagePopup (풀링 20개)

## 알려진 이슈 (건드리지 말 것)
- EnemyHPBar: 단일 적 기준 → Day 6에 수정 예정, 지금은 콘솔 로그로 대체
- unity-cli 포트: 가끔 8092로 튐 → unity-cli status 확인 후 재시도
- Enemy_Template: 씬에 있지만 프리팹 소스용, 게임플레이 영향 없음

## 스킬 수치
- 충격 분쇄: 쿨타임 4초, ATK×1.5, 전방 부채꼴 60도
- 근력 유성타: 쿨타임 8초, ATK×3.0, 원형 r=2.5
- 연쇄 근력 번개: 쿨타임 6초, ATK×1.2×5회, 최대 5마리
- 마력 폭주: 쿨타임 20초, 8초간 공격속도 x2 (버프)

## 밸런스 공식
기본 HP: 30 + (적레벨 × 15) × HP배율
MaxHP: 100 + (Lv-1)*20 + (Lv-1)^2*1.2
ATK: 10 + (Lv-1)*5.7
필요EXP: Floor(100 * 1.45^(Lv-1))
⚠️ 레벨 20 이후 EXP 급증 (레벨 20 ≈ 11,700 EXP) → 스테이지 21~30 보상 역산 필요

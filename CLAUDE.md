# 힘마법사 키우기 — CLAUDE.md

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

---

## PixelLab 에셋 파이프라인 ★★★

### API 환경
- SDK: pixellab (pip install pixellab)
- 환경변수: PIXELLAB_SECRET (절대 코드에 하드코딩 금지)
- SDK 파라미터 시그니처 먼저 확인 후 실행할 것

### style_image 대비 원칙 ★★★ (절대 준수)
주인공과 몬스터는 서로 대비되어야 한다:
- 히로인: 밝고 깨끗 (퍼플, 핑크, 골드, 화이트)
- 몬스터/던전: 어둡고 유기적 (붉은 살색, 뼈 크림, 초록 점액)
- **절대 hero_idle을 몬스터 style_image로 사용하지 말 것!**

올바른 생성 순서:
1. hero_idle → PixFlux로 독립 생성 (완료, seed 4004)
2. flesh_slime_idle → PixFlux로 독립 생성 (어두운 톤, style_image 없이) ← 현재 여기
3. 나머지 몬스터 4종 → Bitforge + flesh_slime_idle을 style_image로
4. 히로인 애니메이션 → 웹 UI 또는 animate_with_text (64×64 → 128×128)

### 모델 선택
- PixFlux: 첫 기준 이미지 (style_image 없이, 최대 400×400)
- Bitforge: 이후 에셋 (style_image로 스타일 통일, 최대 200×200)
- animate_with_text: 정확히 64×64만 허용 → 128×128 Nearest Neighbor 2x 업스케일

### 프롬프트 규칙
- 간결하게! 20~30단어 이내 권장
- "pixel art" 키워드 불필요 (이미 픽셀아트 전용 모델)
- 미사여구 금지 ("award winning", "masterpiece" 등)
- 구체적 외형만: 색상, 형태, 포즈, 방향
- direction, view 등은 파라미터로 지정 (프롬프트보다 우선)

### 생성 규칙
- 1장씩 생성 → 확인 → 확정 or 재생성 (3장 동시 생성은 타임아웃 위험)
- 해상도: 128×128 통일 (애니메이션만 64→128 업스케일)
- no_background: True 필수
- negative_description에 None 전달 금지 → 빈 문자열 "" 사용

### 에셋 경로
```
Assets/Sprites/Characters/hero_idle.png           ← 히로인 기준 (확정)
Assets/Sprites/Monsters/flesh_slime_idle.png      ← 몬스터 스타일 기준 (재생성 필요)
Assets/Sprites/Monsters/eye_stalker_idle.png
Assets/Sprites/Monsters/skeleton_demon_idle.png
Assets/Sprites/Monsters/blood_cells_idle.png
Assets/Sprites/Monsters/twin_head_idle.png
```

### 스프라이트 임포트 설정 (Unity)
- PPU: 128 (128px = 1 월드 유닛)
- Filter Mode: Point (필수! Bilinear 쓰면 도트 뭉개짐)
- Compression: None
- Mipmap: Off

### 기획서 컬러 팔레트
주인공 (밝은):
  머리: #3D2252, #6B3A8A / 눈: #E8457B / 드레스: #2D1B3D
  프릴: #E8A0BF / 스태프: #D4A44C / 상의: #F5F0F0 / 피부: #F5D5C8

던전 (어두운):
  살점: #8B3A3A, #C45C5C, #4A1A1A / 뼈: #E8DCC8, #A89878
  혈액: #CC2222, #8B0000 / 점액: #88CC44 / 신경: #00CCCC

---

## 현재 상태 (밸런스/데이터 구조 보정 진행)
- 컴파일 에러 0개 유지
- P1 밸런스 수정 반영: LevelData/CharacterStats 성장식, SkillData 4종, EnemyData 5종 기본값
- P2 구조 추가 반영: StageData 배율 필드, EnemyData RoleType/MoveSpeed/AttackRange, BossData 3종, Enemy/WaveSpawnManager 배율 초기화
- SkillSystem Buff는 적 유무와 무관하게 자기 버프로 발동
- 프로시저럴 스프라이트 적용 완료 → PixelLab 아트로 교체 진행 중
- TMP 한글 미지원 → 런타임 텍스트 전부 영문

## 파일 구조
```
Assets/Scripts/Core/  → CharacterStats, Enemy, WaveSpawnManager, SkillSystem,
                        SaveSystem, OfflineRewardSystem, DamagePopup,
                        ProceduralSpriteLibrary, SimpleSkillEffect, UpgradeSystem
Assets/Scripts/Data/  → EnemyData, LevelData, SkillData, StageData
Assets/Scripts/UI/    → PlayerHUD, ShopUI, BossUIController, EnemyWorldHPBar
Assets/Editor/        → DataAssetGenerator, CanvasSetupGenerator, ShopSetupGenerator, HUDConnector
Assets/Data/          → Enemies 5종, Stages 30개, DefaultLevelData
Assets/Skills/        → Skill_Buff, Skill_Chain, Skill_Circle, Skill_FanShape
```

## 알려진 이슈
- EnemyHPBar.cs / StageUI.cs: 레거시, 삭제 가능
- Enemy_Template: 씬에 있지만 프리팹 소스용, 건드리지 말 것
- FindAnyObjectByType<Canvas>()는 World Space Canvas 먼저 찾을 수 있음 → FindScreenSpaceCanvas() 사용
- unity-cli 포트 가끔 8092로 튐 → unity-cli status 확인
- 오프라인 보상 UI 팝업 미구현 (OnRewardReady 이벤트만 노출)
- StageData 1~30의 HPMult/ATKMult/DEFMult/GoldMult/BossID 최종 값은 기획서 8장 표 원본 확인 후 Inspector 재입력 필요
- WaveSpawnManager의 bossDataList Inspector 연결 여부는 씬에서 최종 확인 필요

## 밸런스 공식
기본 HP: 30 + (적레벨 × 15) × HP배율
MaxHP: 100 + (Lv-1)*20 + (Lv-1)^2*1.2
ATK: 10 + (Lv-1)*5.7
필요EXP: Floor(100 * 1.45^(Lv-1))
강화 비용: Floor(50 * 1.3^currentLevel)
⚠️ 레벨 20 이후 EXP 급증 (≈11,700) → 스테이지 21~30 보상 역산 필요

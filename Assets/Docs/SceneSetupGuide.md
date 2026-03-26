# IdleRPG 씬 세팅 가이드

SampleScene을 기준으로 게임이 동작하도록 세팅하는 단계별 가이드입니다.

---

## 씬 Hierarchy 최종 구조

```
SampleScene
├── Player              (CharacterStats)
├── EnemySpawnPoint     (빈 오브젝트, 위치 마커)
├── EnemyPrefab         (Enemy + SpriteRenderer → 프리팹화 후 씬에서 삭제)
├── GameManager         (AutoBattleSystem, SaveSystem, OfflineRewardSystem)
└── Canvas              (UI)
    ├── PlayerHUD       (PlayerHUD)
    │   ├── LevelText       (TextMeshPro)
    │   ├── HPText          (TextMeshPro)
    │   ├── HPFill          (Image, Filled)
    │   ├── ExpFill         (Image, Filled)
    │   ├── ATKText         (TextMeshPro)
    │   ├── DEFText         (TextMeshPro)
    │   └── GoldText        (TextMeshPro)
    ├── EnemyHPBar      (EnemyHPBar)
    │   ├── NameText        (TextMeshPro)
    │   ├── HPText          (TextMeshPro)
    │   └── HPFill          (Image, Filled)
    └── StageUI         (StageUI)
        └── StageText       (TextMeshPro)
```

---

## 1단계: ScriptableObject 데이터 에셋 생성

에디터에서 먼저 데이터 에셋을 만들어 둡니다.

### LevelData
1. Project 창 우클릭 → `Create > IdleRPG > Level Data`
2. 이름: `DefaultLevelData`
3. Inspector에서 성장 수치 설정:
   - Base HP: `100` / Base ATK: `10` / Base DEF: `5`
   - HP Per Level: `20` / ATK Per Level: `3` / DEF Per Level: `1.5`
   - Base Exp To Next Level: `100` / Exp Multiplier: `1.5`

### EnemyData (적 종류별로 1개씩)
1. `Create > IdleRPG > Enemy Data`
2. 이름 예시: `EnemyData_Slime`
3. Inspector 설정:
   - Enemy Name: `슬라임`
   - Sprite: 슬라임 스프라이트 할당
   - Base HP: `50` / Base ATK: `8` / Base DEF: `2`
   - Attack Interval: `1.5`
   - Base Exp Reward: `30` / Base Gold Reward: `10`

### StageData (스테이지별로 1개씩)
1. `Create > IdleRPG > Stage Data`
2. 이름 예시: `StageData_01_Grassland`
3. Inspector 설정:
   - Stage Name: `초원`
   - Stage Number: `1`
   - Enemy Pool: 해당 스테이지에 등장할 EnemyData 에셋들을 배열에 추가
   - Enemies Per Stage: `5`
   - Min/Max Enemy Level: `1` / `1`
   - Player Attack Interval: `1.0`
   - Enemy Spawn Delay: `1.0`

---

## 2단계: Player 오브젝트

1. Hierarchy 우클릭 → `Create Empty` → 이름: **Player**
2. `Add Component` → **CharacterStats**
3. Inspector 연결:
   | 필드 | 값 |
   |------|----|
   | Level Data | `DefaultLevelData` 에셋 (선택사항, 없으면 기본값 사용) |
4. SpriteRenderer를 추가하고 플레이어 스프라이트 할당 (시각적 표현이 필요한 경우)

---

## 3단계: Enemy 프리팹

1. Hierarchy 우클릭 → `Create Empty` → 이름: **Enemy**
2. `Add Component` → **Enemy**
3. `Add Component` → **SpriteRenderer** (스프라이트는 Initialize()에서 EnemyData로부터 자동 할당됨)
4. 이 오브젝트를 Project 창의 `Assets/Prefabs/` 폴더로 드래그하여 **프리팹으로 저장**
5. Hierarchy에서 원본 오브젝트 **삭제** (런타임에 AutoBattleSystem이 생성함)

---

## 4단계: EnemySpawnPoint

1. Hierarchy 우클릭 → `Create Empty` → 이름: **EnemySpawnPoint**
2. 적이 생성될 위치로 Transform.Position 이동 (예: X: `3`, Y: `0`, Z: `0`)
3. 컴포넌트 추가 없음 — 위치 마커 역할만 함

---

## 5단계: GameManager 오브젝트

1. Hierarchy 우클릭 → `Create Empty` → 이름: **GameManager**
2. 아래 3개 컴포넌트를 모두 추가:

### AutoBattleSystem
| 필드 | 연결 대상 |
|------|-----------|
| Player | Hierarchy의 `Player` 오브젝트 |
| Enemy Spawn Point | Hierarchy의 `EnemySpawnPoint` 오브젝트 |
| Enemy Prefab | Project 창의 `Enemy` 프리팹 |
| Stages | StageData 에셋 배열 (순서대로 추가, 선택사항) |
| Player Attack Interval | `1.0` (StageData 없을 때 사용) |
| Enemy Spawn Delay | `1.0` (StageData 없을 때 사용) |
| Enemies Per Stage | `5` (StageData 없을 때 사용) |

### SaveSystem
| 필드 | 연결 대상 |
|------|-----------|
| Player | Hierarchy의 `Player` 오브젝트 |
| Battle System | 같은 오브젝트의 `AutoBattleSystem` |
| Auto Save Interval | `30` (초) |

### OfflineRewardSystem
| 필드 | 연결 대상 |
|------|-----------|
| Player | Hierarchy의 `Player` 오브젝트 |
| Save System | 같은 오브젝트의 `SaveSystem` |
| Gold Per Second | `1` |
| Max Offline Hours | `8` |

> 참고: Player, Battle System, Save System 필드를 비워두면 `FindAnyObjectByType`으로 자동 탐색하지만, 직접 할당하는 것이 성능상 권장됩니다.

---

## 6단계: Canvas (UI)

1. Hierarchy 우클릭 → `UI > Canvas`
2. Canvas 설정:
   - Render Mode: `Screen Space - Overlay`
   - Canvas Scaler → UI Scale Mode: `Scale With Screen Size`
   - Reference Resolution: `1920 x 1080`
   - Match: `0.5`

### 6-1. PlayerHUD

1. Canvas 하위에 `Create Empty` → 이름: **PlayerHUD**
2. `Add Component` → **PlayerHUD**
3. 하위에 TextMeshPro 오브젝트 5개 생성:
   - `UI > Text - TextMeshPro` → 이름: **LevelText**, **HPText**, **ATKText**, **DEFText**, **GoldText**
4. 하위에 HP바, EXP바용 Image 2개 생성:
   - `UI > Image` → 이름: **HPFill**, **ExpFill**
   - 각 Image의 Image Type: `Filled`, Fill Method: `Horizontal`
   - HPFill 색상: 초록 또는 빨강 / ExpFill 색상: 노랑 또는 파랑
5. Inspector 연결:

| 필드 | 연결 대상 |
|------|-----------|
| Player | Hierarchy의 `Player` 오브젝트 (또는 비워두면 자동 탐색) |
| Level Text | `LevelText` 오브젝트 |
| Hp Text | `HPText` 오브젝트 |
| Atk Text | `ATKText` 오브젝트 |
| Def Text | `DEFText` 오브젝트 |
| Gold Text | `GoldText` 오브젝트 |
| Hp Fill | `HPFill` 오브젝트의 Image 컴포넌트 |
| Exp Fill | `ExpFill` 오브젝트의 Image 컴포넌트 |

### 6-2. EnemyHPBar

1. Canvas 하위에 `Create Empty` → 이름: **EnemyHPBar**
2. `Add Component` → **EnemyHPBar**
3. 하위에 생성:
   - `Text - TextMeshPro` → 이름: **NameText** (적 이름 + 레벨)
   - `Text - TextMeshPro` → 이름: **HPText** (현재HP / 최대HP)
   - `Image` → 이름: **HPFill** (Image Type: Filled, Fill Method: Horizontal)
4. Inspector 연결:

| 필드 | 연결 대상 |
|------|-----------|
| Battle System | GameManager의 `AutoBattleSystem` (또는 비워두면 자동 탐색) |
| Name Text | `NameText` 오브젝트 |
| Hp Text | `HPText` 오브젝트 |
| Hp Fill | `HPFill` 오브젝트의 Image 컴포넌트 |

### 6-3. StageUI

1. Canvas 하위에 `Create Empty` → 이름: **StageUI**
2. `Add Component` → **StageUI**
3. 하위에 `Text - TextMeshPro` → 이름: **StageText**
4. Inspector 연결:

| 필드 | 연결 대상 |
|------|-----------|
| Battle System | GameManager의 `AutoBattleSystem` (또는 비워두면 자동 탐색) |
| Stage Text | `StageText` 오브젝트 |

---

## 실행 순서 참고 (Awake → Start 흐름)

```
Awake:  CharacterStats (스탯 초기화, HP = MaxHP)
        Enemy (currentHP = MaxHP) — 런타임 생성 시

Start:  SaveSystem.LoadGame()         — 저장된 데이터 복원
        OfflineRewardSystem.Check()   — 오프라인 보상 계산, OnRewardReady 이벤트
        AutoBattleSystem.SpawnEnemy() — 첫 적 생성, 전투 시작
        UI 스크립트들                  — 이벤트 구독 및 초기 표시
```

SaveSystem이 CharacterStats보다 나중에 Start되어야 정상 작동합니다.
필요시 `Edit > Project Settings > Script Execution Order`에서 순서를 지정하세요:
1. `CharacterStats` (기본)
2. `SaveSystem` (+100)
3. `OfflineRewardSystem` (+200)
4. `AutoBattleSystem` (+300)

---

## 체크리스트

- [ ] LevelData 에셋 생성 및 CharacterStats에 연결
- [ ] EnemyData 에셋 최소 1개 생성
- [ ] StageData 에셋 최소 1개 생성 (선택)
- [ ] Enemy 프리팹 생성 (Enemy + SpriteRenderer)
- [ ] Player 오브젝트에 CharacterStats 부착
- [ ] GameManager에 AutoBattleSystem, SaveSystem, OfflineRewardSystem 부착
- [ ] AutoBattleSystem에 Player, EnemySpawnPoint, EnemyPrefab 연결
- [ ] Canvas 생성 및 UI 스크립트 3개 부착
- [ ] 각 UI 스크립트에 Text/Image 연결
- [ ] TextMeshPro 패키지 Import (Window > TextMeshPro > Import TMP Essential Resources)
- [ ] Play 버튼 눌러서 자동 전투 동작 확인

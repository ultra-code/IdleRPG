# Day 5 실행 가이드 — 세션별 작업 분할
Claude Code / Codex 번갈아 사용 시 이 문서를 매 세션 첫 메시지에 첨부할 것

---

## 세션 시작 시 첫 메시지 템플릿

```
나는 Unity 2D 방치형 RPG "힘마법사 키우기"를 개발 중입니다.
@HANDOFF.md 를 읽고 현재 상태를 파악한 뒤
Day 5 Task [번호]를 진행해줘.
Plan 모드로 작업 순서 설계 먼저 해줘.
```

---

## Day 5 전체 목표
골드 소비처를 만들어서 코어루프 완성:
전투 → 골드/EXP 획득 → 레벨업 + **강화** → 더 강한 전투

---

## Task 1: EnemyHPBar 다중 적 대응 (세션 1개)
**예상 시간: 30~45분**

### 현재 문제
- EnemyHPBar.cs가 단일 적 기준으로 작성됨
- 여러 적이 동시에 존재하는데 HP바가 1개뿐

### 목표
- 각 적 오브젝트 머리 위에 개별 HP바 표시
- World Space Canvas 방식 사용

### 구현 명세
1. Enemy.prefab에 자식으로 Canvas (World Space) 추가
   - Canvas Render Mode: World Space
   - Canvas 크기: 1.0 x 0.15 정도
   - 위치: 적 머리 위 (y offset +0.8 정도)
   - Sorting Layer: UI (적보다 앞에)

2. Canvas 안에 HP바 UI 구성
   - Background (검정 또는 회색 Image)
   - Fill (빨간 → 초록 그라데이션 또는 단색 초록 Image)
   - Fill은 Image.fillAmount로 HP 비율 표시

3. EnemyHPBar.cs 수정 또는 신규 EnemyHPBarWorld.cs
   - Enemy 컴포넌트에서 CurrentHP / MaxHP 참조
   - 매 프레임 또는 HP 변경 시 fillAmount 업데이트
   - 적 사망 시 HP바도 함께 비활성화

4. Enemy.prefab 업데이트
   - 기존 EnemyHPBar 참조 제거
   - 새 World Space HP바 연결

### 완료 조건
- 적 3마리 이상 동시에 각자 HP바 표시
- HP 감소에 따라 바가 줄어듦
- 적 사망 시 HP바 사라짐
- 컴파일 에러 0개

### 주의사항
- Enemy_Template (씬에 있는 프리팹 소스) 건드리지 말 것
- Enemy.prefab을 수정하면 반드시 unity-cli menu "File/Save Project"

---

## Task 2: 강화 시스템 구현 (세션 1~2개)
**예상 시간: 45~60분**

### 목표
- 골드를 소비해서 HP, ATK, DEF를 영구적으로 올리는 시스템
- SaveSystem에 강화 데이터 저장

### 구현 명세

#### 2-1. UpgradeSystem.cs (신규 생성)
위치: Assets/Scripts/Core/UpgradeSystem.cs

```
public class UpgradeSystem : MonoBehaviour
{
    // 강화 레벨 (각각 독립)
    public int hpLevel = 0;
    public int atkLevel = 0;
    public int defLevel = 0;
    
    // 강화 효과
    public float GetBonusHP()  => hpLevel * 10f;
    public float GetBonusATK() => atkLevel * 3f;
    public float GetBonusDEF() => defLevel * 1.5f;
    
    // 강화 비용: 기본 50골드, 레벨당 ×1.3 증가
    public int GetUpgradeCost(int currentLevel)
    {
        return Mathf.FloorToInt(50 * Mathf.Pow(1.3f, currentLevel));
    }
    
    // 강화 실행
    public bool TryUpgrade(string statType, CharacterStats player)
    {
        int level, cost;
        switch(statType)
        {
            case "HP":  level = hpLevel;  break;
            case "ATK": level = atkLevel; break;
            case "DEF": level = defLevel; break;
            default: return false;
        }
        cost = GetUpgradeCost(level);
        
        if (player.Gold < cost) return false;
        
        player.Gold -= cost;
        switch(statType)
        {
            case "HP":  hpLevel++;  break;
            case "ATK": atkLevel++; break;
            case "DEF": defLevel++; break;
        }
        
        ApplyBonuses(player);
        return true;
    }
    
    // 보너스 적용 (레벨업 시에도 호출해야 함)
    public void ApplyBonuses(CharacterStats player)
    {
        // CharacterStats에 bonusHP, bonusATK, bonusDEF 필드 추가 필요
        player.bonusHP  = GetBonusHP();
        player.bonusATK = GetBonusATK();
        player.bonusDEF = GetBonusDEF();
    }
}
```

#### 2-2. CharacterStats.cs 수정
- bonusHP, bonusATK, bonusDEF float 필드 추가
- MaxHP, ATK, DEF 프로퍼티에서 base + bonus 합산하도록 수정
- 기존 레벨업 로직은 base 값만 변경, bonus는 UpgradeSystem이 관리

#### 2-3. SaveSystem.cs 수정
- 저장 데이터에 hpLevel, atkLevel, defLevel 추가
- 로드 시 UpgradeSystem에 값 복원 + ApplyBonuses 호출

#### 2-4. GameManager 연결
- GameManager 오브젝트에 UpgradeSystem 컴포넌트 추가
- 또는 별도 오브젝트 생성

### 완료 조건
- 골드 소비하여 HP/ATK/DEF 강화 가능
- 강화 후 실제 스탯 증가 확인
- 저장/로드 시 강화 레벨 유지
- 골드 부족 시 강화 불가
- 컴파일 에러 0개

---

## Task 3: 상점 UI 구현 (세션 1개)
**예상 시간: 30~45분**

### 목표
- 화면에 강화 상점 버튼/패널 추가
- 강화 버튼 3개 (HP/ATK/DEF) + 현재 레벨 + 비용 표시

### 구현 명세

#### 3-1. ShopUI.cs (신규 생성)
위치: Assets/Scripts/UI/ShopUI.cs

기능:
- 상점 패널 열기/닫기 토글
- HP 강화 버튼: "HP 강화 Lv.{n} | {cost}G" → 클릭 시 UpgradeSystem.TryUpgrade("HP")
- ATK 강화 버튼: "ATK 강화 Lv.{n} | {cost}G" → 클릭 시 TryUpgrade("ATK")
- DEF 강화 버튼: "DEF 강화 Lv.{n} | {cost}G" → 클릭 시 TryUpgrade("DEF")
- 강화 성공 시 버튼 텍스트 갱신
- 골드 부족 시 버튼 색상 변경 (회색) 또는 비활성화
- 현재 보유 골드 표시

#### 3-2. UI 배치
- 화면 우하단에 "상점" 버튼 (항상 표시)
- 클릭 시 상점 패널 열림 (화면 중앙 또는 하단)
- 패널에 강화 버튼 3개 세로 배치
- 닫기 버튼

#### 3-3. Canvas 연결
- 기존 Canvas에 ShopUI 추가
- 또는 별도 Canvas (Screen Space - Overlay)

### 완료 조건
- 상점 버튼으로 패널 열기/닫기
- 강화 버튼 클릭 시 골드 소비 + 스탯 증가
- 레벨/비용 텍스트 실시간 갱신
- 골드 부족 시 시각적 피드백
- 전투 중에도 상점 접근 가능
- 컴파일 에러 0개

---

## Task 4: 밸런스 1차 확인 (세션 1개, Task 1~3 완료 후)
**예상 시간: 20~30분**

### 확인 사항
1. 스테이지 1~3: 강화 없이 돌파 가능?
2. 스테이지 4~5: 강화 0~1회로 돌파 가능?
3. 스테이지 6~8: 강화 2~4회 필요?
4. 골드 수급: 스테이지 1~5 클리어 시 누적 골드 vs 강화 3회 비용
5. 레벨업 + 강화 조합으로 스테이지 8 돌파 가능?

### 밸런스 조정이 필요한 경우
- 강화 비용이 너무 비싸면: 기본 비용 50 → 30으로 하향
- 강화 효과가 너무 약하면: HP +10 → +15, ATK +3 → +5로 상향
- 골드 획득이 너무 적으면: EnemyData의 goldMult 상향

### 확인 명령어
```powershell
# 플레이 시작
unity-cli editor play --wait

# 현재 스탯 확인
unity-cli exec 'var p = UnityEngine.Object.FindObjectOfType<CharacterStats>(); return $"HP:{p.CurrentHP}/{p.MaxHP} ATK:{p.ATK} DEF:{p.DEF} Gold:{p.Gold} Lv:{p.Level}";'

# 적 상태 확인
unity-cli exec 'var enemies = UnityEngine.Object.FindObjectsOfType<Enemy>(); var sb = new System.Text.StringBuilder(); foreach(var e in enemies) sb.AppendLine(e.name + " HP:" + e.CurrentHP); return sb.ToString();'

# 플레이 중지
unity-cli editor stop
```

---

## 작업 순서 요약

| 순서 | Task | 세션 수 | 의존성 |
|------|------|--------|--------|
| 1 | EnemyHPBar 다중 적 대응 | 1 | 없음 |
| 2 | 강화 시스템 구현 | 1~2 | 없음 |
| 3 | 상점 UI 구현 | 1 | Task 2 완료 필요 |
| 4 | 밸런스 1차 확인 | 1 | Task 1~3 모두 완료 |

Task 1과 Task 2는 독립적이므로 **동시 진행 가능** (각각 다른 세션에서).
Task 3은 Task 2의 UpgradeSystem이 있어야 함.
Task 4는 전부 완료 후.

---

## 세션 종료 시 확인사항
매 세션 끝에 반드시:
1. `unity-cli menu "File/Save Project"` 실행
2. 컴파일 에러 확인: `unity-cli console --filter error`
3. HANDOFF.md 업데이트 (완료된 작업 체크)
4. 다음 세션에서 할 Task 번호 기록

---

## 환경 주의사항 (HANDOFF.md에서 발췌)
- Unity: 6000.3.6f1 (URP 2D)
- unity-cli: v0.2.27 / 포트 8090 (가끔 8092)
- PowerShell exec 규칙: 바깥 작은따옴표, 안쪽 C# 큰따옴표
- Claude Code는 Bash로 unity-cli 직접 실행 불가 → Editor 스크립트 방식 사용
- 에셋 생성 후 반드시: unity-cli menu "File/Save Project"

## Day 5 목표
- 코어루프 완성 (골드 소비처 만들기)
- Task 1: EnemyHPBar 다중 적 대응 (World Space Canvas)
- Task 2: 강화 시스템 (UpgradeSystem.cs)
- Task 3: 상점 UI (ShopUI.cs)
- Task 4: 밸런스 1차 확인
- Task 1과 2는 독립적, Task 3은 Task 2 필요, Task 4는 전부 필요
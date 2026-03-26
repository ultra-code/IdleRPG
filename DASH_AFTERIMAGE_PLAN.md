# 번개 돌진 + 잔상 이펙트 구현 작업 지시서

프로젝트: C:/UnityPhase1/IdleRPG
먼저 HANDOFF.md를 읽어서 현재 상태를 파악하고, 아래 작업을 진행해줘.

---

## 배경

플레이 테스트에서 "플레이어가 번개 이펙트를 쓰며 이동하면 전투 쾌감이 극대화될 것"이라는 피드백이 나왔다.
작업 4에서 플레이어 자동 이동이 이미 구현되었으므로 (CharacterStats.cs FixedUpdate), 여기에 시각 이펙트를 추가한다.

---

## 작업 A: 잔상 시스템 (AfterImageSystem.cs)

새 스크립트 `Assets/Scripts/Core/AfterImageSystem.cs` 생성.

### 핵심 로직
```csharp
using System.Collections;
using UnityEngine;

public class AfterImageSystem : MonoBehaviour
{
    [Header("잔상 설정")]
    [SerializeField] private float spawnInterval = 0.08f;   // 잔상 생성 간격
    [SerializeField] private float fadeDuration = 0.3f;      // 페이드아웃 시간
    [SerializeField] private Color afterImageColor = new Color(0.25f, 0.5f, 1f, 0.6f); // 파란 번개색
    [SerializeField] private int poolSize = 10;

    private SpriteRenderer sourceRenderer;
    private GameObject[] pool;
    private int poolIndex;
    private float spawnTimer;
    private bool isMoving;
    private Vector3 lastPosition;

    private void Awake()
    {
        sourceRenderer = GetComponent<SpriteRenderer>();
        InitPool();
    }

    private void InitPool()
    {
        pool = new GameObject[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            var go = new GameObject($"AfterImage_{i}");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = sourceRenderer != null ? sourceRenderer.sortingOrder - 1 : 0;
            go.SetActive(false);
            pool[i] = go;
        }
    }

    private void Update()
    {
        // 이동 감지
        float moved = Vector3.Distance(transform.position, lastPosition);
        isMoving = moved > 0.01f;
        lastPosition = transform.position;

        if (!isMoving) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            SpawnAfterImage();
        }
    }

    private void SpawnAfterImage()
    {
        if (sourceRenderer == null || sourceRenderer.sprite == null) return;

        var go = pool[poolIndex];
        poolIndex = (poolIndex + 1) % poolSize;

        go.transform.position = transform.position;
        go.transform.localScale = transform.localScale;

        var sr = go.GetComponent<SpriteRenderer>();
        sr.sprite = sourceRenderer.sprite;
        sr.flipX = sourceRenderer.flipX;
        sr.color = afterImageColor;

        go.SetActive(true);
        StartCoroutine(FadeAndDisable(go, sr));
    }

    private IEnumerator FadeAndDisable(GameObject go, SpriteRenderer sr)
    {
        float elapsed = 0f;
        Color startColor = afterImageColor;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0f, elapsed / fadeDuration);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        go.SetActive(false);
    }

    private void OnDestroy()
    {
        if (pool == null) return;
        for (int i = 0; i < pool.Length; i++)
        {
            if (pool[i] != null)
                Destroy(pool[i]);
        }
    }
}
```

### Player에 컴포넌트 추가
SampleScene의 Player 오브젝트에 AfterImageSystem 컴포넌트를 추가해줘.
unity-cli 또는 에디터 스크립트로.

---

## 작업 B: 버프 중 이동속도 증가

CharacterStats.cs의 FixedUpdate에서, 마력 폭주(Buff) 활성화 시 이동속도를 2배로:

```csharp
// FixedUpdate 내부, 이동 부분:
float currentMoveSpeed = moveSpeed;

// SkillSystem 버프 상태 확인
SkillSystem skillSystem = waveManager?.GetComponent<SkillSystem>();
if (skillSystem == null) skillSystem = FindAnyObjectByType<SkillSystem>();
bool buffActive = skillSystem != null && skillSystem.IsBuffActive;

if (buffActive)
    currentMoveSpeed *= 2f;

rb.MovePosition(rb.position + dir * currentMoveSpeed * Time.fixedDeltaTime);
```

SkillSystem 참조는 캐싱해서 매 프레임 FindAnyObjectByType 호출을 피해야 한다.

---

## 작업 C: 버프 중 잔상 강화

AfterImageSystem에서 버프 활성화 시 잔상을 더 자주, 더 밝게:

```csharp
// Update에서 isMoving 체크 후:
float currentInterval = spawnInterval;
// 버프 중이면 잔상을 더 자주 생성
SkillSystem skillSystem = FindAnyObjectByType<SkillSystem>(); // 캐싱 권장
if (skillSystem != null && skillSystem.IsBuffActive)
{
    currentInterval *= 0.4f; // 2.5배 더 자주
    afterImageColor = new Color(0.4f, 0.2f, 1f, 0.7f); // 보라색으로 변경
}
else
{
    afterImageColor = new Color(0.25f, 0.5f, 1f, 0.6f); // 기본 파란색
}
```

---

## 작업 D: 이동 시 스프라이트 방향 보정

현재 CharacterStats.cs에서 spriteRenderer.flipX로 방향 전환하고 있는데,
Animator가 있는 경우 localScale.x를 음수로 하는 방식이 더 안정적일 수 있다.

확인 사항:
- 현재 flipX 방식이 Animator와 충돌하는지 테스트
- 충돌하면 localScale 방식으로 전환

---

## 완료 후 확인

1. Play 모드에서 플레이어가 이동할 때 파란 잔상이 남는지
2. 마력 폭주(버프) 중에 이동이 빨라지고 잔상이 더 자주/보라색으로 나오는지
3. 잔상이 0.3초 후 자연스럽게 사라지는지
4. 잔상 오브젝트가 풀링되어 메모리 누수가 없는지 (Hierarchy에서 AfterImage_0~9 확인)
5. 전투 중 프레임 드롭이 없는지

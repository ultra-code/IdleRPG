using UnityEngine;
using UnityEngine.UI;

public class EnemyWorldHPBar : MonoBehaviour
{
    private Enemy enemy;
    private Image hpFill;
    private GameObject barRoot;

    public static void Create(Enemy enemy)
    {
        if (enemy == null) return;

        // Canvas 오브젝트
        var canvasObj = new GameObject("HPBarCanvas");
        canvasObj.transform.SetParent(enemy.transform, false);
        canvasObj.transform.localPosition = new Vector3(0f, 0.8f, 0f);

        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 100;

        // World Space Canvas는 RectTransform 크기 × localScale = 실제 월드 크기
        // 100 × 0.01 = 월드 1.0 유닛 폭
        var canvasRt = canvasObj.GetComponent<RectTransform>();
        canvasRt.sizeDelta = new Vector2(100f, 8f);

        // Scale-compensate: cancel out parent (enemy) localScale so HP bar stays a fixed world size
        float parentScale = enemy.transform.localScale.x;
        float invScale = parentScale > 0f ? 1f / parentScale : 1f;
        float baseScale = 0.01f * invScale;
        canvasObj.transform.localScale = new Vector3(baseScale, baseScale, baseScale);

        // CanvasScaler 불필요 — 월드 스페이스에선 localScale로 제어

        // Bar Root (표시/숨김 토글용, Canvas는 항상 활성)
        var barRoot = new GameObject("BarRoot");
        barRoot.transform.SetParent(canvasObj.transform, false);
        var barRootRt = barRoot.AddComponent<RectTransform>();
        barRootRt.anchorMin = Vector2.zero;
        barRootRt.anchorMax = Vector2.one;
        barRootRt.offsetMin = Vector2.zero;
        barRootRt.offsetMax = Vector2.zero;

        // Background
        var bgObj = new GameObject("Background");
        bgObj.transform.SetParent(barRoot.transform, false);
        var bgRt = bgObj.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        var bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);

        // HP Fill
        var fillObj = new GameObject("HPFill");
        fillObj.transform.SetParent(barRoot.transform, false);
        var fillRt = fillObj.AddComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;
        var fillImg = fillObj.AddComponent<Image>();
        fillImg.color = new Color(0.2f, 0.8f, 0.2f, 1f);
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImg.fillAmount = 1f;

        // 컴포넌트를 Canvas 오브젝트에 부착 (Canvas는 항상 활성이므로 Update 동작 보장)
        var hpBar = canvasObj.AddComponent<EnemyWorldHPBar>();
        hpBar.enemy = enemy;
        hpBar.hpFill = fillImg;
        hpBar.barRoot = barRoot;

        // 피격 전엔 바만 숨김 (Canvas/컴포넌트는 활성 유지)
        barRoot.SetActive(false);
    }

    private void Update()
    {
        if (enemy == null || enemy.IsDead)
        {
            if (barRoot != null && barRoot.activeSelf)
                barRoot.SetActive(false);
            return;
        }

        float ratio = enemy.HPRatio;

        // HP 100%: 바 숨김
        if (ratio >= 1f)
        {
            if (barRoot.activeSelf)
                barRoot.SetActive(false);
            return;
        }

        // HP < 100%: 바 표시
        if (!barRoot.activeSelf)
            barRoot.SetActive(true);

        hpFill.fillAmount = ratio;

        // 초록(100%) → 노랑(50%) → 빨강(0%)
        if (ratio > 0.5f)
            hpFill.color = Color.Lerp(new Color(1f, 0.9f, 0.2f), new Color(0.2f, 0.8f, 0.2f), (ratio - 0.5f) * 2f);
        else
            hpFill.color = Color.Lerp(new Color(0.8f, 0.1f, 0.1f), new Color(1f, 0.9f, 0.2f), ratio * 2f);
    }

    private void LateUpdate()
    {
        if (barRoot == null || !barRoot.activeSelf) return;

        var cam = Camera.main;
        if (cam != null)
            transform.rotation = cam.transform.rotation;
    }
}

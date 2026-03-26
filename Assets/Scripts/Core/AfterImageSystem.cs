using System.Collections;
using UnityEngine;

public class AfterImageSystem : MonoBehaviour
{
    [Header("After Image Settings")]
    [SerializeField] private float spawnInterval = 0.08f;
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private Color afterImageColor = new Color(0.25f, 0.5f, 1f, 0.6f);
    [SerializeField] private int poolSize = 10;

    private SpriteRenderer sourceRenderer;
    private GameObject[] pool;
    private int poolIndex;
    private readonly Color normalColor = new Color(0.25f, 0.5f, 1f, 0.6f);
    private readonly Color buffColor = new Color(0.4f, 0.2f, 1f, 0.7f);

    private SkillSystem skillSystemCache;
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
        float moved = Vector3.Distance(transform.position, lastPosition);
        isMoving = moved > 0.01f;
        lastPosition = transform.position;

        if (!isMoving) return;

        if (skillSystemCache == null)
            skillSystemCache = FindAnyObjectByType<SkillSystem>();
        bool buffActive = skillSystemCache != null && skillSystemCache.IsBuffActive;

        float currentInterval = buffActive ? spawnInterval * 0.4f : spawnInterval;
        afterImageColor = buffActive ? buffColor : normalColor;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= currentInterval)
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

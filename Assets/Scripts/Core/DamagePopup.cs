using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum DamagePopupType
{
    Normal,
    Skill,
    Boss
}

public class DamagePopup : MonoBehaviour
{
    private static DamagePopup instance;
    private static readonly Queue<DamagePopup> pool = new Queue<DamagePopup>();
    private const int POOL_SIZE = 20;

    [Header("설정")]
    [SerializeField] private float duration = 0.6f;
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float normalFontSize = 6f;
    [SerializeField] private float criticalFontSize = 9f;

    private TextMeshPro textMesh;
    private float timer;
    private Color startColor;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
        if (textMesh == null)
            textMesh = gameObject.AddComponent<TextMeshPro>();

        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.sortingOrder = 100;
    }

    public static void Create(Vector3 position, int damage, bool isCritical, DamagePopupType popupType = DamagePopupType.Normal)
    {
        DamagePopup popup = GetFromPool(position);
        if (popup == null) return;

        popup.Setup(damage, isCritical, popupType);
    }

    private static DamagePopup GetFromPool(Vector3 position)
    {
        if (pool.Count > 0)
        {
            DamagePopup popup = pool.Dequeue();
            if (popup != null)
            {
                popup.transform.position = position;
                popup.gameObject.SetActive(true);
                return popup;
            }
        }

        if (instance == null)
        {
            InitializePool();
            if (pool.Count > 0)
            {
                DamagePopup popup = pool.Dequeue();
                popup.transform.position = position;
                popup.gameObject.SetActive(true);
                return popup;
            }
        }

        return CreateNewPopup(position);
    }

    private static void InitializePool()
    {
        GameObject container = new GameObject("DamagePopupPool");
        instance = container.AddComponent<DamagePopup>();
        instance.gameObject.SetActive(false);

        for (int i = 0; i < POOL_SIZE; i++)
        {
            DamagePopup popup = CreateNewPopup(Vector3.zero);
            popup.gameObject.SetActive(false);
            pool.Enqueue(popup);
        }
    }

    private static DamagePopup CreateNewPopup(Vector3 position)
    {
        GameObject obj = new GameObject("DamagePopup");
        obj.transform.position = position;

        TextMeshPro tmp = obj.AddComponent<TextMeshPro>();
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.sortingOrder = 100;
        tmp.rectTransform.sizeDelta = new Vector2(4f, 2f);

        DamagePopup popup = obj.AddComponent<DamagePopup>();
        popup.textMesh = tmp;
        return popup;
    }

    private void Setup(int damage, bool isCritical, DamagePopupType popupType)
    {
        timer = 0f;
        textMesh.text = damage.ToString();

        switch (popupType)
        {
            case DamagePopupType.Skill:
                textMesh.fontSize = isCritical ? criticalFontSize + 1f : normalFontSize + 0.5f;
                textMesh.color = new Color(1f, 0.55f, 0.15f, 1f);
                break;
            case DamagePopupType.Boss:
                textMesh.fontSize = (isCritical ? criticalFontSize : normalFontSize) * 1.5f;
                textMesh.color = new Color(1f, 0.2f, 0.2f, 1f);
                break;
            default:
                textMesh.fontSize = isCritical ? criticalFontSize : normalFontSize;
                textMesh.color = isCritical ? new Color(1f, 0.9f, 0.2f, 1f) : Color.white;
                break;
        }

        startColor = textMesh.color;
    }

    private void Update()
    {
        if (textMesh == null) return;

        timer += Time.deltaTime;

        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        float alpha = Mathf.Lerp(1f, 0f, timer / duration);
        textMesh.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

        if (timer >= duration)
        {
            gameObject.SetActive(false);
            pool.Enqueue(this);
        }
    }
}

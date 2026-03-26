using UnityEngine;

public static class MapBounds
{
    public static float HalfWidth { get; private set; }
    public static float HalfHeight { get; private set; }

    private const float PlayerInset = 0.5f;
    private const float SpawnMargin = 1.5f;
    private const float InsideMargin = 1f;

    public static void Initialize()
    {
        var cam = Camera.main;
        if (cam == null) return;
        HalfHeight = cam.orthographicSize;
        HalfWidth = cam.orthographicSize * cam.aspect;
    }

    public static Vector2 PlayerMin =>
        new Vector2(-HalfWidth + PlayerInset, -HalfHeight + PlayerInset);
    public static Vector2 PlayerMax =>
        new Vector2(HalfWidth - PlayerInset, HalfHeight - PlayerInset);

    public static bool IsInside(Vector2 pos)
    {
        return pos.x >= -HalfWidth - InsideMargin && pos.x <= HalfWidth + InsideMargin &&
               pos.y >= -HalfHeight - InsideMargin && pos.y <= HalfHeight + InsideMargin;
    }

    public static Vector2 ClampPlayer(Vector2 pos)
    {
        pos.x = Mathf.Clamp(pos.x, PlayerMin.x, PlayerMax.x);
        pos.y = Mathf.Clamp(pos.y, PlayerMin.y, PlayerMax.y);
        return pos;
    }

    public static Vector3 ClampPlayer(Vector3 pos)
    {
        pos.x = Mathf.Clamp(pos.x, PlayerMin.x, PlayerMax.x);
        pos.y = Mathf.Clamp(pos.y, PlayerMin.y, PlayerMax.y);
        return pos;
    }

    public static Vector2 RandomSpawnEdge()
    {
        float sw = HalfWidth + SpawnMargin;
        float sh = HalfHeight + SpawnMargin;
        int edge = Random.Range(0, 4);
        switch (edge)
        {
            case 0: return new Vector2(Random.Range(-sw, sw), sh);
            case 1: return new Vector2(Random.Range(-sw, sw), -sh);
            case 2: return new Vector2(-sw, Random.Range(-sh, sh));
            default: return new Vector2(sw, Random.Range(-sh, sh));
        }
    }

    public static Vector2 RandomInsideView()
    {
        return new Vector2(
            Random.Range(-HalfWidth + 1f, HalfWidth - 1f),
            Random.Range(-HalfHeight + 1f, HalfHeight - 1f));
    }
}

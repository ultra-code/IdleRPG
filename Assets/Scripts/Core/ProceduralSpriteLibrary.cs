using System.Collections.Generic;
using UnityEngine;

public static class ProceduralSpriteLibrary
{
    private const float PixelsPerUnit = 16f;
    private static readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    public static void ApplyPlayerVisual(SpriteRenderer renderer)
    {
        if (renderer == null || renderer.sprite != null) return;

        renderer.sprite = GetPlayerSprite();
        renderer.color = Color.white;
        renderer.sortingOrder = 10;
        renderer.transform.localScale = Vector3.one * 1.1f;
    }

    public static void ApplyEnemyVisual(SpriteRenderer renderer, string enemyName, bool isBoss)
    {
        if (renderer == null) return;

        if (renderer.sprite == null)
            renderer.sprite = GetEnemySprite(enemyName);

        renderer.color = isBoss ? new Color(1f, 0.8f, 0.8f, 1f) : Color.white;
        renderer.sortingOrder = 8;
        renderer.transform.localScale = isBoss ? Vector3.one * 1.5f : Vector3.one;
    }

    public static Sprite GetEffectCircleSprite()
    {
        return GetOrCreate("effect_circle", new[]
        {
            "................",
            ".....######.....",
            "...##########...",
            "..############..",
            "..############..",
            ".##############.",
            ".##############.",
            ".##############.",
            ".##############.",
            ".##############.",
            ".##############.",
            "..############..",
            "..############..",
            "...##########...",
            ".....######.....",
            "................"
        }, new Dictionary<char, Color32>
        {
            ['#'] = new Color32(255, 255, 255, 255)
        });
    }

    private static Sprite GetPlayerSprite()
    {
        return GetOrCreate("player_mage", new[]
        {
            "................",
            "......####......",
            ".....######.....",
            "....##yyyy##....",
            "....##yyyy##....",
            ".....#ssss#.....",
            ".....bbbbbb.....",
            "....bbbbbbbb....",
            "...bbbbbbbbbb...",
            "...bbwwbbwwbb...",
            "...bbwwbbwwbb...",
            "....bb....bb....",
            "...bb......bb...",
            "..bb........bb..",
            "..gg........gg..",
            "................"
        }, new Dictionary<char, Color32>
        {
            ['#'] = new Color32(92, 55, 31, 255),
            ['y'] = new Color32(243, 207, 92, 255),
            ['s'] = new Color32(241, 212, 179, 255),
            ['b'] = new Color32(58, 102, 201, 255),
            ['w'] = new Color32(215, 229, 255, 255),
            ['g'] = new Color32(78, 78, 90, 255)
        });
    }

    private static Sprite GetEnemySprite(string enemyName)
    {
        string key = (enemyName ?? string.Empty).ToLowerInvariant();

        if (key.Contains("슬라임") || key.Contains("slime"))
        {
            return GetOrCreate("enemy_slime", new[]
            {
                "................",
                "................",
                ".....######.....",
                "...##########...",
                "..############..",
                ".##############.",
                ".##############.",
                ".##############.",
                ".##############.",
                "..############..",
                "..###.####.###..",
                "...##......##...",
                "....##....##....",
                ".....######.....",
                "................",
                "................"
            }, new Dictionary<char, Color32>
            {
                ['#'] = new Color32(94, 220, 88, 255),
                ['.'] = new Color32(0, 0, 0, 0)
            });
        }

        if (key.Contains("고블린") || key.Contains("goblin"))
        {
            return GetOrCreate("enemy_goblin", new[]
            {
                "................",
                "....g......g....",
                "...ggg....ggg...",
                "....########....",
                "...##yyyyyy##...",
                "...##yyyyyy##...",
                "...##..ss..##...",
                "...####mm####...",
                "....#mmmmmm#....",
                "...##mmmmmm##...",
                "...##..mm..##...",
                "...##..mm..##...",
                "....##....##....",
                "...bb......bb...",
                "...bb......bb...",
                "................"
            }, new Dictionary<char, Color32>
            {
                ['#'] = new Color32(41, 82, 35, 255),
                ['g'] = new Color32(82, 166, 67, 255),
                ['y'] = new Color32(125, 184, 88, 255),
                ['s'] = new Color32(236, 208, 171, 255),
                ['m'] = new Color32(96, 54, 36, 255),
                ['b'] = new Color32(71, 48, 29, 255)
            });
        }

        if (key.Contains("골렘") || key.Contains("golem"))
        {
            return GetOrCreate("enemy_golem", new[]
            {
                "................",
                "....########....",
                "...##########...",
                "..############..",
                "..##oo####oo##..",
                "..############..",
                "..#####cc#####..",
                ".##############.",
                ".##############.",
                ".#####.##.#####.",
                "..####.##.####..",
                "..####.##.####..",
                "..###......###..",
                "...##......##...",
                "..rr........rr..",
                "................"
            }, new Dictionary<char, Color32>
            {
                ['#'] = new Color32(116, 120, 129, 255),
                ['o'] = new Color32(255, 148, 66, 255),
                ['c'] = new Color32(82, 96, 110, 255),
                ['r'] = new Color32(86, 89, 96, 255)
            });
        }

        if (key.Contains("박쥐") || key.Contains("bat"))
        {
            return GetOrCreate("enemy_bat", new[]
            {
                "................",
                ".ppp........ppp.",
                "ppppp......ppppp",
                "pppppp....pppppp",
                ".pppppp####pppp.",
                "..ppp########pp.",
                "...##########...",
                "..####wwww####..",
                "..####wwww####..",
                "...##########...",
                "..##..####..##..",
                ".##....##....##.",
                "p......##......p",
                ".......##.......",
                "................",
                "................"
            }, new Dictionary<char, Color32>
            {
                ['#'] = new Color32(71, 47, 114, 255),
                ['p'] = new Color32(104, 70, 161, 255),
                ['w'] = new Color32(229, 197, 234, 255)
            });
        }

        if (key.Contains("해골") || key.Contains("skeleton"))
        {
            return GetOrCreate("enemy_skeleton", new[]
            {
                "................",
                ".....######.....",
                "....########....",
                "...###wwww###...",
                "...###wwww###...",
                "...####..####...",
                "...##########...",
                "....##bbbb##....",
                "....##bbbb##....",
                "...###bbbb###...",
                "...##..bb..##...",
                "...##..bb..##...",
                "...##..bb..##...",
                "..rr........rr..",
                "..rr........rr..",
                "................"
            }, new Dictionary<char, Color32>
            {
                ['#'] = new Color32(228, 227, 213, 255),
                ['w'] = new Color32(56, 56, 62, 255),
                ['b'] = new Color32(121, 90, 66, 255),
                ['r'] = new Color32(97, 71, 53, 255)
            });
        }

        return GetEffectCircleSprite();
    }

    private static Sprite GetOrCreate(string key, string[] pattern, Dictionary<char, Color32> palette)
    {
        if (spriteCache.TryGetValue(key, out Sprite cached) && cached != null)
            return cached;

        int width = pattern[0].Length;
        int height = pattern.Length;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
            name = key
        };

        Color32 clear = new Color32(0, 0, 0, 0);
        Color32[] pixels = new Color32[width * height];

        for (int y = 0; y < height; y++)
        {
            string row = pattern[height - 1 - y];
            for (int x = 0; x < width; x++)
            {
                char code = row[x];
                pixels[y * width + x] = palette.TryGetValue(code, out Color32 color) ? color : clear;
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.15f), PixelsPerUnit);
        sprite.name = key;
        spriteCache[key] = sprite;
        return sprite;
    }
}

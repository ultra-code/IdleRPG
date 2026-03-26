using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public static class GenerateSkillSprites
{
    private const string SpriteDir = "Assets/Sprites/Effects";

    [MenuItem("Tools/Generate Skill Sprites")]
    public static void Execute()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Sprites"))
            AssetDatabase.CreateFolder("Assets", "Sprites");
        if (!AssetDatabase.IsValidFolder(SpriteDir))
            AssetDatabase.CreateFolder("Assets/Sprites", "Effects");

        Save("staff_projectile", 32, 32, DrawStaffProjectile(32, 32));
        Save("meteor_body",      32, 32, DrawMeteorBody(32, 32));
        Save("explosion_flash",  48, 48, DrawExplosionFlash(48, 48));
        Save("lightning_ball",   32, 32, DrawLightningBall(32, 32));
        Save("spark_flash",      24, 24, DrawSparkFlash(24, 24));

        AssetDatabase.Refresh();

        Import("staff_projectile", 32);
        Import("meteor_body",      32);
        Import("explosion_flash",  32);
        Import("lightning_ball",   32);
        Import("spark_flash",      32);

        AssetDatabase.Refresh();
        LinkAll();

        EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("[GenSprites] Complete: 5 sprites generated and linked.");
    }

    // ================================================================
    //  Save / Import / Link helpers
    // ================================================================

    static void Save(string name, int w, int h, Color32[] pixels)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode   = TextureWrapMode.Clamp
        };
        tex.SetPixels32(pixels);
        tex.Apply();

        byte[] png = tex.EncodeToPNG();
        Object.DestroyImmediate(tex);

        string fullPath = Path.Combine(Application.dataPath, "Sprites", "Effects", name + ".png");
        File.WriteAllBytes(fullPath, png);
        Debug.Log("[GenSprites] Saved " + name + ".png (" + w + "x" + h + ")");
    }

    static void Import(string name, int ppu)
    {
        string path = SpriteDir + "/" + name + ".png";
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp == null) { Debug.LogError("[GenSprites] Importer missing: " + path); return; }

        imp.textureType        = TextureImporterType.Sprite;
        imp.spritePixelsPerUnit = ppu;
        imp.filterMode         = FilterMode.Point;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.mipmapEnabled      = false;
        imp.SaveAndReimport();
    }

    static void LinkAll()
    {
        int ok = 0, skip = 0, fail = 0;

        var fan = Object.FindAnyObjectByType<FanSkill>();
        if (fan != null)
            ok += Link(fan, "staffSprite", "staff_projectile", ref skip, ref fail);
        else { Debug.LogWarning("[GenSprites] FanSkill not in scene"); fail++; }

        var meteor = Object.FindAnyObjectByType<MeteorSkill>();
        if (meteor != null)
        {
            ok += Link(meteor, "meteorSprite",  "meteor_body",      ref skip, ref fail);
            ok += Link(meteor, "explosionSprite", "explosion_flash", ref skip, ref fail);
        }
        else { Debug.LogWarning("[GenSprites] MeteorSkill not in scene"); fail += 2; }

        var chain = Object.FindAnyObjectByType<ChainSkill>();
        if (chain != null)
        {
            ok += Link(chain, "ballSprite",  "lightning_ball", ref skip, ref fail);
            ok += Link(chain, "sparkSprite", "spark_flash",    ref skip, ref fail);
        }
        else { Debug.LogWarning("[GenSprites] ChainSkill not in scene"); fail += 2; }

        Debug.Log($"[GenSprites] Link results — OK: {ok}, Already: {skip}, Fail: {fail}");
    }

    static int Link(Component target, string propName, string spriteName,
                     ref int skip, ref int fail)
    {
        var so   = new SerializedObject(target);
        var prop = so.FindProperty(propName);
        if (prop == null)
        {
            Debug.LogError($"[GenSprites] Property '{propName}' not found on {target.GetType().Name}");
            fail++; return 0;
        }

        string path = SpriteDir + "/" + spriteName + ".png";
        var sprite  = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
        {
            Debug.LogError($"[GenSprites] Sprite not found: {path}");
            fail++; return 0;
        }

        if (prop.objectReferenceValue == sprite) { skip++; return 0; }

        prop.objectReferenceValue = sprite;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
        Debug.Log($"[GenSprites] Linked {target.GetType().Name}.{propName} <- {spriteName}");
        return 1;
    }

    // ================================================================
    //  Pixel helpers
    // ================================================================

    static float Hash(int x, int y, int seed)
    {
        int n = x * 374761393 + y * 668265263 + seed * 1274126177;
        n = (n ^ (n >> 13)) * 1274126177;
        n = n ^ (n >> 16);
        return (n & 0x7fffffff) / (float)0x7fffffff;
    }

    static Color32 Lerp32(Color32 a, Color32 b, float t)
    {
        t = Mathf.Clamp01(t);
        return new Color32(
            (byte)(a.r + (b.r - a.r) * t),
            (byte)(a.g + (b.g - a.g) * t),
            (byte)(a.b + (b.b - a.b) * t),
            (byte)(a.a + (b.a - a.a) * t));
    }

    // ================================================================
    //  1. Staff Projectile  (32x32)
    //     Diagonal lance: gold core, pink glow edge, pointed tip
    // ================================================================

    static Color32[] DrawStaffProjectile(int w, int h)
    {
        var px = new Color32[w * h];
        float cx = (w - 1) * 0.5f, cy = (h - 1) * 0.5f;

        Color32 coreWhite = new Color32(255, 255, 230, 255);
        Color32 coreGold  = new Color32(255, 210, 90,  255);
        Color32 bodyGold  = new Color32(212, 164, 76,  255);
        Color32 edgePink  = new Color32(232, 69,  123, 255);

        float cos45 = 0.7071f, sin45 = 0.7071f;

        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            float dx = x - cx, dy = y - cy;
            float ax =  dx * cos45 + dy * sin45;   // along staff axis
            float ay = -dx * sin45 + dy * cos45;   // perpendicular

            float halfLen = 13.5f;
            if (ax < -halfLen || ax > halfLen) continue;

            // t: 0 = tail, 1 = tip (upper-right)
            float t = (ax + halfLen) / (2f * halfLen);

            // Body width tapers: wide at tail, sharp at tip
            float bodyW = 2.8f * Mathf.Pow(1f - t, 0.5f);
            // Taper tail end too
            if (t < 0.08f) bodyW *= t / 0.08f;

            float absAy = Mathf.Abs(ay);
            float glowW = bodyW + 1.2f;
            if (absAy > glowW) continue;

            Color32 col;
            if (absAy <= bodyW * 0.28f)
            {
                // Bright core — bloom trigger
                col = Lerp32(coreGold, coreWhite, t * 0.5f + 0.3f);
            }
            else if (absAy <= bodyW * 0.6f)
            {
                col = Lerp32(coreGold, bodyGold, (absAy / bodyW - 0.28f) / 0.32f);
            }
            else if (absAy <= bodyW)
            {
                col = edgePink;
            }
            else
            {
                float fade = 1f - (absAy - bodyW) / (glowW - bodyW);
                col = new Color32(edgePink.r, edgePink.g, edgePink.b, (byte)(fade * 80));
            }

            // Bright gem at very tip
            if (t > 0.88f && absAy < 1.2f)
                col = coreWhite;

            px[y * w + x] = col;
        }
        return px;
    }

    // ================================================================
    //  2. Meteor Body  (32x32)
    //     Fireball: bright orange center, irregular flame edges
    // ================================================================

    static Color32[] DrawMeteorBody(int w, int h)
    {
        var px = new Color32[w * h];
        float cx = (w - 1) * 0.5f, cy = (h - 1) * 0.5f;

        Color32 white  = new Color32(255, 255, 210, 255);
        Color32 bright = new Color32(255, 200, 60,  255);
        Color32 orange = new Color32(255, 140, 0,   255);
        Color32 red    = new Color32(255, 34,  0,   255);
        Color32 dark   = new Color32(160, 20,  5,   255);

        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            float dx = x - cx, dy = y - cy;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);

            // Angular sector noise for flame shape
            float ang = Mathf.Atan2(dy, dx);
            int sector = ((int)((ang + Mathf.PI) / (Mathf.PI * 2f) * 14f)) % 14;
            float sNoise = (Hash(sector, 0, 55) - 0.5f) * 3.5f;
            float pNoise = (Hash(x, y, 42) - 0.5f) * 1.8f;

            float r0 = 3.5f;                        // white core
            float r1 = 6.5f;                        // bright
            float r2 = 9f;                           // orange
            float r3 = 11.5f + sNoise;              // red, wavy
            float r4 = 13.5f + sNoise + pNoise;     // flame tips

            Color32 col;
            if (dist < r0)
                col = Lerp32(white, bright, dist / r0);
            else if (dist < r1)
                col = Lerp32(bright, orange, (dist - r0) / (r1 - r0));
            else if (dist < r2)
                col = Lerp32(orange, red, (dist - r1) / (r2 - r1));
            else if (dist < r3)
                col = Lerp32(red, dark, (dist - r2) / Mathf.Max(r3 - r2, 0.5f));
            else if (dist < r4)
            {
                float fade = (dist - r3) / Mathf.Max(r4 - r3, 0.5f);
                byte a = (byte)((1f - fade) * 140f);
                col = new Color32(dark.r, dark.g, dark.b, a);
            }
            else continue;

            px[y * w + x] = col;
        }
        return px;
    }

    // ================================================================
    //  3. Explosion Flash  (48x48)
    //     Star burst: white→orange→red, 8 directional rays
    // ================================================================

    static Color32[] DrawExplosionFlash(int w, int h)
    {
        var px = new Color32[w * h];
        float cx = (w - 1) * 0.5f, cy = (h - 1) * 0.5f;

        Color32 white  = new Color32(255, 255, 255, 255);
        Color32 yellow = new Color32(255, 230, 120, 255);
        Color32 orange = new Color32(255, 150, 50,  255);
        Color32 red    = new Color32(220, 50,  20,  255);

        const int rays = 8;
        float maxR = 22f;
        float coreR = 6.5f;

        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            float dx = x - cx, dy = y - cy;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);
            if (dist > maxR) continue;

            float angDeg = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;

            // Nearest ray angular distance
            float minAng = 180f;
            for (int r = 0; r < rays; r++)
            {
                float rayDeg = r * (360f / rays);
                float diff = Mathf.Abs(Mathf.DeltaAngle(angDeg, rayDeg));
                if (diff < minAng) minAng = diff;
            }

            // Ray half-angle widens near center, narrows outward
            float distRatio = dist / maxR;
            float rayHalf = Mathf.Lerp(55f, 4f, Mathf.Pow(distRatio, 0.45f));

            bool inCore = dist < coreR;
            bool inRay  = minAng < rayHalf;
            // Inter-ray glow near center
            bool inGlow = dist < coreR * 1.8f;

            if (!inCore && !inRay && !inGlow) continue;

            Color32 col;
            if (inCore)
            {
                col = Lerp32(white, yellow, dist / coreR);
            }
            else if (inRay)
            {
                float angFade = minAng / rayHalf;
                if (distRatio < 0.3f)
                    col = Lerp32(yellow, orange, distRatio / 0.3f);
                else if (distRatio < 0.6f)
                    col = Lerp32(orange, red, (distRatio - 0.3f) / 0.3f);
                else
                {
                    float tipFade = (distRatio - 0.6f) / 0.4f;
                    byte a = (byte)((1f - tipFade) * 255f);
                    col = new Color32(red.r, red.g, red.b, a);
                }
                // Soften ray edges
                if (angFade > 0.5f)
                {
                    float ef = (angFade - 0.5f) / 0.5f;
                    col = new Color32(col.r, col.g, col.b, (byte)(col.a * (1f - ef * 0.7f)));
                }
            }
            else // inGlow only
            {
                float fade = (dist - coreR) / (coreR * 0.8f);
                byte a = (byte)((1f - fade) * 120f);
                col = new Color32(orange.r, orange.g, orange.b, a);
            }

            if (col.a > 0)
                px[y * w + x] = col;
        }
        return px;
    }

    // ================================================================
    //  4. Lightning Ball  (32x32)
    //     Electric orb: cyan center, blue body, jagged crackle edges
    // ================================================================

    static Color32[] DrawLightningBall(int w, int h)
    {
        var px = new Color32[w * h];
        float cx = (w - 1) * 0.5f, cy = (h - 1) * 0.5f;

        Color32 white = new Color32(255, 255, 255, 255);
        Color32 cyan  = new Color32(136, 221, 255, 255);
        Color32 blue  = new Color32(48,  160, 255, 255);
        Color32 deep  = new Color32(30,  100, 200, 255);
        Color32 spark = new Color32(200, 240, 255, 200);

        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            float dx = x - cx, dy = y - cy;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);

            float ang = Mathf.Atan2(dy, dx);
            int sector = ((int)((ang + Mathf.PI) / (Mathf.PI * 2f) * 18f)) % 18;
            float eNoise = (Hash(sector, 1, 33) - 0.5f) * 4f;
            float pxNoise = (Hash(x, y, 88) - 0.5f) * 1.6f;

            float r0 = 3.5f;
            float r1 = 6.5f;
            float r2 = 9.5f;
            float r3 = 11.5f + eNoise;
            float r4 = 13.5f + eNoise * 1.2f + pxNoise;

            Color32 col;
            if (dist < r0)
                col = Lerp32(white, cyan, dist / r0);
            else if (dist < r1)
                col = Lerp32(cyan, blue, (dist - r0) / (r1 - r0));
            else if (dist < r2)
                col = Lerp32(blue, deep, (dist - r1) / (r2 - r1));
            else if (dist < r3)
            {
                float fade = (dist - r2) / Mathf.Max(r3 - r2, 0.5f);
                col = Lerp32(deep, new Color32(deep.r, deep.g, deep.b, 160), fade);
            }
            else if (dist < r4 && Hash(x, y, 22) > 0.5f)
            {
                float fade = (dist - r3) / Mathf.Max(r4 - r3, 0.5f);
                byte a = (byte)((1f - fade) * 170f);
                col = new Color32(spark.r, spark.g, spark.b, a);
            }
            else continue;

            // Random inner lightning streaks
            if (dist > r0 && dist < r2 && Hash(x, y, 111) > 0.88f)
                col = Lerp32(col, white, 0.7f);

            px[y * w + x] = col;
        }
        return px;
    }

    // ================================================================
    //  5. Spark Flash  (24x24)
    //     Cross (+) shape: white center, 4 blue arms tapering out
    // ================================================================

    static Color32[] DrawSparkFlash(int w, int h)
    {
        var px = new Color32[w * h];
        float cx = (w - 1) * 0.5f, cy = (h - 1) * 0.5f;

        Color32 white = new Color32(255, 255, 255, 255);
        Color32 bCyan = new Color32(180, 230, 255, 255);
        Color32 blue  = new Color32(48,  160, 255, 255);

        float coreR   = 2.8f;
        float armLen  = 10.5f;
        float armHalf = 1.6f;    // arm half-width at center

        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            float dx = x - cx, dy = y - cy;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);
            float absDx = Mathf.Abs(dx), absDy = Mathf.Abs(dy);

            bool inCore = dist < coreR;

            // Horizontal arm: |dy| < tapering width, |dx| < length
            float hTaper = armHalf * (1f - absDx / armLen * 0.7f);
            bool inH = absDy < hTaper && absDx < armLen;

            // Vertical arm: |dx| < tapering width, |dy| < length
            float vTaper = armHalf * (1f - absDy / armLen * 0.7f);
            bool inV = absDx < vTaper && absDy < armLen;

            if (!inCore && !inH && !inV) continue;

            Color32 col;
            if (inCore)
            {
                col = Lerp32(white, bCyan, dist / coreR);
            }
            else
            {
                float armDist = Mathf.Max(absDx, absDy);
                float armRatio = armDist / armLen;
                col = Lerp32(bCyan, blue, armRatio);

                // Fade alpha at tips
                if (armRatio > 0.65f)
                {
                    float fade = (armRatio - 0.65f) / 0.35f;
                    col = new Color32(col.r, col.g, col.b, (byte)((1f - fade) * 255));
                }
            }

            px[y * w + x] = col;
        }
        return px;
    }
}

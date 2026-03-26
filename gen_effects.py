"""Generate 7 skill effect sprite sheets using PixFlux API."""
import os, sys, time
from pathlib import Path
from PIL import Image
from pixellab import Client

OUT_DIR = Path("C:/UnityPhase1/IdleRPG/Assets/Sprites/Effects")
OUT_DIR.mkdir(parents=True, exist_ok=True)

client = Client(secret=os.environ["PIXELLAB_SECRET"])

COMMON_NEG = "realistic, 3D, blurry, character, monster, background, text, signature, white background"

def gen(description, w, h, outline, shading, detail, neg=COMMON_NEG, seed=0, retries=3):
    """Generate a single frame with retries."""
    for attempt in range(retries):
        try:
            resp = client.generate_image_pixflux(
                description=description,
                image_size={"width": w, "height": h},
                negative_description=neg,
                no_background=True,
                outline=outline,
                shading=shading,
                detail=detail,
                seed=seed,
            )
            return resp.image.pil_image()
        except Exception as e:
            print(f"  [attempt {attempt+1}/{retries}] Error: {e}")
            if attempt < retries - 1:
                time.sleep(3)
            else:
                raise

def make_sheet(frames, out_path):
    """Combine frames into a horizontal sprite sheet."""
    w = sum(f.width for f in frames)
    h = max(f.height for f in frames)
    sheet = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    x = 0
    for f in frames:
        sheet.paste(f, (x, 0))
        x += f.width
    sheet.save(out_path)
    print(f"  -> Saved {out_path.name} ({sheet.width}x{sheet.height})")

def write_meta(png_path, ppu=128):
    """Write a Unity .meta file for a sprite with Point filter, no compression."""
    guid = hex(abs(hash(str(png_path))))[2:].ljust(32, '0')[:32]
    meta = f"""fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: -1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 0
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 0
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 0
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: {ppu}
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  swizzle: 50462976
  cookieLightType: 1
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 0
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    ignorePlatformSupport: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    customData:
    physicsShape: []
    bones: []
    spriteID: 00000000000000000000000000000000
    internalID: 0
    vertices: []
    indices:
    edges: []
    weights: []
    secondaryTextures: []
    spriteCustomMetadata:
      entries: []
    nameFileIdTable: {{}}
  mipmapLimitGroupName:
  pSDRemoveMatte: 0
  userData:
  assetBundleName:
  assetBundleVariant:
"""
    meta_path = Path(str(png_path) + ".meta")
    meta_path.write_text(meta, encoding="utf-8")


# ============================================================
# SKILL 1: Fan Shape (4 frames, 128x128)
# ============================================================
print("=== Skill 1: Fan Shape (4 frames 128x128) ===")
fan_descs = [
    "fan shaped shockwave, 60 degree arc, initial burst, gold center (#D4A44C), compact crescent shape",
    "fan shaped shockwave, 60 degree arc expanding, bright gold (#FFE080) outer edge, energy spreading outward",
    "fan shaped shockwave, 60 degree arc wide, golden yellow energy wave fully expanded, radiant",
    "fan shaped shockwave, 60 degree arc fading, gold energy dissolving, dissipating particles",
]
fan_frames = []
for i, desc in enumerate(fan_descs):
    print(f"  Frame {i+1}/4...")
    fan_frames.append(gen(desc, 128, 128, "single color black outline", "flat shading", "medium detail", seed=100+i))
out = OUT_DIR / "skill_fan_sheet.png"
make_sheet(fan_frames, out)
write_meta(out)

# ============================================================
# SKILL 2: Circle (6 frames, 128x128)
# ============================================================
print("=== Skill 2: Circle (6 frames 128x128) ===")
circle_descs = [
    "circular impact point, small orange center dot (#FF6030), initial strike moment",
    "circular shockwave ring expanding, orange red (#FF6030), ground crack starting from center",
    "circular explosion ring, orange red energy, radial cracks on floor, white hot center core",
    "circular blast wave, large ring expanding, orange (#FF4500) energy ring, debris flying outward",
    "circular explosion wide, full expansion, orange red shockwave ring, dust and debris",
    "circular explosion fading, orange energy ring dissipating, debris settling",
]
circle_frames = []
for i, desc in enumerate(circle_descs):
    print(f"  Frame {i+1}/6...")
    circle_frames.append(gen(desc, 128, 128, "single color black outline", "medium shading", "highly detailed", seed=200+i))
out = OUT_DIR / "skill_circle_sheet.png"
make_sheet(circle_frames, out)
write_meta(out)

# ============================================================
# SKILL 3: Chain Bolt (4 frames, 64x64 -> crop to 64x16)
# ============================================================
print("=== Skill 3: Chain Bolt (4 frames 64x64 -> 64x16 crop) ===")
bolt_descs = [
    "horizontal lightning bolt, jagged electric line, bright cyan (#30A0FF), white electric core, thin horizontal discharge",
    "horizontal lightning bolt flickering, cyan blue (#80E0FF), slightly different jagged path",
    "horizontal lightning bolt bright flash, pure white core, cyan glow, maximum brightness",
    "horizontal lightning bolt dim, fading cyan electric line, dissipating",
]
bolt_frames = []
for i, desc in enumerate(bolt_descs):
    print(f"  Frame {i+1}/4...")
    img = gen(desc, 64, 64, "single color black outline", "flat shading", "low detail", seed=300+i)
    # Crop top 16px strip
    cropped = img.crop((0, 0, 64, 16))
    bolt_frames.append(cropped)
out = OUT_DIR / "skill_chain_bolt_sheet.png"
make_sheet(bolt_frames, out)
write_meta(out)

# ============================================================
# SKILL 4: Chain Spark (3 frames, 32x32)
# ============================================================
print("=== Skill 4: Chain Spark (3 frames 32x32) ===")
spark_descs = [
    "small electric spark burst, cyan blue (#30A0FF), 4-pointed star shape, impact flash",
    "electric spark exploding, bright cyan white (#80E0FF), rays shooting outward, maximum flash",
    "electric spark fading, small cyan particles dispersing, dissipating glow",
]
spark_frames = []
for i, desc in enumerate(spark_descs):
    print(f"  Frame {i+1}/3...")
    spark_frames.append(gen(desc, 32, 32, "single color black outline", "flat shading", "low detail", seed=400+i))
out = OUT_DIR / "skill_chain_spark_sheet.png"
make_sheet(spark_frames, out)
write_meta(out)

# ============================================================
# SKILL 5: Buff Burst (4 frames, 64x64)
# ============================================================
print("=== Skill 5: Buff Burst (4 frames 64x64) ===")
burst_descs = [
    "power aura burst initial, small red energy ring (#FF3030), tight circle around center",
    "power aura burst expanding, crimson red energy ring expanding outward, dark purple accents (#E8457B)",
    "power aura burst full, large red energy ring (#FF3030), maximum expansion, radiant energy",
    "power aura burst fading, red energy ring dissipating, purple particles scattering",
]
burst_frames = []
for i, desc in enumerate(burst_descs):
    print(f"  Frame {i+1}/4...")
    burst_frames.append(gen(desc, 64, 64, "single color black outline", "medium shading", "medium detail", seed=500+i))
out = OUT_DIR / "skill_buff_burst_sheet.png"
make_sheet(burst_frames, out)
write_meta(out)

# ============================================================
# SKILL 6: Buff Aura Loop (4 frames, 64x64)
# ============================================================
print("=== Skill 6: Buff Aura Loop (4 frames 64x64) ===")
aura_descs = [
    "pulsing aura ring, red energy halo (#FF3030), medium size, orbiting energy particles",
    "pulsing aura ring expanding slightly, crimson red (#FF3030) purple tint (#E8457B), energy waves",
    "pulsing aura ring maximum, bright red energy halo, glowing particles orbiting",
    "pulsing aura ring contracting, red energy returning to medium size, same as frame1 feel",
]
aura_frames = []
for i, desc in enumerate(aura_descs):
    print(f"  Frame {i+1}/4...")
    aura_frames.append(gen(desc, 64, 64, "single color black outline", "medium shading", "medium detail", seed=600+i))
out = OUT_DIR / "skill_buff_aura_sheet.png"
make_sheet(aura_frames, out)
write_meta(out)

# ============================================================
# SKILL 7: Dash Afterimage (1 frame, 64x64)
# ============================================================
print("=== Skill 7: Dash Afterimage (1 frame 64x64) ===")
dash_neg = "filled body, solid color, red, purple, orange, background"
img = gen(
    "chibi character silhouette ghost, blue cyan outline only (#4080FF), hollow inside transparent, afterimage trail ghost, facing right side view, standing pose",
    64, 64, "single color black outline", "flat shading", "low detail",
    neg=dash_neg, seed=700
)
out = OUT_DIR / "dash_afterimage.png"
img.save(out)
print(f"  -> Saved {out.name} ({img.width}x{img.height})")
write_meta(out)

# ============================================================
# Summary
# ============================================================
print("\n=== Summary ===")
for f in sorted(OUT_DIR.glob("*.png")):
    size = f.stat().st_size
    img = Image.open(f)
    print(f"  {f.name}: {img.width}x{img.height}, {size:,} bytes")
    meta = Path(str(f) + ".meta")
    print(f"    .meta: {'exists' if meta.exists() else 'MISSING'}")

print("\nDone!")

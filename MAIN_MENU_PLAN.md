# 메인 화면 (타이틀 스크린) 구현 작업 지시서

프로젝트: C:/UnityPhase1/IdleRPG
먼저 HANDOFF.md를 읽어서 현재 상태를 파악해줘.

## 배경

ChatGPT로 생성한 게임 시작 화면 이미지가 준비되었다.
이 이미지를 통째로 배경으로 사용하고, "GAME START" 버튼 위치에 투명 클릭 영역만 올려서 메인 화면을 구현한다.

이미지 파일: Assets/Sprites/UI/main_menu_bg.png (사용자가 직접 배치할 예정)
해상도: 16:9 (약 1456×816 또는 유사)

---

## 사전 작업 (사용자가 수동으로 해야 할 것)

1. 첨부된 메인 화면 이미지를 `Assets/Sprites/UI/main_menu_bg.png`로 저장
2. Unity에서 Import Settings:
   - Texture Type: Sprite (2D and UI)
   - Filter Mode: Bilinear
   - Compression: None
   - Max Size: 2048 이상

---

## 작업 1: MainMenuScene 생성

### 1-1. 새 씬 생성
unity-cli로 새 씬 `Assets/Scenes/MainMenuScene.unity`를 생성해줘.

### 1-2. 씬 구조 (코드로 전부 생성)

```
MainMenuScene
├── Main Camera (Background Color: #0D0618, Orthographic)
├── Canvas (Screen Space - Overlay, Canvas Scaler: Scale With Screen Size, Reference 1920×1080)
│   ├── BackgroundImage (RawImage 또는 Image, 전체 화면, main_menu_bg.png)
│   ├── GameStartButton (Button, 투명 배경, GAME START 이미지 위치에 정확히 맞춤)
│   └── VersionText (TextMeshProUGUI, 하단 중앙, 매우 작게)
└── EventSystem
```

---

## 작업 2: MainMenuUI.cs 스크립트 생성

새 스크립트 `Assets/Scripts/UI/MainMenuUI.cs` 생성.

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button gameStartButton;

    private const string GAME_SCENE = "SampleScene";

    private void Start()
    {
        if (gameStartButton != null)
            gameStartButton.onClick.AddListener(OnGameStart);
    }

    private void OnGameStart()
    {
        SceneManager.LoadScene(GAME_SCENE);
    }
}
```

핵심 포인트:
- 이미지에 이미 "GAME START" 버튼이 그려져 있으므로, Unity Button은 투명하게 만들고 위치만 맞춤
- 버튼 클릭 시 SampleScene으로 전환
- 복잡한 로직 없이 단순하게

---

## 작업 3: 씬 구성 (unity-cli로 실행)

### 3-1. 카메라 설정
```
Main Camera의 Background Color를 #0D0618 (어두운 보라)로 설정
```

### 3-2. Canvas 설정
```csharp
// Canvas 생성
var canvasObj = new GameObject("Canvas");
var canvas = canvasObj.AddComponent<Canvas>();
canvas.renderMode = RenderMode.ScreenSpaceOverlay;
canvas.sortingOrder = 0;

var scaler = canvasObj.AddComponent<CanvasScaler>();
scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
scaler.referenceResolution = new Vector2(1920, 1080);
scaler.matchWidthOrHeight = 0.5f; // 가로세로 균형

canvasObj.AddComponent<GraphicRaycaster>();
```

### 3-3. 배경 이미지
```csharp
// 전체 화면을 채우는 배경 이미지
var bgObj = new GameObject("BackgroundImage");
bgObj.transform.SetParent(canvasObj.transform, false);

var bgImage = bgObj.AddComponent<Image>();
// main_menu_bg.png 스프라이트를 할당
// bgImage.sprite = 로드한 스프라이트

var bgRect = bgObj.GetComponent<RectTransform>();
bgRect.anchorMin = Vector2.zero;
bgRect.anchorMax = Vector2.one;
bgRect.offsetMin = Vector2.zero;
bgRect.offsetMax = Vector2.zero;
// 전체 화면을 꽉 채움

bgImage.preserveAspect = false; // 화면에 꽉 채우기
```

### 3-4. GAME START 버튼 (투명, 이미지 위 위치에 맞춤)

이미지에서 "GAME START" 버튼의 위치:
- 좌측 하단 영역 (대략 좌측 30%, 하단 30% 부근)
- 이미지 전체 기준으로 약: x=25%, y=30% 위치, 너비 약 20%, 높이 약 8%

```csharp
var btnObj = new GameObject("GameStartButton");
btnObj.transform.SetParent(canvasObj.transform, false);

var btnImage = btnObj.AddComponent<Image>();
btnImage.color = new Color(1, 1, 1, 0); // 완전 투명 — 이미지의 버튼 그래픽이 보임

var btn = btnObj.AddComponent<Button>();
// 버튼 Transition을 None으로 설정 — 호버/클릭 시 색상 변화 없음
btn.transition = Selectable.Transition.None;

var btnRect = btnObj.GetComponent<RectTransform>();
// 앵커를 이미지 내 GAME START 버튼 위치에 맞춤
// 이미지 분석 결과: 버튼은 좌측 약 10~35%, 하단 약 22~32% 영역
btnRect.anchorMin = new Vector2(0.10f, 0.22f);
btnRect.anchorMax = new Vector2(0.35f, 0.32f);
btnRect.offsetMin = Vector2.zero;
btnRect.offsetMax = Vector2.zero;
```

참고: 버튼 위치는 Play 모드에서 실제 이미지와 대조하며 미세 조정이 필요할 수 있다. 처음에는 대략적으로 배치하고, 사용자가 Unity Inspector에서 앵커값을 조정하면 된다.

### 3-5. 하단 크레딧 텍스트 (선택적 — 이미지에 이미 있으므로 생략 가능)

이미지 하단에 "CHOI YU TAEK"이 이미 포함되어 있으므로, 별도 텍스트는 추가하지 않아도 된다.

---

## 작업 4: Build Settings에 씬 등록

MainMenuScene을 Build Settings의 0번(첫 번째)으로 등록해야 게임 시작 시 메인 화면이 먼저 나온다.

```csharp
// Editor 스크립트 또는 unity-cli로:
var scenes = UnityEditor.EditorBuildSettings.scenes;
// MainMenuScene을 index 0으로
// SampleScene을 index 1로
```

unity-cli로 실행:
```
unity-cli exec 'var scenes = new UnityEditor.EditorBuildSettingsScene[] { new UnityEditor.EditorBuildSettingsScene("Assets/Scenes/MainMenuScene.unity", true), new UnityEditor.EditorBuildSettingsScene("Assets/Scenes/SampleScene.unity", true) }; UnityEditor.EditorBuildSettings.scenes = scenes; return "Build settings updated: " + UnityEditor.EditorBuildSettings.scenes.Length + " scenes";'
```

---

## 작업 5: 배경 이미지 Import 설정 (unity-cli)

```
// main_menu_bg.png Import 설정
var importer = UnityEditor.AssetImporter.GetAtPath("Assets/Sprites/UI/main_menu_bg.png") as UnityEditor.TextureImporter;
importer.textureType = UnityEditor.TextureImporterType.Sprite;
importer.spriteImportMode = UnityEditor.SpriteImportMode.Single;
importer.filterMode = FilterMode.Bilinear;
importer.textureCompression = UnityEditor.TextureImporterCompression.Uncompressed;
importer.maxTextureSize = 4096;
importer.mipmapEnabled = false;
UnityEditor.AssetDatabase.ImportAsset("Assets/Sprites/UI/main_menu_bg.png");
```

---

## 작업 6: MainMenuUI 컴포넌트 연결

Canvas 오브젝트(또는 별도 빈 오브젝트)에 MainMenuUI 컴포넌트를 추가하고,
gameStartButton에 GameStartButton 오브젝트의 Button 컴포넌트를 연결해줘.

---

## 완료 후 확인

1. Build Settings에서 MainMenuScene이 0번, SampleScene이 1번인지
2. Play 모드에서 메인 화면 이미지가 전체 화면에 꽉 차는지
3. "GAME START" 영역을 클릭하면 SampleScene으로 전환되는지
4. SampleScene에서 전투가 정상적으로 시작되는지
5. 투명 버튼이 이미지의 "GAME START" 버튼 위에 정확히 위치하는지

File/Save Project 실행해줘.

# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

IdleRPG is a 2D idle RPG built with **Unity 6.0.3** (version 6000.3.6f1) using the **Universal Render Pipeline (URP)** 2D renderer. The project is based on the official Universal 2D Template.

## Key Technical Details

- **Scripting Backend**: Mono (Standalone), IL2CPP (Android)
- **Input System**: New Input System (`Assets/InputSystem_Actions.inputactions`) with Move, Look, Attack, Interact (hold), and Crouch actions
- **Serialization**: Force Text (YAML) — git-friendly
- **Color Space**: Linear
- **Default Resolution**: 1920x1080
- **Target Platforms**: Desktop, Android (SDK 25+)

## Build & Run

Open the project in Unity 6.0.3+ via Unity Hub. The solution file is `IdleRPG.sln`.

**Build from command line** (adjust Unity path as needed):
```bash
Unity -batchmode -projectPath . -buildTarget StandaloneWindows64 -executeMethod BuildScript.Build -quit
```

**Run tests** (Unity Test Framework 1.6.0 is installed):
```bash
Unity -batchmode -projectPath . -runTests -testResults results.xml -quit
# Run specific test category:
Unity -batchmode -projectPath . -runTests -testFilter "CategoryName" -quit
```

Tests can also be run via Unity Editor: Window > General > Test Runner.

## Architecture

The project is currently in template/scaffold state with no custom scripts. When building out the game:

- **Scene**: `Assets/Scenes/SampleScene.unity` is the single scene
- **Packages**: Managed via UPM (`Packages/manifest.json`). Key packages include 2D Animation, 2D Sprite, 2D Tilemap, Input System, and URP
- **Rendering**: Configured in `Assets/Settings/UniversalRP.asset` and `Assets/Settings/Renderer2D.asset`

## Unity Conventions

- Place C# scripts under `Assets/Scripts/` with subdirectories by system/feature
- Use assembly definitions (`.asmdef`) when the project grows to manage compilation
- Prefabs go in `Assets/Prefabs/`, art assets in `Assets/Art/`
- ScriptableObjects are preferred for game data/configuration
- The project uses the new Input System — do not use `UnityEngine.Input` (legacy)

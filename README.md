# Endless Survival

A Unity 2D survival game project.

## Requirements

- Unity Hub
- Unity Editor `2021.3.45f2`
- Git

Use the exact Unity version above when opening the project. Other Unity versions may upgrade project files or create compatibility issues.

## Clone and Setup

1. Clone the repository:

   ```bash
   git clone <repository-url>
   cd "Endless Survival"
   ```

2. Open Unity Hub.
3. Select **Add** or **Open** and choose the cloned `Endless Survival` folder.
4. Open the project with Unity `2021.3.45f2`.
5. Wait for Unity to import assets and restore packages from `Packages/manifest.json`.
6. Open a scene from `Assets/Scenes/`, such as `Selection.unity` or `GamePlay.unity`.
7. Press **Play** in the Unity Editor to run the game.

## Project Structure

- `Assets/` - scenes, scripts, prefabs, and game assets
- `Packages/` - Unity package manifest and lock file
- `ProjectSettings/` - Unity project configuration
- `README.md` - setup instructions

## Version Control Notes

Unity-generated folders and files such as `Library/`, `Temp/`, `Logs/`, `UserSettings/`, and generated project files are excluded by `.gitignore`. They are recreated automatically by Unity after cloning.

If Unity reports missing packages, close the editor, remove the `Library/` folder, reopen the project with Unity `2021.3.45f2`, and allow Unity to reimport the project.

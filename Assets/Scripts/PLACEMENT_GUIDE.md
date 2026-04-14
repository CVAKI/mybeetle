# MyBeetle — Scripts Folder Guide

## Folder Structure → Unity Placement

```
MyBeetle_Scripts/
│
├── Core/
│   ├── FirebaseManager.cs        → Assets/Scripts/Core/
│   ├── GameTimeManager.cs        → Assets/Scripts/Core/
│   └── BeetleData.cs             → Assets/Scripts/Core/
│
├── Beetle/
│   ├── BeetleIdentity.cs         → Assets/Scripts/Beetle/
│   ├── BeetleStats.cs            → Assets/Scripts/Beetle/
│   ├── BeetleLifeCycle.cs        → Assets/Scripts/Beetle/
│   ├── BeetleAnimationController.cs → Assets/Scripts/Beetle/
│   ├── BeetleRLAgent.cs          → Assets/Scripts/Beetle/
│   └── BeetleController.cs       → Assets/Scripts/Beetle/
│
├── World/
│   └── BeetleSpawner.cs          → Assets/Scripts/World/
│
├── Camera/
│   └── BeetleCameraController.cs → Assets/Scripts/Camera/
│
├── UI/
│   └── BeetleHUD.cs              → Assets/Scripts/UI/
│
└── Editor/
    └── BeetleAnimationLister.cs  → Assets/Editor/
```

---

## Scene Setup

### 1. GameManagers (Empty GameObject)
Attach:
- `FirebaseManager`
- `GameTimeManager`

### 2. Beetle Prefab (root GameObject)
Attach ALL of these:
- `BeetleIdentity`
- `BeetleStats`
- `BeetleLifeCycle`
- `BeetleRLAgent`
- `BeetleAnimationController`
- `BeetleController`
- `CharacterController` (capsule collider, center Y=0.5, height=1)

Child object with the FBX mesh:
- `Animator` (with your FBX avatar + animation clips)

### 3. BeetleSpawner (Empty GameObject in scene)
Attach: `BeetleSpawner`
Assign:
- `maleChildPrefab` → your beetle prefab (male)
- `femaleChildPrefab` → your beetle prefab (female)
- `cameraController` → your camera

### 4. Main Camera
Attach: `BeetleCameraController`
Set profile offsets:
- Walk:  offset (0, 2, -4), FOV 60
- Run:   offset (0, 2.5, -5), FOV 70
- Fly:   offset (0, 4, -8), FOV 80
- Fight: offset (0, 1.5, -3), FOV 75
- Death: offset (0, 3, -5), FOV 50

### 5. Canvas (Screen Space Overlay)
Attach: `BeetleHUD`
Create UI elements (Sliders + TextMeshPro) and assign in Inspector:
- HP Bar, EP Bar, Hunger Bar, Strength Bar
- nameLabel, stageLabel, genLabel, actionLabel, ageLabel

---

## Firebase Credentials (already baked in)
- **URL:** `https://mybettle-default-rtdb.asia-southeast1.firebasedatabase.app`
- **API Key:** `AIzaSyDQ6BJkbRM--oxopV0JyHzS6w_YBu6Ts80`

These are set inside `FirebaseManager.cs` directly. No extra setup needed.

---

## Gemini API Key
In `BeetleIdentity.cs`, replace:
```
private const string GEMINI_KEY = "YOUR_GEMINI_API_KEY_HERE";
```
with your actual Gemini API key.

---

## Food Setup
- Tag food GameObjects as `"Food"` in Unity
- Beetle's sensor radius for food: 5 units (configurable on BeetleController)

---

## Next Steps (not yet implemented — future scripts)
- `BeetleMatingSystem.cs` — nest quality check, egg laying, egg timer
- `BeetleClimbController.cs` — vertical surface movement
- `MobController.cs` — mob AI that threatens beetles
- `NestBuilder.cs` — clay ball placement system

# MyBettle — Quick Setup Guide

## 📁 Where to Put Each File

```
YourUnityProject/
└── Assets/
    ├── Editor/
    │   └── BeetleAnimationFetcher.cs   ← animation tool (Editor only)
    │
    ├── Scripts/
    │   └── Beetle/
    │       ├── BeetleIdentity.cs        ← hex ID, name, Firebase, Gemini
    │       └── BeetleTextureManager.cs  ← texture swap per stage/sex
    │
    └── Beetle/                          ← YOUR existing beetle assets
        ├── adult/
        │   ├── male/
        │   └── female/
        ├── youth/
        │   ├── male/
        │   └── female/
        └── child/
            ├── male/
            └── female/
```

---

## 🔧 Step 1 — Get Animation Names

1. Put `BeetleAnimationFetcher.cs` in `Assets/Editor/`
2. Unity menu → **MyBettle ▶ Fetch Beetle Animations**
3. Drag your GLB model into the slot
4. Click **Get All Animations**
5. List prints to Console + saved to `Assets/BeetleAnimationList.txt`

---

## 🎨 Step 2 — Texture Setup

1. Put `BeetleTextureManager.cs` in `Assets/Scripts/Beetle/`
2. Add the component to your Beetle prefab
3. Assign the `SkinnedMeshRenderer` in Inspector
4. Create 6 albedo textures (one per stage+sex combo):
   - `beetle_child_m.png`
   - `beetle_child_f.png`
   - `beetle_youth_m.png`
   - `beetle_youth_f.png`
   - `beetle_adult_m.png`
   - `beetle_adult_f.png`
5. Drag each into the matching slot in the Inspector
6. Right-click the component → **Preview** any combo to test

---

## 🪪 Step 3 — Identity & Firebase

1. Put `BeetleIdentity.cs` in `Assets/Scripts/Beetle/`
2. Add to Beetle prefab (requires `BeetleTextureManager` on same object)
3. Enter your **Gemini API key** in the Inspector field
4. Firebase is pre-configured from your `google-services.json`:
   - URL: `https://mybettle-default-rtdb.asia-southeast1.firebasedatabase.app`
   - Key: embedded in script
5. On Awake: hex ID generates → Gemini assigns name → data saves to Firebase

---

## 🔑 Gemini API Key

Get your key from: https://aistudio.google.com/app/apikey  
Paste it in `BeetleIdentity.cs` → Inspector → **Gemini Api Key** field

---

## ❓ Next Steps (after answering open questions in README.md)
- BeetleStats.cs — hunger timer, HP decay, EP recovery
- BeetleRL.cs — reinforcement learning agent
- BeetleCamera.cs — cinematic follow camera
- BeetleSpawner.cs — world population manager
- BeetleMating.cs — nest building + reproduction

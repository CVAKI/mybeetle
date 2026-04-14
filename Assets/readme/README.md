# 🪲 MyBettle — Life Cycle & World Design Document

> A living reinforcement-learning beetle simulation built in Unity.  
> Each beetle is a unique AI agent that learns, survives, mates, and passes memory to the next generation.

---

## 📋 Table of Contents
1. [World & Time System](#world--time-system)
2. [Beetle Identity System](#beetle-identity-system)
3. [Life Stages](#life-stages)
4. [Stats & Attributes](#stats--attributes)
5. [Hunger & Survival](#hunger--survival)
6. [Reinforcement Learning & Memory](#reinforcement-learning--memory)
7. [Abilities & Skills](#abilities--skills)
8. [Combat System](#combat-system)
9. [Mating & Reproduction](#mating--reproduction)
10. [Camera System](#camera-system)
11. [Mob System](#mob-system)
12. [Firebase Data Storage](#firebase-data-storage)
13. [Visual Identity (Textures)](#visual-identity-textures)
14. [Animation Map](#animation-map)
15. [Open Questions / Clarifications Needed](#open-questions--clarifications-needed)

---

## 🌍 World & Time System

| Real Time | In-Game Time |
|-----------|-------------|
| 10 minutes | 1 month |
| 20 minutes | 2 months (1 year at 6 months/yr?) |

> ⚠️ **Clarification needed:** How many in-game months make 1 in-game year?  
> For now the design assumes **10 real minutes = 1 in-game month**, and **1 in-game year = 12 in-game months = 120 real minutes (2 hours)**.

---

## 🪪 Beetle Identity System

Every beetle that spawns in the world is assigned:

- **Hex ID** — 12-digit hexadecimal unique identifier (e.g., `A3F9C2B10D45`)
- **Human-readable Name** — Generated via **Gemini 2.5 Flash API** on spawn (e.g., *"Thornwick"*, *"Mira"*)
- **Sex** — Male (`M`) or Female (`F`)
- **Generation** — Which generation they belong to (`Gen 0`, `Gen 1`, … `Gen N`)
- **DNA** — Inherited or mutated traits passed down from parents (affects base stats)

### ID Generation (Runtime)
```
HexID = Random 12-char hex string
Name  = GeminiAPI.Generate(prompt: "Give a nature-themed name for a beetle with ID {HexID}")
```

---

## 🥚 Life Stages

```
Egg → Child → Youth → Adult → (Death / Mate)
```

### Stage Breakdown

| Stage | Duration | HP Range | Notes |
|-------|----------|----------|-------|
| **Egg** | 1 month (10 min) | — | Laid by female, guarded by male |
| **Child** | 2 in-game years | 50 HP | Basic walking only, learning begins |
| **Youth** | 3 in-game years | 70–80 HP | Starts climbing, foraging |
| **Adult** | Up to 10 in-game years | 100–200 HP | Can fly, fight, mate, build nest |

> HP in Adult stage is **not fixed** — it scales with food quality and daily activity.

### Adult Mating Window
- First **2 years** of adult life → must build a nest before mating
- After successful mating → male guards female + egg for 1 month
- If mating fails → beetle tries again for another 2-year cycle
- **Total adult lifespan:** 10 in-game years
- If no offspring produced by year 10 → beetle **dies**, camera transfers, new identity + new DNA spawns

---

## 📊 Stats & Attributes

| Stat | Max | Notes |
|------|-----|-------|
| **HP** (Health Points) | Stage-dependent (50/70-80/100-200) | Drops from hunger, combat, environment |
| **EP** (Energy Points) | = HP value at spawn | Used for flying and fighting |
| **Hunger** | 100 (full) → 0 (starving) | Drops by 1 per in-game day if no food |
| **Strength** | Starts at 0 | +5 per won fight/challenge |

### HP Loss from Hunger
- If beetle does **not eat** for 1 in-game day (10 min real time): **−5 HP**
- This continues daily until food is consumed or beetle dies

### EP Recovery
- EP recovers passively when resting
- EP is depleted by: flying, fighting

---

## 🍄 Hunger & Survival

- Beetle must find food in the environment (fungi, leaves, decaying matter — TBD)
- **Not eating for 1 in-game day = −5 HP**
- If HP reaches 0 → beetle **dies**

### Death & Respawn
- Beetle dies → **5 second wait** → respawn at world origin or last nest location
- Respawned beetle **retains memory** from past life (RL model weights carry over)
- New beetle has same Hex ID lineage but is flagged as a new "run"
- Goal: beetle should learn from past deaths and **not repeat mistakes**

---

## 🧠 Reinforcement Learning & Memory

- Each beetle runs an RL agent (e.g., Unity ML-Agents or custom Q-learning)
- **Rewards:**
  - +reward for eating
  - +reward for building shelter
  - +reward for winning fights
  - +reward for surviving longer
  - −penalty for starvation
  - −penalty for dying
- On death → model weights / memory saved to **Firebase Realtime Database**
- On respawn → weights loaded → beetle continues learning from prior experience
- Newborn beetles **inherit** a compressed version of parent's memory (DNA-encoded traits)

---

## 🛠️ Abilities & Skills

### Starting Abilities (Child)
- ✅ Walk (forward, left, right)

### Learned Over Time via RL
| Ability | Unlock Stage | Notes |
|---------|-------------|-------|
| Climb tree | Youth+ | Learned by RL exploration |
| Forage food | Youth+ | Find and identify food sources |
| Build nest (clay ball stacking) | Adult | Better nest = attract female |
| Fly | Adult | Costs EP; learned via RL |
| Fight | Adult | Same-species combat only |

### Nest Building
- Made from **clay balls** that stick together
- Nest quality is scored → female evaluates nest quality before choosing mate
- Poor nest = female rejects → male tries again next 2-year cycle

---

## ⚔️ Combat System

- Combat only with **same species** (beetle vs beetle)
- Reasons for fighting:
  - Competing for food
  - Competing for a mate
  - Protecting egg / female
- **Win:** +5 Strength, opponent loses HP
- **Lose:** Lose HP, EP drained
- **Key animations used:** `Beetle_Attack`, `Beetle_mh_Aggro`, `Beetle_MultiSlam_Attack`, `Beetle_StompAttack`, `Beetle_TakeDamage`, `Beetle_TakeDamage_Minor_*`

---

## 💕 Mating & Reproduction

### Requirements
- Both beetles must be **Adult stage**
- Male must complete nest within 2-year window
- Female evaluates nest quality → accepts or rejects

### Mating Process
```
Adult Male builds nest (2 years)
  └─ Female inspects nest
       ├─ ACCEPTED → Mating occurs
       │     └─ Female lays egg (1 month incubation)
       │           └─ Male guards + provides food
       │                 └─ Egg hatches → Child spawns
       │                       └─ Camera transfers to Child
       └─ REJECTED → Male waits another 2 years, tries again
```

### Camera Transfer Rules
- Camera attached to **Player Beetle** (single beetle, tracked for life)
- On successful reproduction → camera transfers to **child beetle**
- Child inherits **memory DNA** from both parents
- Cycle continues to **generation N**

---

## 🎥 Camera System

- Main camera attached to **one tracked beetle** at all times
- Camera style: **Cinematic** — dynamic angle changes based on action
  - Combat → close, dramatic angle
  - Flying → wide aerial shot
  - Foraging → low ground angle
  - Resting → slow orbit
- HUD overlays on camera:
  - HP bar
  - EP bar
  - Hunger bar
  - Strength value
  - Beetle Name + Hex ID
  - Current generation

---

## 👾 Mob System

- Other creatures in the world that **threaten beetles**
- Types TBD (birds, spiders, ants?)
- Spawn alongside beetles
- Beetle RL agent must learn to **avoid or fight mobs**

> ⚠️ **Clarification needed:** What mob types do you want? Should mobs have AI too?

---

## 🔥 Firebase Realtime Database

**Connection:** Direct REST API (no SDK)  
**Endpoint:** `https://mybettle-default-rtdb.asia-southeast1.firebasedatabase.app`  
**API Key:** (from `google-services.json`)

### Data Structure
```json
{
  "beetles": {
    "A3F9C2B10D45": {
      "name": "Thornwick",
      "sex": "M",
      "generation": 2,
      "stage": "adult",
      "hp": 145,
      "ep": 145,
      "hunger": 72,
      "strength": 20,
      "alive": true,
      "age_months": 84,
      "parent_id": "7B2A10F39C81",
      "rl_weights": { ... },
      "memory_summary": "avoid_spiders, climb_pine_tree, eat_fungi",
      "nest_quality": 78,
      "spawn_position": { "x": 12.5, "y": 0.0, "z": -8.3 },
      "last_updated": 1712345678
    }
  },
  "world": {
    "current_day": 42,
    "tracked_beetle_id": "A3F9C2B10D45"
  }
}
```

---

## 🎨 Visual Identity (Textures)

All beetle models share the same mesh and rig. **Only the Albedo texture changes** to visually differentiate:

| Category | Texture Suffix | Color Hint |
|----------|---------------|-----------|
| Child Male | `_child_m` | Light brown / tan |
| Child Female | `_child_f` | Pale yellow / cream |
| Youth Male | `_youth_m` | Mid brown / olive |
| Youth Female | `_youth_f` | Warm amber |
| Adult Male | `_adult_m` | Deep dark brown / black-green iridescent |
| Adult Female | `_adult_f` | Reddish-brown / mahogany |

> Textures will be AI-generated and swapped at runtime using `Material.SetTexture("_MainTex", ...)`.  
> The `BeetleTextureManager.cs` script handles runtime texture assignment based on sex + stage.

---

## 🎬 Animation Map

> Full list of 78 clips mapped to beetle behaviors:

| Clip Name | Used For |
|-----------|---------|
| `Beetle_Ride_Run_Fwd/Left/Right` | Normal running (fast) |
| `Beetle_Run_Fwd/Left/Right` | Slower walking/running |
| `Beetle_Run_Start` | Starting to run |
| `Beetle_mh` | Idle (trying to move but stuck) |
| `Beetle_mh_Aggro` | Pre-attack aggro stance |
| `Beetle_mh_Gesture_Roar` | Victory roar / intimidation |
| `Beetle_mh_Gesture_Shake` | Wake-up / recovery shake |
| `Beetle_Attack` | Main attack (jump + horn strike) |
| `Beetle_MultiSlam_Aggro` | Pre-flight wing flap attempt |
| `Beetle_MultiSlam_Attack` | Wing flap + jump (early flight learning) |
| `Beetle_Pounce_Aggro` | Wing open, learning to fly |
| `Beetle_Pounce_Start` | Takeoff |
| `Beetle_Pounce_Flight_Mh` | Mid-air flying idle |
| `Beetle_Pounce_Land` | Landing from flight |
| `Beetle_Ride_Jump_Start` | Jump takeoff |
| `Beetle_Ride_Jump_Mh` | Mid-air state |
| `Beetle_Ride_Jump_Land` | Jump landing |
| `Beetle_Ride_Attack` | Attack while airborne |
| `Beetle_Ride_Jump_Miss` | Hit while flying → falling |
| `Beetle_Ride_Jump_Crash` | Crash into obstacle mid-flight |
| `Beetle_Crash_Fwd` | Failed flight → crash forward |
| `Beetle_Crash_mh` | Exhausted/tired fall |
| `Beetle_Crash_Exit` | Wake up from crash/sleep |
| `Beetle_Crash_Ride_Run_Start` | Wake from tired → run |
| `Beetle_StompAttack` | Jump + ground slam |
| `Beetle_TakeDamage` | General damage hit |
| `Beetle_TakeDamage_Minor_Front/Left/Right` | Small hits to head |
| `Beetle_TurnInPlace_Left/Right` | Damaged limp turn |
| `Beetle_TurnInPlace_Left180/Right180` | Quick 180° turn |
| `Beetle_Ride_Run_Death_Var1` | Running → death collapse |
| `Cody_*` (14 clips) | Headless death sequence (decapitation death) |
| `May_Bhv_BeetleRide_AS_*` (15 clips) | Static death poses (various angles) |
| `May_Crash_*` / `May_Ride_*` (12 clips) | Damaged/dying struggle animations |

---

## ❓ Open Questions / Clarifications Needed

Please answer these so the full system can be built accurately:

1. **Time scale:** How many in-game months = 1 in-game year? (Assumed 12)
2. **Food types:** What does the beetle eat? (Fungi, leaves, dung, insects?)
3. **Mob types:** What enemies threaten the beetle? (Birds, spiders, ants, lizards?)
4. **Mob AI:** Do mobs have their own AI, or are they scripted?
5. **Nest materials:** Is the clay ball system physics-based (Rigidbody stacking) or visual only?
6. **Multiple beetles:** How many beetles spawn at world start? Is there a max population?
7. **Female beetle behavior:** Does the female beetle also have RL/AI, or is she scripted?
8. **Climbing:** Is tree climbing physics-based or animation-driven along a spline?
9. **World size:** How large is the environment? Open world or bounded arena?
10. **DNA system:** What traits are passed from parents to children? (HP max, strength base, hunger rate?)
11. **Camera beetle respawn:** When the tracked beetle respawns, does the camera stay attached or go to a neutral position during the 5-second wait?
12. **Gemini name generation:** Should names be unique globally (checked against Firebase) or can duplicates exist?

---

*Document version: 1.0 — Generated from initial design brief.*  
*Update this document as the project evolves.*

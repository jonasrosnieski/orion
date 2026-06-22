# ORION — BRAIN

> Sci-fi game — Unity 6 + URP. Part of the AETHER constellation (VEGA).

---

## Concept (draft — refine with Jonas)

**ORION** is a science-fiction game about a lone operative navigating hostile deep-space sectors — derelict stations, anomaly fields, and forgotten colonies. Tone: tense exploration, environmental storytelling, and tactical decision-making under pressure.

Working title only. Gameplay genre (survival, narrative adventure, tactical, etc.) TBD in first design session.

---

## Pillars

| Pillar | Description | Priority |
|--------|-------------|----------|
| Atmosphere | Isolation, scale of space, readable sci-fi UI | High |
| Exploration | Sectors to uncover, logs, environmental clues | High |
| Tension | Limited resources, unknown threats, meaningful choices | High |
| Identity | Distinct visual language — cold blues, amber alerts, void black | Medium |

---

## Stack

| Layer | Choice |
|-------|--------|
| Engine | Unity 6 (6000.0 LTS) |
| Render | Universal Render Pipeline (URP) |
| Input | Unity Input System |
| Language | C# (.NET Standard 2.1) |
| VCS | Git + GitHub (`jonasrosnieski/orion`) |

---

## Roadmap

| Phase | Goal | Status |
|-------|------|--------|
| 0 | Repo + BRAIN + Unity project opens cleanly | building |
| 1 | Core loop prototype (movement + one interactable sector) | planned |
| 2 | First playable vertical slice (one mission arc) | planned |
| 3 | Art pass + audio + polish | planned |

---

## Open decisions

- [ ] Camera: first-person / third-person / top-down?
- [ ] Single-player only or co-op later?
- [ ] Target platforms: PC first, then console?
- [ ] Art style: realistic / stylized / pixel hybrid?

---

## Architecture (initial)

```
Assets/
  Scripts/
    Core/       — game manager, scene flow
    Player/     — movement, interaction
    World/      — sectors, triggers, narrative hooks
  Scenes/
    Main.unity  — bootstrap (create in Unity Editor)
```

---

## Shipped checklist

- [x] Repository created
- [x] BRAIN defined
- [ ] Unity project opens without errors
- [ ] Main scene + player controller MVP
- [ ] First sector greybox

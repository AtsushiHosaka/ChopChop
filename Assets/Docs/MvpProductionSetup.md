# ChopChop MVP Production Setup

This project intentionally does not keep mock gameplay UI or mock input in the shipped flow. Lv.5 starts only when the production scene references are assigned.

## Unity Packages

- Resolve `com.unity.localization@1.5.9` from Package Manager.
- After package resolution, run `ChopChop > Localization > Rebuild String Tables`.
- Confirm `Assets/Localization/StringTables/ChopChopText` exists with `ja` and `en` tables.

## Build Scenes

Build Settings must keep this order:

1. `Assets/Scenes/StartScene.unity`
2. `Assets/Scenes/LevelSelectScene.unity`
3. `Assets/Scenes/Levels/Level05_SukiyakiScene.unity`
4. `Assets/Scenes/ResultScene.unity`

## Lv.5 Required Scene References

`Level05_SukiyakiScene` needs one `SukiyakiSceneReferences` component assigned with:

- `SukiyakiStageController`
- `HashiInputProvider`
- `WaypointRailMover`
- `MeatTearController` when the Obi meat prefab is ready

The game scene should not create food, chopsticks, or targets at runtime for production. Place the real objects in the scene or load production prefabs.

## Hashi Setup

The Hashi root should contain:

- `HashiInputProvider` configured with device name, service UUID, and characteristic UUID when finalized.
- `HashiPosePresenter` connected to lower and upper chopstick transforms.
- `WaypointRailMover` connected to the production rail waypoints.
- Chopstick tip colliders with `ChopstickTip` for food contact analysis.

## Audio Policy

- Keep `AudioService.PlayCue` as the single runtime entry point for SE.
- Do not ship placeholder/random BGM playback. Current builds should not start any looping background music.
- Future BGM work should add a real licensed loop intentionally, with scene/state selection, fade handling, and settings persistence tracked in `TODO.md`.

## Meat Setup

The meat prefab should contain:

- `MeatInteractionActor`
- `MeatTearController`
- `MeatContactZone`
- Obi `ObiTearableCloth` once Obi Cloth is imported
- `ObiTearableClothReflectionDriver` connected to the Obi component
- `MeatTearProfile`, normally `DefaultSukiyakiMeatTearProfile`

## Result Flow

`SukiyakiStageController.Completed` stores the result in `LevelResultStore` and loads `ResultScene`. Result retry returns to `Level05_SukiyakiScene`.

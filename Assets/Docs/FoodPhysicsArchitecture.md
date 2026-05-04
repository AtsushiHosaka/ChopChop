# ChopChop Food Physics Architecture

## Library Strategy

ChopChop uses food-specific interaction logic and keeps external physics libraries behind adapter components.

- Sukiyaki thin beef: Obi Cloth / Obi Tearable Cloth.
- Sashimi: custom grip and slip logic, optional Obi Softbody.
- Tofu: Obi Softbody for deformation, pre-authored chunks for breaking.
- Tamago kake gohan: shader/state mixing first, optional Obi Fluid later.
- Grilled fish: pre-authored bones, skin, and flesh flakes. Obi Cloth can help skin, Obi Softbody can help larger flesh groups.

## Meat Tearing

The meat does not tear because it is touched for a long time. It tears only when harmful contact metrics exceed the meat profile:

- Poke: chopstick tip velocity into the meat surface.
- Shear: chopstick tip velocity across the meat surface.
- Twist: rotation away from the allowed flip axis.
- Tension: local stretch from the initial grip point.
- Compression: over-pinching from tip distance and pinch input.

`MeatTearController` combines two tear sources:

- Physical tear events from a `MeatTearDriverBehaviour`, such as an Obi bridge.
- Gameplay stress events from `MeatContactAnalyzer`.

When tearing occurs, `MeatInteractionActor` enters `Torn`, failure count increases, and `MeatRetryCoordinator` can move the meat back to the flip retry point without stopping the timer.

## Obi Integration

`ObiTearableClothReflectionDriver` is intentionally reflection-based so the project compiles before Obi Cloth is imported. After Obi Cloth is added, assign an `ObiTearableCloth` component to the driver.

The reflection driver configures common tear settings and listens for `OnClothTorn` if the event signature is compatible. If deeper programmatic tearing is needed, add a compile-time Obi adapter under a dedicated define after the Obi package is present.

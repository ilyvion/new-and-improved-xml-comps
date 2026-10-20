# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Dependencies

-   ilyvion's Laboratory: v0.25.1

> [!IMPORTANT]  
> This release requires an update to ilyvion's Laboratory!

### Added

-   Support for RimWorld 1.7.

### Fixed

-   Selecting a human embryo threw an error, and its buttons failed to show, when the map had no free growth vat.
-   In growth vats with a custom embryoGestationTicks, the fetus is now drawn at a size matching its actual gestation progress. Before, it was sized against the vanilla 9-day duration, so with a shorter gestation it appeared almost fully grown from the start.
-   A grav engine's cooldownGlowRadius now always lights up the map. Before, the larger radius only showed if cooldownGlowColor was also set, and not at all after loading a save made during the cooldown.
-   A glow color the player picked for a grav engine is no longer reset to the default when its cooldown ends.
-   Reduced the performance cost of NewAndImprovedXmlComps_PowerGenFactor on maps with many powered buildings. A building's factor is now recalculated every 250 ticks, so a change to it, like linking or unlinking a facility, can take a few seconds to show in its power output.
-   An embryoGestationTicks of 0 or less is now reported as a config error at startup. Before, such a value made growth vats finish gestation instantly and draw the fetus incorrectly.

## [0.8.0] - 2025-09-20

### Added

-   cooldownGlowColor and cooldownGlowRadius support in GravshipCooldownExtension for custom glow on cooldown.

## [0.7.0] - 2025-09-19

### Added

-   cooldownOrbGraphic support in GravshipOrbExtension for separate orb graphic on cooldown.

## [0.6.0] - 2025-08-09

### Added

-   Added StatDef NewAndImprovedXmlComps_PowerGenFactor, which when applied to a building with a CompPowerTrader, can be used to adjust the building's power output. An example usage is adding a facility that adds a statOffset for the NewAndImprovedXmlComps_PowerGenFactor stat.

## [0.5.0] - 2025-08-08

### Added

-   GravshipCooldownExtension and GravshipOrbExtension modExtensions that can be used on a ThingDef with a Building_GravEngine to customize the orb and cooldown graphics, which are hardcoded in vanilla RimWorld.

## [0.4.0] - 2025-08-01

### Added

-   CompProperties_EmbryoGestation that can be added to the Growth Vat building, with a field embryoGestationTicks that overrides the number of ticks it takes for an embryo to gestate. The default is 9 days (540,000 ticks).

## [0.3.0] 2025-07-16

### Added

-   Rimworld 1.6 support.

## [0.2.0] 2024-08-01

### Added

-   CompProperties_ImprovedPower that can be used in place of CompProperties_Power that lets you hide the power wire that the game normally renders.

## [0.1.1] 2024-08-01

### Fixed

-   The logic should be that if both requiresPower and requiresFuel are enabled, then both are, in fact, required. Before this fix, it would run in an either-or capacity instead.

## [0.1.0] 2024-08-01

### Added

-   First implementation of the mod.

[Unreleased]: https://github.com/ilyvion/new-and-improved-xml-comps/compare/v0.8.0...HEAD
[0.8.0]: https://github.com/ilyvion/new-and-improved-xml-comps/compare/v0.7.0..v0.8.0
[0.7.0]: https://github.com/ilyvion/new-and-improved-xml-comps/compare/v0.6.0..v0.7.0
[0.6.0]: https://github.com/ilyvion/new-and-improved-xml-comps/compare/v0.5.0..v0.6.0
[0.5.0]: https://github.com/ilyvion/new-and-improved-xml-comps/compare/v0.4.0..v0.5.0
[0.4.0]: https://github.com/ilyvion/new-and-improved-xml-comps/compare/v0.3.0..v0.4.0
[0.3.0]: https://github.com/ilyvion/new-and-improved-xml-comps/compare/v0.2.0...v0.3.0
[0.2.0]: https://github.com/ilyvion/new-and-improved-xml-comps/compare/v0.1.1...v0.2.0
[0.1.1]: https://github.com/ilyvion/new-and-improved-xml-comps/compare/v0.1.0...v0.1.1
[0.1.0]: https://github.com/ilyvion/new-and-improved-xml-comps/releases/tag/v0.1.0

# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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

[Unreleased]: https://github.com/ilyvion/new-and-improved-xml-comps/compare/v0.5.0...HEAD
[0.5.0]: https://github.com/ilyvion/new-and-improved-xml-comps/compare/v0.4.0..v0.5.0
[0.4.0]: https://github.com/ilyvion/new-and-improved-xml-comps/compare/v0.3.0..v0.4.0
[0.3.0]: https://github.com/ilyvion/new-and-improved-xml-comps/compare/v0.2.0...v0.3.0
[0.2.0]: https://github.com/ilyvion/new-and-improved-xml-comps/compare/v0.1.1...v0.2.0
[0.1.1]: https://github.com/ilyvion/new-and-improved-xml-comps/compare/v0.1.0...v0.1.1
[0.1.0]: https://github.com/ilyvion/new-and-improved-xml-comps/releases/tag/v0.1.0

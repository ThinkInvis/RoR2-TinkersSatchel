# Tinker's Satchel

A mod for Risk of Rain 2. Built with BepInEx and R2API.

Adds items to the game, all of which may or may not be a little weird.

## Installation

Release builds are published to Thunderstore: https://thunderstore.io/package/ThinkInvis/TinkersSatchel/

**Use of a mod manager is recommended**. If not using a mod manager: extract ThinkInvis-TinkersSatchel-[version].zip into your BepInEx plugins folder such that the following path exists: `[RoR2 game folder]/BepInEx/Plugins/ThinkInvis-TinkersSatchel-[version]/TinkersSatchel.dll`.
Installation of TILER2 is also required: https://thunderstore.io/package/ThinkInvis/TILER2/

## Building

Building Tinker's Satchel locally will require setup of the postbuild event:
- The middle 3 xcopy calls need to either be updated with the path to your copy of RoR2, or removed entirely if you don't want copies of the mod moved for testing.
- Installation of Weaver (postbuild variant) is left as an exercise for the user. https://github.com/risk-of-thunder/R2Wiki/wiki/Networking-with-Weaver:-The-Unity-Way
- WARNING: Weaver currently does not copy files properly, as there are not yet any NetworkBehaviours in this mod. Builds will need to be moved manually/xcopy will need to be modified until this is resolved.

You may also need to change the reference path to TILER2, which is expected to be built in a parallel solution folder.
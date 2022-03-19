# Tinker's Satchel Changelog

**1.5.3**

- Fixed missing load request for R2API.UnlockableAPI.

**1.5.2**

- Fixed Unstable Klein Bottle being enabled preventing some of the base game's on-take-damage effects (Medkit item, red vignette effect).
- Fixed Percussive Maintenance being enabled preventing luck stat, Heretic transformation, and gummy clones from working/updating.
- Fixed Percussive Maintenance not removing its hooks on uninstall (possible performance/compatibility issue).
- Updated TILER2 dependency to 6.0.2.

**1.5.1**

- Added an unlock achievement to Percussive Maintenance.
- Unlock achievements for all items are now implemented correctly, which will no longer look like always-unlocked items in the logbook.
- Cardboard Box now removes nav nodes. No nav nodes are created by placement (yet), as a now-intentional barricade mechanic.

**1.5.0** *The Engi Update*

- ADDED ITEMS: RC Controller, Unstable Klein Bottle, Cardboard Box, Percussive Maintenance!
- Sturdy Mug no longer works on Artificer's Nano-Spear (as its projectiles can self-collide and explode immediately).
- Fixed some config options on Mostly-Tame Mimic and H3AD-53T not updating item descriptions if changed mid-game.
- Increased NRE safety of H3AD-53T.
- Some behind-the-scenes project cleanup.

**1.4.0**

- ADDED ITEMS: Armor Prism, H3AD-53T, Sturdy Mug!
- Mostly-Tame Mimic now has weighted item tier selection (configurable).
- Fixed missing buff icon on Armor Crystal.
- Removed startup index dump.
- Updated TILER2 dependency to 6.0.1.

**1.3.6**

- Mostly-Tame Mimic: added hidden/tierless items to blacklist.
	- Now has its own blacklist in addition to the more general FakeInventory blacklist, to be exposed to public API in a future update.
- Updated TILER2 dependency to 5.0.3.

**1.3.5**

- Compatibility update for Risk of Rain 2 Expansion 1 (SotV).
- Updated R2API dependency to 4.0.11.
- Updated BepInEx dependency to 5.4.1902.
- Updated TILER2 dependency to 5.0.2.

**1.3.4**

- Switched from now-removed TILER2.StatHooks to R2API.RecalculateStatsAPI.
- Fixed another incompatibility with most recent R2API.

**1.3.3**

- Maintenance for RoR2 updates: PC Patch v1.1.1.4.
	- No changes were required beyond updating libraries/references.

**1.3.2**

- Compatibility changes for Risk of Rain 2 Anniversary Update.
- Artifact of Danger will now only allow OHP while cursed if a default-disabled config option `PreventCurseWhileOff` is enabled.

**1.3.1**

- Added extra safety checks to Artifact of Suppression.

**1.3.0**

- Implements changes from TILER2 3.0.0.

**1.2.2**

- Implements changes from TILER2 2.2.3.
	- Mostly-Tame Mimic is now FakeInventory blacklisted, and will not mimic other blacklisted items. Fixes incompatibility with ClassicItems (in combination with a ClassicItems update).
- Bumped R2API dependency to 2.5.14.

**1.2.1**

- Implements changes from TILER2 2.1.0.
	- Mostly-Tame Mimic is now networked, and its items will now correctly appear as temporary (and not permanent) in multiplayer.
- Bumped R2API dependency to 2.5.11.

**1.2.0**

- Mostly-Tame Mimic now uses TILER2.FakeInventory, causing several behavior changes:
	- Mimics will now properly appear in inventory, logbook, stats, etc.
	- Mimics will no longer remain as copies of an item after you lose your last real copy of that item.
	- Display for mimicked item counts is now shared with other sources of temporary items.
- Fixed disabling Artifact of Danger not allowing OHP while cursed.
- Updated libraries for RoR2 1.0.

**1.1.2**

- Artifact of Danger no longer applies OHP to enemies while disabled.
- Severely nerfed default config settings for Armor Crystal.
- Now uses plugin-specific console logger.

**1.1.1**

- Fixed missing buff icon for Artifact of Tactics.
- Mimic count is now displayed per-item, and is no longer added to the item's normal display count.
- Mimics no longer randomly decide to stop being mimics during shuffles.
- Individual Mimics can no longer shuffle from an item to the same item. Swaps between two are still fair game.
- Tweaked Mimic default config settings. Added a new setting to help reduce lag at extremely high itemcounts (limits maximum number of changes per shuffle).

**1.1.0**

- ADDED ITEM: Armor Crystal!
- ADDED ARTIFACTS: Danger, Tactics, Suppression, Haste!
	- Migrated Artifact of Danger from mod GlassArtifactOHP.
- Index dump during game startup is now much prettier.
- GitHub repo is now licensed (GNU GPL3).

**1.0.0**

- Initial version. Adds the following items to the game: Silver Compass, Mostly-Tame Mimic.
# Tinker's Satchel Changelog

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
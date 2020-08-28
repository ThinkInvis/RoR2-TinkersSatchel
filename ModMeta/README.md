# Tinker's Satchel

## SUPPORT DISCLAIMER

### Use of a mod manager is STRONGLY RECOMMENDED.

Seriously, use a mod manager.

If the versions of Tinker's Satchel or TILER2 (or possibly any other mods) are different between your game and other players' in multiplayer, things WILL break. If TILER2 is causing kicks for "unspecified reason", it's likely due to a mod version mismatch. Ensure that all players in a server, including the host and/or dedicated server, are using the same mod versions before reporting a bug.

**While reporting a bug, make sure to post a console log** (`path/to/RoR2/BepInEx/LogOutput.log`) from a run of the game where the bug happened; this often provides important information about why the bug is happening. If the bug is multiplayer-only, please try to include logs from both server and client.

## Description

This is a collection of items and artifacts which sprung from me thinking "hey, what if...?," writing the idea in a file, and forgetting about it (until now).

### Current Additions

#### Tier-1 Item

- Mostly-Tame Mimic: "Mimics your other items at random."
	- Each individual stack has a small chance over time to switch which item it's mimicking.

#### Tier-2 Item

- Armor Crystal: "Gain armor by hoarding money."

#### Lunar Equipment

- Silver Compass: "Shows you a path... but it will be fraught with danger."
	- Pings the Teleporter and adds TWO stacks of Challenge of the Mountain, only ONE of which will count towards extra rewards.

#### Artifact

- Tactics: "All combatants give nearby teammates small, stacking boosts to speed, damage, and armor."
- Suppression: "Players take heavily increased damage while airborne."
- Haste: "All combatants attack 10x faster and deal 1/20x damage."
- Danger: "Players can be killed in one hit."
	- Removes this now-redundant functionality from Artifact of Glass/other sources of Curse.
	- Deactivate the artifact using the mod's config file to restore original OHP behavior.

## Issues/TODO

- Items have no DisplayRules.
- Armor Crystal needs a better model.
- Jigglebones on Mimic lid (and a less half-assed model) may happen eventually.
- Mimic usually displays a count of 0 in chat pickup announcements; might also not count towards logbook stat tracker.
- See the GitHub repo for more!

## Changelog

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
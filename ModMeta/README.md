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

The 5 latest updates are listed below. For a full changelog, see: https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/changelog.md

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
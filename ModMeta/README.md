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
	- Has weighted tiers similar to a T1 chest. Tier weights can be configured.

- Sturdy Mug: "Chance to shoot extra, unpredictable projectiles."
	- Works on most dumbfire projectiles, but not missiles or ground-target AoEs.
	- Stacks linearly past 100% (becomes a chance to fire a 2nd extra projectile, then a 3rd, etc.).
	- Unlock by missing 1000 TOTAL projectile attacks.

- Percussive Maintenance: "Hit allies to heal them."
	- Only 1 HP per stack. Best bring some attack speed!
	- Unlock by having 3 different musical instruments at once.
            <details><summary>Spoiler: Specifically...</summary>Ukulele, War Horn, and Gorag's Opus.</details>

#### Tier-2 Item

- Armor Crystal: "Gain armor by hoarding money."

- Unstable Klein Bottle: "Chance to push nearby enemies on taking damage."

#### Tier-3 Item

- H3AD-53T: "Your Utility skill builds a stunning static charge."
	- Grants or refreshes charges of a buff. Running into or phasing through an enemy spends a charge to deal damage and stun.
	- Unlock by killing a boss with a maximum damage H3AD-5T v2 explosion.

#### Lunar Item

- RC Controller: "Nearby turrets and drones attack with you... but no longer attack automatically."
	- Also adds a +100% (+25% per stack) attack speed buff to affected turrets/drones.

#### Equipment

- Cardboard Box: "Pack up and move."
	- Use once to pick up a turret, shrine, purchasable, etc. Use again to put it back down.

#### Lunar Equipment

- Silver Compass: "Shows you a path... but it will be fraught with danger."
	- Pings the Teleporter and adds TWO stacks of Challenge of the Mountain, only ONE of which will count towards extra rewards.

#### Tier-2 Void Item

###### &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;(requires Survivors of the Void DLC)

- Armor Prism: "Gain massive armor by focusing your item build. Corrupts all Armor Crystals."
	- More item types = less armor.

#### Artifact

- Tactics: "All combatants give nearby teammates small, stacking boosts to speed, damage, and armor."
- Suppression: "Players take heavily increased damage while airborne."
- Haste: "All combatants attack 10x faster and deal 1/20x damage."
- Danger: "Players can be killed in one hit."
	- Has a config option (disabled by default) to force one-hit protection while this artifact is off, even while cursed (e.g. Artifact of Glass).

## Issues/TODO

- Items have no DisplayRules.
- Most items need some effects & model polish in general.
- Mimic usually displays a count of 0 in chat pickup announcements; might also not count towards logbook stat tracker.
- See the GitHub repo for more!

## Changelog

The 5 latest updates are listed below. For a full changelog, see: https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/changelog.md

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
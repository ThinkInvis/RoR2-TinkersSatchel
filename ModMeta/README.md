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
- Safekeeping: "All item drops are taken and guarded by the teleporter boss, which will explode in a shower of loot when killed."

## Issues/TODO

- Items have no DisplayRules.
- Most items need some effects & model polish in general.
- Mimic usually displays a count of 0 in chat pickup announcements; might also not count towards logbook stat tracker.
- See the GitHub repo for more!

## Changelog

The 5 latest updates are listed below. For a full changelog, see: https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/changelog.md

**1.6.1**

- Cardboard Box no longer causes NRE spam if it encounters a bodyless CharacterMaster (e.g. turrets/drones after loading into Bazaar).
- Updated R2API dependency to 4.2.1 (switched to NuGet package).

**1.6.0**

- ADDED ARTIFACT: Artifact of Safekeeping!
- Fixed Sturdy Mug partial proc chance being 1/100 the intended amount (stacking to 100% would still always proc).
- Cardboard Box now replaces the icons of packed allies in the HUD's ally card list.
- Patched for latest game version (no changes were necessary).
- Updated R2API dependency to 4.1.1.

**1.5.5**

- Fixed Mostly-Tame Mimic keeping items selected after losing all real stacks.
- Made Unstable Klein Bottle more consistent.
	- Added a short internal cooldown to prevent multishot attacks from resulting in greatly increased push force.
	- Now always pushes with at least some upwards component.
	- Push force against different enemy types is less varied.
		- Beetles in particular will no longer remain rooted in place if pushed while attacking.
- VFX pass on most item models and all icons.
	- Added missing metallic/smoothness material info in many cases.
	- Finalized lightning particles on Unstable Klein Bottle.
	- Unstable Klein Bottle explosion VFX is now separated from the item's effect and no longer placed far below characters.
	- Icon texturing and outlines are now more consistent with each other and with vanilla graphics.
- New mod icon.

**1.5.4**

- Attempted fix for Mostly-Tame Mimic tier weighting inordinately preferring the highest-weighted tier available.
	- Reworked internals of this item. Each mimic's selected item is now tracked individually, instead of keeping a list of counts. This appears to have also reduced the need for the LagLimit config option.

**1.5.3**

- Fixed missing load request for R2API.UnlockableAPI.
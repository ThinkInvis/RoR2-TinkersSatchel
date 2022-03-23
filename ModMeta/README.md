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

- Macho Moustache: "Deal more damage when surrounded."
	- Focus Crystal but more risk for more reward.

#### Tier-2 Item

- Armor Crystal: "Gain armor by hoarding money."

- Unstable Klein Bottle: "Chance to push nearby enemies on taking damage."

- Pulse Monitor: "Activate your equipment for free at low health."
	- Auto-triggers equipment, does not allow using equipment *manually* for free.
	- Uses equipment's unmodified cooldown and applies its own ICD.
	- Has a config option to allow other ICD sources e.g. Fuel Cell to also apply.

#### Tier-3 Item

- H3AD-53T: "Your Utility skill builds a stunning static charge."
	- Grants or refreshes charges of a buff. Running into or phasing through an enemy spends a charge to deal damage and stun.
	- Unlock by killing a boss with a maximum damage H3AD-5T v2 explosion.

- Pinball Wizard: "Projectiles may bounce, gaining damage and homing."
	- Chance to proc is high, but unaffected by luck. Stacking increases maximum bounce count.
	- Overrides gravity, impact fuse time, etc. on affected projectiles to unerringly track towards a target.

#### Lunar Item

- RC Controller: "Nearby turrets and drones attack with you... BUT no longer attack automatically."
	- Also adds a +100% (+25% per stack) attack speed buff to affected turrets/drones.

#### Equipment

- Cardboard Box: "Pack up and move."
	- Use once to pick up a turret, shrine, purchasable, etc. Use again to put it back down.

#### Lunar Equipment

- Silver Compass: "Shows you a path... BUT it will be fraught with danger."
	- Pings the Teleporter and adds TWO stacks of Challenge of the Mountain, only ONE of which will count towards extra rewards.

#### Void Item

###### &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;(requires Survivors of the Void DLC)

- Villainous Visage (T1): "Deal more damage when given time to plot. Corrupts all Macho Moustaches."
	- Ramps up a damage bonus while out of combat/danger, which only lasts a short time in combat/danger.

- Armor Prism (T2): "Gain massive armor by focusing your item build. Corrupts all Armor Crystals."
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
- Pinball Wizard's internal mechanics are held together with duct tape and a prayer. No known issues but they're definitely there somewhere.
- Mimic usually displays a count of 0 in chat pickup announcements; might also not count towards logbook stat tracker.
- See the GitHub repo for more!

## Changelog

The 5 latest updates are listed below. For a full changelog, see: https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/changelog.md

**1.7.0** *The Evil Lich Update*

- ADDED ITEMS: Macho Moustache, Villainous Visage, Pulse Monitor, Pinball Wizard!
- Yeah yeah it's not really a themed update, I know the name is a stretch :(
- Updated TILER2 dependency to 6.1.0.

**1.6.3**

- Fixed Artifact of Safekeeping trying and failing to work in teleporterless stages (incl. Bazaar, final boss fights).
- Fixed an edge-case NRE in Percussive Maintenance.

**1.6.2**

- Cardboard Box is now synced properly in multiplayer.
- Many, *many* H3AD-53T fixes.
	- No longer has a duplicate internal buff name of Armor Prism's.
	- Now has a per-target internal cooldown instead of a fixed proc frequency.
		- No longer causes NREs if one of its pending targets gets killed before a proc happens, because targets will no longer pend.
	- Actually procs on enemies now.
	- Fixed proc rate being way too fast (once/frame instead of intended ICD of 0.5s).
	- No longer deals 0 damage sometimes.
- Fixed Mostly-Tame Mimic keeping items selected after losing all real stacks. Again. For real this time I promise.
- Fixed Armor Prism pickup model being huge and unrotated.

**1.6.1**

- Cardboard Box no longer causes NRE spam if it encounters a bodyless CharacterMaster (e.g. turrets/drones after loading into Bazaar).
- Updated R2API dependency to 4.2.1 (switched to NuGet package).

**1.6.0**

- ADDED ARTIFACT: Artifact of Safekeeping!
- Fixed Sturdy Mug partial proc chance being 1/100 the intended amount (stacking to 100% would still always proc).
- Cardboard Box now replaces the icons of packed allies in the HUD's ally card list.
- Patched for latest game version (no changes were necessary).
- Updated R2API dependency to 4.1.1.
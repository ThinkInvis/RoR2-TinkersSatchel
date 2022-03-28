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

- Triskelion Brooch: "Chance to combine ignite, freeze, and stun."
	- Inflicting one of these effects has a small chance to also inflict one of the others for a small amount of extra damage.
	- Frozen enemies are stunproof, so only extra damage will occur if a freeze also procs a stun.
	- Unlock by stunning, then freezing, then igniting the same enemy within 3 seconds (any player may contribute).

#### Tier-2 Item

- Armor Crystal: "Gain armor by hoarding money."

- Unstable Klein Bottle: "Chance to push nearby enemies on taking damage."

- Pulse Monitor: "Activate your equipment for free at low health."
	- Auto-triggers equipment, does not allow using equipment *manually* for free.
	- Uses equipment's unmodified cooldown and applies its own ICD.
	- Has a config option to allow other ICD sources e.g. Fuel Cell to also apply.
	- Unlock by falling below 25% health, then returning above 50%, 9 times in the same run.

- Negative Feedback Loop: "Some incoming damage is dealt over time."

- Pixie Tube: "Drop random buffs on using non-primary skills."
	- Buffs attack speed, move speed, damage, or armor.
	- Can be picked up by allies.
		- Has a short pickup delay so you don't eat them all instantly.
		- Pickup range will increase over time to reduce the pressure to chase orbs around constantly.

#### Tier-3 Item

- H3AD-53T: "Your Utility skill builds a stunning static charge."
	- Grants or refreshes charges of a buff. Running into or phasing through an enemy spends a charge to deal damage and stun.
	- Unlock by killing a boss with a maximum damage H3AD-5T v2 explosion.

- Pinball Wizard: "Projectiles may bounce and home."
	- Stacking increases maximum bounce count.
	- Overrides gravity, impact fuse time, etc. on affected projectiles to unerringly track towards a target.

- Go-Faster Stripes: "Your Utility skill gains more mobility."
	- <details><summary>Class-specific details (click to expand):</summary>

		- Most classes: ~+50% move/launch/jump/etc. speed during Utility skill.
		- Huntress: also has reduced blink duration.
		- MUL-T: reactivate Utility skill while active to extend its duration by 1 second. Boosted speed is applied during this duration.
		- Engi: shield applies a stacking speed buff to anyone inside; missiles can be self-targeted (fire with no targets) to explosive jump.
		- Bandit: Explosive-jump with the blast from your smokebomb.
		- Artificer: teleports to wall location (placeholder, intended effect is launch ramp or icy ground).
		- Captain: small airstrike causes a no-damage blast jump; nuke launch becomes more controllable and displays a trajectory preview. Both grant fall damage prevention until your next collision with terrain.
		- DLC characters: WIP! No item effect yet.

	</details>

	- Unlock by trimping (jump or fall onto a ramp fast enough that you get launched upwards).

#### Lunar Item

- RC Controller: "Nearby turrets and drones attack with you... BUT no longer attack automatically."
	- Also adds a +100% (+25% per stack) attack speed buff to affected turrets/drones.

- Bismuth Tonic: "Gain resistance when hit by one enemy type... BUT gain weakness to the others."

#### Equipment

- Cardboard Box: "Pack up and move."
	- Use once to pick up a turret, shrine, purchasable, etc. Use again to put it back down.

- Causal Camera: "Phase briefly and rewind yourself 10 seconds."
	- Rewinds position, velocity, health, shields, barrier, buffs, debuffs, DoTs, and skill cooldowns/stock (except equipment).

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
- Some class-specific item behaviors on Go-Faster Stripes are missing or placeholders.
- Pinball Wizard's internal mechanics are held together with duct tape and a prayer. No known issues but they're definitely there somewhere.
- Mimic usually displays a count of 0 in chat pickup announcements; might also not count towards logbook stat tracker.
- See the GitHub repo for more!

## Changelog

The 5 latest updates are listed below. For a full changelog, see: https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/changelog.md

**1.9.2**

- Balance pass.
	- Bismuth Flask:
		- Now has an effect duration (10s by default) instead of lasting forever.
		- This item is meant to reward skillful kiting, not punish you for fighting more than one enemy type ever.
	- Pinball Wizard:
		- I greatly underestimated how powerful auto-aim bounces on missed attacks could be >_>
		- Now affected by luck, but has a lower default proc chance (35% --> 15%).
		- No longer stacks damage per bounce.
		- Bouncing projectiles now deal less damage (50% by default).
		- Extra bounces per additional stack buffed (1 --> 2), as this is the only remaining per-stack scaling on this item.
	- Triskelion Brooch:
		- Shock from Captain's secondary now counts as a stun for purposes of triggering this item.
	- Mostly-Tame Mimic:
		- No longer mimics useless items (scrap/consumed).
	- Percussive Maintenance:
		- Now scales based on proc coefficient (matches Leeching Seed behavior).
		- Buffed default healing amount per stack (1hp --> 2hp) due to the opportunity cost of dealing damage, which Leeching Seed does not have.

- Bismuth Tonic: Fixed a serious NRE related to Engineer turrets.

- Fixed Cardboard Box not syncing for clients on non-dedicated servers.

- Triskelion Brooch:
	- Fixed proc chance not being checked and always proccing in some cases.
	- Fixed proc chance being capped at 1% and displaying in logbook at 100x its actual value.
	- Split proc chance into base/stack configs.
	- Removed upper cap from damage configs.

- Percussive Maintenance healing amount is now configurable.

- Mostly-Tame Mimic:
	- No longer mimics AI summons (Queen's Gland and Empathy Cores) due to a bug.
	- Now exposes the public API method `Mimic.instance.BlacklistItem(ItemDef)` to add an item to its internal blacklist.

- Causal Camera:
	- Removed a debug log that managed to slip through the cracks.
	- Added missing entry to the Thunderstore description.

**1.9.1**

- Fixed Causal Camera spamming console errors while equipped on bodies with fewer components than usual (e.g. Engi turrets).

**1.9.0** *The Artifice Update, Part 2*

- ADDED ITEMS: Triskelion Brooch, Bismuth Tonic!

- Greatly reduced texture scales to match vanilla (for filesize purposes).

- Causal Camera: should work properly in multiplayer now. Probably. ...Maybe.

- Pulse Monitor: Added an unlock achievement.

- Go-Faster Stripes:
	- Made the pickup model and icon less octagonal.
	- Now works on Captain. Airstrike has launch force vs allies with no damage, nuke has trajectory preview and more controllable launch, both provide fall damage protection.
	- Removed unintentional duplicated force application from Engineer missile effect.
	- Added an unlock achievement.

- Pixie Tube:
	- Wisps now increase the range at which they gravitate towards allies over time (6 m --> 36 m).
	- Shortened wisp arming delay (2.5 s --> 2 s) and increased lifetime to account for arming delay (10s --> 12s).
	- Wisps are now dimmer during their arming delay.
	- Made timing on wisp VFX more consistent.
	- Pickup model now flickers its fuses on at random.

**1.8.0** *The Artifice Update, Part 1*

- ADDED ITEMS: Negative Feedback Loop, Pixie Tube, Go-Faster Stripes, Causal Camera!
- Added extra null safety to some Percussive Maintenance hooks (may fix console spam).
- Fixed the AI blacklist setting on Pulse Monitor not having a config option.

**1.7.2**

- Fixed Armor Prism yoinking some of Armor Crystal's code in addition to its itemcount.
	- (after both were enabled and obtained in the same run, latter would give no armor and former would give double).
- Pulse Monitor no longer spams NREs after placing an Engi turret, and is now AI-blacklisted.
- Added extra null safety to Percussive Maintenance melee attack handling.

**1.7.1**

- Fixed Cardboard Box not moving the healing zone of heal shrines.
- Reduced default Cardboard Box cooldown from 180 seconds to 60 seconds.
- Added missing "corrupts all..." pickup text to Villainous Visage.
- Properly capitalized "BUT" in the Silver Compass description.
- Added extra null safety to all R2API.RecalculateStatsAPI.GetStatCoefficients subscribers (may fix an edge-case hang).
- Added missing Thunderstore description text for 1.7.0 content.
- Updated TILER2 dependency to 6.1.2.
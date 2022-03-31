# Tinker's Satchel

## SUPPORT DISCLAIMER

### Use of a mod manager is STRONGLY RECOMMENDED.

Seriously, use a mod manager.

If the versions of Tinker's Satchel or TILER2 (or possibly any other mods) are different between your game and other players' in multiplayer, things WILL break. If TILER2 is causing kicks for "unspecified reason", it's likely due to a mod version mismatch. Ensure that all players in a server, including the host and/or dedicated server, are using the same mod versions before reporting a bug.

**While reporting a bug, make sure to post a console log** (`path/to/RoR2/BepInEx/LogOutput.log`) from a run of the game where the bug happened; this often provides important information about why the bug is happening. If the bug is multiplayer-only, please try to include logs from both server and client.

## Description

Tinker's Satchel is a general content pack, containing assorted items, equipments, and artifacts (for now -- more content types pending!).

### Mod Content: Items & Equipments

<table>
	<thead>
		<tr>
			<th>Icon</th>
			<th>Name</th>
			<th>Description<br><small>Pickup; click &rtrif; for logbook description</small></th>
			<th>Notes</th>
		</tr>
	</thead>
	<tbody>
		<tr>
			<td colspan="4" align="center"><h3>Tier-1 Items</h3></td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/mimicIcon.png?raw=true" width=256></td>
			<td><b>Mostly-Tame Mimic</b></td>
			<td><details>
				<summary>Mimics your other items at random.</summary>
				Picks one of your other items to mimic <small>(each stack is tracked separately)</small>. Every 3 seconds, the mimic has a 15% chance to switch to a new item.
			</details></td>
			<td><ul>
				<li>Each individual stack has a small chance over time to switch which item it's mimicking.</li>
				<li>Has weighted tiers similar to a T1 chest. Tier weights can be configured.</li>
			</ul></td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/mugIcon.png?raw=true" width=256></td>
			<td><b>Sturdy Mug</b></td>
			<td><details>
				<summary>Chance to shoot extra, unpredictable projectiles.</summary>
				All projectile attacks gain a 10% <small>(+10% per stack)</small> chance to fire an extra copy with 17.5&degr; of inaccuracy.
			</details></td>
			<td><ul>
				<li>Works on most dumbfire projectiles, but not missiles, ground-target AoEs, or deployables.</li>
				<li>Stacks linearly past 100% (becomes a chance to fire a 2nd extra projectile, then a 3rd, etc.).</li>
				<li><details><summary>Unlock (spoilers!):</summary>Miss 1000 TOTAL projectile attacks.</details></li>
			</ul></td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/shootToHealIcon.png?raw=true" width=256></td>
			<td><b>Percussive Maintenance</b></td>
			<td><details>
				<summary>Hit allies to heal them.</summary>
				Hitting an ally with a direct attack heals them for 2.0 health <small>(+2.0 per stack)</small>.
			</details></td>
			<td><ul>
				<li>Double healing of and lower tier than Leeching Seed, due to the opportunity cost of dealing damage to enemies, needing to see and hit your probably-juking-very-hard teammates, etc.</li>
				<li><details><summary>Unlock (spoilers!):</summary>Have Ukulele, War Horn, and Gorag's Opus all at once.</details></li>
			</ul></td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/moustacheIcon.png?raw=true" width=256></td>
			<td><b>Macho Moustache</b></td>
			<td><details>
				<summary>Deal more damage when surrounded.</summary>
				Gain +5% base damage <small>(+5% per stack, linear)</small> per enemy within 10 m.
			</details></td>
			<td><ul>
				<li>Lower range than Focus Crystal, and provides ~1/4 as much damage per stack; however, the effect ceiling is higher if you're willing to put yourself in additional danger.</li>
			</ul></td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/triBroochIcon.png?raw=true" width=256></td>
			<td><b>Triskelion Brooch</b></td>
			<td><details>
				<summary>Chance to combine ignite, freeze, and stun.</summary>
				Ignites, freezes, and stuns have a 9% <small>(+9% per stack)</small> chance to also cause one of the other effects listed for 100% base damage <small>(+25% per stack)</small>.
			</details></td>
			<td><ul>
				<li>Frozen enemies are stunproof, so only extra damage will occur if a freeze also procs a stun.</li>
				<li><details><summary>Unlock (spoilers!):</summary>Stun, then freeze, then ignite the same enemy within 3 seconds (any player may contribute).</details></li>
			</ul></td>
		</tr>
		<tr>
			<td colspan="4" align="center"><h3>Tier-2 Items</h3></td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/goldenGearIcon.png?raw=true" width=256></td>
			<td><b>Armor Crystal</b></td>
			<td><details>
				<summary>Gain armor by hoarding money.</summary>
				Gain armor based on your currently held money. The first point of armor costs $10 <small>(-10% per stack, exponential; scales with difficulty)</small>; each subsequent point costs 7.5% more than the last.
			</details></td>
			<td><ul>
			</ul></td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/kleinBottleIcon.png?raw=true" width=256></td>
			<td><b>Unstable Klein Bottle</b></td>
			<td><details>
				<summary>Chance to push nearby enemies on taking damage.</summary>
				8.0% (+8.0% per stack, mult.) chance to push away enemies within 20 m after taking damage. <small>Has an internal cooldown of 0.5 s.</small>
			</details></td>
			<td><ul>
			</ul></td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/deadManSwitchIcon.png?raw=true" width=256></td>
			<td><b>Pulse Monitor</b></td>
			<td><details>
				<summary>Auto-activate your equipment for free at low health.</summary>
				Falling below 25% health activates your equipment without putting it on cooldown. This effect has its own cooldown equal to the cooldown of the activated equipment <small>(-15% per stack, mult.)</small>.
			</details></td>
			<td><ul>
				<li>Uses equipment's unmodified cooldown and applies its own ICD. Has a config option to allow other ICD sources like Fuel Cell to also apply, disabled by default.</li>
				<li><details><summary>Unlock (spoilers!):</summary>Fall below 25% health, then return above 50%, 9 times in the same run.</details></li>
			</ul></td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/damageBufferIcon.png?raw=true" width=256></td>
			<td><b>Negative Feedback Loop</b></td>
			<td><details>
				<summary>Some incoming damage is dealt over time.</summary>
				20% <small>(+20% per stack, hyperbolic)</small> of incoming damage is applied gradually over 5 seconds, ticking every 0.2 seconds. Healing past max health will apply to the pool of delayed damage.
			</details></td>
			<td><ul>
			</ul></td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/pixieTubeIcon.png?raw=true" width=256></td>
			<td><b>Pixie Tube</b></td>
			<td><details>
				<summary>Drop random buffs on using non-primary skills.</summary>
				You drop 1 <small>(+1 per stack)</small> random elemental wisp when you use a non-primary skill. Elemental wisps can be picked up by any ally as a small, stacking buff for 10 seconds: +3% damage, +5% movement speed, +5% attack speed, or 10 armor.
			</details></td>
			<td><ul>
				<li>Has a short pickup delay so you don't eat them all instantly if you're moving away from them.</li>
				<li>Pickup range will increase over time to reduce the pressure to chase orbs around constantly.</li>
				<li>It's a <a href="https://en.wikipedia.org/wiki/Nixie_tube">Nixie tube</a>.</li>
			</ul></td>
		</tr>
		<tr>
			<td colspan="4" align="center"><h3>Tier-3 Items</h3></td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/headsetIcon.png?raw=true" width=256></td>
			<td><b>H3AD-53T</b></td>
			<td><details>
				<summary>Your Utility skill builds a stunning static charge.</summary>
				After activating your Utility skill, the next 5 enemies <small>(+3 per stack)</small> your path crosses will take 400% damage <small>(+150% per stack)</small> and be stunned for 5 seconds.
			</details></td>
			<td><ul>
				<li>Grants or refreshes charges of a buff. Running into or phasing through an enemy spends a charge to deal damage and stun.</li>
				<li><details><summary>Unlock (spoilers!):</summary>Kill a boss with a maximum damage H3AD-5T v2 explosion.</details></li>
			</ul></td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/pinballIcon.png?raw=true" width=256></td>
			<td><b>Pinball Wizard</b></td>
			<td><details>
				<summary>Projectiles may bounce and home.</summary>
				All your projectile attacks have a 15% chance to bounce, exploding one extra time and homing towards a random enemy with 50% of their original damage. Can happen up to 3 times <small>(+2 per stack)</small> per projectile.
			</details></td>
			<td><ul>
				<li>Overrides gravity, impact fuse time, etc. on affected projectiles to unerringly track towards a target.</li>
				<li>Ding!</li>
			</ul></td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/goFasterIcon.png?raw=true" width=256></td>
			<td><b>Go-Faster Stripes</b></td>
			<td><details>
				<summary>Your Utility skill gains more mobility.</summary>
				Upgrades your Utility skill, greatly increasing its mobility.
			</details></td>
			<td><ul>
				<li><details><summary>Class-specific details (click to expand):</summary><ul>
					<li>Most classes: ~+50% move/launch/jump/etc. speed during Utility skill.</li>
					<li>Huntress: also has reduced blink duration.</li>
					<li>MUL-T: reactivate Utility skill up to 3 times while active to extend its duration by 1 second. Boosted speed is applied during this duration.</li>
					<li>Engi: shield applies a stacking speed buff to anyone inside; missiles can be self-targeted (fire with no targets) to explosive jump.</li>
					<li>Bandit: Explosive-jump with the blast from your smokebomb.</li>
					<li>Artificer: teleports to wall location (placeholder, intended effect is launch ramp or icy ground).</li>
					<li>Captain: small airstrike causes a no-damage blast jump; nuke launch becomes more controllable and displays a trajectory preview. Both grant fall damage prevention until your next collision with terrain.</li>
					<li>DLC characters: WIP! No item effect yet.</li>
				</ul></details></li>
				<li><details><summary>Unlock (spoilers!):</summary>Trimp -- jump or fall onto a ramp fast enough that you get launched upwards. Running off a ramp won't work.</details></li>
			</ul></td>
		</tr>
		<tr>
			<td colspan="4" align="center"><h3>Equipments</h3></td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/packBoxIconOpen.png?raw=true" width=256></td>
			<td><b>Cardboard Box</b></td>
			<td><details>
				<summary>Pack up and move. <small>[Cooldown: 60 s]</small></summary>
					Use once to pack up a turret, healing shrine, or most other interactables. Use again to place the packed object and put the Cardboard Box on cooldown.
			</details></td>
			<td><ul>
			</ul></td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/rewindIcon.png?raw=true" width=256></td>
			<td><b>Causal Camera</b></td>
			<td><details>
				<summary>Phase briefly and rewind yourself 10 seconds. <small>[Cooldown: 90 s]</small></summary>
					Phase out of existence for 2.0 seconds. Rewind your position, health, and skill cooldowns <small>(except equipment)</small> to their states from up to 10.0 seconds ago.
			</details></td>
			<td><ul>
				<li>Rewinds position, velocity, health, shields, barrier, and skill cooldowns/stock (except equipment).</li>
			</ul></td>
		</tr>
		<tr>
			<td colspan="4" align="center"><h3>Lunar Items</h3></td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/wranglerIcon.png?raw=true" width=256></td>
			<td><b>RC Controller</b></td>
			<td><details>
				<summary>Nearby turrets and drones attack with you... <i>BUT no longer attack automatically.</i></summary>
				All turrets and drones under your ownership within 150 meters will no longer auto-target, auto-attack, or chase enemies. Order drones to fire by holding your Primary skill keybind. Affected turrets and drones gain +100% attack speed <small>(+25% per stack)</small>.
			</details></td>
			<td><ul>
			</ul></td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/bismuthFlaskIcon.png?raw=true" width=256></td>
			<td><b>Bismuth Tonic</b></td>
			<td><details>
				<summary>Gain resistance when hit by one enemy type... <i>BUT gain weakness to the others.</i></summary>
					On being hit by one type of enemy: take 12.5% less damage from subsequent attacks from that type, but 20% more damage from all other types. Wears off after 10 seconds.
			</details></td>
			<td><ul>
			</ul></td>
		</tr>
		<tr>
			<td colspan="4" align="center"><h3>Lunar Equipments</h3></td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/compassIcon.png?raw=true" width=256></td>
			<td><b>Silver Compass</b></td>
			<td><details>
				<summary>Shows you a path... <i>BUT it will be fraught with danger.</i> <small>[Cooldown*: 180 s]</small></summary>
					Immediately reveals the teleporter. Also adds two stacks of Challenge of the Mountain to the current stage, one of which will not provide extra item drops. Works only once per player per stage.
			</details></td>
			<td><ul>
				<li>*Works only once per player per stage, <i>in addition to</i> a long cooldown in case of very fast stage clears. Both of these are configurable.</li>
			</ul></td>
		</tr>
		<tr>
			<td colspan="4" align="center"><h3>Void Items</h3><h4>&emsp;&emsp;(requires Survivors of the Void DLC)</h4></td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/voidMoustacheIcon.png?raw=true" width=256></td>
			<td><b>Villainous Visage (T1)</b></td>
			<td><details>
				<summary>Deal more damage when given time to plot. Corrupts all Macho Moustaches.</summary>
					While out of combat, build up a damage buff that will last 2 seconds once in combat. Builds 3% damage per second <small>(+3% per stack)</small>, up to 15% <small>(+15% per stack)</small>. Corrupts all Macho Moustaches.
			</details></td>
			<td><ul>
			</ul></td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/voidGoldenGearIcon.png?raw=true" width=256></td>
			<td><b>Armor Prism (T2)</b></td>
			<td><details>
				<summary>Gain massive armor by focusing your item build. Corrupts all Armor Crystals.</summary>
					Gain armor based on your currently held types of item (fewer is better). Having only Armor Prisms gives 500 armor <small>(+25% per stack, inverse-exponential)</small>; each subsequent item type reduces armor by 12.5%. Corrupts all Armor Crystals.
			</details></td>
			<td><ul>
			</ul></td>
		</tr>
	</tbody>
</table>

### Mod Content: Artifacts

<table>
	<thead>
		<tr>
			<th>Icon</th>
			<th>Name</th>
			<th>Description</th>
			<th>Notes</th>
		</tr>
	</thead>
	<tbody>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/tactics_on.png?raw=true" width=128></td>
			<td><b>Artifact of Tactics</b></td>
			<td>All combatants give nearby teammates small, stacking boosts to speed, damage, and armor.</td>
			<td><ul>
				<li>Buff is 5% speed, 10% damage, and 15 armor per ally (including self) within 25 m.</li>
				<li>All listed aspects of buff can be configured.</li>
			</ul></td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/antiair_on.png?raw=true" width=128></td>
			<td><b>Artifact of Suppression</b></td>
			<td>Players take heavily increased damage while airborne.</td>
			<td><ul>
				<li>Incoming damage is multiplied by 5 (configurable).</li>
			</ul></td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/butterknife_on.png?raw=true" width=128></td>
			<td><b>Artifact of Haste</b></td>
			<td>All combatants attack 10x faster and deal 1/20x damage.</td>
			<td><ul>
				<li>Values are <i>not</i> configurable.</li>
			</ul></td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/danger_on.png?raw=true" width=128></td>
			<td><b>Artifact of Danger</b></td>
			<td>Players can be killed in one hit.</td>
			<td><ul>
				<li>Provides a config option (disabled by default) to force one-hit protection while this artifact is off, even while cursed (e.g. Artifact of Glass).</li>
			</ul></td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/delayitems_on.png?raw=true" width=128></td>
			<td><b>Artifact of Safekeeping</b></td>
			<td>All item drops are taken and guarded by the teleporter boss, which will explode in a shower of loot when killed.</td>
			<td><ul>
			</ul></td>
		</tr>
	</tbody>
</table>

## Issues/TODO

- Items have no DisplayRules.
- Most items need some effects & model polish in general.
- Some class-specific item behaviors on Go-Faster Stripes are missing or placeholders.
- Pinball Wizard's internal mechanics are held together with duct tape and a prayer. No known issues but they're definitely there somewhere.
- Mimic usually displays a count of 0 in chat pickup announcements; might also not count towards logbook stat tracker.
- See the GitHub repo for more!

## Changelog

The 5 latest updates are listed below. For a full changelog, see: https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/changelog.md

**1.9.5**

- Added informational buffs to Pulse Monitor (cooldown), Causal Camera (number of saved rewind frames), and Bismuth Tonic (remaining duration).
- Negative Feedback Loop no longer spams red vignette and flinch animation (added Silent damage type).
- Cardboard Box:
	- Now returns packed objects to their original locations when unequipped.
	- Can now move Warbanners.
- Fixed Bismuth Tonic cooldown only applying to subsequent hits taken after the first.
- Sturdy Mug no longer works on *any* Deployables (was case-by-case blacklist before). Fixes poor interaction with Railgunner utility and future-proofs for similar skills.
- Pinball Wizard:
	- Fixed causing sticky projectiles to not explode (e.g. Sticky Bomb, Railgunner specials) -- no longer procs on these.
	- Now ignores the Neutral team (e.g. flying rocks hazard on Sky Meadows).
	- Improved SFX. Now works on all projectiles and uses a more fitting vanilla sound effect.

**1.9.4**

- Balance pass.
	- Go-Faster Stripes:
		- Bandit: Buffed default launch force scalar from 20 to 30.
		- MUL-T: Now has configurable number of boosts per skill cast per stack. Defaults to 3 (buffed from 1).
	- Unstable Klein Bottle:
		- Buffed default proc chance from 5% to 8%.
- Causal Camera:
	- No longer rewinds buffs, debuffs, or DoTs. This was causing *way* too many problems.
	- No longer rewinds Captain's once-per-stage beacon skills.
- Pixie Tube:
	- Prevented from working with Railgunner's secondary.
	- Reduced wisp arming time (2s --> 1.5s).
	- Wisps are now wispier (no gravity, flit about randomly in midair, smaller particle size).
	- Wisps on player team now wait until near the end of their lifetime (2s left) to gravitate towards non-player allies.
- Pinball Wizard: stole some vanilla SFX for the proc effect. Only works on physical projectiles for now, and not hitscan attacks.

**1.9.3**

- Causal Camera no longer restores drop pod/teleport immunity.

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
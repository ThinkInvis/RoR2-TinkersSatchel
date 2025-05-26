# Tinker's Satchel

## SUPPORT DISCLAIMER

### Use of a mod manager is STRONGLY RECOMMENDED.

Seriously, use a mod manager.

If the versions of Tinker's Satchel or TILER2 (or possibly any other mods) are different between your game and other players' in multiplayer, things WILL break. If TILER2 is causing kicks for "unspecified reason", it's likely due to a mod version mismatch. Ensure that all players in a server, including the host and/or dedicated server, are using the same mod versions before reporting a bug.

**While reporting a bug, make sure to post a console log** (`path/to/RoR2/BepInEx/LogOutput.log`) from a run of the game where the bug happened; this often provides important information about why the bug is happening. If the bug is multiplayer-only, please try to include logs from both server and client.

## Description

Tinker's Satchel is a general content pack, containing assorted items, equipments, interactables, artifacts, and skill variants. In total, this mod includes:

- 47 items/equipments:
	- 8 tier-1,
	- 10 tier-2,
	- 7 tier-3,
	- 1 boss item,
	- 6 equipments,
	- 5 lunar items,
	- 4 lunar equipments,
	- 3 tier-1 void,
	- 3 tier-2 void,
	- 3 tier-3 void;
- 2 interactables:
	- 2 drones;
- 8 skill variants:
	- 3 for Commando,
	- 2 for Huntress,
	- and 3 for Engineer;
- 6 artifacts;
- and 3 other features:
	- a UI tweak,
	- an off-by-default module allowing one-shot protection while cursed,
	- and a module allowing easy changes to equipment max charges.

Short summaries are provided below. For a full description of each item, see: https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ContentSummary.md

### Mod Content: Items & Equipments

<table>
	<thead>
		<tr>
			<th>Icon</th>
			<th>Name/Description</th>
		</tr>
	</thead>
	<tbody>
		<tr>
			<td colspan="2" align="center"><h3>Tier-1 Items</h3></td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/extendoArmsIcon.png?raw=true" width=128></td>
			<td>
				<b>Extendo-Arms</b><br>
				Attacks reach farther and deal slightly more damage.
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/magnetismIcon.png?raw=true" width=128></td>
			<td>
				<b>Ferrofluid</b><br>
				Your attacks become slightly magnetic and gain crit chance.
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/moustacheIcon.png?raw=true" width=128></td>
			<td>
				<b>Macho Moustache</b><br>
				Chance to Taunt on hit. Resist Taunted enemies.
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/mimicIcon.png?raw=true" width=128></td>
			<td>
				<b>Mostly-Tame Mimic</b><br>
				Mimics your other items at random.
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/motionTrackerIcon.png?raw=true" width=128></td>
			<td>
				<b>Old War Lidar</b><br>
				Periodically fire weak projectiles at all hostiles.
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/shootToHealIcon.png?raw=true" width=128></td>
			<td>
				<b>Percussive Maintenance</b><br>
				Hit allies to heal them.
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/mugIcon.png?raw=true" width=128></td>
			<td>
				<b>Sturdy Mug</b><br>
				Chance to shoot extra, unpredictable projectiles.
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/triBroochIcon.png?raw=true" width=128></td>
			<td>
				<b>Triskelion Brooch</b><br>
				Chance to combine ignite, freeze, and stun.
			</td>
		</tr>
		<tr>
			<td colspan="2" align="center"><h3>Tier-2 Items</h3></td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/goldenGearIcon.png?raw=true" width=128></td>
			<td>
				<b>Chestplate</b><br>
				Collecting money grants temporary armor.
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/defibIcon.png?raw=true" width=128></td>
			<td>
				<b>Defibrillator</b><br>
				Your heals can crit.
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/enPassantIcon.png?raw=true" width=128></td>
			<td>
				<b>En Passant</b><br>
				Melee strike with your Utility skill to recharge it.
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/fudgeDiceIcon.png?raw=true" width=128></td>
			<td>
				<b>Fudge Dice</b><br>
				Periodically guarantee luck.
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/damageBufferIcon.png?raw=true" width=128></td>
			<td>
				<b>Negative Feedback Loop</b><br>
				Taking damage creates a healing barrier over time.
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/hurdyGurdyIcon.png?raw=true" width=128></td>
			<td>
				<b>Hurdy-Gurdy</b><br>
				Wind up with uninterrupted Secondary skills to fire burning projectiles.
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/pixieTubeIcon.png?raw=true" width=128></td>
			<td>
				<b>Pixie Tube</b><br>
				Drop random buffs on using skills.
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/deadManSwitchIcon.png?raw=true" width=128></td>
			<td>
				<b>Pulse Monitor</b><br>
				Auto-activate your equipment for free at low health.
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/swordbreakerIcon.png?raw=true" width=128></td>
			<td>
				<b>Swordbreaker</b><br>
				Retaliate with exploding sparks when your shield is struck.
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/kleinBottleIcon.png?raw=true" width=128></td>
			<td>
				<b>Unstable Klein Bottle</b><br>
				Chance to float and stun nearby enemies on taking damage.
			</td>
		</tr>
		<tr>
			<td colspan="2" align="center"><h3>Tier-3 Items</h3></td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/goFasterIcon.png?raw=true" width=128></td>
			<td>
				<b>Go-Faster Stripes</b><br>
				Your Utility skill gains more mobility.
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/headsetIcon.png?raw=true" width=128></td>
			<td>
				<b>H3AD-53T</b><br>
				Your Utility skill builds a stunning static charge.
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/kintsugiIcon.png?raw=true" width=128></td>
			<td>
				<b>Kintsugi</b><br>
				Your broken/consumed/scrapped items increase all your stats.
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/pinballIcon.png?raw=true" width=128></td>
			<td>
				<b>Pinball Wizard</b><br>
				Projectiles may bounce and home.
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/wranglerIcon.png?raw=true" width=128></td>
			<td>
				<b>RC Controller</b><br>
				Nearby turrets and drones gain attack speed. Ping to take control.
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/shrinkRayIcon.png?raw=true" width=128></td>
			<td>
				<b>Shrink Ray</b><br>
				Suppress a single target's non-primary skills and damage.
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/skeinIcon.png?raw=true" width=128></td>
			<td>
				<b>Spacetime Skein</b><br>
				Gain mass while stationary. Lose mass while moving.
			</td>
		</tr>
		<tr>
			<td colspan="2" align="center"><h3>Boss Items</h3></td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/extraEquipmentIcon.png?raw=true" width=128></td>
			<td>
				<b>Scavenger's Rucksack</b><br>
				Hold an extra Equipment. Activate with scoreboard open to rummage through the rucksack.
			</td>
		</tr>
		<tr>
			<td colspan="2" align="center"><h3>Equipments</h3></td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/packBoxIconOpen.png?raw=true" width=128></td>
			<td>
				<b>Cardboard Box</b><br>
				Pack up and move. <small>[Cooldown: 60 s]</small>
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/rewindIcon.png?raw=true" width=128></td>
			<td>
				<b>Causal Camera</b><br>
				Phase briefly and rewind yourself 10 seconds. <small>[Cooldown: 90 s]</small>
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/reviveOnceIcon.png?raw=true" width=128></td>
			<td>
				<b>Command Terminal</b><br>
				Revive an ally or summon a drone. Consumed on use. <small>[Cooldown: 10 s]</small>
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/lodestoneIcon.png?raw=true" width=128></td>
			<td>
				<b>Lodestone</b><br>
				Pull nearby enemies and allied item effects. <small>[Cooldown: 20 s]</small>
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/recombobulatorIcon.png?raw=true" width=128></td>
			<td>
				<b>Quantum Recombobulator</b><br>
				Reroll an interactable once. <small>[Cooldown: 60 s]</small>
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/dodgeIcon.png?raw=true" width=128></td>
			<td>
				<b>Stamina Bar</b><br>
				Perform up to 3 dodge rolls. <small>[Cooldown: 10 s]</small>
			</td>
		</tr>
		<tr>
			<td colspan="2" align="center"><h3>Lunar Items</h3></td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/bismuthFlaskIcon.png?raw=true" width=128></td>
			<td>
				<b>Bismuth Tonic</b><br>
				Reduce duration of debuffs... <i>BUT also reduce duration of buffs.</i>
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/mountainTokenIcon.png?raw=true" width=128></td>
			<td>
				<b>Celestial Gambit</b><br>
				Gain an extra item reward from teleporters... <i>BUT jumping for too long gives the item to enemies instead.</i>
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/concentratingAlembicIcon.png?raw=true" width=128></td>
			<td>
				<b>Concentrating Alembic</b><br>
				Gain debuff strength and duration... <i>BUT lose reach.</i>
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/healsToDamageIcon.png?raw=true" width=128></td>
			<td>
				<b>Hydroponic Cell</b><br>
				Some healed health grows a plant which boosts base stats... <i>BUT you don't receive the converted healing.</i>
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/waxFeatherIcon.png?raw=true" width=128></td>
			<td>
				<b>Wax Feather</b><br>
				Staying airborne ignites your attacks and reduces gravity... <i>BUT also weakens your armor and speed.</i>
			</td>
		</tr>
		<tr>
			<td colspan="2" align="center"><h3>Lunar Equipments</h3></td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/EMPIcon.png?raw=true" width=128></td>
			<td>
				<b>EMP Device</b><br>
				Disable skills on enemies... <i>BUT disable non-primary skills on survivors.</i> <small>[Cooldown: 60 s]</small>
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/unstableBombIcon.png?raw=true" width=128></td>
			<td>
				<b>Faulty Mortar Tube</b><br>
				Throw a bomb that will detonate when damaged... <i>BUT it may damage survivors too.</i> <small>[Cooldown: 40 s]</small>
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/monkeysPawIcon.png?raw=true" width=128></td>
			<td>
				<b>Lemurian's Claw</b><br>
				Open a chest for free and drop an extra item... <i>BUT living enemies also receive items.</i> <small>[Cooldown: 120 s]</small>
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/compassIcon.png?raw=true" width=128></td>
			<td>
				<b>Silver Compass</b><br>
				Shows you a path... <i>BUT it will be fraught with danger.</i> <small>[Cooldown*: 180 s]</small>
			</td>
		</tr>
		<tr>
			<td colspan="2" align="center"><h3>Void Items</h3><h4>(requires Survivors of the Void DLC)</h4></td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/brambleRingIcon.png?raw=true" width=128></td>
			<td>
				<b>Bramble Band</b> (T2)<br>
				Taking damage creates a thorny barrier. Corrupts all Negative Feedback Loops.
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/gupRayIcon.png?raw=true" width=128></td>
			<td>
				<b>Gup Ray</b> (T3)<br>
				Split enemies into two much weaker copies. Corrupts all Shrink Rays.
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/orderedArmorIcon.png?raw=true" width=128></td>
			<td>
				<b>Lens of Order</b> (T2)<br>
				Gain massive armor by focusing your item build. Corrupts all Chestplates.
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/nautilusIcon.png?raw=true" width=128></td>
			<td>
				<b>Nautilus Protocol</b> (T3)<br>
				All turrets and drones gain flat armor and regen, and a slight damage bonus. Ping to detonate. Corrupts all RC Controllers.
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/obsidianBroochIcon.png?raw=true" width=128></td>
			<td>
				<b>Obsidian Brooch</b> (T1)<br>
				Chance to spread DoTs on hit. Corrupts all Triskelion Brooches.
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/timelostRumIcon.png?raw=true" width=128></td>
			<td>
				<b>Timelost Rum</b> (T1)<br>
				Chance to cause temporal echoes of attacks. Corrupts all Sturdy Mugs.
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/loomIcon.png?raw=true" width=128></td>
			<td>
				<b>Unraveling Loom</b> (T3)<br>
				All your attacks become progressively slower and more powerful. Corrupts all Spacetime Skeins.
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/enterCombatDamageIcon.png?raw=true" width=128></td>
			<td>
				<b>Villainous Visage</b> (T1)<br>
				Briefly stealth for a damage boost after killing powerful opponents. Corrupts all Macho Moustaches.
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/voidwispHiveIcon.png?raw=true" width=128></td>
			<td>
				<b>Voidwisp Hive</b> (T2)<br>
				Drop damaging wisp allies on using skills. Corrupts all Pixie Tubes.
			</td>
		</tr>
	</tbody>
</table>

### Mod Content: Skill Variants

<table>
	<thead>
		<tr>
			<th>Icon</th>
			<th>Name/Description</th>
		</tr>
	</thead>
	<tbody>
		<tr>
			<td colspan="2" align="center"><h3>Commando</h3></td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/CommandoPrimaryPulse.png?raw=true" width=128></td>
			<td>
				<b>Pulse</b> (Primary)<br>
				Rapidly shoot an enemy 4 times with high recoil. Damage per shot ramps from 75% to 150% over the course of the burst.
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/CommandoUtilityJinkJet.png?raw=true" width=128></td>
			<td>
				<b>Jink Jet</b> (Utility)<br>
				Perform a small jet-assisted horizontal jump in your aim direction. Hold up to 3.
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/CommandoSpecialPlasmaGrenade.png?raw=true" width=128></td>
			<td>
				<b>Plasma Grenade</b> (Special)<br>
				<i>Ignite</i>. Throw a sticky grenade with very-close-range homing that explodes for 500% damage. Can hold up to 2. <small>Watch your aim near low walls.</small>
			</td>
		</tr>
		<tr>
			<td colspan="2" align="center"><h3>Huntress</h3></td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/HuntressPrimaryBombArrow.png?raw=true" width=128></td>
			<td>
				<b>MK7b Rockeye Mini</b> (Primary)<br>
				Agile. Quickly fire a non-seeking arrow which sticks for 100% damage, then explodes for another 100% after a short delay. Both hits trigger on-hit effects.
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/HuntressSecondaryBola.png?raw=true" width=128></td>
			<td>
				<b>Laser Bola</b> (Secondary)<br>
				Throw a seeking hard-light net which slows and pulls groups of targets, and deals 300% damage over time.
			</td>
		</tr>
		<tr>
			<td colspan="2" align="center"><h3>Engineer</h3></td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/EngiPrimaryFlak.png?raw=true" width=128></td>
			<td>
				<b>Smart Flak</b> (Primary)<br>
				Continuously fire proximity fragmentation shells. Direct hits deal 50% damage. Shrapnel tracks enemies for up to 8x25% damage (maximum 5x25% on a single target).
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/EngiSecondaryChaff.png?raw=true" width=128></td>
			<td>
				<b>Decoy Chaff</b> (Secondary)<br>
				Deal 4x75% damage and clear enemy projectiles in a frontal cone. Struck enemies within line of sight of any of your turrets will be Taunted by a turret for 6 seconds.
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/EngiUtilitySpeedispenser.png?raw=true" width=128></td>
			<td>
				<b>Speed Dispenser</b> (Utility)<br>
				Deploy a stationary decanter that stores up to 4 delicious, caffeinated, precision-brewed charges of +50% speed while sprinting and +25% jump height. Charges last 15 seconds; restores 1 charge every 7.5 seconds.
			</td>
		</tr>
	</tbody>
</table>

### Mod Content: Interactables

<table>
	<thead>
		<tr>
			<th>Icon</th>
			<th>Name/Description</th>
		</tr>
	</thead>
	<tbody>
		<tr>
			<td colspan="2" align="center"><h3>Drones</h3></td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/ItemDroneIcon.png?raw=true" width=128></td>
			<td>
				<b>Item Drone</b><br>
				Give items to share them with allies near the drone.
			</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/BulwarkDroneIcon.png?raw=true" width=128></td>
			<td>
				<b>Bulwark Drone</b><br>
				Enemies near the drone will attack it more often. Has high health and innate shield and armor.
			</td>
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
		</tr>
	</thead>
	<tbody>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/tactics_on.png?raw=true" width=128></td>
			<td><b>Artifact of Tactics</b></td>
			<td>All combatants give nearby teammates small, stacking boosts to speed, damage, and armor.</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/antiair_on.png?raw=true" width=128></td>
			<td><b>Artifact of Suppression</b></td>
			<td>Players take heavily increased damage while airborne.</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/butterknife_on.png?raw=true" width=128></td>
			<td><b>Artifact of Haste</b></td>
			<td>All combatants attack 10x faster and deal 1/20x damage.</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/danger_on.png?raw=true" width=128></td>
			<td><b>Artifact of Danger</b></td>
			<td>Players can be killed in one hit.</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/delayitems_on.png?raw=true" width=128></td>
			<td><b>Artifact of Safekeeping</b></td>
			<td>All item drops are taken and guarded by the teleporter boss, which will explode in a shower of loot when killed.</td>
		</tr>
		<tr>
			<td><img src="https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/ModMeta/Assets/DisposableEquip_on.png?raw=true" width=128></td>
			<td><b>Artifact of Reconfiguration</b></td>
			<td>Start with 3 equipment slots. Equipment is more common, and is consumed instead of going on cooldown.</td>
		</tr>
	</tbody>
</table>

### Mod Content: Other

<table>
	<thead>
		<tr>
			<th>Name</th>
			<th>Description</th>
		</tr>
	</thead>
	<tbody>
		<tr>
			<td><b>Equipment Drone Labels</b></td>
			<td>If enabled, this module will apply the same naming scheme that Item Drones have ("Item Drone (colored name of item)") to vanilla Equipment Drones ("Equipment Drone (colored name of equipment)").</td>
		</tr>
		<tr>
			<td><b>Moddable Equipment Slot Max Charges Patch</b></td>
			<td>This module causes `Inventory.GetEquipmentSlotMaxCharges`, which is normally only referenced by UI code, to also affect the actual max stock of each equipment slot of an inventory. Dependency of some mod content; Stamina Bar may not work correctly if disabled.</td>
		</tr>
		<tr>
			<td><b>Curses Keep OSP</b></td>
			<td>This module causes One-Shot Protection to be kept while cursed (e.g. Shaped Glass). Disabled by default.</td>
		</tr>
	</tbody>
</table>

## Issues/TODO

- Stamina Bar max charges change, and maybe the relevant patch, is very broken.
- Pulse Grenade causes console log spam, magnetism may be broken or undertuned.
- Fudge Dice needs an indicator buff.
- Taunt and Float debuffs need custom icons.
- ItemDisplayRule incompleteness:
	- A few TkSat items have absent or incomplete ItemDisplayRules.
	- Non-Survivor vanilla characters have no display rules for TkSat items.
- Most items need some effects & model polish in general.
	- Chestplate texture has unintentional specularity in some areas.
	- Broken drones are missing smoke/sparks effects.
	- Cardboard Box alternate icon has not been updated to vanilla style like other icons as of v2.2.3.
- Some class-specific item behaviors on Go-Faster Stripes are missing or placeholders.
- Mimic usually displays a count of 0 in chat pickup announcements; might also not count towards logbook stat tracker.
- See the GitHub repo for more!

## Changelog

The 5 latest updates are listed below. For a full changelog, see: https://github.com/ThinkInvis/RoR2-TinkersSatchel/blob/master/changelog.md

(🌧︎: Involves an accepted GitHub Pull Request or other significant assistance from the community. Thanks for your help!)

**5.2.0**

- Recompiled, remapped signatures, and updated dependencies for recent vanilla updates.
	- Fixed Pulse Monitor unintentionally consuming equipment charges.
- Fixed Old-War Lidar still working and applying damage even after the last item stack is lost.
- Spacetime Skein's attack and move speed bonuses can now be configured separately.
- Removed the Knockback Fin Float tweak module (vanilla item behavior was changed and the tweak behavior no longer really fits).
- Balance changes:
	- Prevented self-damage/debuff by default on the following items: Old-War Lidar, Macho Moustache, Negative Feedback Loop, Unstable Klein Bottle, Swordbreaker, Shrink Ray, Obsidian Brooch, Villainous Visage, Bramble Band.
		- Self-damage/debuff on these items is now configurable.
	- Major nerf to Old-War Lidar.
		- Attack frequency: 0.33/sec -> 0.25/sec.
		- Attack damage: 40% -> 20%.
		- Attack proc coefficient: 50% -> 35%.
		- *On closer inspection, this was kind of outperforming Ukelele, a similar Uncommon item.*
	- Moderate nerf to Spacetime Skein.
		- Attack speed bonus: +50% -> +30%.
		- Move speed bonus: +50% -> +40%.
- Balance-like item bugfixes:
	- Allowed Luck stat (57-Leaf Clover and Fudge Dice) to affect the following items: Wax Feather, Macho Moustache, Triskelion Brooch, Unstable Klein Bottle, Pinball Wizard, Obsidian Brooch.
		- Notably, this does not include Sturdy Mug nor Timelost Rum.
	- Sturdy Mug and Swordbreaker now use proper conic spread (was square).
	- Applied missing damage source information to all skills and Lodestone equipment.
		- Allows Luminous Shot to work on all 3 primary skills.
	- Excluded some buffs/debuffs from Growth Nectar and/or Noxious Thorn.
- Fixed a formatting issue in the Sturdy Mug description.
- Some backend improvements:
	- Suppressed a harmless (but very annoying) warning on a dependency.
	- Made BepInEx dependency version static.
	- Postbuild event can now create the Build folder and project name subfolder if missing (e.g. on a new clone of the repo).

**5.1.1**

- Fixed missing orig calls causing several vanilla items (Bustling Fungus, Warbanner, Mercurial Rachis, Effigy of Grief, Interstellar Deskplant, maybe others) to not function.

**5.1.0**

- Added inspect text to broken Item Drones and Bulwark Drones.
- Improved performance of Bulwark Drone, Celestial Gambit, EMP Device, Cardboard Box, and Lodestone by introducing component caching.
- Added the Knockback Fin Float module.
	- Reworks the SotS item Knockback Fin to use Unstable Klein Bottle's effect, minus the stun and plus some damage.
	- Now works on bosses and other unstunnable targets, inflicting damage only.
	- Slightly lowered proc chance by default.
- Separated Unstable Klein Bottle's effect into a new standalone debuff, Float.
	- Float holds a character in a fixed spot in the air for some time, then inflicts damage and flings the character downwards.
	- Additional stacks of Float cause previous ones to expire immediately, but the previous stack's timer is kept if it's still longer than the new duration.
	- Unstable Klein Bottle now inflicts stun as a separate effect.
	- Float, and by extension the Unstable Klein Bottle effect, can now be cleansed.
	- Significantly improved smoothness of midair hold animation.
- Pulse Monitor now triggers on-equipment-use items (e.g. Bottled Chaos, War Horn).
- Percussive Maintenance now counts equipment stashed in Scavenger's Rucksack for its unlock achievement.
- Removed an old debug log from a Quantum Recombobulator hook, which probably wasn't even showing up due to using Unity's logger.

**5.0.1**

- Small patch for incorrect language token argument count on Negative Feedback Loop description.

**5.0.0** *The Grinds My Gears Update*

- BREAKING:
	- Compat classes are now Internal. *Nobody should have been depending on these anyways, but major version increment it is.*
	- DelayedDamageBufferComponent was renamed and reworked to DelayedBarrierComponent.
- Fixes for Seekers of the Storm:
	- All achievements now have appropriate lunar coin rewards.
	- Causal Camera overlay now uses the new TemporaryOverlayManager system.
	- Fixed Scavenger's Rucksack activating equipment just before changing slots (activation is supposed to be suppressed while switching).
	- Fixed Pinball Wizard not working and causing errors on raycast bullet attacks.
	- Fixed Unstable Mortar Tube projectiles exploding immediately when fired, and then also on impact with anything.
	- Added EffectHelperComponent to projectile ghosts (fixes invisible projectiles).
	- Updated dependencies for new patch.
	- Retargeted changed hook signatures (fixes errors preventing mod load).
- Balance pass:
	- Unstable Mortar Tube projectiles now have 300 non-scaling health instead of 1 (to reduce the chances of them blowing up in your face immediately, at least early-game).
	- Stamina Bar now provides moderately more speed while grounded.
	- Reworked Negative Feedback Loop.
		- New behavior: grants barrier over time in response to damage taken, and having barrier multiplies effectiveness of regen stat based on barrier fraction.
		- Old behavior: converts some damage into a healable DoT.
		- *This item became redundant with a new item in SotS, Warped Echo.*
- Balance-like item bugfixes:
	- Percussive Maintenance can now crit-heal with Defibrillator.
	- Nautilus Protocol now works on Bulwark and Item Drones by default.
	- Silver Compass can no longer be activated during or after the Teleporter event.
	- Hurdy-Gurdy no longer works on Secondary skills with no cooldown.
	- Hurdy-Gurdy now works on specific configured skills, even if not Secondary (works with Railgunner's non-alternate Primary while scoped, by default).
- Removed a debug log that was inadvertently left in Kintsugi item count calculation.
- Migrated the PreventCurseWhileOff config from Artifact of Danger to its own tweak module, CurseKeepOSP.
- Fixed Hurdy-Gurdy using character forward (no vertical component) instead of aim forward.
- Fixed a hook subscription leak in Sturdy Mug (should have had minimal-to-no effect, unless repeatedly disabling and enabling the item in ingame config hundreds of times in one session).
- Project-wide code and comment cleanup.
	- Implemented some C#9 features made available by SotS.
	- Removed a bunch of TODO comments. *This is what we have GitHub issues and/or a separate private text file for.*
	- Removed some dead code and unnecessary using directives.
	- Suppressed some compiler messages.
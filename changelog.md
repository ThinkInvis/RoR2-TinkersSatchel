# Tinker's Satchel Changelog

(🌧︎: Involves an accepted GitHub Pull Request from the community. Thanks for your help!)

**3.0.0**

- BREAKING CHANGES:
	- Kintsugi valid items are now configurable as a list of item name tokens.
		- REMOVED `public static (int t1, int t2, int t3plus) GetConsumedItemCountByTier(Inventory inventory)`.
		- Added `public static Dictionary<ItemTier, int> GetConsumedItemCountByTier(Inventory inventory)`.
		- Added `public string validItemNameTokens {get; private set;}` config property.
		- Added `public static bool GetIsItemValid(ItemDef item)` public API method.
	- Sturdy Mug and Timelost Rum now use ints instead of bools to track ignore state.
		- Fixes hitscan projectiles repeatedly firing forever under some circumstances (e.g. when fired at Newt).
		- REMOVED `public bool ignoreMugs = false;` from both classes.
		- Added `public int ignoreStack = 0;` to both classes.
- Balance pass:
	- H3AD-53T now counts as a melee attack for purposes of Extendo-Arms.
	- Causal Camera now has a configurable minimum time (default 3s), to make Gesture stacking while you have one less existentially hazardous.
- Command Terminal will no longer summon Item Drone or Bulwark Drone if they are disabled.
- If Command Terminal is allowed to work with Bottled Chaos in config (disabled by default), it will no longer remove other equipments when triggered by Bottled Chaos.
- Timelost Rum now properly spawns projectiles at a muzzle point, instead of at the owner's core position (could lead to self-collision if stationary).
- Improved an error message on Defibrillator to match newer, similar systems in Sturdy Mug and Timelost Rum.
- Added some calculation shortcuts to Extendo-Arms public API (`public static float GetRangeMultiplier(CharacterBody body)`, `public static float GetDamageMultiplier(CharacterBody body)`).
- Addressed some compiler messages and removed a duplicate state flag from Timelost Rum (reduced code complexity; should have minimal user-facing effect).
- Updated libraries for latest Risk of Rain 2 patch.

**2.3.3**

- Improved stability of recently added Old-War Lidar VFX.
- Fixed Unraveling Loom corrupting Macho Moustache instead of Spacetime Skein.
- 🌧︎ Fixed Silver Compass allowing 1 extra application above the configured limit.

**2.3.2**

- Balance pass:
	- Extendo-Arms now works on PBAoE attacks (any blast attack within 5 m, range configurable).
	- Buffed default Extendo-Arms stats (damage 5% --> 6.25%, range 8% --> 12.5%).
	- Quantum Recombobulator can no longer be used on fallen drones.
	- Silver Compass no longer applies an extra, no-item-drop stack of Challenge of the Mountain (restorable with config).
		- This was a balance point from back when the teleporter was much harder to find.
- Old-War Lidar now has an in-UI state indicator per enemy (can be disabled in config).
- Fixed Command Terminal using the cooldown of the next equipment to be picked up.
- Fixed a missing unhook on Old-War Lidar.

**2.3.1** *The Time for Crab Mini-Update*

- ADDED ITEM: Extendo-Arms!
- Removed inadvertent color grading on all item icons.
- Improved/fixed descriptions of Macho Moustache, Defibrillator, and Unstable Klein Bottle.
- Removed compatibility for a deprecated+removed Classic Items Ancient Scepter method.
- Fixed console warning spam caused by Pixie Tube pickups.

**2.3.0** *The I Finally Got My Hands on the DLC for Testing Update, Part 2*

- ADDED ITEMS: Kintsugi, Timelost Rum, Unraveling Loom, Obsidian Brooch!
	- Adding the first 3 of these to the readme early was totally an intentional teaser and *not* a Git mishap at all I swear <_< >_>
- Slightly improved shader on Voidwisp Hive (now uses fresnel emission). Still needs some tweaking.

**2.2.3**

- VFX pass:
	- Updated ALL item models to use HG shaders instead of default Unity shaders.
	- Updated most item icons with ingame renders + a much more vanilla-faithful outline generator.
		- Cardboard Box "in use" alternate icon has not yet been updated.
- Voidwisp Hive no longer targets neutral team.
- Removed an unneeded debug log from Voidwisp Hive.
- Removed some old UnlockableAPI code from Macho Moustache. Should have no user-facing effect.
- Updated R2API dependency to 4.3.21.

**2.2.2**

- Slightly expanded the definition of "musical instrument" for the Percussive Maintenance achievement.
- Increased null safety of Pulse Monitor achievement check.
- Artifact of Safekeeping should no longer bug out if another mod destroys a tracked item droplet.
- Fixed Gup Ray unintentionally working on the SotV final boss.

**2.2.1**

- Fixed achievements/unlockables (now uses new vanilla system instead of R2API.UnlockableAPI).

**2.2.0** *The Classic Update*

- Implemented Classic Items Embryo/Scepter integration for all relevant equipments/skills added by this mod.
	- Affected equipments: Lodestone, Cardboard Box, Quantum Recombobulator, Command Terminal, Causal Camera.
	- Affected skills: Commando: Plasma Grenade.
- Commando: Jink Jet now has Go-Faster Stripes support.
- Go-Faster Stripes now displays a red stripe overlay over the utility skill in UI.
- Fixed a broken config (ArmorAmtBase) on Villainous Visage.
- Added soft dependency on Classic Items to ensure correct load order.

**2.1.0** *The Non-Optional Options Update*

- Implemented Risk Of Options support on ALL items/equipments, via new AutoConfig attributes in TILER2 7.
- Possible fix for characters and projectiles not appearing for network clients.
- Added an alternate mode for Commando: Pulse that removes its recoil in exchange for needing to land consecutive hits to ramp up damage bonus.
- Added a config option to Unstable Klein Bottle to invert its melee character whitelist.
- Bramble Ring now deals less damage to players (-75% by default).
- Bramble Ring no longer unnecessarily recalculates stats when its DamageFrac config is changed.
- Silver Compass now uses an enum for its use limit config, and exposes an additional config for use count (no longer strictly limited to 1).
- Fixed Villainous Visage ArmorAmtBase config unintentionally being an int instead of a float.
- Made all Lunar Equipments unselectable by Artifact of Enigma.
- Updated for latest Risk of Rain 2 version.
- Updated TILER2 dependency to 7.0.1.

**2.0.0** *The I Finally Got My Hands on the DLC for Testing Update, Part 1*

- ADDED ITEMS: Voidwisp Hive, Bramble Ring, Gup Ray!
- BREAKING CHANGES:
	- Villainous Visage internal code name has changed (VoidMoustache --> EnterCombatDamage).
		- Component name has also changed (VoidMoustacheDamageTracker --> EnterCombatDamageTracker).
	- Armor Prism internal code name has changed (VoidGoldenGear --> OrderedArmor).
		- Component name has also changed (VoidGoldenGearComponent --> OrderedArmorComponent).
	- These changes make these items much easier to address with TILER2 NetConfig commands and breaks this pattern for future items. Previously, full name e.g. 'Items.Moustache' would have to be used to resolve ambiguity with the void variant name.
- Go-Faster Stripes now gives a +50% speed buff for 3 seconds when an unhandled utility skill is used.
	- Other mods can indicate that they have handling for a utility skill set up by adding their SkillDefs to `GoFaster.instance.handledSkillDefs`.
- All Void items now have the expansion icon overlay on their item icons.
- All Void items now have required expansion correctly specified and should no longer appear in logbook while unavailable.
- Fixed missing buff icons on Villainous Visage.
- Fixed a minor punctuation typo in Villainous Visage pickup text.
- Fixed a missing hook removal during Pixie Tube uninstall that could cause Engineer skills to multiproc unintentionally.
- Pixie Tube handling for Engineer skills now respects the item's internal cooldown.

**1.14.1**
- Balance pass.
	- Commando: Jink Jet:
		- Allowed slightly more vertical component to jump trajectory (0.125 --> 0.15).
		- Increased forced vertical component used while grounded (0.25 --> 0.3).
	- Commando: Plasma Grenade:
		- Buffed cooldown to match Frag Grenade (8s --> 6s).
		- Homing now has a brief arming delay (0.5s) to prevent nearby enemies from intercepting throws. Direct impacts will still stick.
	- Macho Moustache:
		- Nerfed default elite stacking bonus from +2 to +1.
		- Nerfed default champion stacking bonus from +4 to +2.
		- This item was performing too well in lategame; the unlock achievement was also far easier to trigger than intended.
- Fixed Quantum Recombobulator not checking its valid object name list while selecting an object to recombobulate into.
	- The barrels have been banished for good this time. Definitely.
	- In an extreme edge case (no spawncards available at all), this could also have led to a vanishing object or an exception. Equipment should now fail to activate, as intended, in this case.
- Fixed Commando: Plasma Grenade potentially slowing to a crawl during homing.
- Slightly improved Commando: Plasma Grenade VFX/SFX:
	- Fuse ray effects are slightly thinner and no longer disconnected from the model.
	- Made sound effect tempo slower to make it more distinct from Sticky Bomb.
- Sturdy Mug can no longer cause multiple Goobos Jr. to spawn per equipment cast.
- Quantum Recombobulator no longer adds the Cardboard Box player data storage component on equip (had no player-facing effect other than an infinitesimal amount of performance loss).

**1.14.0** *The Skillful Update, Part Commando*

- ADDED SKILL VARIANTS:
	- Commando: Pulse, Jink Jet, and Plasma Grenade!
- Changed Taunt debuff icon to a slightly more fitting vanilla one.
- Fixed Taunt keyword token not resolving.
- General behind-the-scenes cleanup and Visual Studio Intellisense message resolution.

**1.13.2**

- Balance pass.
	- Shrink Ray no longer procs on self damage.
		- This was unintentional, but can be considered a balance change as some base game items proc on self damage.
	- Partially reworked Hydroponic Cell's mechanics to be simpler.
		- While it provided some flavor, the 'nutrients' buff stage gained by taking damage was redundant, and provided a balance lever that was also redundant. This has been removed and reduced to the core purpose of requiring you to heal health (i.e. NOT overheal, barrier, etc.) to gain the item's benefit. Flavor has been relocated to the pickup text.
- Improved descriptions of Hydroponic Cell, Engineer: Smart Flak, and Engineer: Decoy Chaff.
- Clarified Macho Moustache config descriptions.
- Fixed skills appearing in the loadout menu while disabled.
- Command Terminal now exposes its list of summonable drones to config.
- Added extra null safety to Sturdy Mug.
	- Specifically, affected projectiles should no longer cause further errors if something causes the networked prefab list to desync in multiplayer.

**1.13.1**

- Balance pass.
	- RC Controller: buffed default attack speed bonus (25% &rarr; 40%).
	- Engineer: Speed Dispenser:
		- Now provides a 25% jump height bonus.
		- Now holds 4 charges (up from 3).
		- Charges now recharge in 7.5 seconds (up from 10).
	- Engineer: Chaff:
		- Now deals 4x75% damage with 0.5 proc coefficient (was 1x200% damage with 1 proc coefficient).
		- Will cleanse projectiles and taunt enemies with every hit, making it *slightly* easier to time.
- Engineer: Chaff: Renamed to Decoy Chaff for clarity.
- Engineer: Speed Dispenser fixes:
	- Fixed not being set to player team.
	- Now has its own deployable slot so mods that tweak the max shield count won't affect it.

**1.13.0** *The Skillful Update, Part Engineer*

- ADDED SKILL VARIANTS:
	- Engineer: Smart Flak, Chaff, Speed Dispenser!
- Bulwark Drone now uses the same Taunt debuff added by Chaff (will now apply a damage debuff vs non-Taunters and allow attacking any Taunter).
- Updated an older language token name format on Silver Compass (all instances of "TINKSATCH" --> "TKSAT").
- Fixed Percussive Maintenance not working if used by multiplayer clients.
- Fixed some Mostly-Tame Mimic NREs during object creation/destruction.

**1.12.3**

- Fixed Unstable Klein Bottle pull-mode force being *much* higher than intended.
- Fixed missing style tags in Unstable Klein Bottle description.

**1.12.2**

- Balance pass.
	- Macho Moustache:
		- Now has a radius of 100 m (was 10 m).
		- Now provides a damage bonus of 1% per enemy per stack (was 5%).
		- Now only works on enemies in combat (used a skill recently) or danger (was hurt recently).
		- Now counts elites and champions/bosses as multiple enemies (default +2/+4).
		- Renamed existing config options to force changes, as this is effectively a major rework to further differentiate the item from Focus Crystal.
	- Negative Feedback Loop:
		- No longer triggers on-hurt items.
		- No longer applies to fall damage.
		- Now procs its DoT once per frame (effectively continuously) by default. This config was not forced to update; default 0.2s will still be in place for already-installed copies of the mod.
	- Percussive Maintenance:
		- Now also heals you when you heal an ally (for 25% the amount by default).
	- Unstable Klein Bottle:
		- Now pulls enemies instead of pushing if procced by a melee survivor. List of melee survivors is configurable (by body name).
	- RC Controller:
		- Nerfed first-stack fire rate to match per-extra-stack rate (100% --> 25%).
	- Quantum Recombobulator:
		- By default, can no longer work on, nor turn objects into, money barrels or Rusty Key boxes (latter did not work anyways).
- Percussive Maintenance now passes through proc chain masks instead of using an empty one.
- Phrasing tweak on Defibrillator description.
- Fixed Pinball Wizard unintentionally affecting deployables and Rex's Special skill. Now has a configurable projectile name blacklist.
- Quantum Recombobulator and Cardboard Box now expose their lists of valid object names to config.
	- WARNING: May have unintended results on some untested objects!
- Cardboard Box and Quantum Recombobulator now work on Broken Equipment Drones and Broken Item Drones, for real this time.

**1.12.1**

- Balance pass.
	- Hydroponic Cell now adds base damage instead of direct damage.
	- Hydroponic Cell now has a minimum hit threshold similar to the elemental bands (defaults to the same amount, 400%).
	- Unstable Klein Bottle now deals damage (50% by default).
	- Allowed Pixie Tube to work on some previously-blacklisted skills (MUL-T and Railgunner mode switches) now that an ICD is in place.
	- Reduced Bulwark Drone shield (100 --> 60) and armor (50 --> 20). Testing found it to be both too evasive and too tanky at once.
- Hydroponic Cell informational buffs are now percentages of max charge (was previously integer times max health / damage stat).
- Hydroponic Cell is now implemented as a separate instance of damage due to technical limitations on the minimum hit threshold.
- Improved AI on Item Drone and Equipment Drone. Slightly. They're still drones.
- Item Drone and Equipment Drone now use continuous collision detection (much lower chance of flying through walls).

**1.12.0** *The Science! Update, Part 3*

- ADDED INTERACTABLES: Bulwark Drone!
- ADDED ITEMS: Faulty Mortar Tube, Hydroponic Cell, EMP Device, Shrink Ray!
- Balance pass.
	- Buffed Item Drone radius (60 m --> 100 m).
	- Item Drone no longer accepts AI summon items like Queen's Gland, nor consumable/consumed items.
	- Mostly-Tame Mimic no longer mimics consumable items.
	- Pixie Tube now has a separate 3 second cooldown on each non-primary skill (addresses spammability on modded characters with mode swap skills).
	- Pixie Tube now triggers on equipment.
- Item Drone:
	- Now has a custom model and cleaner setup code (thanks ThunderKit!).
	- Improved spawncard compatibility (now properly registered/deregistered instead of setting spawn weight to 0).
- Cardboard Box and Quantum Recombobulator now work on Broken Equipment Drones, Broken Item Drones, and Casino Chests.

**1.11.3**

- Fixed Unstable Klein Bottle's unlock achievement not triggering.

**1.11.2**

- Fixed Item Drone interactables spawning while disabled.

**1.11.1**

- Added unlock achievements to Macho Moustache (+Villainous Visage), Old War Lidar, Unstable Klein Bottle, Defibrillator, Pinball Wizard, Spacetime Skein, Lodestone, and Quantum Recombobulator.
- Added a config to disable self-proc on Triskellion Brooch (self-proc is allowed by default).
- Fixed Quantum Recombobulator being usable twice on the same object if it recombobulates into a multishop.
- Artifact of Safekeeping:
	- Added a config option controlling what to display in chat when an item is taken (specific item name, item tier only, vague "something was taken", or nothing).
	- Now has a configurable announcement for items dropped when the boss dies (item tier counts, total item count, or nothing).
- All unlock achievement icons now display with the correct background (instead of no background).
- Updated TILER2 dependency to 6.3.0.

**1.11.0** *The Science! Update, Part 2*

- ADDED INTERACTABLES: Item Drone!
- ADDED ITEMS/EQUIPMENTS: Lodestone, Defibrillator!
- Balance pass.
	- Buffed Go-Faster Stripes' effect on Engineer's missiles (35 m/s --> 50 m/s).
	- Go-Faster Stripes blast-jumping on Engineer and Bandit now resets vertical velocity.
	- RC Controller now causes Equipment Drones to activate equipment.
	- Pixie Tube may now proc on primary skill once every 5 seconds (allows effects on characters with only a primary skill, e.g. some monsters).
- Artifact of Safekeeping:
	- Now displays a chat message for each item taken by the teleporter boss.
	- Vastly improved item drop trajectories (notably: much smaller chance to send items off cliffs).
		- Item droplets will now try to find clear trajectories towards open navigation graph nodes.
		- If not enough clear paths exist around the boss, will try scanning from the teleporter instead.
		- If both methods fail to find any nodes, items will launch in a fixed circle around the teleporter.
	- Hooks made more compatible with other mods.
- Fixed missing TILER2 AutoConfigUpdateActions on many items (changing some configs mid-game would not immediately update language and/or character stats).
- Updated R2API dependency to 4.3.5.
- Updated TILER2 dependency to 6.2.0.

**1.10.2**

- Fixed any and all game VFX going missing at random (caused by Pinball Wizard effect prefab waking up during game load).
- Go-Faster Stripes: self-targeted Engi missiles now properly explode (with a grenade explosion but we can't win 'em all).

**1.10.1**

- Spacetime Skein now has a percentage buff indicating its ramp-up progress.
- Added a few greebles to Old War Lidar.
- UV-mapped and baked Old War Lidar multi-material into a single material (experimenting with workflow, will apply to other items later).
- Fixed Spacetime Skein almost never detecting movement.
- Quantum Recombobulator no longer works on interactables that have already been used/are in use.
- Command Terminal now displays a chat message when it revives someone.
- Command Terminal now exposes its list of valid random drones for other mods to add to (`ReviveOnce.instance.droneMasterPrefabs.Add(GameObject prefab);`).
- Fixed an uncommon, mostly-harmless NRE in Quantum Recombobulator and Cardboard Box.

**1.10.0** *The Science! Update, Part 1*

- ADDED ITEMS/EQUIPMENTS: Motion Tracker, Spacetime Skein, Command Terminal, Quantum Recombobulator!
- Cardboard Box now works on Shrines of the Mountain, Cleansing Pools, Shrines of Order, Altars of Gold, and Explosive Tar Pots.
- Cardboard Box and Silver Compass can no longer be triggered by Bottled Chaos.
- Causal Camera can now be triggered by Bottled Chaos.

**1.9.6**

- Fixed Artifact of Tactics not working and spamming NREs per frame.
- Added extra null safety in a couple other places.
- Major rework of Thunderstore readme page layout.
	- We got tables!
	- And icons!
	- And lists! But less of these than before actually!

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
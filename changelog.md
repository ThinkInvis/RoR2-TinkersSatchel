# Tinker's Satchel Changelog

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
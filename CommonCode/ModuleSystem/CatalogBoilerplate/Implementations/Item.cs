using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace ThinkInvisible.TinkersSatchel {
    public abstract class Item<T>:Item where T : Item<T> {
        public static T instance {get;private set;}

        public Item() {
            if(instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBoilerplate/Item was instantiated twice");
            instance = this as T;
        }
    }

    public abstract class Item : CatalogBoilerplate {
        public override string configCategoryPrefix => "Items.";

        public ItemIndex catalogIndex { get {
                if(itemDef == null) {
                    TinkersSatchelPlugin._logger.LogError($"Module Item {name} has null ItemDef! Cannot retrieve ItemIndex");
                    return ItemIndex.None;
                }
                return itemDef.itemIndex;
            }}
        public ItemDef itemDef {get; private set;}
        public CustomItem customItem {get; private set;}

        public abstract ItemTier itemTier {get; }

        [AutoConfigRoOString()]
        [AutoConfig("The internal name of this item for use in other config entries. No effect if changed; will be reset on game launch.")]
        public virtual string configNameInternal { get; protected set; } = null;

        [AutoConfigRoOString()]
        [AutoConfig("The name token of this item for use in other config entries. No effect if changed; will be reset on game launch.")]
        public virtual string configNameToken { get; protected set; } = null;

        [AutoConfigRoOCheckbox()]
        [AutoConfig("If true, the item will not be given to enemies by Evolution nor in the arena map, and it will not be found by Scavengers.")]
        public virtual bool itemIsAIBlacklisted {get;protected set;} = false;

        public virtual ReadOnlyCollection<ItemTag> itemTags {get; private set;}
        protected ItemDisplayRuleDict displayRules = new();

        public override void SetupConfig() {
            base.SetupConfig();
            ConfigEntryChanged += (sender, args) => {
                if(args.target.boundProperty.Name == nameof(enabled)) {
                    var runIsActive = Run.instance != null && Run.instance.enabled;
                    if(runIsActive)
                        Run.instance.BuildDropTable();
                } else if(args.target.boundProperty.Name == nameof(itemIsAIBlacklisted)) {
                    var hasAIB = itemDef.tags.Contains(ItemTag.AIBlacklist);

                    if(hasAIB && !itemIsAIBlacklisted) {
                        itemDef.tags = itemDef.tags.Where(tag => tag != ItemTag.AIBlacklist).ToArray();
                    } else if(!hasAIB && itemIsAIBlacklisted) {
                        var nl = itemDef.tags.ToList();
                        nl.Add(ItemTag.AIBlacklist);
                        itemDef.tags = nl.ToArray();
                    }
                }
            };
        }

        public override void SetupAttributes() {
            base.SetupAttributes();
            
            var _itemTags = new List<ItemTag>(itemTags);
            if(itemIsAIBlacklisted) _itemTags.Add(ItemTag.AIBlacklist);
            var iarr = _itemTags.ToArray();
            itemDef = ScriptableObject.CreateInstance<ItemDef>();
            itemDef.name = modInfo.shortIdentifier + name;
            itemDef.tier = itemTier;
            itemDef.pickupModelPrefab = modelResource;
            itemDef.pickupIconSprite = iconResource;
            itemDef.nameToken = this.nameToken + "_RENDERED";
            itemDef.pickupToken = this.pickupToken + "_RENDERED";
            itemDef.descriptionToken = this.descToken + "_RENDERED";
            itemDef.loreToken = this.loreToken + "_RENDERED";
            itemDef.tags = iarr;

            SetupModifyItemDef();

            itemTags = Array.AsReadOnly(iarr);
            customItem = new CustomItem(itemDef, displayRules);

            ItemAPI.Add(customItem);

            ItemCatalog.availability.CallWhenAvailable(this.SetupCatalogReady);
        }

        public override void SetupCatalogReady() {
            base.SetupCatalogReady();
            var ce1 = bindings.Find(x => x.boundProperty.Name == nameof(configNameInternal)).configEntry;
            ce1.BoxedValue = this.itemDef.name;
            if(!ce1.ConfigFile.SaveOnConfigSet) ce1.ConfigFile.Save();
            var ce2 = bindings.Find(x => x.boundProperty.Name == nameof(configNameToken)).configEntry;
            ce2.BoxedValue = this.itemDef.nameToken;
            if(!ce2.ConfigFile.SaveOnConfigSet) ce2.ConfigFile.Save();
        }

        public virtual void SetupModifyItemDef() { }

        public override void Install() {
            base.Install();
            itemDef.pickupIconSprite = iconResource;
        }

        public override void Uninstall() {
            base.Uninstall();
            itemDef.pickupIconSprite = CatalogBoilerplateModule.lockIcon;
        }

        [Obsolete("Use .GetCountEffective or .GetCountPermanent instead.")]
        public int GetCount(Inventory inv) {
            return (inv == null) ? 0 : inv.GetItemCount(catalogIndex);
        }
        [Obsolete("Use .GetCountEffective or .GetCountPermanent instead.")]
        public int GetCount(CharacterMaster chrm) {
            if(!chrm || !chrm.inventory) return 0;
            return chrm.inventory.GetItemCount(catalogIndex);
        }
        [Obsolete("Use .GetCountEffective or .GetCountPermanent instead.")]
        public int GetCount(CharacterBody body) {
            if(!body || !body.inventory) return 0;
            return body.inventory.GetItemCount(catalogIndex);
        }
        public int GetCountEffective(Inventory inv) {
            return (inv == null) ? 0 : inv.GetItemCountEffective(catalogIndex);
        }
        public int GetCountEffective(CharacterMaster chrm) {
            if(!chrm || !chrm.inventory) return 0;
            return chrm.inventory.GetItemCountEffective(catalogIndex);
        }
        public int GetCountEffective(CharacterBody body) {
            if(!body || !body.inventory) return 0;
            return body.inventory.GetItemCountEffective(catalogIndex);
        }
        public int GetCountPermanent(Inventory inv) {
            return (inv == null) ? 0 : inv.GetItemCountPermanent(catalogIndex);
        }
        public int GetCountPermanent(CharacterMaster chrm) {
            if(!chrm || !chrm.inventory) return 0;
            return chrm.inventory.GetItemCountPermanent(catalogIndex);
        }
        public int GetCountPermanent(CharacterBody body) {
            if(!body || !body.inventory) return 0;
            return body.inventory.GetItemCountPermanent(catalogIndex);
        }
        [Obsolete("Use .GetCountOnDeployablesEffective or .GetCountOnDeployablesPermanent instead.")]
        public int GetCountOnDeployables(CharacterMaster master) {
            if(master == null) return 0;
            var dplist = master.deployablesList;
            if(dplist == null) return 0;
            int count = 0;
            foreach(DeployableInfo d in dplist) {
                count += GetCount(d.deployable.gameObject.GetComponent<Inventory>());
            }
            return count;
        }
        public int GetCountOnDeployablesEffective(CharacterMaster master) {
            if(master == null) return 0;
            var dplist = master.deployablesList;
            if(dplist == null) return 0;
            int count = 0;
            foreach(DeployableInfo d in dplist) {
                count += GetCountEffective(d.deployable.gameObject.GetComponent<Inventory>());
            }
            return count;
        }
        public int GetCountOnDeployablesPermanent(CharacterMaster master) {
            if(master == null) return 0;
            var dplist = master.deployablesList;
            if(dplist == null) return 0;
            int count = 0;
            foreach(DeployableInfo d in dplist) {
                count += GetCountPermanent(d.deployable.gameObject.GetComponent<Inventory>());
            }
            return count;
        }

        public override ConsoleStrings GetConsoleStrings() {
            return new ConsoleStrings {
                className = "Item",
                objectName = this.name,
                formattedIndex = ((int)this.catalogIndex).ToString()
            };
        }
    }
}

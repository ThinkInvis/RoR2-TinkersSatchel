using R2API;
using RoR2;
using System;
using UnityEngine;

namespace ThinkInvisible.TinkersSatchel {
    public abstract class Artifact<T>:Artifact where T : Artifact<T> {
        public static T instance {get;private set;}

        public Artifact() {
            if(instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting CatalogBoilerplate/Artifact was instantiated twice");
            instance = this as T;
        }
    }

    public abstract class Artifact : CatalogBoilerplate {
        public override string configCategoryPrefix => "Artifacts.";
        protected override string GetLoreString(string langID = null) => null;
        protected override string GetPickupString(string langID = null) => null;

        public Sprite iconResourceDisabled {get; protected set;} = null;

        public ArtifactIndex catalogIndex => artifactDef.artifactIndex;
        public ArtifactDef artifactDef {get; private set; }

        [AutoConfigRoOString()]
        [AutoConfig("The internal name of this artifact for use in other config entries. No effect if changed; will be reset on game launch.")]
        public virtual string configNameInternal { get; protected set; } = null;

        [AutoConfigRoOString()]
        [AutoConfig("The name token of this artifact for use in other config entries. No effect if changed; will be reset on game launch.")]
        public virtual string configNameToken { get; protected set; } = null;

        public override void SetupConfig() {
            base.SetupConfig();
            ConfigEntryChanged += (sender, args) => {
                if(args.target.boundProperty.Name == nameof(enabled)) {
                    if(args.oldValue != args.newValue) {
                        if((bool)args.newValue == true) {
                            artifactDef.descriptionToken = descToken;
                            artifactDef.smallIconDeselectedSprite = iconResourceDisabled;
                            artifactDef.smallIconSelectedSprite = iconResource;
                        } else {
                            artifactDef.descriptionToken = "TKSAT_DISABLED_ARTIFACT_DESCRIPTION";
                            artifactDef.smallIconDeselectedSprite = LegacyResourcesAPI.Load<Sprite>("textures/miscicons/texUnlockIcon");
                            artifactDef.smallIconSelectedSprite = LegacyResourcesAPI.Load<Sprite>("textures/miscicons/texUnlockIcon");
                        }
                    }
                }
            };
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            artifactDef = ScriptableObject.CreateInstance<ArtifactDef>();
            artifactDef.nameToken = nameToken + "_RENDERED";
            artifactDef.descriptionToken = descToken + "_RENDERED";
            artifactDef.smallIconDeselectedSprite = iconResourceDisabled;
            artifactDef.smallIconSelectedSprite = iconResource;

            SetupModifyArtifactDef();

            ContentAddition.AddArtifactDef(artifactDef);

            ArtifactCatalog.availability.CallWhenAvailable(this.SetupCatalogReady);
        }

        public override void SetupCatalogReady() {
            base.SetupCatalogReady();
            var ce1 = bindings.Find(x => x.boundProperty.Name == nameof(configNameInternal)).configEntry;
            ce1.BoxedValue = this.artifactDef.cachedName;
            if(!ce1.ConfigFile.SaveOnConfigSet) ce1.ConfigFile.Save();
            var ce2 = bindings.Find(x => x.boundProperty.Name == nameof(configNameToken)).configEntry;
            ce2.BoxedValue = this.artifactDef.nameToken;
            if(!ce2.ConfigFile.SaveOnConfigSet) ce2.ConfigFile.Save();
        }

        public virtual void SetupModifyArtifactDef() {}

        public override void Install() {
            base.Install();
            artifactDef.smallIconDeselectedSprite = iconResourceDisabled;
            artifactDef.smallIconSelectedSprite = iconResource;
        }

        public override void Uninstall() {
            base.Uninstall();
            artifactDef.smallIconDeselectedSprite = CatalogBoilerplateModule.lockIcon;
            artifactDef.smallIconSelectedSprite = CatalogBoilerplateModule.lockIcon;
        }

        public bool IsActiveAndEnabled() {
            return enabled && RunArtifactManager.instance && RunArtifactManager.instance.IsArtifactEnabled(catalogIndex);
        }

        public override ConsoleStrings GetConsoleStrings() {
            return new ConsoleStrings {
                className = "Artifact",
                objectName = this.name,
                formattedIndex = ((int)this.catalogIndex).ToString()
            };
        }
    }
}

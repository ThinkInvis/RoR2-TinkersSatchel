using BepInEx.Configuration;
using RoR2;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace ThinkInvisible.TinkersSatchel {
    public class AutoConfigBinding {
        internal readonly static List<AutoConfigBinding> instances = new();
        internal readonly static Dictionary<AutoConfigBinding, (object, bool)> stageDirtyInstances = new();
        internal readonly static Dictionary<AutoConfigBinding, object> runDirtyInstances = new();

        internal static void CleanupDirty(bool isRunEnd) {
            TinkersSatchelPlugin._logger.LogDebug($"Stage ended; applying {stageDirtyInstances.Count} deferred config changes...");
            foreach(AutoConfigBinding k in stageDirtyInstances.Keys) {
                k.DeferredUpdateProperty(stageDirtyInstances[k].Item1, stageDirtyInstances[k].Item2);
            }
            stageDirtyInstances.Clear();
            if(isRunEnd) {
                TinkersSatchelPlugin._logger.LogDebug($"Run ended; applying {runDirtyInstances.Count} deferred config changes...");
                foreach(AutoConfigBinding k in runDirtyInstances.Keys) {
                    k.DeferredUpdateProperty(runDirtyInstances[k], true);
                }
                runDirtyInstances.Clear();
            }
        }

        public AutoConfigContainer owner { get; internal set; }
        public object target { get; internal set; }
        public ConfigEntryBase configEntry { get; internal set; }
        public PropertyInfo boundProperty { get; internal set; }
        public string modName { get; internal set; }

        public AutoConfigUpdateActionsAttribute updateEventAttribute { get; internal set; }

        public MethodInfo propGetter { get; internal set; }
        public MethodInfo propSetter { get; internal set; }
        public Type propType { get; internal set; }

        public object boundKey { get; internal set; }
        public bool onDict { get; internal set; }

        public bool allowConCmd { get; internal set; }

        public object cachedValue { get; internal set; }

        internal bool isOverridden = false;

        public enum DeferType {
            UpdateImmediately, WaitForNextStage, WaitForRunEnd, NeverAutoUpdate
        }
        public DeferType deferType { get; internal set; }

        public string readablePath {
            get { return $"{modName}/{configEntry.Definition.Section}/{configEntry.Definition.Key}"; }
        }

        internal AutoConfigBinding() {
            instances.Add(this);
        }

        ~AutoConfigBinding() {
            if(instances.Contains(this))
                instances.Remove(this);
        }

        internal void OverrideProperty(object newValue, bool silent = false) {
            if(!isOverridden) runDirtyInstances[this] = cachedValue;
            isOverridden = true;
            UpdateProperty(newValue, silent);
        }

        private void DeferredUpdateProperty(object newValue, bool silent = false) {
            var oldValue = propGetter.Invoke(target, onDict ? new[] { boundKey } : new object[] { });
            propSetter.Invoke(target, onDict ? new[] { boundKey, newValue } : new[] { newValue });
            var flags = updateEventAttribute?.flags ?? AutoConfigUpdateActionTypes.None;
            if(updateEventAttribute?.ignoreDefault == false) flags |= owner.defaultEnabledUpdateFlags;
            cachedValue = newValue;
            owner.OnConfigChanged(new AutoConfigUpdateActionEventArgs {
                flags = flags,
                oldValue = oldValue,
                newValue = newValue,
                target = this,
                silent = silent });
        }

        internal void UpdateProperty(object newValue, bool silent = false) {
            if(deferType == DeferType.UpdateImmediately || Run.instance == null || !Run.instance.enabled) {
                DeferredUpdateProperty(newValue, silent);
            } else if(deferType == DeferType.WaitForNextStage) {
                AutoConfigBinding.stageDirtyInstances[this] = (newValue, silent);
            } else if(deferType == DeferType.WaitForRunEnd) {
                AutoConfigBinding.runDirtyInstances[this] = newValue;
            } else {
                TinkersSatchelPlugin._logger.LogWarning($"Something attempted to set the value of an AutoConfigBinding with the DeferForever flag: \"{readablePath}\"");
            }
        }

        public static (List<AutoConfigBinding> results, string errorMsg) FindFromPath(string path1, string path2, string path3) {
            var p1u = path1.ToUpper();
            var p2u = path2?.ToUpper();
            var p3u = path3?.ToUpper();

            List<AutoConfigBinding> matchesLevel1 = new(); //no enforced order, no enforced caps, partial matches
            List<AutoConfigBinding> matchesLevel2 = new(); //enforced order, no enforced caps, partial matches
            List<AutoConfigBinding> matchesLevel3 = new(); //enforced order, no enforced caps, full matches
            List<AutoConfigBinding> matchesLevel4 = new(); //enforced order, enforced caps, full matches

            AutoConfigBinding.instances.ForEach(x => {
                if(!x.allowConCmd) return;

                var name = x.configEntry.Definition.Key;
                var nameu = name.ToUpper();
                var cat = x.configEntry.Definition.Section;
                var catu = cat.ToUpper();
                var mod = x.modName;
                var modu = mod.ToUpper();

                if(path2 == null) {
                    //passed 1 part; could be mod, cat, or name
                    if(nameu.Contains(p1u)
                    || catu.Contains(p1u)
                    || modu.Contains(p1u)) {
                        matchesLevel1.Add(x);
                        matchesLevel2.Add(x);
                    } else return;
                    if(nameu == p1u)
                        matchesLevel3.Add(x);
                    else return;
                    if(name == path1)
                        matchesLevel4.Add(x);
                } else if(path3 == null) {
                    //passed 2 parts; could be mod/cat, mod/name, or cat/name
                    //enforced order only matches mod/cat or cat/name
                    var modMatch1u = modu.Contains(p1u);
                    var catMatch1u = catu.Contains(p1u);
                    var catMatch2u = catu.Contains(p2u);
                    var nameMatch2u = nameu.Contains(p2u);
                    if((modMatch1u && catMatch2u) || (catMatch1u && nameMatch2u) || (modMatch1u && nameMatch2u))
                        matchesLevel1.Add(x);
                    else return;

                    if(!(modMatch1u && nameMatch2u))
                        matchesLevel2.Add(x);
                    else return;

                    var modMatch1 = mod.Contains(path1);
                    var catMatch1 = cat.Contains(path1);
                    var catMatch2 = cat.Contains(path2);
                    var nameMatch2 = name.Contains(path2);

                    if((modMatch1 && catMatch2) || (catMatch1 && nameMatch2))
                        matchesLevel3.Add(x);
                    else return;

                    var modExact1 = mod == path1;
                    var catExact1 = cat == path1;
                    var catExact2 = cat == path2;
                    var nameExact2 = name == path2;

                    if((modExact1 && catExact2) || (catExact1 && nameExact2))
                        matchesLevel4.Add(x);
                } else {
                    //passed 3 parts; must be mod/cat/name
                    if(nameu.Contains(p3u)
                    && catu.Contains(p2u)
                    && modu.Contains(p1u)) {
                        matchesLevel1.Add(x);
                        matchesLevel2.Add(x);
                    } else return;
                    if(modu == p3u && catu == p2u && nameu == p1u)
                        matchesLevel3.Add(x);
                    else return;
                    if(mod == path3 && cat == path2 && name == path1)
                        matchesLevel4.Add(x);
                }
            });

            if(matchesLevel1.Count == 0) return (null, "no level 1 matches");
            else if(matchesLevel1.Count == 1) return (matchesLevel1, null);

            if(matchesLevel2.Count == 0) return (matchesLevel1, "multiple level 1 matches, no level 2 matches");
            else if(matchesLevel2.Count == 1) return (matchesLevel2, null);

            if(matchesLevel3.Count == 0) return (matchesLevel2, "multiple level 2 matches, no level 3 matches");
            else if(matchesLevel3.Count == 1) return (matchesLevel3, null);

            if(matchesLevel4.Count == 0) return (matchesLevel3, "multiple level 3 matches, no level 4 matches");
            else if(matchesLevel4.Count == 1) return (matchesLevel4, null);
            else {
                Debug.LogError($"AutoConfig: There are multiple config entries with the path \"{matchesLevel4[0].readablePath}\"; this should never happen! Please report this as a bug.");
                return (matchesLevel4, "multiple level 4 matches");
            }
        }
    }
}

using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using RiskOfOptions;
using BepInEx.Configuration;
using RiskOfOptions.Options;
using System.Text.RegularExpressions;
using System.Collections;
using System;
using System.Linq;

namespace GiGaGonsItemBlacklister
{
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInDependency("com.rune580.riskofoptions")]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class GiGaGonsItemBlacklister : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "GiGaGon";
        public const string PluginName = "GiGaGonsItemBlacklister";
        public const string PluginVersion = "1.0.0";

        public static ConfigFile BlacklistConfig = new(Paths.ConfigPath + "\\GiGaGon.ItemBlacklister.cfg", true);
        public void Awake()
        {
            On.RoR2.RoR2Application.OnLoad += AfterLoad;
            On.RoR2.Run.Start += (orig, self) =>
            {
                orig(self);
                foreach (ItemDef item in ItemCatalog.allItemDefs)
                {
                    string name = RemoveIllegalChars(GetEnglishFromToken(item.nameToken));
                    if (name == "") continue;
                    BlacklistConfig.TryGetEntry<bool>(item.tier.ToString(), name, out ConfigEntry<bool> state);
                    MakeChanges(item, state.Value);
                }
            };
        }

        public static IEnumerator AfterLoad(On.RoR2.RoR2Application.orig_OnLoad orig, RoR2Application self)
        {
            yield return orig(self);

            foreach (ItemTier itemTier in Enum.GetValues(typeof(ItemTier)))
            {
                ModSettingsManager.AddOption(new CheckBoxOption(BlacklistConfig.Bind(itemTier.ToString(), "_", true, "Does nothing but help order the tabs")));
            }

            foreach (ItemDef item in ItemCatalog.allItemDefs)
            {
                string name = RemoveIllegalChars(GetEnglishFromToken(item.nameToken));
                if (name == "") continue;

                ConfigEntry<bool> config = BlacklistConfig.Bind(item.tier.ToString(), name, true, GetEnglishFromToken(item.descriptionToken));
                config.SettingChanged += (a, args) =>
                {
                    MakeChanges(item, config.Value);
                };
                ModSettingsManager.AddOption(new CheckBoxOption(config));
            }
        }

        public static void MakeChanges(ItemDef item, bool state)
        {
            if (Run.instance)
            {
                PickupIndex index = PickupCatalog.FindPickupIndex(item.itemIndex);
                if (state)
                {
                    Run.instance.EnablePickupDrop(index);
                }
                else
                {
                    Run.instance.DisablePickupDrop(index);
                }
            }
        }

        public static string RemoveIllegalChars(string input)
        {
            return Regex.Replace(input, @"[^\w]", "");
        }

        public static string GetEnglishFromToken(string input)
        {
            return Language.english.GetLocalizedStringByToken(input);
        }

        public static void LogWrap(params string[] input)
        {
            Debug.Log($"{PluginName} - {string.Join(", ", input)}");
        }
    }
}

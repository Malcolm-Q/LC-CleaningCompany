using HarmonyLib;
using LethalLib.Modules;
using static LethalLib.Modules.Items;
using System.Linq;
using System;

namespace CleaningCompany.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch("Awake")]
        static void RemoveScrap(StartOfRound __instance)
        {
            foreach (SelectableLevel level in __instance.levels)
            {
                var name = level.name;

                if (Enum.IsDefined(typeof(Levels.LevelTypes), name))
                {
                    var levelEnum = (Levels.LevelTypes)Enum.Parse(typeof(Levels.LevelTypes), name);
                    foreach(SpawnableItemWithRarity item in level.spawnableScrap)
                    {
                        if(item.spawnableItem.isScrap)
                        {
                            item.rarity = 0;
                        }
                    }
                }
            }
        }
    }
}

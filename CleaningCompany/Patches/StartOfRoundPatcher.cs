using GameNetcodeStuff;
using HarmonyLib;
using LethalLib.Modules;
using System;
using UnityEngine;

namespace CleaningCompany.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch("Awake")]
        static void RemoveScrap(StartOfRound __instance)
        {
            foreach (SelectableLevel level in __instance.levels) //prevent all scrap ffrom spawning
            {
                var name = level.name;

                if (Enum.IsDefined(typeof(Levels.LevelTypes), name))
                {
                    var levelEnum = (Levels.LevelTypes)Enum.Parse(typeof(Levels.LevelTypes), name);
                    foreach (SpawnableItemWithRarity item in level.spawnableScrap)
                    {
                        if (item.spawnableItem.isScrap)
                        {
                            item.rarity = 0;
                        }
                    }
                }
            }
            //change into janitor suit
            foreach (UnlockableItem suit in __instance.unlockablesList.unlockables)
            {
                if (suit.suitMaterial != null && suit.alreadyUnlocked)
                {
                    suit.suitMaterial.mainTexture = Plugin.instance.janitorMat;
                    break;
                }
            }
            foreach (PlayerControllerB player in GameObject.FindObjectsOfType<PlayerControllerB>())
            {
                player.thisPlayerModel.material.mainTexture = Plugin.instance.janitorMat;
                player.thisPlayerModelLOD1.material.mainTexture = Plugin.instance.janitorMat;
                player.thisPlayerModelLOD2.material.mainTexture = Plugin.instance.janitorMat;
                player.thisPlayerModelArms.material.mainTexture = Plugin.instance.janitorMat;
            }
        }
    }
}

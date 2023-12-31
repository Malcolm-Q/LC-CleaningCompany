using GameNetcodeStuff;
using HarmonyLib;
using LethalLib.Modules;
using System;
using System.Collections;
using Unity.Netcode;
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
            if (!Plugin.cfg.SCRAP_SPAWN)
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
            }
            //change into janitor suit
            if (Plugin.cfg.JANITOR_SUIT)
            {
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
            if (Plugin.cfg.JANITOR_VA)
            {
                __instance.zeroDaysLeftAlertSFX = Plugin.instance.zeroDays;
                __instance.firedVoiceSFX = Plugin.instance.firedSFX;
                __instance.shipIntroSpeechSFX = Plugin.instance.introSFX;
            }
            __instance.StartCoroutine(SpawnCabinet(__instance));
        }
        private static IEnumerator SpawnCabinet(StartOfRound __instance)
        {
            yield return new WaitForSeconds(0.5f);
            if (GameObject.Find("TestCabinet(Clone)") == null)
            {
                GameObject cabinet = null;
                foreach (UnlockableItem unlockable in __instance.unlockablesList.unlockables)
                {
                    if (unlockable.unlockableName == "Cleaning Cabinet")
                    {
                        cabinet = unlockable.prefabObject;
                        break;
                    }
                }
                GameObject terminal = GameObject.Find("Terminal");
                GameObject cab = GameObject.Instantiate(cabinet, terminal.transform.parent);
                cab.GetComponent<NetworkObject>().Spawn();
            }
        }
    }
}

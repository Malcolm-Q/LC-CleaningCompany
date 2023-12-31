using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace CleaningCompany.Patches
{
    [HarmonyPatch(typeof(EnemyAI))]
    internal class EnemyAIPatcher
    {
        private static ulong currentEnemy = 9999999;
        [HarmonyPrefix]
        [HarmonyPatch("KillEnemyServerRpc")]
        static void SpawnScrapBody(EnemyAI __instance)
        {
            if (currentEnemy == __instance.NetworkObject.NetworkObjectId) return;
            currentEnemy = __instance.NetworkObject.NetworkObjectId;
            string name = __instance.enemyType.enemyName;
            if (Plugin.instance.BodySpawns.ContainsKey(name))
            {
                GameObject go = GameObject.Instantiate(Plugin.instance.BodySpawns[name].spawnPrefab, __instance.transform.position + Vector3.up, Quaternion.identity);
                go.GetComponent<NetworkObject>().Spawn();
                __instance.gameObject.SetActive(false);
            }

        }
    }
}

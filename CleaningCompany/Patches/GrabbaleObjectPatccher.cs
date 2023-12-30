using CleaningCompany.Monos;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace CleaningCompany.Patches
{
    [HarmonyPatch(typeof(GrabbableObject))]
    internal class GrabbableObjectPatcher
    {
        [HarmonyPrefix]
        [HarmonyPatch("SetScrapValue")]
        static bool ReturnFalseGaming(GrabbableObject __instance)
        {
            if (__instance.GetComponent<MessItem>() != null) return false;
            return true;
        }
    }
}

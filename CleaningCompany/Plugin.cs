﻿using BepInEx;
using HarmonyLib;

namespace LethalCompanyTemplate
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency("evaisa.lethallib","0.6.0")]
    public class Plugin : BaseUnityPlugin
    {
        readonly Harmony harmony = new Harmony(GUID);
        const string GUID = "malco.cleaning_company";
        const string NAME = "Cleaning Company";
        const string VERSION = "0.0.1";

        Assetbundle bundle;


        void Awake()
        {
            // we're gonna need netcode
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }

            string assetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "CleaningAssets");
            bundle = AssetBundle.LoadFromFile(assetDir);

            SetupItems();
            SetupScrap();

            Logger.LogInfo($"Cleaning Company is patched!");
        }


        void SetupItems()
        {
            /* EXAMPLE
            Item broom = bundle.loadAsset<Item>("Assets/CleaningAssets/Broom.asset");
            CleaningToolScript broomScript = Broom.AddComponent<CleaningToolScript>();
            broomScript.toolName = "Broom";

            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(broom.spawnPrefab);
            TerminalNode node = ScriptableObject.CreateInstance<TerminalNode>();
            node.displayText = "You need this to clean up dusty messes.\n\n";
            Items.RegisterShopItem(broom, null, null, node, 0);
            */
        }

        void SetupScrap()
        {
            /* EXAMPLE
            Item garbageBag = bundle.loadAsset<Item>("Assets/CleaningAssets/GarbageBag.asset");
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(garbageBag.spawnPrefab);

            // load and and apply cleaning sfx too.

            Item brackenDust = bundle.loadAsset<Item>("Assets/CleaningAssets/BroomScrap.asset");
            MessScript brackScript = scrap.AddComponent<MessScript>();
            brackScript.itemProperties = brackenDust;
            brackScript.toolName = "Broom";
            brackScript.lootSpawn = garbageBag.spawnPrefab;
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(brackenDust.spawnPrefab);
            Items.RegisterScrap(brackenDust,20,Levels.LevelTypes.ALL);
            */
        }
    }
}
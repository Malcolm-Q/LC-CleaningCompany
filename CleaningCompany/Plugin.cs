using BepInEx;
using CleaningCompany.Monos;
using HarmonyLib;
using LethalLib.Extras;
using LethalLib.Modules;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace CleaningCompany
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency("evaisa.lethallib", "0.6.0")]
    public class Plugin : BaseUnityPlugin
    {
        readonly Harmony harmony = new Harmony(GUID);
        const string GUID = "malco.cleaning_company";
        const string NAME = "Cleaning Company";
        const string VERSION = "0.0.1";

        static string root = "Assets/CleaningAssets/";

        Dictionary<string,AnimationCurve> bodyPaths = new Dictionary<string, AnimationCurve>()
        {
            { root+"ScavItem.asset", new AnimationCurve(new Keyframe(0,1), new Keyframe(1,4)) },
            { root+"HoarderItem.asset", new AnimationCurve(new Keyframe(0,0), new Keyframe(0.35f,0), new Keyframe(1,2)) },
            { root+"SpiderItem.asset", new AnimationCurve(new Keyframe(0,0), new Keyframe(0.50f,0), new Keyframe(1,2)) },
            { root+"ThumperItem.asset", new AnimationCurve(new Keyframe(0,0), new Keyframe(0.75f,0), new Keyframe(1,2)) },
            { root+"CentipedeItem.asset", new AnimationCurve(new Keyframe(0,0), new Keyframe(0.25f,0), new Keyframe(1,3)) },
        };

        Dictionary<string, string> messPaths = new Dictionary<string, string>()
        {
            { root+"MessLootTest.asset", "mop" },
        };

        AssetBundle bundle;
        public Texture2D janitorMat;
        public static Plugin instance;

        void Awake()
        {
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

            instance = this;

            string assetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "cleaningassets");
            bundle = AssetBundle.LoadFromFile(assetDir);
            janitorMat = bundle.LoadAsset<Texture2D>("Assets/CleaningAssets/JanitorOutfit.png");

            SetupItems();
            SetupScrap();

            harmony.PatchAll();
            Logger.LogInfo($"Cleaning Company is patched!");
        }


        void SetupItems() // this could go in a for loop but I mean it's not getting any bigger
        {
            //mop
            Item mop = bundle.LoadAsset<Item>("Assets/CleaningAssets/MopItem.asset");
            ToolItem tool = mop.spawnPrefab.AddComponent<ToolItem>();
            mop.itemSpawnsOnGround = true;
            tool.toolType = "mop";
            tool.grabbable = true;
            tool.grabbableToEnemies = true;
            tool.itemProperties = mop;

            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(mop.spawnPrefab);
            TerminalNode node = ScriptableObject.CreateInstance<TerminalNode>();
            node.displayText = "You need this to clean up wet messes.\n\n";
            Items.RegisterShopItem(mop, null, null, node, 0);

            //broom
            Item broom = bundle.LoadAsset<Item>("Assets/CleaningAssets/BroomItem.asset");
            ToolItem broomTool = broom.spawnPrefab.AddComponent<ToolItem>();
            broom.itemSpawnsOnGround = true;
            broomTool.toolType = "broom";
            broomTool.grabbable = true;
            broomTool.grabbableToEnemies = true;
            broomTool.itemProperties = broom;

            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(broom.spawnPrefab);
            TerminalNode broomNode = ScriptableObject.CreateInstance<TerminalNode>();
            broomNode.displayText = "You need this to clean up loose messes.\n\n";
            Items.RegisterShopItem(broom, null, null, broomNode, 0);

            //garbage bag box
            Item garb = bundle.LoadAsset<Item>("Assets/CleaningAssets/GarbageBox.asset");
            ToolItem garbTool = garb.spawnPrefab.AddComponent<ToolItem>();
            garb.itemSpawnsOnGround = true;
            garbTool.toolType = "garbage";
            garbTool.grabbable = true;
            garbTool.grabbableToEnemies = true;
            garbTool.itemProperties = garb;

            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(garb.spawnPrefab);
            TerminalNode garbNode = ScriptableObject.CreateInstance<TerminalNode>();
            garbNode.displayText = "You need this to clean up piles of solid objects.\n\n";
            Items.RegisterShopItem(garb, null, null, garbNode, 0);

            //duster // this is actually gonna be a vauum now
            Item dust = bundle.LoadAsset<Item>("Assets/CleaningAssets/DusterItem.asset");
            ToolItem dustTool = dust.spawnPrefab.AddComponent<ToolItem>();
            dust.itemSpawnsOnGround = true;
            dustTool.toolType = "vacuum";
            dustTool.grabbable = true;
            dustTool.grabbableToEnemies = true;
            dustTool.itemProperties = dust;

            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(dust.spawnPrefab);
            TerminalNode dustNode = ScriptableObject.CreateInstance<TerminalNode>();
            dustNode.displayText = "You need this to clean up small piles of fine dust.\n\n";
            Items.RegisterShopItem(dust, null, null, dustNode, 0);
        }

        void SetupScrap()
        {
            Dictionary<string, AudioClip> sfx = new Dictionary<string, AudioClip>();
            AudioClip mopSound = bundle.LoadAsset<AudioClip>("Assets/CleaningAssets/mop.mp3");

            sfx.Add("mop", mopSound);

            Dictionary<string, Item> trash = new Dictionary<string, Item>();
            Item trashTest = bundle.LoadAsset<Item>("Assets/CleaningAssets/MessLootTest.asset");

            trash.Add("mop", trashTest);


            foreach(KeyValuePair<string,string> bundleData in messPaths)
            {
                Item item = bundle.LoadAsset<Item>(bundleData.Key);
                MessItem mess = item.spawnPrefab.AddComponent<MessItem>();

                mess.loot = trash[bundleData.Value].spawnPrefab;
                mess.toolType = bundleData.Value;
                mess.cleanNoise = sfx[bundleData.Value];

                LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(item.spawnPrefab);
                Items.RegisterScrap(item, 100, Levels.LevelTypes.All);
            }

            foreach(KeyValuePair<string, AnimationCurve> pair in bodyPaths)
            {
                Item body = bundle.LoadAsset<Item>(pair.Key);
                body.spawnPrefab.AddComponent<BodySyncer>();
                LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(body.spawnPrefab);
                SpawnableMapObjectDef mapObjDef = ScriptableObject.CreateInstance<SpawnableMapObjectDef>();
                mapObjDef.spawnableMapObject = new SpawnableMapObject();
                mapObjDef.spawnableMapObject.prefabToSpawn = body.spawnPrefab;
                MapObjects.RegisterMapObject(mapObjDef, Levels.LevelTypes.All,(level) => pair.Value);
                Items.RegisterItem(body);
            }
        }
    }
}
using BepInEx;
using BepInEx.Configuration;
using CleaningCompany.Misc;
using CleaningCompany.Monos;
using HarmonyLib;
using LethalLib.Extras;
using LethalLib.Modules;
using System.Collections.Generic;
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
        const string VERSION = "1.0.0";

        static string root = "Assets/CleaningAssets/";


        Dictionary<string, int> minBodyValues;
        Dictionary<string, int> maxBodyValues;
        Dictionary<string, float> bodyWeights;

        Dictionary<string, AnimationCurve> bodyPaths;
        Dictionary<string,string> pathToName = new Dictionary<string, string>()
        {
            { root+"ScavItem.asset", "IgnoreThis" },
            { root+"HoarderItem.asset", "Hoarding bug" },
            { root+"SpiderItem.asset", "Bunker Spider" },
            { root+"ThumperItem.asset", "Crawler" },
            { root+"CentipedeItem.asset", "Centipede"},
            { root+"BrackenDustItem.asset", "Flowerman"},
        };

        Dictionary<string, string> messPaths = new Dictionary<string, string>()
        {
            { root+"ThumperDroolItem.asset", "mop" },
            { root+"SporePileItem.asset", "vacuum" },
            { root+"EtherealItem.asset", "vacuum" },
            { root+"ScavGoopItem.asset", "mop" },
            { root+"BrackenDustItem.asset", "broom"},
            { root+"NailPileItem.asset", "broom"},
            { root+"HoardingEggItem.asset", "garbage"},
            { root+"BonesItem.asset", "garbage"},
        };

        public Dictionary<string, Item> BodySpawns = new Dictionary<string, Item>();
        public List<GameObject> tools = new List<GameObject>();

        AssetBundle bundle;
        public Texture2D janitorMat;
        public static Plugin instance;
        public AudioClip open, close;
        public AudioClip zeroDays;
        public AudioClip firedSFX;
        public AudioClip introSFX;

        public static PluginConfig cfg { get; private set; }

        void Awake()
        {
            cfg = new(base.Config);
            cfg.InitBindings();


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

            open =  bundle.LoadAsset<AudioClip>("Assets/CleaningAssets/open.ogg");
            close =  bundle.LoadAsset<AudioClip>("Assets/CleaningAssets/close.ogg");
            zeroDays =  bundle.LoadAsset<AudioClip>("Assets/CleaningAssets/VA/zeroDays.mp3");
            firedSFX =  bundle.LoadAsset<AudioClip>("Assets/CleaningAssets/VA/fired.mp3");
            introSFX =  bundle.LoadAsset<AudioClip>("Assets/CleaningAssets/VA/intro.mp3");

            ApplyConfig();
            SetupItems();
            SetupScrap();

            harmony.PatchAll();
            Logger.LogInfo($"Cleaning Company is patched!");
            Logger.LogInfo(string.Join(", ",BodySpawns.Keys));
        }

        void ApplyConfig()
        {
            bodyWeights = new Dictionary<string, float>()
            {
                { root+"ScavItem.asset", cfg.SCAV_WEIGHT },
                { root+"HoarderItem.asset", cfg.HOARDER_WEIGHT },
                { root+"SpiderItem.asset", cfg.SPIDER_WEIGHT },
                { root+"ThumperItem.asset", cfg.THUMPER_WEIGHT },
                { root+"CentipedeItem.asset", cfg.CENTIPEDE_WEIGHT }
            };

            maxBodyValues = new Dictionary<string, int>()
            {
                { root+"ScavItem.asset", cfg.SCAV_MAX },
                { root+"HoarderItem.asset", cfg.HOARDER_MAX },
                { root+"SpiderItem.asset", cfg.SPIDER_MAX },
                { root+"ThumperItem.asset", cfg.THUMPER_MAX },
                { root+"CentipedeItem.asset", cfg.CENTIPEDE_MAX }
            };

            minBodyValues = new Dictionary<string, int>()
            {
                { root+"ScavItem.asset", cfg.SCAV_MIN },
                { root+"HoarderItem.asset", cfg.HOARDER_MIN },
                { root+"SpiderItem.asset", cfg.SPIDER_MIN },
                { root+"ThumperItem.asset", cfg.THUMPER_MIN },
                { root+"CentipedeItem.asset", cfg.CENTIPEDE_MIN }
            };

            bodyPaths = new Dictionary<string, AnimationCurve>()
            {
                { root+"ScavItem.asset", new AnimationCurve(new Keyframe(0,1), new Keyframe(cfg.SCAV_CURVE,0), new Keyframe(1,cfg.MAX_SCAVS)) },
                { root+"HoarderItem.asset", new AnimationCurve(new Keyframe(0,0), new Keyframe(cfg.HOARDER_CURVE,0), new Keyframe(1,cfg.MAX_HOARDERS)) },
                { root+"SpiderItem.asset", new AnimationCurve(new Keyframe(0,0), new Keyframe(cfg.SPIDER_CURVE,0), new Keyframe(1,cfg.MAX_SPIDERS)) },
                { root+"ThumperItem.asset", new AnimationCurve(new Keyframe(0,0), new Keyframe(cfg.THUMPER_CURVE,0), new Keyframe(1,cfg.MAX_THUMPERS)) },
                { root+"CentipedeItem.asset", new AnimationCurve(new Keyframe(0,0), new Keyframe(cfg.CENTIPEDE_CURVE,0), new Keyframe(1,cfg.MAX_CENTIPEDES)) },
            };
        }

        void SetupItems() // this could go in a for loop but I mean it's not getting any bigger
        {
            //mop
            Item mop = bundle.LoadAsset<Item>("Assets/CleaningAssets/MopItem.asset");
            ToolItem tool = mop.spawnPrefab.AddComponent<ToolItem>();
            mop.spawnPrefab.AddComponent<AudioMixerFixer>();
            mop.itemSpawnsOnGround = true;
            mop.canBeGrabbedBeforeGameStart = true;
            tool.toolType = "mop";
            tool.grabbable = true;
            tool.grabbableToEnemies = true;
            tool.itemProperties = mop;
            Items.RegisterItem(mop);

            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(mop.spawnPrefab);
            tools.Add(mop.spawnPrefab);

            //broom
            Item broom = bundle.LoadAsset<Item>("Assets/CleaningAssets/BroomItem.asset");
            ToolItem broomTool = broom.spawnPrefab.AddComponent<ToolItem>();
            broom.spawnPrefab.AddComponent<AudioMixerFixer>();
            broom.itemSpawnsOnGround = true;
            broom.canBeGrabbedBeforeGameStart = true;
            broomTool.toolType = "broom";
            broomTool.grabbable = true;
            broomTool.grabbableToEnemies = true;
            broomTool.itemProperties = broom;
            Items.RegisterItem(broom);

            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(broom.spawnPrefab);
            tools.Add(broom.spawnPrefab);

            //garbage bag box
            Item garb = bundle.LoadAsset<Item>("Assets/CleaningAssets/GarbageBox.asset");
            ToolItem garbTool = garb.spawnPrefab.AddComponent<ToolItem>();
            garb.spawnPrefab.AddComponent<AudioMixerFixer>();
            garb.itemSpawnsOnGround = true;
            garb.canBeGrabbedBeforeGameStart = true;
            garbTool.toolType = "garbage";
            garbTool.grabbable = true;
            garbTool.grabbableToEnemies = true;
            garbTool.itemProperties = garb;
            Items.RegisterItem(garb);

            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(garb.spawnPrefab);
            tools.Add(garb.spawnPrefab);

            //duster // this is actually gonna be a vauum now
            Item dust = bundle.LoadAsset<Item>("Assets/CleaningAssets/DusterItem.asset");
            ToolItem dustTool = dust.spawnPrefab.AddComponent<ToolItem>();
            dust.spawnPrefab.AddComponent<AudioMixerFixer>();
            dust.canBeGrabbedBeforeGameStart = true;
            dust.itemSpawnsOnGround = true;
            dustTool.toolType = "vacuum";
            dustTool.grabbable = true;
            dustTool.grabbableToEnemies = true;
            dustTool.itemProperties = dust;
            Items.RegisterItem(dust);

            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(dust.spawnPrefab);
            tools.Add(dust.spawnPrefab);

            // cleaning cabinet
            UnlockablesList list = bundle.LoadAsset<UnlockablesList>("Assets/CleaningAssets/TestUnlocks.asset");
            list.unlockables[0].prefabObject.AddComponent<CabinetScript>();
            list.unlockables[0].prefabObject.AddComponent<AudioMixerFixer>();
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(list.unlockables[0].prefabObject);
            Unlockables.RegisterUnlockable(list.unlockables[0],0,StoreType.None);
        }

        void SetupScrap()
        {
            Dictionary<string, AudioClip> sfx = new Dictionary<string, AudioClip>();
            AudioClip mopSound = bundle.LoadAsset<AudioClip>("Assets/CleaningAssets/mop.mp3");
            AudioClip vacSound = bundle.LoadAsset<AudioClip>("Assets/CleaningAssets/vac.mp3");
            AudioClip sweepSound = bundle.LoadAsset<AudioClip>("Assets/CleaningAssets/sweep.mp3");
            AudioClip garbSound = bundle.LoadAsset<AudioClip>("Assets/CleaningAssets/garb.mp3");

            sfx.Add("mop", mopSound);
            sfx.Add("vacuum", vacSound);
            sfx.Add("broom", sweepSound);
            sfx.Add("garbage", garbSound);

            Dictionary<string, Item> trash = new Dictionary<string, Item>();
            Item trashBag = bundle.LoadAsset<Item>("Assets/CleaningAssets/GarbageBagItem.asset");
            trashBag.spawnPrefab.AddComponent<AudioMixerFixer>();
            Item bucket = bundle.LoadAsset<Item>("Assets/CleaningAssets/BucketItem.asset");
            bucket.spawnPrefab.AddComponent<AudioMixerFixer>();
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(trashBag.spawnPrefab);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(bucket.spawnPrefab);

            trash.Add("mop", bucket);
            trash.Add("vacuum", trashBag);
            trash.Add("broom", trashBag);
            trash.Add("garbage", trashBag);


            foreach(KeyValuePair<string,string> bundleData in messPaths)
            {
                Item item = bundle.LoadAsset<Item>(bundleData.Key);
                item.spawnPrefab.AddComponent<AudioMixerFixer>();
                MessItem mess = item.spawnPrefab.AddComponent<MessItem>();

                mess.loot = trash[bundleData.Value].spawnPrefab;
                mess.toolType = bundleData.Value;
                mess.cleanNoise = sfx[bundleData.Value];
                item.maxValue = cfg.TRASH_MAX;
                item.minValue = cfg.TRASH_MIN;

                LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(item.spawnPrefab);
                Items.RegisterScrap(item, Plugin.cfg.MESS_RARITY, Levels.LevelTypes.All);
                if (bundleData.Key.Contains("Bracken"))
                {
                    BodySpawns.Add("Flowerman", item);
                }
            }

            foreach(KeyValuePair<string, AnimationCurve> pair in bodyPaths)
            {
                Item body = bundle.LoadAsset<Item>(pair.Key);
                body.spawnPrefab.AddComponent<AudioMixerFixer>();
                body.spawnPrefab.AddComponent<BodySyncer>();
                body.maxValue = maxBodyValues[pair.Key];
                body.minValue = minBodyValues[pair.Key];
                body.weight = bodyWeights[pair.Key];
                LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(body.spawnPrefab);
                SpawnableMapObjectDef mapObjDef = ScriptableObject.CreateInstance<SpawnableMapObjectDef>();
                mapObjDef.spawnableMapObject = new SpawnableMapObject();
                mapObjDef.spawnableMapObject.prefabToSpawn = body.spawnPrefab;
                MapObjects.RegisterMapObject(mapObjDef, Levels.LevelTypes.All,(level) => pair.Value);
                Items.RegisterItem(body);

                BodySpawns.Add(pathToName[pair.Key], body);
            }
        }
    }
}
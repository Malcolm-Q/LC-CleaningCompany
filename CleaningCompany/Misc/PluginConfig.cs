using BepInEx.Configuration;

namespace CleaningCompany.Misc
{
    public class PluginConfig
    {
        readonly ConfigFile configFile;

        //theming
        public bool JANITOR_SUIT {  get; set; }
        public bool JANITOR_VA {  get; set; }

        // max bodySpawns
        public int MAX_SCAVS { get; set; }
        public int MAX_SPIDERS { get; set; }
        public int MAX_HOARDERS { get; set; }
        public int MAX_THUMPERS { get; set; }
        public int MAX_CENTIPEDES { get; set; }

        // bodyspawn chances
        public float SCAV_CURVE {  get; set; }
        public float SPIDER_CURVE {  get; set; }
        public float THUMPER_CURVE {  get; set; }
        public float CENTIPEDE_CURVE {  get; set; }
        public float HOARDER_CURVE {  get; set; }

        // bodyspawn values
        public int SCAV_MIN {  get; set; }
        public int SPIDER_MIN {  get; set; }
        public int THUMPER_MIN {  get; set; }
        public int CENTIPEDE_MIN {  get; set; }
        public int HOARDER_MIN {  get; set; }
        public int SCAV_MAX {  get; set; }
        public int SPIDER_MAX {  get; set; }
        public int THUMPER_MAX {  get; set; }
        public int CENTIPEDE_MAX {  get; set; }
        public int HOARDER_MAX {  get; set; }

        // bodystpawn weights
        public float SCAV_WEIGHT {  get; set; }
        public float SPIDER_WEIGHT {  get; set; }
        public float THUMPER_WEIGHT {  get; set; }
        public float CENTIPEDE_WEIGHT {  get; set; }
        public float HOARDER_WEIGHT {  get; set; }

        // trash value
        public int TRASH_MIN {  get; set; }
        public int TRASH_MAX {  get; set; }



        public PluginConfig(ConfigFile cfg)
        {
            configFile = cfg;
        }

        private T ConfigEntry<T>(string section, string key, T defaultVal, string description)
        {
            return configFile.Bind(section, key, defaultVal, description).Value;
        }

        public void InitBindings()
        {
            JANITOR_SUIT = ConfigEntry("Janitor Theme", "Janitor Suits", true, "If true the default suit will be replaced with a janitor suit.");
            JANITOR_VA = ConfigEntry("Janitor Theme", "Janitor Voice Acting", true, "If true the default loud speaker voicelines will be replaced with Rufus'.");

            MAX_CENTIPEDES = ConfigEntry("Max Body Spawns", "Max number of Centipede Bodies", 2, "");
            MAX_HOARDERS = ConfigEntry("Max Body Spawns", "Max number of Hoarding Bug Bodies", 1, "");
            MAX_SCAVS = ConfigEntry("Max Body Spawns", "Max number of Scavenger Bodies", 3, "");
            MAX_SPIDERS = ConfigEntry("Max Body Spawns", "Max number of Spider Bodies", 1, "");
            MAX_THUMPERS = ConfigEntry("Max Body Spawns", "Max number of Thumper Bodies", 1, "");

            CENTIPEDE_CURVE = ConfigEntry("Body Spawn Chance", "Max number of Centipede Bodies", 0.25f, "Lower this number to increase spawn chance.");
            HOARDER_CURVE = ConfigEntry("Body Spawn Chance", "Max number of Hoarding Bug Bodies", 0.35f, "Lower this number to increase spawn chance.");
            SCAV_CURVE = ConfigEntry("Body Spawn Chance", "Max number of Scavenger Bodies", 0.35f, "Lower this number to increase spawn chance.");
            SPIDER_CURVE = ConfigEntry("Body Spawn Chance", "Max number of Spider Bodies", 0.5f, "Lower this number to increase spawn chance.");
            THUMPER_CURVE = ConfigEntry("Body Spawn Chance", "Max number of Thumper Bodies", 0.75f, "Lower this number to increase spawn chance.");

            CENTIPEDE_MIN = ConfigEntry("Body Values", "Min price of Centipede Bodies", 45, "");
            CENTIPEDE_MAX = ConfigEntry("Body Values", "Max price of Centipede Bodies", 70, "");
            HOARDER_MIN = ConfigEntry("Body Values", "Min price of Hoarding Bug Bodies", 55, "");
            HOARDER_MAX = ConfigEntry("Body Values", "Max price of Hoarding Bug Bodies", 88, "");
            SCAV_MIN = ConfigEntry("Body Values", "Min price of Scavenger Bodies", 100, "");
            SCAV_MAX = ConfigEntry("Body Values", "Max price of Scavenger Bodies", 170, "");
            SPIDER_MIN = ConfigEntry("Body Values", "Min price of Spider Bodies", 70, "");
            SPIDER_MAX = ConfigEntry("Body Values", "Max price of Spider Bodies", 110, "");
            THUMPER_MIN = ConfigEntry("Body Values", "Min price of Thumper Bodies", 120, "");
            THUMPER_MAX = ConfigEntry("Body Values", "Max price of Thumper Bodies", 160, "");

            CENTIPEDE_WEIGHT = ConfigEntry("Body Weights", "Weight of Centipede Bodies", 1.65f, "");
            HOARDER_WEIGHT = ConfigEntry("Body Weights", "Weight of Hoarding Bug Bodies", 1.6f, "");
            SCAV_WEIGHT = ConfigEntry("Body Weights", "Weight of Scavenger Bodies", 2.5f, "");
            SPIDER_WEIGHT = ConfigEntry("Body Weights", "Weight of Spider Bodies", 2.3f, "");
            THUMPER_WEIGHT = ConfigEntry("Body Weights", "Weight of Thumper Bodies", 2.9f, "");

            TRASH_MIN = ConfigEntry("Trash Value", "Min price of trash", 20, "");
            TRASH_MAX = ConfigEntry("Trash Value", "Max price of trash", 70, "");

        }
    }
}

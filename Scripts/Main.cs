using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;

namespace BribeForLocation
{
    public class Main : MonoBehaviour
    {
        private static Mod mod;
        private static Main instance;

        public static bool ScaleByLevel { get; private set; }
        public static int BribeAmount { get; private set; }

        public string Title => mod.Title ?? "BribeForLocation";
        public static Main Instance => instance ?? (instance = FindObjectOfType<Main>());

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;

            instance = new GameObject(mod.Title)
                .AddComponent<Main>();

            mod.LoadSettingsCallback = LoadSettings;

            mod.IsReady = true;
        }

        private void Start()
        {
            mod.LoadSettings();
            UIWindowFactory.RegisterCustomUIWindow(UIWindowType.Talk, typeof(BribeableTalkWindow));
        }

        private static void LoadSettings(ModSettings modSettings, ModSettingsChange change)
        {
            ScaleByLevel = modSettings.GetBool("General", "Scale By Level");
            BribeAmount = modSettings.GetInt("General", "Base Bribe Amount");
        }
    }
}

using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;

namespace BribeForLocation
{
    public class BribeForLocationMain : MonoBehaviour
    {
        private static Mod mod;
        private static BribeForLocationMain instance;
        public Texture2D BribeButtonEnabledTexture { get; private set; }

        public static BribeSettings Settings { get; private set; }
        public static BribeForLocationMain Instance => instance ?? (instance = FindObjectOfType<BribeForLocationMain>());

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;

            instance = new GameObject(mod.Title)
                .AddComponent<BribeForLocationMain>();

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
            var personalityScalers = modSettings.GetTupleFloat("General", "Personality Scale Amount");
            Settings = new BribeSettings
            {
                ScaleByLevel = modSettings.GetBool("General", "Enable Scale By Level"),
                AmountToScaleBy = modSettings.GetFloat("General", "Level Scale Amount"),
                StartingBribeAmount = modSettings.GetInt("General", "Base Bribe Amount"),
                ScaleByPersonality = modSettings.GetBool("General", "Enable Scale By Personality"),
                PersonalityScaleMin = personalityScalers.First,
                PersonalityScaleMax = personalityScalers.Second,
                EnableKnowlegeChecking = modSettings.GetBool("Experimental", "Enable Knowledge Check"),
            };
        }
    }
}

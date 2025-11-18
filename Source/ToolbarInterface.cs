using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KSP.UI.Screens;
using UnityEngine;

using ToolbarControl_NS;

namespace OrbitalDecay
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]

    public class ToolbarInterface : MonoBehaviour
    {
        internal const string MODID = "OrbitalDecay_NS";
        internal const string MODNAME = "Orbital Decay";
        private static ToolbarControl toolbarControl;
        private static bool visible = false;
        private static bool hideUI = false;
        private static bool paused = false;
        private GameScenes curScene;

        public static bool DecayBreakdownVisible;
        public static bool Visible { get { return visible && !hideUI && !paused; } }

        public void Awake()
        {
            if (toolbarControl == null)
            {
                toolbarControl = gameObject.AddComponent<ToolbarControl>();
                toolbarControl.AddToAllToolbars(GuiOn, GuiOff,
                    ApplicationLauncher.AppScenes.TRACKSTATION | ApplicationLauncher.AppScenes.SPACECENTER | ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW | ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH,
                    MODID,
                    "OrbitalDecayButton",
                    "WhitecatIndustries/OrbitalDecay/Icon/Icon_Toolbar_highlighted",
                    "WhitecatIndustries/OrbitalDecay/Icon/Icon_Toolbar",
                    "WhitecatIndustries/OrbitalDecay/Icon/Icon_Toolbar_highlighted",
                    "WhitecatIndustries/OrbitalDecay/Icon/Icon_Toolbar",
                    MODNAME
                );
            }
            curScene = HighLogic.LoadedScene;
            GameEvents.onHideUI.Add(onHideUI);
            GameEvents.onShowUI.Add(onShowUI);
            GameEvents.onGamePause.Add(onGamePause);
            GameEvents.onGameUnpause.Add(onGameUnpause);
            GameEvents.onGameSceneLoadRequested.Add(onGameSceneLoadRequested);
            visible = hideUI = paused = false;
            DontDestroyOnLoad(this);
        }

        internal void OnDestroy()
        {
            if (!HighLogic.LoadedSceneIsEditor)
            {
                GameEvents.onHideUI.Remove(onHideUI);
                GameEvents.onShowUI.Remove(onShowUI);
                GameEvents.onGamePause.Remove(onGamePause);
                GameEvents.onGameUnpause.Remove(onGameUnpause);
            }
        }
        void onGameSceneLoadRequested(GameScenes gs)
        {
            visible = hideUI = paused = false;
            toolbarControl.SetFalse(true);
        }

        void onGamePause()
        {
            paused = true;
        }
        void onGameUnpause()
        {
            paused = false;
        }
        void onHideUI()
        {
            hideUI = true;
        }
        void onShowUI()
        {
            hideUI = false;

        }
        internal static void GuiOn()
        {
            visible = true;
        }

        internal static void GuiOff()
        {
            visible = false;
            DecayBreakdownVisible = false;
        }
    }
}

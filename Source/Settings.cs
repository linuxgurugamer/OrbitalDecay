/*
 * Whitecat Industries Orbital Decay for Kerbal Space Program. 
 * 
 * Written by Whitecat106 (Marcus Hehir).
 * 
 * Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
 * project is in no way associated with nor endorsed by Squad.
 * 
 * This code is licensed under the Attribution-NonCommercial-ShareAlike 3.0 (CC BY-NC-SA 3.0)
 * creative commons license. See <http://creativecommons.org/licenses/by-nc-sa/3.0/legalcode>
 * for full details.
 * 
 * Attribution — You are free to modify this code, so long as you mention that the resulting
 * work is based upon or adapted from this code.
 * 
 * Non-commercial - You may not use this work for commercial purposes.
 * 
 * Share Alike — If you alter, transform, or build upon this work, you may distribute the
 * resulting work only under the same or similar license to the CC BY-NC-SA 3.0 license.
 * 
 * Note that Whitecat Industries is a ficticious entity created for entertainment
 * purposes. It is in no way meant to represent a real entity. Any similarity to a real entity
 * is purely coincidental.
 */

using SpaceTuxUtility;
using UnityEngine;

namespace OrbitalDecay
{

    public struct RESOURCES
    {
        public string StatKeepResource;
        public double ResourceRateDifficulty;
    }

    [KSPAddon(KSPAddon.Startup.FlightAndKSC, false)]
    public class SettingsFlightAndKSP : Settings
    {

    }

    [KSPAddon(KSPAddon.Startup.TrackingStation, false)]
    public class SettingsrTrackingStation : Settings
    {

    }

    public class Settings : MonoBehaviour
    {
        public static string FilePath;
        public static ConfigNode SettingData = new ConfigNode();
        internal static ConfigNode SettingsNode;

        static RESOURCES resources;
        public void Start()
        {
            FilePath = KSPUtil.ApplicationRootPath +
                       "GameData/WhitecatIndustries/OrbitalDecay/PluginData/Settings.cfg";
            CheckStockSettings();

            SettingData.ClearData();
            SettingsNode = ConfigNode.Load(FilePath);

            var node = SettingsNode.GetNode("RESOURCES");
            if (node != null)
            {
                resources.StatKeepResource = node.SafeLoad("StatKeepResources","MonoPropellant");
                resources.ResourceRateDifficulty = node.SafeLoad("ResourceRateDifficulty", 1d);
            }
        }

        public void CheckStockSettings() // 1.6.0 Stock give me back my decaying orbits!!
        {
            if (HighLogic.LoadedSceneIsGame)
            {
                if (GameSettings.ORBIT_DRIFT_COMPENSATION)
                {
                    GameSettings.ORBIT_DRIFT_COMPENSATION = false;
                }
            }
        }

        public void OnDestroy()
        {
            //SettingsNode.ClearData();
            //SettingData.Save(FilePath);
        }

        public static void WriteRD(bool RD)
        {
            HighLogic.CurrentGame.Parameters.CustomParams<OD>().RealisticDecay = RD;
        }



        public static void Write24H(bool H24)
        {
            HighLogic.CurrentGame.Parameters.CustomParams<OD>()._24HourClock = H24;
        }
        public static void WritePlanetariumTracking(bool PT)
        {
            HighLogic.CurrentGame.Parameters.CustomParams<OD>().PlanetariumTracking = PT;
        }

        public static void WritePDebrisTracking(bool DT)
        {
            HighLogic.CurrentGame.Parameters.CustomParams<OD>().PlanetariumDebrisTracking = DT;
        }

        public static void WriteDifficulty(double Difficulty)
        {
            HighLogic.CurrentGame.Parameters.CustomParams<OD>().DecayDifficulty = Difficulty;

        }
        public static void WriteResourceRateDifficulty(double Difficulty)
        {
            HighLogic.CurrentGame.Parameters.CustomParams<OD2>().ResourceRateDifficulty = Difficulty;
        }
        public static void WriteStatKeepResource(string Resource)
        {
            ConfigNode Data = SettingData;
            ConfigNode Resources = Data.GetNode("RESOURCES");
            Resources.SetValue("StatKeepResource", Resource);
        }
        public static bool ReadRD()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<OD>().RealisticDecay;
        }

        public static bool Read24Hr()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<OD>()._24HourClock ;
        }

        public static bool ReadPT()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<OD>().PlanetariumTracking;
        }

        public static bool ReadDT()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<OD>().PlanetariumDebrisTracking;
        }

        public static double ReadDecayDifficulty()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<OD>().DecayDifficulty;

        }

        public static string ReadStationKeepingResource()
        {
            ConfigNode Data = SettingData;
            ConfigNode Resources = Data.GetNode("RESOURCES");
            string FavouredResource = resources.StatKeepResource;
            return FavouredResource;
        }

        public static double ReadResourceRateDifficulty()
        {
            ConfigNode Data = SettingData;
            ConfigNode Resources = Data.GetNode("RESOURCES");
            double FavouredResource = resources.ResourceRateDifficulty;
            return FavouredResource;
        }
    }
}

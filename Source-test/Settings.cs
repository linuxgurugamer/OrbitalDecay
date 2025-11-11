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

using UnityEngine;

namespace WhitecatIndustries
{
    [KSPAddon(KSPAddon.Startup.EveryScene,false)]
    public class Settings : MonoBehaviour
    {
        //public static string FilePath;
        //public static ConfigNode SettingData = new ConfigNode();
        //internal static ConfigNode SettingsNode;

        public void Start()
        {
            if (HighLogic.CurrentGame == null) return;
            //FilePath = KSPUtil.ApplicationRootPath +                       "GameData/WhitecatIndustries/OrbitalDecay/PluginData/Settings.cfg";
            CheckStockSettings();

           // SettingData.ClearData();
           // SettingsNode = ConfigNode.Load(FilePath);
           // foreach (ConfigNode item in SettingsNode.nodes)
           // {
           //     SettingData.AddNode(item);
           // }
            UserInterface.NBodyStepsContainer = (float) ReadNBCC();
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
#if false
            ConfigNode Data = SettingData;
            ConfigNode SimSet = Data.GetNode("SIMULATION");
            SimSet.SetValue("RealisticDecay", RD.ToString());   
#endif
        }

        public static void WriteNBody(bool NB) // 1.6.0 NBody
        {
            HighLogic.CurrentGame.Parameters.CustomParams<OD>().NBodySimulation = NB;
#if false
                ConfigNode Data = SettingData;
                ConfigNode SimSet = Data.GetNode("SIMULATION");
                SimSet.SetValue("NBodySimulation", NB.ToString());
#endif
        }

        public static void WriteNBodyConics(bool NBC) // 1.6.0 NBody
        {
            HighLogic.CurrentGame.Parameters.CustomParams<OD>().NBodySimulationConics = NBC;
#if false
            ConfigNode Data = SettingData;
            ConfigNode SimSet = Data.GetNode("SIMULATION");
            SimSet.SetValue("NBodySimulationConics", NBC.ToString());
#endif
        }

        public static void WriteNBodyConicsPatches(double NBCC) // 1.6.0 NBody
        {
            HighLogic.CurrentGame.Parameters.CustomParams<OD>().NBodySimulationConicsPatches = NBCC;
#if false
            ConfigNode Data = SettingData;
            ConfigNode SimSet = Data.GetNode("SIMULATION");
            SimSet.SetValue("NBodySimulationConicsPatches", NBCC.ToString());
#endif
        }

        public static void WriteNBodyBodyUpdating(bool NBB) // 1.6.0 NBody
        {
            HighLogic.CurrentGame.Parameters.CustomParams<OD>().NBodySimulationBodyUpdating = NBB;
#if false
            ConfigNode Data = SettingData;
            ConfigNode SimSet = Data.GetNode("SIMULATION");
            SimSet.SetValue("NBodySimulationBodyUpdating", NBB.ToString());
#endif
        }


        public static void Write24H(bool H24)
        {
            HighLogic.CurrentGame.Parameters.CustomParams<OD>()._24HourClock = H24;
#if false
            ConfigNode Data = SettingData;
            ConfigNode SimSet = Data.GetNode("SIMULATION");
            SimSet.SetValue("24HourClock", H24.ToString());  
#endif
        }
        public static void WritePlanetariumTracking(bool PT)
        {
            HighLogic.CurrentGame.Parameters.CustomParams<OD>().PlanetariumTracking = PT;
#if false
            ConfigNode Data = SettingData;
            ConfigNode SimSet = Data.GetNode("SIMULATION");
            SimSet.SetValue("PlanetariumTracking", PT.ToString()); 
#endif
        }

        public static void WritePDebrisTracking(bool DT)
        {
            HighLogic.CurrentGame.Parameters.CustomParams<OD>().PlanetariumDebrisTracking = DT;
#if false
            ConfigNode Data = SettingData;
            ConfigNode SimSet = Data.GetNode("SIMULATION");
            SimSet.SetValue("PlanetariumDebrisTracking", DT.ToString());
#endif
        }

        public static void WriteDifficulty(double Difficulty)
        {
            HighLogic.CurrentGame.Parameters.CustomParams<OD>().DecayDifficulty = Difficulty;
#if false
            ConfigNode Data = SettingData;
            ConfigNode SimSet = Data.GetNode("SIMULATION");
            SimSet.SetValue("DecayDifficulty", Difficulty.ToString());
#endif
        }
        public static void WriteResourceRateDifficulty(double Difficulty)
        {
            HighLogic.CurrentGame.Parameters.CustomParams<OD2>().ResourceRateDifficulty = Difficulty;
#if false
            ConfigNode Data = SettingData;
            ConfigNode Resources = Data.GetNode("RESOURCES");
            Resources.SetValue("ResourceRateDifficulty", Difficulty.ToString());
#endif
        }

#if false

        public static void WriteStatKeepResource(string Resource)
        {
            //ConfigNode Data = SettingData;
            //ConfigNode Resources = Data.GetNode("RESOURCES");
            //Resources.SetValue("StatKeepResource", Resource);
        }
#endif

        public static bool ReadRD()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<OD>().RealisticDecay;
#if false
            ConfigNode Data = SettingData;
            ConfigNode SimSet = Data.GetNode("SIMULATION");
            bool RD = bool.Parse(SimSet.GetValue("RealisticDecay"));
            return RD;
#endif
        }

        public static bool ReadNB() // 1.6.0 NBody
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<OD>().NBodySimulation;
#if false
            ConfigNode Data = SettingData;
            ConfigNode SimSet = Data.GetNode("SIMULATION");
            bool NB = bool.Parse(SimSet.GetValue("NBodySimulation"));
            return NB;
#endif
        }

        public static bool ReadNBC() // 1.6.0 NBody Conics
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<OD>().NBodySimulationConics;
#if false
            ConfigNode Data = SettingData;
            ConfigNode SimSet = Data.GetNode("SIMULATION");
            bool NBC = bool.Parse(SimSet.GetValue("NBodySimulationConics"));
            return NBC;
#endif
        }

        public static double ReadNBCC() // 1.6.0 NBody Conics
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<OD>().NBodySimulationConicsPatches;
#if false
            ConfigNode Data = SettingData;
            ConfigNode SimSet = Data.GetNode("SIMULATION");
            double NBCC = double.Parse(SimSet.GetValue("NBodySimulationConicsPatches"));
            return NBCC;
#endif
        }

        public static bool ReadNBB() // 1.6.0 NBody bodies
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<OD>().NBodySimulationBodyUpdating;
#if false
            ConfigNode Data = SettingData;
            ConfigNode SimSet = Data.GetNode("SIMULATION");
            bool NBCC = bool.Parse(SimSet.GetValue("NBodySimulationBodyUpdating"));
            return NBCC;
#endif
        }

        public static bool Read24Hr()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<OD>()._24HourClock;
#if false
            ConfigNode Data = SettingData;
            ConfigNode SimSet = Data.GetNode("SIMULATION");
            bool R24H = bool.Parse(SimSet.GetValue("24HourClock"));
            return R24H;
#endif
        }

        public static bool ReadPT()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<OD>().PlanetariumTracking;
#if false
            ConfigNode Data = SettingData;
            ConfigNode SimSet = Data.GetNode("SIMULATION");
            bool PT = bool.Parse(SimSet.GetValue("PlanetariumTracking"));
            return PT;
#endif
        }

        public static bool ReadDT()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<OD>().PlanetariumDebrisTracking;
#if false
            ConfigNode Data = SettingData;
            ConfigNode SimSet = Data.GetNode("SIMULATION");
            bool DT = bool.Parse(SimSet.GetValue("PlanetariumDebrisTracking"));
            return DT;
#endif
        }

        public static double ReadDecayDifficulty()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<OD>().DecayDifficulty;
#if false
            ConfigNode Data = SettingData;
            ConfigNode SimSet = Data.GetNode("SIMULATION");
            double Difficulty = double.Parse(SimSet.GetValue("DecayDifficulty"));
            return Difficulty;
#endif
        }
#if false

        public static string ReadStationKeepingResource()
        {
            ConfigNode Data = SettingData;
            ConfigNode Resources = Data.GetNode("RESOURCES");
            string FavouredResource = Resources.GetValue("StatKeepResource");
            return FavouredResource;
        }
#endif

        public static double ReadResourceRateDifficulty()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<OD2>().ResourceRateDifficulty;
#if false
            ConfigNode Data = SettingData;
            ConfigNode Resources = Data.GetNode("RESOURCES");
            double FavouredResource = double.Parse(Resources.GetValue("ResourceRateDifficulty"));
            return FavouredResource;
#endif
        }

    }
}

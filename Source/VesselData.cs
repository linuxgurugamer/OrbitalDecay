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
using System;
using System.Collections.Generic;
using UnityEngine;
using static GameEvents;
using static OrbitalDecay.RegisterToolbar;

namespace OrbitalDecay
{
    [KSPAddon(KSPAddon.Startup.FlightAndKSC, false)]
    public class VesselDataFlightAndKSP : VesselData { }

    [KSPAddon(KSPAddon.Startup.TrackingStation, false)]
    public class VesselDataTrackingStation : VesselData { }


    public class VesselData : MonoBehaviour
    {
        public static Dictionary<Guid, Vessel_Information> VesselInfo = new Dictionary<Guid, Vessel_Information>();

        public static string FilePath;

        public static double EndSceneWaitTime = 0;
        public static double StartSceneWaitTime = 0;
        public static bool VesselMovementUpdate;
        public static bool VesselMoving;
        public static bool VesselsLoaded = false;
        public static double TimeOfLastMovement;
        public static bool ClearedOld = false;
        private float UPTInterval = 1.0f;
        private float lastUpdate;

        public void Awake()
        {
            if (!HighLogic.LoadedSceneIsGame) return;

            ConfigNode File = ConfigNode.Load(FilePath);
            VesselInfo.Clear();

            print("WhitecatIndustries - OrbitalDecay - Loaded vessel data, there are " + VesselInfo.Count + " vessels");

            GameEvents.onGameSceneSwitchRequested.Add(onGameSceneSwitchRequested);
            GameEvents.onLevelWasLoadedGUIReady.Add(onLevelWasLoadedGUIReady);

            DontDestroyOnLoad(this);
        }

        void onLevelWasLoadedGUIReady(GameScenes scene)
        {
            foreach (var vessel in FlightGlobals.Vessels)
            {
                VesselData.UpdateVesselSMA(vessel, vessel.orbit.semiMajorAxis);
                VesselData.UpdateVesselINC(vessel, vessel.orbit.inclination);
                VesselData.UpdateVesselECC(vessel, vessel.orbit.eccentricity);
                VesselData.UpdateVesselLAN(vessel, vessel.orbit.LAN);
            }
        }


        public void FixedUpdate()
        {
            if (Time.timeSinceLevelLoad > 1 && VesselsLoaded)
            {
                if (Time.time - lastUpdate > UPTInterval) // 1.4.0 Lag Busting
                {
                    lastUpdate = Time.time;

                    if (HighLogic.LoadedSceneIsGame &&
                        HighLogic.LoadedScene != GameScenes.LOADING &&
                        HighLogic.LoadedScene != GameScenes.LOADINGBUFFER &&
                        HighLogic.LoadedScene != GameScenes.MAINMENU)
                    {
                        Vessel vessel = new Vessel();
                        for (int i = 0; i < FlightGlobals.Vessels.Count; i++)
                        {
                            vessel = FlightGlobals.Vessels[i];
                            WriteVesselData(vessel);
                        }
                    }
                }
            }
        }

        public void OnDestroy()
        {
            if (HighLogic.CurrentGame != null && DecayManager.CheckSceneStateMain(HighLogic.LoadedScene))
            {
                if (Planetarium.GetUniversalTime() == HighLogic.CurrentGame.UniversalTime || HighLogic.LoadedScene == GameScenes.FLIGHT)
                {
                    print("WhitecatIndustries - OrbitalDecay - Vessel Information saved. Ondestroy");
                    Vessel_Information.Save(VesselInfo, FilePath);
                }
            }
            GameEvents.onGameSceneSwitchRequested.Remove(onGameSceneSwitchRequested);
            GameEvents.onLevelWasLoadedGUIReady.Remove(onLevelWasLoadedGUIReady);

        }

        void onGameSceneSwitchRequested(FromToAction<GameScenes, GameScenes> fta)
        {
            Vessel_Information.Save(VesselInfo, FilePath);
        }

        public static void OnQuickSave()
        {
            if (DecayManager.CheckSceneStateMain(HighLogic.LoadedScene))
            {
                if (Planetarium.GetUniversalTime() == HighLogic.CurrentGame.UniversalTime || HighLogic.LoadedScene == GameScenes.FLIGHT)
                {
                    print("WhitecatIndustries - OrbitalDecay - Vessel Information saved " + VesselInfo.Count + " vessels."); ;
                    Vessel_Information.Save(VesselInfo, FilePath);
                    VesselInfo.Clear();
                    print("WhitecatIndustries - OrbitalDecay - Vessel Information lost OnQuickSave");
                }
            }
        }

        public static void OnQuickLoad() // 1.5.3 quick load fixes
        {
            VesselInfo.Clear();
            print("WhitecatIndustries - OrbitalDecay - Vessel Information lost OnQuickLoad");

        }

        public static bool CheckIfContained(Vessel vessel)
        {
            return VesselInfo.ContainsKey(vessel.id);
        }

        public static void WriteVesselData(Vessel vessel)
        {
            if (CheckIfContained(vessel) == false)
            {
                var vD = BuildConfigNode(vessel);
                VesselInfo[vessel.id] = vD;
            }

            if (vessel == FlightGlobals.ActiveVessel)
            {
                if (FlightGlobals.ActiveVessel.geeForce > 0.01) // Checks if a vessel is still moving between orbits (Average GForce around 0.0001)
                {
                    VesselMovementUpdate = false;
                    VesselMoving = true;
                    TimeOfLastMovement = HighLogic.CurrentGame.UniversalTime;
                }
                else
                {
                    VesselMoving = false;
                }

                if (VesselMoving == false && HighLogic.CurrentGame.UniversalTime - TimeOfLastMovement < 1 && VesselMovementUpdate == false)
                {
                    UpdateVesselSMA(vessel, vessel.orbitDriver.orbit.semiMajorAxis);
                    UpdateVesselECC(vessel, vessel.orbitDriver.orbit.eccentricity);
                    UpdateVesselINC(vessel, vessel.orbitDriver.orbit.inclination);
                    UpdateVesselEPH(vessel, vessel.orbitDriver.orbit.epoch);
                    UpdateVesselLAN(vessel, vessel.orbitDriver.orbit.LAN);
                    UpdateVesselMNA(vessel, vessel.orbitDriver.orbit.meanAnomalyAtEpoch);
                    UpdateVesselLPE(vessel, vessel.orbitDriver.orbit.argumentOfPeriapsis);

                    VesselMovementUpdate = true;
                }
            }
        }

        public static void UpdateActiveVesselData(Vessel vessel)
        {
            if (!VesselInfo.ContainsKey(vessel.id))
            {
                Log.Info($"UpdateActiveVesselData, vessel: {vessel.vesselName}  {vessel.id} not found");
                var vD = VesselData.BuildConfigNode(vessel);
                VesselData.VesselInfo[vessel.id] = vD;
            }
            VesselInfo[vessel.id].Mass = vessel.GetTotalMass() * 1000;
            VesselInfo[vessel.id].Area = CalculateVesselArea(vessel);
        }

        public static void ClearVesselData(Vessel vessel)
        {
            if (VesselInfo.ContainsKey(vessel.id))
                VesselInfo.Remove(vessel.id); ;
        }

        public static Vessel_Information BuildConfigNode(Vessel vessel)
        {
            float Mass;
            double Area;
            if (vessel == FlightGlobals.ActiveVessel)
            {
                Mass = vessel.GetTotalMass() * 1000;
                Area = CalculateVesselArea(vessel);
            }
            else
            {
                Mass = vessel.GetTotalMass() * 1000;
                Area = CalculateVesselArea(vessel);
            }
            Vessel_Information vi = new Vessel_Information(
                vessel.GetName(),
                vessel.id,
                vessel.vesselType.ToString().Substring(0, 1) + vessel.GetInstanceID(),
                Mass,
                Area,
                vessel.orbitDriver.orbit.referenceBody.GetName(),
                vessel.GetOrbitDriver().orbit.semiMajorAxis,

                vessel.GetOrbitDriver().orbit.eccentricity,
                vessel.GetOrbitDriver().orbit.inclination,
                vessel.GetOrbitDriver().orbit.argumentOfPeriapsis,
                vessel.GetOrbitDriver().orbit.LAN,
                vessel.GetOrbitDriver().orbit.meanAnomalyAtEpoch,
                vessel.GetOrbitDriver().orbit.epoch);
            return vi;
        }

        public static bool FetchStationKeeping(Vessel vessel)
        {
            bool StationKeeping = false;
            if (vessel == FlightGlobals.ActiveVessel)
            {
                List<ModuleOrbitalDecay> modlist = vessel.FindPartModulesImplementing<ModuleOrbitalDecay>();
                foreach (ModuleOrbitalDecay module in modlist)
                {
                    StationKeeping = module.stationKeepData.IsStationKeeping;
                    break;
                }

            }
            else
            {
                ProtoVessel proto = vessel.protoVessel;

                foreach (ProtoPartSnapshot protopart in proto.protoPartSnapshots)
                {
                    foreach (ProtoPartModuleSnapshot protopartmodulesnapshot in protopart.modules)
                    {
                        if (protopartmodulesnapshot.moduleName == "ModuleOrbitalDecay")
                        {
                            ConfigNode node = protopartmodulesnapshot.moduleValues.GetNode("stationKeepData");
                            StationKeeping = node.SafeLoad("IsStationKeeping", false);
                            break;
                        }
                    }
                }
            }

            return StationKeeping;
        }


        public static double FetchFuelLost(OrbitalDecay.ModuleOrbitalDecay mod)
        {
            double FuelLost = 0;
            if (mod != null)
                FuelLost = mod.stationKeepData.fuelLost;
            return FuelLost;
        }


        public static void SetFuelLost(double FuelLost)
        {

            List<ModuleOrbitalDecay> modlist = FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleOrbitalDecay>();
            foreach (ModuleOrbitalDecay module in modlist)
            {
                module.stationKeepData.fuelLost = FuelLost;
            }
        }


        public static double FetchMass(Vessel vessel)
        {
            if (VesselInfo.ContainsKey(vessel.id))
                return VesselInfo[vessel.id].Mass;
            return 0;
        }


        public static double FetchArea(Vessel vessel)
        {
            if (VesselInfo.ContainsKey(vessel.id))
            {
                return VesselInfo[vessel.id].Area;
            }

            return 0;
        }

        public static void UpdateStationKeeping(Vessel vessel, bool StationKeeping)
        {
            if (vessel == FlightGlobals.ActiveVessel)
            {
                List<ModuleOrbitalDecay> modlist = vessel.FindPartModulesImplementing<ModuleOrbitalDecay>();
                foreach (ModuleOrbitalDecay module in modlist)
                {
                    module.stationKeepData.IsStationKeeping = StationKeeping;
                }
            }
            else
            {
                ProtoVessel proto = vessel.protoVessel;
                foreach (ProtoPartSnapshot protopart in proto.protoPartSnapshots)
                {
                    foreach (ProtoPartModuleSnapshot protopartmodulesnapshot in protopart.modules)
                    {
                        if (protopartmodulesnapshot.moduleName == "ModuleOrbitalDecay")
                        {
                            ConfigNode node = protopartmodulesnapshot.moduleValues.GetNode("stationKeepData");
                            node.SetValue("IsStationKeeping", StationKeeping.ToString());
                            break;
                        }
                    }
                }
            }
        }

        public static void UpdateVesselSMA(Vessel vessel, double SMA)
        {
            if (VesselInfo.ContainsKey(vessel.id))
                VesselInfo[vessel.id].SMA = SMA;
            else
                Log.Info($"UpdateVesselSMA, {vessel.vesselName} not found in VesselInfo");

        }

        public static double FetchSMA(Vessel vessel)
        {
            if (VesselInfo.ContainsKey(vessel.id))
                return VesselInfo[vessel.id].SMA;
            return 0;

        }

        public static void UpdateVesselECC(Vessel vessel, double ECC)
        {
            if (VesselInfo.ContainsKey(vessel.id))
                VesselInfo[vessel.id].ECC = ECC;
            else
                Log.Info($"UpdateVesselECC, {vessel.vesselName} not found in VesselInfo");

        }

        public static double FetchECC(Vessel vessel)
        {
            if (VesselInfo.ContainsKey(vessel.id))
                return VesselInfo[vessel.id].ECC;
            return 0;
        }

        public static void UpdateVesselINC(Vessel vessel, double INC)
        {
            if (VesselInfo.ContainsKey(vessel.id))
                VesselInfo[vessel.id].INC = INC;
            else
                Log.Info($"UpdateVesselINC, {vessel.vesselName} not found in VesselInfo");

        }

        public static double FetchINC(Vessel vessel)
        {
            if (VesselInfo.ContainsKey(vessel.id))
                return VesselInfo[vessel.id].INC;
            return 0;
        }

        public static void UpdateVesselLPE(Vessel vessel, double LPE)
        {
            if (VesselInfo.ContainsKey(vessel.id))
                VesselInfo[vessel.id].LPE = LPE;
            else
                Log.Info($"UpdateVesselLPE, {vessel.vesselName} not found in VesselInfo");

        }

        public static double FetchLPE(Vessel vessel)
        {
            if (VesselInfo.ContainsKey(vessel.id))
                return VesselInfo[vessel.id].LPE;
            return 0;
        }

        public static void UpdateVesselLAN(Vessel vessel, double LAN)
        {
            if (VesselInfo.ContainsKey(vessel.id))
                VesselInfo[vessel.id].LAN = LAN;
            else
                Log.Info($"UpdateVesselLAN, {vessel.vesselName} not found in VesselInfo");
        }

        public static double FetchLAN(Vessel vessel)
        {
            if (VesselInfo.ContainsKey(vessel.id))
                return VesselInfo[vessel.id].LAN;
            return 0;
        }

        public static void UpdateVesselMNA(Vessel vessel, double MNA)
        {
            if (VesselInfo.ContainsKey(vessel.id))
                VesselInfo[vessel.id].MNA = MNA;
            else
                Log.Info($"UpdateVesselMNA, {vessel.vesselName} not found in VesselInfo");
        }

        public static double FetchMNA(Vessel vessel)
        {
            if (VesselInfo.ContainsKey(vessel.id))
                return VesselInfo[vessel.id].MNA;
            return 0;
        }

        public static void UpdateVesselEPH(Vessel vessel, double EPH)
        {
            if (VesselInfo.ContainsKey(vessel.id))
                VesselInfo[vessel.id].EPH = EPH;
            else
                Log.Info($"UpdateVesselEPH, {vessel.vesselName} not found in VesselInfo");
        }

        public static double FetchEPH(Vessel vessel)
        {
            if (VesselInfo.ContainsKey(vessel.id))
                return VesselInfo[vessel.id].EPH;
            return 0;
        }


        /* simple ISP dependant effi calculation.
         * NEEDS ballancing
         * removing fuel based on dV would make more sense,
         * and more thinkering to firgure it out
         * 1.60 milestone maybe?
         */
        public static float FetchEfficiency(Vessel vessel)
        {
            float Efficiency = 0;
            if (vessel == FlightGlobals.ActiveVessel)
            {
                List<ModuleOrbitalDecay> modlist = vessel.FindPartModulesImplementing<ModuleOrbitalDecay>();
                if (modlist.Count > 0)
                {
                    Efficiency = modlist[0].stationKeepData.ISP;
                }

            }
            else
            {
                ProtoVessel proto = vessel.protoVessel;

                foreach (ProtoPartSnapshot protopart in proto.protoPartSnapshots)
                {
                    foreach (ProtoPartModuleSnapshot protopartmodulesnapshot in protopart.modules)
                    {
                        if (protopartmodulesnapshot.moduleName == "ModuleOrbitalDecay")
                        {
                            ConfigNode node = protopartmodulesnapshot.moduleValues.GetNode("stationKeepData");
                            Efficiency = node.SafeLoad("ISP", 0f);
                            break;
                        }
                    }
                }
            }
            if (Settings.ReadRD())
            {
                Efficiency *= 0.5f; // Balance here!
            }


            return 1 / Efficiency;
        }

        public static void UpdateBody(Vessel vessel, CelestialBody body)
        {
            if (VesselInfo.ContainsKey(vessel.id))
            {
                VesselInfo[vessel.id].ReferenceBody = body.bodyName; // GetName();
            }
        }

        public static void UpdateVesselFuel(Vessel vessel, double Fuel)
        {
            if (VesselInfo.ContainsKey(vessel.id))
            {
                VesselInfo[vessel.id].Fuel = Fuel;
            }
        }

        public static double CalculateVesselArea(Vessel vessel)
        {
            double Area = FindVesselArea(vessel);
            return Area;
        }

        public static double FindVesselArea(Vessel vessel)
        {
            double Area = 0.0;
            ProtoVessel vesselImage = vessel.protoVessel;
            List<ProtoPartSnapshot> PartSnapshots = vesselImage.protoPartSnapshots;
            foreach (ProtoPartSnapshot part in PartSnapshots)
            {
#if false
                if (vessel == FlightGlobals.ActiveVessel)
                {
                    if (part.partRef != null)
                    {
                        Area = Area + part.partRef.radiativeArea;
                    }
                }
                else
                {
                    Area = Area + part.partInfo.partSize * 2.0 * Math.PI;
                }
#else
                Area = Area + part.partInfo.partSize * 2.0; // * Math.PI;
#endif
            }

            return Area / 4.0; // only one side facing prograde
        }
    }
}

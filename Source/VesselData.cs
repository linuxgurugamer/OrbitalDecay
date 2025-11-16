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
    public class Vessel_Information
    {
        public string name;
        public Guid id;
        public string code;
        public float Mass;
        public double Area;
        public string ReferenceBody;

        public double SMA;
        public double ECC;
        public double INC;
        public double LPE;
        public double LAN;
        public double MNA;
        public double EPH;
        public double Fuel = 0;

        Vessel_Information() { }
        public Vessel_Information(string name, Guid id)
        {
            this.name = name;
            this.id = id;
        }

        public Vessel_Information(string name, Guid id, string code, float Mass,
            double Area, string ReferenceBody, double SMA,
            double ECC, double INC, double LPE, double LAN,
            double MNA, double EPH)
        {
            this.name = name;
            this.id = id;
            this.code = code;
            this.Mass = Mass;
            this.Area = Area;
            this.ReferenceBody = ReferenceBody;
            this.SMA = SMA;
            this.ECC = ECC;
            this.INC = INC;
            this.LPE = LPE;
            this.LAN = LAN;
            this.MNA = MNA;
            this.EPH = EPH;
        }

        static public ConfigNode Save(Dictionary<Guid, Vessel_Information> vi, string filename)
        {
            ConfigNode file = new ConfigNode();
            ConfigNode configNode = new ConfigNode("Vessels");
            file.AddNode(configNode);
            foreach (var v in vi.Values)
            {
                ConfigNode node = new ConfigNode("VESSEL");
                node.AddValue("name", v.name);
                node.AddValue("id", v.id);
                node.AddValue("code", v.code);
                node.AddValue("Mass", v.Mass);
                node.AddValue("Area", v.Area);
                node.AddValue("ReferenceBody", v.ReferenceBody);
                node.AddValue("SMA", v.SMA);
                node.AddValue("ECC", v.ECC);
                node.AddValue("INC", v.INC);
                node.AddValue("LPE", v.LPE);
                node.AddValue("LAN", v.LAN);
                node.AddValue("MNA", v.MNA);
                node.AddValue("EPH", v.EPH);
                node.AddValue("Fuel", v.Fuel);

                configNode.AddNode(node);
            }
            if (!string.IsNullOrEmpty(filename))
                file.Save(filename);
            return configNode;
        }

        static public Vessel_Information Load(ConfigNode cn)
        {
            Vessel_Information v = new Vessel_Information();
#if false
            v.name = cn.SafeLoad("name", "NoName");
            v.id = cn.SafeLoad("id", Guid.Empty);
            v.code = cn.SafeLoad("code", "");
            v.Mass = cn.SafeLoad("Mass", 0f);
            v.Area = cn.SafeLoad("Area", 0d);
            v.ReferenceBody = cn.SafeLoad("ReferenceBody", "");
            v.SMA = cn.SafeLoad("SMA", 0d);
            v.ECC = cn.SafeLoad("ECC", 0d);
            v.INC = cn.SafeLoad("INC", 0d);
            v.LPE = cn.SafeLoad("LPE", 0d);
            v.LAN = cn.SafeLoad("LAN", 0d);
            v.MNA = cn.SafeLoad("MNA", 0d);
            v.EPH = cn.SafeLoad("EPH", 0d);
            v.Fuel = cn.SafeLoad("Fuel", 0d);
#endif
            return v;
        }
    }


    [KSPAddon(KSPAddon.Startup.EveryScene, true)]
    public class VesselData : MonoBehaviour
    {
        //public static ConfigNode VesselInformation = new ConfigNode();
        public static Dictionary<Guid, Vessel_Information> VesselInfo = new Dictionary<Guid, Vessel_Information>();

        public static string FilePath;
        //public static ConfigNode File;

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
            FilePath = KSPUtil.ApplicationRootPath +
                       "GameData/WhitecatIndustries/OrbitalDecay/PluginData/VesselData.cfg";
            ConfigNode File = ConfigNode.Load(FilePath);
            VesselInfo.Clear();
#if false
            if (File.HasNode("Vessels"))
            {
                var configNode = File.GetNode("Vessels");
                foreach (var n in configNode.GetNodes("VESSEL"))
                {
                    var vi = Vessel_Information.Load(n);
                    VesselInfo[vi.id] = vi;
                }
            }
#endif
            print("WhitecatIndustries - OrbitalDecay - Loaded vessel data, there are " + VesselInfo.Count + " vessels");


            GameEvents.onGameSceneSwitchRequested.Add(onGameSceneSwitchRequested);

            /*  VesselInformation.ClearNodes();

              if (HighLogic.LoadedSceneIsGame && (HighLogic.LoadedScene != GameScenes.LOADING || HighLogic.LoadedScene != GameScenes.LOADINGBUFFER))
              {
                  if (System.IO.File.ReadAllLines(FilePath).Length == 0)
                  {
                      ConfigNode FileM = new ConfigNode();
                      ConfigNode FileN = new ConfigNode("VESSEL");
                      FileN.AddValue("name", "WhitecatsDummyVessel");
                      FileN.AddValue("id", "000");
                      FileN.AddValue("persistence", "WhitecatsDummySaveFileThatNoOneShouldNameTheirSave");
                      FileM.AddNode(FileN);
                      FileM.Save(FilePath);
                  }

                  File = ConfigNode.Load(FilePath);

                  if (File.nodes.Count > 0)
                  {
                      foreach (ConfigNode vessel in File.GetNodes("VESSEL"))
                      {
                          string Persistence = vessel.GetValue("persistence");
                          if (Persistence == HighLogic.SaveFolder.ToString() || Persistence == "WhitecatsDummySaveFileThatNoOneShouldNameTheirSave")
                          {
                              VesselInformation.AddNode(vessel);
                          }
                      }
                  }
              }*/
            DontDestroyOnLoad(this);
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
#if false
                            {
                                if (CheckIfContained(vessel))
                                {
                                    if (vessel.situation == Vessel.Situations.ORBITING || vessel.situation == Vessel.Situations.SUB_ORBITAL)
                                    {
                                        WriteVesselData(vessel);
                                    }
                                }
                                else
                                {
                                    if (vessel.situation == Vessel.Situations.ORBITING || vessel.situation == Vessel.Situations.SUB_ORBITAL) // 1.4.2
                                    {
                                        WriteVesselData(vessel);
                                    }
                                }
                            }
#endif
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
                    //File.ClearNodes();
                    //VesselInformation.Save(FilePath);
                    Vessel_Information.Save(VesselInfo, FilePath);
                    // VesselInformation.ClearNodes();
                }
            }
            GameEvents.onGameSceneSwitchRequested.Remove(onGameSceneSwitchRequested);
        }

        void onGameSceneSwitchRequested(FromToAction<GameScenes, GameScenes> fta)
        {
            print("WhitecatIndustries - OrbitalDecay - onGameSceneSwitchRequested - Vessel Information saved, " + VesselInfo.Count + " vessels.");
            //File.ClearNodes();
            //VesselInformation.Save(FilePath);
            Vessel_Information.Save(VesselInfo, FilePath);
        }

        public static void OnQuickSave()
        {
            if (DecayManager.CheckSceneStateMain(HighLogic.LoadedScene))
            {
                if (Planetarium.GetUniversalTime() == HighLogic.CurrentGame.UniversalTime || HighLogic.LoadedScene == GameScenes.FLIGHT)
                {
                    print("WhitecatIndustries - OrbitalDecay - Vessel Information saved " + VesselInfo.Count + " vessels."); ;
                    //File.ClearNodes();
                    //VesselInformation.Save(FilePath);
                    Vessel_Information.Save(VesselInfo, FilePath);
                    //VesselInformation.ClearNodes();
                    VesselInfo.Clear();
                    print("WhitecatIndustries - OrbitalDecay - Vessel Information lost OnQuickSave");
                }
            }
        }

        public static void OnQuickLoad() // 1.5.3 quick load fixes
        {
            //File.ClearNodes();
            //VesselInformation.ClearNodes();
            VesselInfo.Clear();
            print("WhitecatIndustries - OrbitalDecay - Vessel Information lost OnQuickLoad");

        }

        public static bool CheckIfContained(Vessel vessel)
        {
            bool Contained = false;

            return VesselInfo.ContainsKey(vessel.id);
#if false
            foreach (ConfigNode node in VesselInformation.GetNodes("VESSEL"))
            {
                if (node.GetValue("id") == vessel.id.ToString())
                {
                    Contained = true;
                }
            }
            return Contained;
#endif
        }

        public static void WriteVesselData(Vessel vessel)
        {
            if (CheckIfContained(vessel) == false)
            {
                var vD = BuildConfigNode(vessel);
                VesselInfo[vessel.id] = vD;
                //ConfigNode vesselData = BuildConfigNode(vessel);
                //VesselInformation.AddNode(vesselData);
            }

            //if (CheckIfContained(vessel))
            {
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

        }

        public static void UpdateActiveVesselData(Vessel vessel)
        {
            //ConfigNode VesselNode = new ConfigNode("VESSEL");
            bool found = false;

            found = VesselInfo.ContainsKey(vessel.id);
#if false
            foreach (ConfigNode node in VesselInformation.GetNodes("VESSEL")
            {
                if (node.GetValue("id") == vessel.id.ToString())
                {
                    VesselNode = node;
                    found = true;
                    break;
                }
            }
#endif

            if (found)
            {
                VesselInfo[vessel.id].Mass = vessel.GetTotalMass() * 1000;
                VesselInfo[vessel.id].Area = CalculateVesselArea(vessel);

                //VesselNode.SetValue("Mass", (vessel.GetTotalMass() * 1000).ToString());
                //VesselNode.SetValue("Area", CalculateVesselArea(vessel).ToString());
            }
            else
            {
                Log.Info($"UpdateActiveVesselData, vessel: {vessel.vesselName}  {vessel.id} not found");
                foreach (var v in VesselInfo.Keys)
                    Log.Info($"id: {v}");
            }
        }

        public static void ClearVesselData(Vessel vessel)
        {
            if (VesselInfo.ContainsKey(vessel.id))
                VesselInfo.Remove(vessel.id); ;
#if false
            ConfigNode VesselNode = new ConfigNode("VESSEL");
            bool found = false;
            foreach (ConfigNode node in VesselInformation.GetNodes("VESSEL"))
            {
                if (node.GetValue("id") == vessel.id.ToString())
                {
                    VesselNode = node;
                    found = true;
                    break;
                }

            }

            if (found)
            {
                VesselInformation.RemoveNode(VesselNode);
            }
#endif
        }

        public static Vessel_Information BuildConfigNode(Vessel vessel)
        {
            float Mass;
            double Area;
            Log.Info("BuildConfigNode");
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
#if false
            ConfigNode newVessel = new ConfigNode("VESSEL");
            newVessel.AddValue("name", vessel.GetName());
            newVessel.AddValue("id", vessel.id.ToString());
            newVessel.AddValue("persistence", HighLogic.SaveFolder.ToString());
            string CatalogueCode = vessel.vesselType.ToString().Substring(0, 1) + vessel.GetInstanceID().ToString();
            newVessel.AddValue("code", CatalogueCode);
            if (vessel == FlightGlobals.ActiveVessel)
            {
                newVessel.AddValue("Mass", (vessel.GetTotalMass() * 1000).ToString()); // 1.1.0 in kilograms!
                newVessel.AddValue("Area", CalculateVesselArea(vessel).ToString()); // Try?
            }
            else
            {
                newVessel.AddValue("Mass", (vessel.GetTotalMass() * 1000).ToString()); // getTotalMass returns bullshit for unloaded vessels.
                newVessel.AddValue("Area", CalculateVesselArea(vessel).ToString()); // Still getting bugs here
            }
            newVessel.AddValue("ReferenceBody", vessel.orbitDriver.orbit.referenceBody.GetName());
            newVessel.AddValue("SMA", vessel.GetOrbitDriver().orbit.semiMajorAxis);

            newVessel.AddValue("ECC", vessel.GetOrbitDriver().orbit.eccentricity);       // 1.4.0 greater information.
            newVessel.AddValue("INC", vessel.GetOrbitDriver().orbit.inclination);
            newVessel.AddValue("LPE", vessel.GetOrbitDriver().orbit.argumentOfPeriapsis);
            newVessel.AddValue("LAN", vessel.GetOrbitDriver().orbit.LAN);
            newVessel.AddValue("MNA", vessel.GetOrbitDriver().orbit.meanAnomalyAtEpoch);
            newVessel.AddValue("EPH", vessel.GetOrbitDriver().orbit.epoch);

            // newVessel.AddValue("StationKeeping", false.ToString());
            //151newVessel.AddValue("Fuel", ResourceManager.GetResources(vessel, ResourceName));
            //    newVessel.AddValue("Fuel", ResourceManager.GetResources(vessel));//151
            //    newVessel.AddValue("Resource", ResourceManager.GetResourceNames(vessel));//151

            return newVessel;
#endif
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
                            StationKeeping = bool.Parse(node.GetValue("IsStationKeeping"));
                            break;
                        }
                    }
                }
            }

            return StationKeeping;
        }


        public static double FetchFuelLost()
        {
            double FuelLost = 0;
            List<ModuleOrbitalDecay> modlist = FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleOrbitalDecay>();
            if (modlist.Count > 0)
            {
                FuelLost = modlist[0].stationKeepData.fuelLost;
            }
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
#if false
            ConfigNode Data = VesselInformation;
            bool Vesselfound = false;
            double Mass = 0.0;

            foreach (ConfigNode Vessel in Data.GetNodes("VESSEL"))
            {
                string id = Vessel.GetValue("id");
                if (id == vessel.id.ToString())
                {
                    Vesselfound = true;
                }

                if (Vesselfound)
                {
                    Mass = double.Parse(Vessel.GetValue("Mass"));
                    break;
                }
            }
            return Mass;
#endif
        }


        public static double FetchArea(Vessel vessel)
        {
            if (VesselInfo.ContainsKey(vessel.id))
            {
                if (VesselInfo[vessel.id].Area == 0)
                    Log.Info($"FetchArea: id: {vessel.id}   Area: {VesselInfo[vessel.id].Area}");
                return VesselInfo[vessel.id].Area;
            }
            Log.Info("FetchArea, vessel not found in VesselInfo");
            return 0;
#if false
            ConfigNode Data = VesselInformation;
            bool Vesselfound = false;
            double Area = 0.0;

            foreach (ConfigNode Vessel in Data.GetNodes("VESSEL"))
            {
                string id = Vessel.GetValue("id");
                if (id == vessel.id.ToString())
                {
                    Vesselfound = true;
                }

                if (Vesselfound)
                {
                    Area = double.Parse(Vessel.GetValue("Area"));
                    break;
                }
            }
            return Area;
#endif
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
#if false
            ConfigNode Data = VesselInformation;
            bool Vesselfound = false;

            foreach (ConfigNode Vessel in Data.GetNodes("VESSEL"))
            {
                string id = Vessel.GetValue("id");
                if (id == vessel.id.ToString())
                {
                    Vesselfound = true;
                }

                if (Vesselfound)
                {
                    Vessel.SetValue("SMA", SMA.ToString());
                    break;
                }
            }
#endif
        }

        public static double FetchSMA(Vessel vessel)
        {
            if (VesselInfo.ContainsKey(vessel.id))
                return VesselInfo[vessel.id].SMA;
            return 0;

#if false
            ConfigNode Data = VesselInformation;
            bool Vesselfound = false;
            double SMA = 0.0;

            foreach (ConfigNode Vessel in Data.GetNodes("VESSEL"))
            {
                string id = Vessel.GetValue("id");
                if (id == vessel.id.ToString())
                {
                    Vesselfound = true;
                }

                if (Vesselfound)
                {
                    SMA = double.Parse(Vessel.GetValue("SMA"));
                    break;
                }
            }

            return SMA;
#endif
        }

        public static void UpdateVesselECC(Vessel vessel, double ECC)
        {
            if (VesselInfo.ContainsKey(vessel.id))
                VesselInfo[vessel.id].ECC = ECC;
#if false
            ConfigNode Data = VesselInformation;
            bool Vesselfound = false;

            if (double.IsNaN(ECC)) // No NANs here please!
            {
                ECC = 0.0;
            }

            foreach (ConfigNode Vessel in Data.GetNodes("VESSEL"))
            {
                string id = Vessel.GetValue("id");
                if (id == vessel.id.ToString())
                {
                    Vesselfound = true;
                }

                if (Vesselfound)
                {
                    Vessel.SetValue("ECC", ECC.ToString());
                    break;
                }
            }
#endif
        }

        public static double FetchECC(Vessel vessel)
        {
            if (VesselInfo.ContainsKey(vessel.id))
                return VesselInfo[vessel.id].ECC;
            return 0;
#if false
            ConfigNode Data = VesselInformation;
            bool Vesselfound = false;
            double ECC = 0.0;

            foreach (ConfigNode Vessel in Data.GetNodes("VESSEL"))
            {
                string id = Vessel.GetValue("id");
                if (id == vessel.id.ToString())
                {
                    Vesselfound = true;
                }

                if (Vesselfound)
                {
                    ECC = double.Parse(Vessel.GetValue("ECC"));
                    break;
                }
            }
            if (double.IsNaN(ECC)) // No NANs here please!
            {
                ECC = 0.0;
            }
            return ECC;
#endif
        }

        public static void UpdateVesselINC(Vessel vessel, double INC)
        {
            if (VesselInfo.ContainsKey(vessel.id))
                VesselInfo[vessel.id].INC = INC;
#if false
            ConfigNode Data = VesselInformation;
            bool Vesselfound = false;

            foreach (ConfigNode Vessel in Data.GetNodes("VESSEL"))
            {
                string id = Vessel.GetValue("id");
                if (id == vessel.id.ToString())
                {
                    Vesselfound = true;
                }

                if (Vesselfound)
                {
                    Vessel.SetValue("INC", INC.ToString());
                    break;
                }
            }
#endif
        }

        public static double FetchINC(Vessel vessel)
        {
            if (VesselInfo.ContainsKey(vessel.id))
                return VesselInfo[vessel.id].INC;
            return 0;
#if false
            ConfigNode Data = VesselInformation;
            bool Vesselfound = false;
            double INC = 0.0;

            foreach (ConfigNode Vessel in Data.GetNodes("VESSEL"))
            {
                string id = Vessel.GetValue("id");
                if (id == vessel.id.ToString())
                {
                    Vesselfound = true;
                }

                if (Vesselfound)
                {
                    INC = double.Parse(Vessel.GetValue("INC"));
                    break;
                }
            }

            return INC;
#endif
        }

        public static void UpdateVesselLPE(Vessel vessel, double LPE)
        {
            if (VesselInfo.ContainsKey(vessel.id))
                VesselInfo[vessel.id].LPE = LPE;
#if false
            ConfigNode Data = VesselInformation;
            bool Vesselfound = false;

            foreach (ConfigNode Vessel in Data.GetNodes("VESSEL"))
            {
                string id = Vessel.GetValue("id");
                if (id == vessel.id.ToString())
                {
                    Vesselfound = true;
                }

                if (Vesselfound)
                {
                    Vessel.SetValue("LPE", LPE.ToString());
                    break;
                }
            }
#endif
        }

        public static double FetchLPE(Vessel vessel)
        {
            if (VesselInfo.ContainsKey(vessel.id))
                return VesselInfo[vessel.id].LPE;
            return 0;
#if false
            ConfigNode Data = VesselInformation;
            bool Vesselfound = false;
            double LPE = 0.0;

            foreach (ConfigNode Vessel in Data.GetNodes("VESSEL"))
            {
                string id = Vessel.GetValue("id");
                if (id == vessel.id.ToString())
                {
                    Vesselfound = true;
                }

                if (Vesselfound)
                {
                    LPE = double.Parse(Vessel.GetValue("LPE"));
                    break;
                }
            }

            return LPE;
#endif
        }

        public static void UpdateVesselLAN(Vessel vessel, double LAN)
        {
            if (VesselInfo.ContainsKey(vessel.id))
                VesselInfo[vessel.id].LAN = LAN;
#if false
            ConfigNode Data = VesselInformation;
            bool Vesselfound = false;

            foreach (ConfigNode Vessel in Data.GetNodes("VESSEL"))
            {
                string id = Vessel.GetValue("id");
                if (id == vessel.id.ToString())
                {
                    Vesselfound = true;
                }

                if (Vesselfound)
                {
                    Vessel.SetValue("LAN", LAN.ToString());
                    break;
                }
            }
#endif
        }

        public static double FetchLAN(Vessel vessel)
        {
            if (VesselInfo.ContainsKey(vessel.id))
                return VesselInfo[vessel.id].LAN;
            return 0;
#if false
            ConfigNode Data = VesselInformation;
            bool Vesselfound = false;
            double LAN = 0.0;

            foreach (ConfigNode Vessel in Data.GetNodes("VESSEL"))
            {
                string id = Vessel.GetValue("id");
                if (id == vessel.id.ToString())
                {
                    Vesselfound = true;
                }

                if (Vesselfound)
                {
                    LAN = double.Parse(Vessel.GetValue("LAN"));
                    break;
                }
            }

            return LAN;
#endif
        }

        public static void UpdateVesselMNA(Vessel vessel, double MNA)
        {
            if (VesselInfo.ContainsKey(vessel.id))
                VesselInfo[vessel.id].MNA = MNA;
#if false
            ConfigNode Data = VesselInformation;
            bool Vesselfound = false;

            foreach (ConfigNode Vessel in Data.GetNodes("VESSEL"))
            {
                string id = Vessel.GetValue("id");
                if (id == vessel.id.ToString())
                {
                    Vesselfound = true;
                }

                if (Vesselfound)
                {
                    Vessel.SetValue("MNA", MNA.ToString());
                    break;
                }
            }
#endif
        }

        public static double FetchMNA(Vessel vessel)
        {
            if (VesselInfo.ContainsKey(vessel.id))
                return VesselInfo[vessel.id].MNA;
            return 0;
#if false
            ConfigNode Data = VesselInformation;
            bool Vesselfound = false;
            double MNA = 0.0;

            foreach (ConfigNode Vessel in Data.GetNodes("VESSEL"))
            {
                string id = Vessel.GetValue("id");
                if (id == vessel.id.ToString())
                {
                    Vesselfound = true;
                }

                if (Vesselfound)
                {
                    MNA = double.Parse(Vessel.GetValue("MNA"));
                    break;
                }
            }

            return MNA;
#endif
        }

        public static void UpdateVesselEPH(Vessel vessel, double EPH)
        {
            if (VesselInfo.ContainsKey(vessel.id))
                VesselInfo[vessel.id].EPH = EPH;
#if false
            ConfigNode Data = VesselInformation;
            bool Vesselfound = false;

            foreach (ConfigNode Vessel in Data.GetNodes("VESSEL"))
            {
                string id = Vessel.GetValue("id");
                if (id == vessel.id.ToString())
                {
                    Vesselfound = true;
                }

                if (Vesselfound)
                {
                    Vessel.SetValue("EPH", EPH.ToString());
                    break;
                }
            }
#endif
        }

        public static double FetchEPH(Vessel vessel)
        {
            if (VesselInfo.ContainsKey(vessel.id))
                return VesselInfo[vessel.id].EPH;
            return 0;
#if false
            ConfigNode Data = VesselInformation;
            bool Vesselfound = false;
            double EPH = 0.0;

            foreach (ConfigNode Vessel in Data.GetNodes("VESSEL"))
            {
                string id = Vessel.GetValue("id");
                if (id == vessel.id.ToString())
                {
                    Vesselfound = true;
                }

                if (Vesselfound)
                {
                    EPH = double.Parse(Vessel.GetValue("EPH"));
                    break;
                }
            }

            return EPH;
#endif
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
                            Efficiency = float.Parse(node.GetValue("ISP"));
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
                VesselInfo[vessel.id].ReferenceBody = body.GetName();
            }
#if false
            ConfigNode Data = VesselInformation;
            bool Vesselfound = false;

            foreach (ConfigNode Vessel in Data.GetNodes("VESSEL"))
            {
                string id = Vessel.GetValue("id");
                if (id == vessel.id.ToString())
                {
                    Vesselfound = true;
                }

                if (Vesselfound)
                {
                    Vessel.SetValue("ReferenceBody", body.GetName());
                    break;
                }
            }
#endif
        }

        public static void UpdateVesselFuel(Vessel vessel, double Fuel)
        {
            if (VesselInfo.ContainsKey(vessel.id))
            {
                VesselInfo[vessel.id].Fuel = Fuel;
            }
#if false
            ConfigNode Data = VesselInformation;
            bool Vesselfound = false;

            foreach (ConfigNode Vessel in Data.GetNodes("VESSEL"))
            {
                string id = Vessel.GetValue("id");
                if (id == vessel.id.ToString())
                {
                    Vesselfound = true;
                }

                if (Vesselfound)
                {
                    Vessel.SetValue("Fuel", Fuel.ToString());
                    break;
                }
            }
#endif
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
                if (vessel == FlightGlobals.ActiveVessel)
                {
                    Area = Area + part.partRef.radiativeArea;
                }
                else
                {
                    Area = Area + part.partInfo.partSize * 2.0 * Math.PI;
                }
            }

            return Area / 4.0; // only one side facing prograde
        }
    }
}

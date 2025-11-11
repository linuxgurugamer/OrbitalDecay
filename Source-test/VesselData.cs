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

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WhitecatIndustries
{
    public class VesselDataClass
    {
        internal string name;
        internal Guid id;
        internal string persistence;
        internal string code; // change to catalogueCode when complete
        internal double Mass;
        internal double Area;
        internal CelestialBody ReferenceBody;
        internal string ReferenceBodyName;
        internal double SMA;
        internal double ECC;
        internal double INC;
        internal double LPE;
        internal double LAN;
        internal double MNA;
        internal double EPH;

        //internal double Fuel;

        internal VesselDataClass(Vessel v)
        {
            name = v.name;
            id = v.id;
            persistence = HighLogic.SaveFolder;
            string CatalogueCode = v.vesselType.ToString().Substring(0, 1) + v.GetInstanceID().ToString();
            code = CatalogueCode;
            Mass = v.GetCorrectVesselMass() * 1000;
            Area = v.CalculateVesselArea();
            ReferenceBody = v.orbitDriver.orbit.referenceBody;
            ReferenceBodyName = v.orbitDriver.orbit.referenceBody.GetName();

            SMA = v.GetOrbitDriver().orbit.semiMajorAxis;
            ECC = v.GetOrbitDriver().orbit.eccentricity;
            INC = v.GetOrbitDriver().orbit.inclination;
            LPE = v.GetOrbitDriver().orbit.argumentOfPeriapsis;
            LAN = v.GetOrbitDriver().orbit.LAN;
            MNA = v.GetOrbitDriver().orbit.meanAnomalyAtEpoch;
            EPH = v.GetOrbitDriver().orbit.epoch;

            // Fuel = ???;
        }

        internal VesselDataClass(ConfigNode node)
        {
            name = node.GetValue("name");
            id = Guid.Parse(node.GetValue("id"));
            persistence = node.GetValue("persistence");
            code = node.GetValue("code");
            Mass = Double.Parse(node.GetValue("Mass"));
            Area = Double.Parse(node.GetValue("Area"));
            ReferenceBodyName = node.GetValue("ReferenceBodyName");
            SMA = Double.Parse(node.GetValue("SMA"));
            ECC = Double.Parse(node.GetValue("ECC"));
            INC = Double.Parse(node.GetValue("INC"));
            LPE = Double.Parse(node.GetValue("LPE"));
            LAN = Double.Parse(node.GetValue("LAN"));
            MNA = Double.Parse(node.GetValue("MNA"));
            EPH = Double.Parse(node.GetValue("EPH"));
        }

        internal ConfigNode GetConfigNode()
        {
            ConfigNode newVessel = new ConfigNode("VESSEL");
            newVessel.AddValue("name", name);
            newVessel.AddValue("id", id.ToString());
            newVessel.AddValue("persistence", persistence);

            newVessel.AddValue("code", code);
            newVessel.AddValue("Mass", Mass); // 1.1.0 in kilograms!
            newVessel.AddValue("Area", Area); // Try?
            newVessel.AddValue("ReferenceBodyName", ReferenceBodyName);
            newVessel.AddValue("SMA", SMA);

            newVessel.AddValue("ECC", ECC);       // 1.4.0 greater information.
            newVessel.AddValue("INC", INC);
            newVessel.AddValue("LPE", LPE);
            newVessel.AddValue("LAN", LAN);
            newVessel.AddValue("MNA", MNA);
            newVessel.AddValue("EPH", EPH);

            // newVessel.AddValue("StationKeeping", false.ToString());
            //151newVessel.AddValue("Fuel", ResourceManager.GetResources(vessel, ResourceName));
            //    newVessel.AddValue("Fuel", ResourceManager.GetResources(vessel));//151
            //    newVessel.AddValue("Resource", ResourceManager.GetResourceNames(vessel));//151

            return newVessel;
        }

    }




    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class VesselData : MonoBehaviour
    {


        internal static Dictionary<Guid, VesselDataClass> VesselInformationDict = new Dictionary<Guid, VesselDataClass>();

        //public static ConfigNode VesselInformation = new ConfigNode();
        public static string FilePath;
        public static ConfigNode File;

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

            LoadVesselInformation();
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
            print("WhitecatIndustries - Orbital Decay - Loaded vessel data.");
        }
    
        void LoadVesselInformation()
        {
            File = ConfigNode.Load(FilePath);
            VesselInformationDict.Clear();

            foreach (var node in File.GetNodes("VESSEL"))
            {
                var vi = new VesselDataClass(node);
                VesselInformationDict.Add(vi.id, vi);
            }
        }

        static void SaveVesselInformation()
        {
            File = new ConfigNode();
            foreach (var vdc in VesselInformationDict.Values)
            {
                File.AddNode("VESSEL", vdc.GetConfigNode());
            }
            File.Save(FilePath);
        }

        public void FixedUpdate()
        {
            if (Time.timeSinceLevelLoad > 1 && VesselsLoaded)
            {
                if (Time.time - lastUpdate > UPTInterval) // 1.4.0 Lag Busting
                {
                    lastUpdate = Time.time;

#if false
                    if (HighLogic.LoadedSceneIsGame && 
                        HighLogic.LoadedScene != GameScenes.LOADING && 
                        HighLogic.LoadedScene != GameScenes.LOADINGBUFFER && 
                        HighLogic.LoadedScene != GameScenes.MAINMENU)
#else
                    if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedScene != GameScenes.TRACKSTATION || HighLogic.LoadedScene == GameScenes.SPACECENTER)
#endif
                    {
                        Vessel vessel = null;
                        for (int i = 0; i < FlightGlobals.Vessels.Count; i++)
                        {
                            vessel = FlightGlobals.Vessels[i];
                            {
                                if (VesselInformationDict.ContainsKey(vessel.id))
                                //if (CheckIfContained(vessel))
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
                        }
                    }
                }
            }
        }

        public void OnDestroy()
        {
            if (DecayManager.CheckSceneStateMain(HighLogic.LoadedScene))
            {
                if (Planetarium.GetUniversalTime() == HighLogic.CurrentGame.UniversalTime || HighLogic.LoadedScene == GameScenes.FLIGHT)
                {
                    print("WhitecatIndustries - Orbital Decay - Vessel Information saved. Ondestroy");
                    SaveVesselInformation();
#if false
                    File.ClearNodes();
                    VesselInformation.Save(FilePath);
                   // VesselInformation.ClearNodes();
#endif
                }
            }
        }

        public static void OnQuickSave()
        {
            if (DecayManager.CheckSceneStateMain(HighLogic.LoadedScene))
            {
                if (Planetarium.GetUniversalTime() == HighLogic.CurrentGame.UniversalTime || HighLogic.LoadedScene == GameScenes.FLIGHT)
                {
                    print("WhitecatIndustries - Orbital Decay - Vessel Information saved.");
                    SaveVesselInformation();
#if false
                    File.ClearNodes();
                    VesselInformation.Save(FilePath);
                    VesselInformation.ClearNodes();
#endif
                    print("WhitecatIndustries - Orbital Decay - Vessel Information lost OnQuickSave");
                }
            }
        }

        public static void OnQuickLoad() // 1.5.3 quick load fixes
        {
            VesselInformationDict.Clear();
#if false
            File.ClearNodes();
            VesselInformation.ClearNodes();
#endif
            print("WhitecatIndustries - Orbital Decay - Vessel Information lost OnQuickLoad");

        }

#if false
        public static bool CheckIfContained(Vessel vessel)
        {
            bool Contained = false;

            foreach (ConfigNode node in VesselInformation.GetNodes("VESSEL"))
            {
                if (node.GetValue("id") == vessel.id.ToString())
                {
                    Contained = true;
                }
            }
            return Contained;
        }
#endif

        public static void WriteVesselData(Vessel vessel)
        {
            if (!VesselInformationDict.ContainsKey(vessel.id))
            {
                //ConfigNode vesselData = BuildConfigNode(vessel);
                //VesselInformation.AddNode(vesselData);
                VesselInformationDict.Add(vessel.id, new VesselDataClass(vessel));
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
            VesselDataClass vdc;
            if (VesselInformationDict.TryGetValue(vessel.id, out vdc))
            {
                vdc.Mass = vessel.GetCorrectVesselMass() * 1000;
                vdc.Area = vessel.CalculateVesselArea();
            }
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
                VesselNode.SetValue("Mass", (vessel.GetCorrectVesselMass() * 1000).ToString());
                
                VesselNode.SetValue("Area", CalculateVesselArea(vessel).ToString());
            }
#endif
        }

        public static void ClearVesselData(Vessel vessel)
        {
            VesselDataClass vdc;
            if (VesselInformationDict.TryGetValue(vessel.id, out vdc))
            {
                VesselInformationDict.Remove(vessel.id);
            }

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

#if false
        public static ConfigNode BuildConfigNode(Vessel vessel)
        {
            ConfigNode newVessel = new ConfigNode("VESSEL");
            newVessel.AddValue("name", vessel.GetName());
            newVessel.AddValue("id", vessel.id.ToString());
            newVessel.AddValue("persistence", HighLogic.SaveFolder.ToString());
            string CatalogueCode = vessel.vesselType.ToString().Substring(0, 1) + vessel.GetInstanceID().ToString();
            newVessel.AddValue("code", CatalogueCode);
            if (vessel == FlightGlobals.ActiveVessel)
            {
                newVessel.AddValue("Mass", (vessel.GetCorrectVesselMass() * 1000).ToString()); // 1.1.0 in kilograms!
                newVessel.AddValue("Area", CalculateVesselArea(vessel).ToString()); // Try?
            }
            else
            {
                newVessel.AddValue("Mass", (vessel.GetCorrectVesselMass() * 1000).ToString()); // getTotalMass returns bullshit for unloaded vessels.
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
        }
#endif

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
            foreach(ModuleOrbitalDecay module in modlist )
            {
                module.stationKeepData.fuelLost = FuelLost;
            }
        }


        public static double FetchMass(Vessel vessel)
        {
            VesselDataClass vdc;
            if (VesselInformationDict.TryGetValue(vessel.id, out vdc))
            {
                return vdc.Mass;
            }
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
            VesselDataClass vdc;
            if (VesselInformationDict.TryGetValue(vessel.id, out vdc))
            {
                return vdc.Area;
            }
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
            VesselDataClass vdc;
            if (VesselInformationDict.TryGetValue(vessel.id, out vdc))
            {
                vdc.SMA = SMA;
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
                    Vessel.SetValue("SMA", SMA.ToString());
                    break;
                }
            }
#endif
        }

        public static double FetchSMA(Vessel vessel)
        {
            VesselDataClass vdc;
            if (VesselInformationDict.TryGetValue(vessel.id, out vdc))
            {
                return vdc.SMA;
            }
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
            if (double.IsNaN(ECC)) // No NANs here please!
                ECC = 0.0;
            VesselDataClass vdc;
            if (VesselInformationDict.TryGetValue(vessel.id, out vdc))
            {
                vdc.ECC = ECC;
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
                    Vessel.SetValue("ECC", ECC.ToString());
                    break;
                }
            }
#endif
        }

        public static double FetchECC(Vessel vessel)
        {
            VesselDataClass vdc;
            if (VesselInformationDict.TryGetValue(vessel.id, out vdc))
            {
                return vdc.ECC;
            }
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
            VesselDataClass vdc;
            if (VesselInformationDict.TryGetValue(vessel.id, out vdc))
            {
                vdc.INC = INC;
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
                    Vessel.SetValue("INC", INC.ToString());
                    break;
                }
            }
#endif
        }

        public static double FetchINC(Vessel vessel)
        {
            VesselDataClass vdc;
            if (VesselInformationDict.TryGetValue(vessel.id, out vdc))
            {
                return vdc.INC;
            }
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
            VesselDataClass vdc;
            if (VesselInformationDict.TryGetValue(vessel.id, out vdc))
            {
                vdc.LPE = LPE;
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
                    Vessel.SetValue("LPE", LPE.ToString());
                    break;
                }
            }
#endif
        }

        public static double FetchLPE(Vessel vessel)
        {
            VesselDataClass vdc;
            if (VesselInformationDict.TryGetValue(vessel.id, out vdc))
            {
                return vdc.LPE;
            }
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
            VesselDataClass vdc;
            if (VesselInformationDict.TryGetValue(vessel.id, out vdc))
            {
                vdc.LAN = LAN;
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
                    Vessel.SetValue("LAN", LAN.ToString());
                    break;
                }
            }
#endif
        }

        public static double FetchLAN(Vessel vessel)
        {
            VesselDataClass vdc;
            if (VesselInformationDict.TryGetValue(vessel.id, out vdc))
            {
                return vdc.LAN;
            }
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
            VesselDataClass vdc;
            if (VesselInformationDict.TryGetValue(vessel.id, out vdc))
            {
                vdc.MNA = MNA;
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
                    Vessel.SetValue("MNA", MNA.ToString());
                    break;
                }
            }
#endif
        }

        public static double FetchMNA(Vessel vessel)
        {
            VesselDataClass vdc;
            if (VesselInformationDict.TryGetValue(vessel.id, out vdc))
            {
                return vdc.MNA;
            }
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
            VesselDataClass vdc;
            if (VesselInformationDict.TryGetValue(vessel.id, out vdc))
            {
                vdc.EPH = EPH;
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
                    Vessel.SetValue("EPH", EPH.ToString());
                    break;
                }
            }
#endif
        }

        public static double FetchEPH(Vessel vessel)
        {
            VesselDataClass vdc;
            if (VesselInformationDict.TryGetValue(vessel.id, out vdc))
            {
                return vdc.EPH;
            }
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
                List<ModuleOrbitalDecay> modlist  = vessel.FindPartModulesImplementing<ModuleOrbitalDecay>();
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
            

            return 1/Efficiency;
        }

        public static void UpdateBody(Vessel vessel, CelestialBody body)
        {
            VesselDataClass vdc;
            if (VesselInformationDict.TryGetValue(vessel.id, out vdc))
            {
                vdc.ReferenceBody = body;
                vdc.ReferenceBodyName = body.name;
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

#if false
        public static void UpdateVesselFuel(Vessel vessel, double Fuel)
        {
            VesselDataClass vdc;
            if (VesselInformationDict.TryGetValue(vessel.id, out vdc))
            {
                vdc.Fuel = Fuel;
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
#endif
    }
}

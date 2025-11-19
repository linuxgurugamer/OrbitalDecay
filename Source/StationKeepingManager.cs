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
using UnityEngine;

namespace OrbitalDecay
{

    public class StationKeepingManager : MonoBehaviour
    {
        public static bool EngineCheck(Vessel vessel) // 1.3.0
        {

            bool HasEngine = false;
            bool HasRCS = false;

            if (vessel != FlightGlobals.ActiveVessel)
            {
                ProtoVessel protovessel = vessel.protoVessel;
                List<ProtoPartSnapshot> PPSL = protovessel.protoPartSnapshots;

                foreach (ProtoPartSnapshot PPS in PPSL)
                {
                    List<ProtoPartModuleSnapshot> PPMSL = PPS.modules;
                    foreach (ProtoPartModuleSnapshot PPMS in PPMSL)
                    {
                        if (PPMS.moduleName.Contains("ModuleRCS"))
                        {
                            if (!bool.Parse(PPMS.moduleValues.GetValue("rcsEnabled"))) continue;
                            HasRCS = true;
                            //break;
                        }
                        if (PPMS.moduleName.Contains("ModuleEngines")) // 1.5.0 Part Module Fixes
                        {
                            if (bool.Parse(PPMS.moduleValues.GetValue("EngineIgnited")))
                            {
                                HasEngine = true;
                                //break;
                            }
                        }
                    }

                    if (HasEngine || HasRCS)
                    {
                        break;
                    }
                }
            }
            else
            {
                if (vessel.FindPartModulesImplementing<ModuleEngines>().Count > 0 ||
                    vessel.FindPartModulesImplementing<ModuleEnginesFX>().Count > 0 ||
                    vessel.FindPartModulesImplementing<ModuleRCS>().Count > 0 ||
                    vessel.FindPartModulesImplementing<ModuleRCSFX>().Count > 0)
                {
                    HasEngine = true;
                }
            }
            return HasEngine || HasRCS;
        }

        public static void FuelManager(Vessel vessel)
        {
            string ResourceName = "";
            ResourceName = ResourceManager.GetResourceNames(vessel);
            double CurrentFuel = 0;
            CurrentFuel = ResourceManager.GetResources(vessel);
            double ResourceEfficiency = VesselData.FetchEfficiency(vessel);// effi calculation based on vessel engine ISP stored in stationKeepData.ISP NEED ballance
            double LostFuel = 0.0;

            LostFuel = Math.Abs(DecayManager.DecayRateAtmosphericDrag(vessel) + DecayManager.DecayRateRadiationPressure(vessel) + DecayManager.DecayRateYarkovskyEffect(vessel)) * Settings.ReadResourceRateDifficulty() * ResourceEfficiency; // * Fuel Multiplier

            double FuelNew = CurrentFuel - LostFuel;

            // Remote tech compatibility - 1.6.0 // 
            bool RemoteTechInstalled = LoadingCheck.RemoteTechInstalled;
            bool SignalCheck = false;
            bool ConnectionToVessel = true;

            if (RemoteTechInstalled)
            {

                // Add Part Module Checks Here // Maybe 1.6.0 


                if (ConnectionToVessel == false)
                {
                    SignalCheck = false;
                    ScreenMessages.PostScreenMessage("Warning: " + vessel.vesselName + " has no connection to send Station Keeping command!");
                    VesselData.UpdateStationKeeping(vessel, false);
                }
            }
            else
            {
                SignalCheck = true;
            }

            if (!SignalCheck) return;
            if (EngineCheck(vessel) == false) // 1.3.0
            {
                ScreenMessages.PostScreenMessage("Warning: " + vessel.vesselName + " has no operational Engines or RCS modules, Station Keeping disabled.");
                VesselData.UpdateStationKeeping(vessel, false);
            }

            else if (EngineCheck(vessel))
            {
                if (FuelNew <= 0)
                {
                    ScreenMessages.PostScreenMessage("Warning: " + vessel.vesselName + " has run out of " + ResourceName + ", Station Keeping disabled.");
                    VesselData.UpdateStationKeeping(vessel, false);
                    VesselData.UpdateVesselFuel(vessel, 0);
                }
                else
                {
                    VesselData.UpdateVesselFuel(vessel, FuelNew);
                    ResourceManager.RemoveResources(vessel, LostFuel);
                }
            }
        }
    }
}

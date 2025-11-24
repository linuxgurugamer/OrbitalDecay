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
using System.Linq;
using UnityEngine;

using static OrbitalDecay.RegisterToolbar;

namespace OrbitalDecay
{
    public class ResourceManager : MonoBehaviour
    {

        public static void RemoveResources(Vessel vessel, double quantity)//151 new wersion consuming multiple resources saved on vessel
        {
            string resource = GetResourceNames(vessel);
            int index = 0;
            if (vessel == FlightGlobals.ActiveVessel)
            {

                foreach (string res in resource.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    float ratio = GetResourceRatio(vessel, index++);
                    int MonoPropId = PartResourceLibrary.Instance.GetDefinition(res).id;
                    vessel.rootPart.RequestResource(MonoPropId, quantity / 2 * ratio, ResourceFlowMode.STAGE_PRIORITY_FLOW);
                }
            }
            else
            {
                ProtoVessel proto = vessel.protoVessel;

                foreach (ProtoPartSnapshot protopart in proto.protoPartSnapshots)
                {
                    foreach (ProtoPartModuleSnapshot protopartmodulesnapshot in protopart.modules)
                    {
                        if (protopartmodulesnapshot.moduleName != "ModuleOrbitalDecay") continue;
                        ConfigNode node = protopartmodulesnapshot.moduleValues.GetNode("stationKeepData");
                        double d = node.SafeLoad("fuelLost", 0d);
                        node.SetValue("fuelLost", (quantity + d).ToString());
                        break;
                    }
                }
            }
        }

        public static string GetResourceNames(Vessel vessel)//151 
        {
            string ResourceNames = "No Resources Available";
            if (vessel == FlightGlobals.ActiveVessel)
            {
                List<ModuleOrbitalDecay> modlist = vessel.FindPartModulesImplementing<ModuleOrbitalDecay>();
                if (modlist.Count > 0)
                    ResourceNames = modlist[0].StationKeepResources;
            }
            else
            {
                ProtoVessel proto = vessel.protoVessel;

                foreach (ProtoPartSnapshot protopart in proto.protoPartSnapshots)
                {
                    foreach (ProtoPartModuleSnapshot protopartmodulesnapshot in protopart.modules)
                    {
                        if (protopartmodulesnapshot.moduleName != "ModuleOrbitalDecay") continue;
                        ConfigNode node = protopartmodulesnapshot.moduleValues.GetNode("stationKeepData");
                        ResourceNames = node.GetValue("resources");
                        break;
                    }
                }
            }
            return ResourceNames;
        }
        public static float GetResourceRatio(Vessel vessel, int index)//151 
        {
            float ResourceRatio = 0;
            if (vessel == FlightGlobals.ActiveVessel)
            {
                List<ModuleOrbitalDecay> modlist = vessel.FindPartModulesImplementing<ModuleOrbitalDecay>();
                if (modlist.Count > 0)
                    ResourceRatio = modlist.ElementAt(0).stationKeepData.ratios[index];
            }
            else
            {
                ProtoVessel proto = vessel.protoVessel;

                foreach (ProtoPartSnapshot protopart in proto.protoPartSnapshots)
                {
                    foreach (ProtoPartModuleSnapshot protopartmodulesnapshot in protopart.modules)
                    {
                        if (protopartmodulesnapshot.moduleName != "ModuleOrbitalDecay") continue;
                        ConfigNode node = protopartmodulesnapshot.moduleValues.GetNode("stationKeepData");
                        int i = 0;

                        foreach (string str in node.GetValue("ratios").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (i == index)
                            {
                                ResourceRatio = float.TryParse(str, out float v) ? v : 0f;
                                break;
                            }
                            i++;
                        }
                        break;
                    }
                }
            }
            return ResourceRatio;
        }

        public static double GetResources(Vessel vessel)//returns sum of used resources
        {
            double fuel = 0;
            if (vessel == FlightGlobals.ActiveVessel)
            {
                List<ModuleOrbitalDecay> modlist = vessel.FindPartModulesImplementing<ModuleOrbitalDecay>();
                foreach (ModuleOrbitalDecay module in modlist)
                {
                    for (int i = 0; i < module.stationKeepData.amounts.Count(); i++)
                    {
                        //Log.Info($"Resource[{i}]  {module.stationKeepData.resources[i]}   fuel: {module.stationKeepData.amounts[i]}");
                        fuel += module.stationKeepData.amounts[i];
                    }
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
                        if (protopartmodulesnapshot.moduleName != "ModuleOrbitalDecay" || fuel != 0) continue;
                        ConfigNode node = protopartmodulesnapshot.moduleValues.GetNode("stationKeepData");

                        string amounts = node.SafeLoad("amounts", "");
                        var ar = amounts.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)   // split on whitespace
                         .Select(token =>
                         {
                             double v;
                             return double.TryParse(token, out v) ? v : 0f;
                         }).ToArray();

                        for (int i = 0; i < ar.Length; i++)
                            fuel += ar[i];
                        if (node.HasValue("fuelLost"))
                        {
                            double d = node.SafeLoad("fuelLost", 0);
                            fuel -= d;
                        }
                        break;
                    }
                }
            }
            return fuel;
        }
    }
}

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

namespace OrbitalDecay
{
    public class Propellant_Data
    {
        public string name;
        public int id;
        public float ratio;
        public double Available;

        public Propellant_Data(string name, int id, float ratio, double Available)
        {
            this.name = name;
            this.id = id;
            this.ratio = ratio;
            this.Available = Available;
        }
    }

    public class Engine_Data
    {
        public string name;
        public float ISP;

        public List<Propellant_Data> propellantData = new List<Propellant_Data>();

        public Engine_Data(string name, float ISP)
        {
            this.name = name;
            this.ISP = ISP;
        }
    }

    public class ModuleOrbitalDecay : PartModule
    {

        [KSPField(isPersistant = false, guiActive = true, guiActiveEditor = false, guiName = "Use")]
        public string ODSKengine = "";
        [KSPField(isPersistant = false, guiActive = true, guiName = "Resources")]
        public string StationKeepResources;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Available")]
        public string amounts;
        [KSPField(isPersistant = false, guiActive = true, guiName = "ISP")]
        public float ISP;
        [KSPField(isPersistant = true, guiActive = false)]
        public int EngineIndex;

        [KSPField(isPersistant = true)]
        public StationKeepData stationKeepData;

        Dictionary<string, Engine_Data> engineData = new Dictionary<string, Engine_Data>();
        //public ConfigNode EngineData = new ConfigNode();
        public string[] EngineList = { "" };

        private float UPTInterval = 1.0f;
        private float lastUpdate;

        [KSPEvent(active = true, guiActive = true, guiName = "Enable Station Keeping")]
        public void ToggleSK()
        {
            VesselData.UpdateStationKeeping(vessel, VesselData.FetchStationKeeping(vessel));
            stationKeepData.IsStationKeeping = !stationKeepData.IsStationKeeping;
            updatedisplayedData();

        }
        [KSPEvent(active = true, guiActive = true, guiName = "Next engine")]
        public void NextEngine()
        {
            if (EngineList.Count() > 0)
            {
                EngineIndex++;
                if (EngineIndex >= EngineList.Count())
                {
                    EngineIndex = 0;
                }
                updatedisplayedData();
            }
        }

        [KSPEvent(active = true, guiActive = true, guiName = "Previous engine")]
        public void PreviousEngine()
        {
            if (EngineList.Count() > 0)
            {
                EngineIndex--;
                if (EngineIndex <= 0)
                {
                    EngineIndex = EngineList.Count() - 1;
                }
                updatedisplayedData();
            }
        }



        public ModuleOrbitalDecay()
        {

            if (stationKeepData == null)
            {
                stationKeepData = new StationKeepData();

            }
        }

        public override void OnStart(StartState state)
        {
            BaseEvent even = Events["ToggleSK"];
            if (stationKeepData.IsStationKeeping)
            {
                even.guiName = "Disable Station Keeping";

            }
            else
            {
                even.guiName = "Enable Station Keeping";
            }


        }


        public void updatedisplayedData()
        {

            foreach (ModuleOrbitalDecay module in vessel.FindPartModulesImplementing<ModuleOrbitalDecay>())
            {
                module.stationKeepData.IsStationKeeping = stationKeepData.IsStationKeeping;
                module.EngineIndex = EngineIndex;
            }

            BaseEvent even = Events["ToggleSK"];
            even.guiName = stationKeepData.IsStationKeeping ? "Disable Station Keeping" : "Enable Station Keeping";
            if (vessel.situation == Vessel.Situations.ORBITING || vessel.situation == Vessel.Situations.SUB_ORBITAL)
            {
                even.guiActive = true;
            }
            else
            {
                even.guiActive = false;
            }


            List<string> proplist = new List<string>();
            List<double> amountlist = new List<double>();
            List<float> ratiolist = new List<float>();

            ConfigNode engineNode = new ConfigNode();
            if (engineData.ContainsKey(ODSKengine))
            {
                var engine = engineData[ODSKengine];

                foreach (var p in engine.propellantData)
                {
                    proplist.Add(p.name);
                    amountlist.Add(p.Available);
                    ratiolist.Add(p.ratio);
                }
                stationKeepData.ISP = engine.ISP;
            }
            else
            {
                proplist.Add("No Resoures Available");
                amountlist.Add(0);
                ratiolist.Add(0);
                stationKeepData.ISP = 0;
            }

            stationKeepData.resources = new string[proplist.Capacity];
            stationKeepData.amounts = new double[amountlist.Capacity];
            stationKeepData.ratios = new float[ratiolist.Capacity];
            stationKeepData.resources = proplist.ToArray();
            stationKeepData.amounts = amountlist.ToArray();
            stationKeepData.ratios = ratiolist.ToArray();


            lastUpdate = Time.time - UPTInterval;

            ODSKengine = EngineIndex < EngineList.Length ? EngineList[EngineIndex] : EngineList[EngineList.Length - 1];

        }

        public void FetchEngineData()
        {
            //EngineData.RemoveNodes("ENGINE");
            engineData.Clear();
            for (int i = 0; i < vessel.parts.Count; i++)
            {
                var part = vessel.parts[i];
                for (int j = 0; j < part.Modules.Count; j++)
                {

                    if (part.Modules[j].moduleName.StartsWith("ModuleEngines"))
                    {
                        ModuleEngines module = part.Modules[j] as ModuleEngines;
                        if (module.EngineIgnited && !engineData.ContainsKey(module.part.protoPartSnapshot.partInfo.title))
                        {
                            engineData.Add(module.part.protoPartSnapshot.partInfo.title,
                                new Engine_Data(module.part.protoPartSnapshot.partInfo.title, module.atmosphereCurve.Evaluate(0)));
                            foreach (Propellant propellant in module.propellants)
                            {
                                if (propellant.name == "ElectricCharge") continue;
                                var p = new Propellant_Data(propellant.name,
                                                            propellant.id,
                                                            propellant.ratio,
                                                            fetchPartResource(module.part, propellant.id, ResourceFlowMode.STAGE_PRIORITY_FLOW)
                                    );
                                engineData[module.part.protoPartSnapshot.partInfo.title].propellantData.Add(p);
                            }

                        }
                        if (stationKeepData.IsStationKeeping && module.currentThrottle > 0.0)
                        {
                            ScreenMessages.PostScreenMessage("Warning: Vessel is under thrust, station keeping disabled.");
                            VesselData.UpdateStationKeeping(vessel, false);
                        }
                    }

                    if (part.Modules[j].moduleName.StartsWith("ModuleRCS"))
                    {
                        ModuleRCS module = part.Modules[j] as ModuleRCS;
                        if ( /* !module.rcsEnabled && */ !engineData.ContainsKey(module.part.protoPartSnapshot.partInfo.title))
                        {
                            engineData.Add(module.part.protoPartSnapshot.partInfo.title,
                                new Engine_Data(module.part.protoPartSnapshot.partInfo.title, module.atmosphereCurve.Evaluate(0)));
                            foreach (Propellant propellant in module.propellants)
                            {
                                if (propellant.name == "ElectricCharge") continue;
                                var p = new Propellant_Data(propellant.name,
                                                            propellant.id,
                                                            propellant.ratio,
                                                            fetchPartResource(module.part, propellant.id, ResourceFlowMode.STAGE_PRIORITY_FLOW)
                                    );
                                engineData[module.part.protoPartSnapshot.partInfo.title].propellantData.Add(p);
                            }

                        }
                    }
                }
            }
        }

        public override void OnUpdate()
        {
            if (stationKeepData.IsStationKeeping && vessel == FlightGlobals.ActiveVessel)
            {
                foreach (ModuleEngines module in vessel.FindPartModulesImplementing<ModuleEngines>())
                {
                    if (module.currentThrottle == 0) continue;
                    ScreenMessages.PostScreenMessage("Warning: Vessel is under thrust, station keeping disabled.");
                    VesselData.UpdateStationKeeping(vessel, false);
                    break;
                }
            }

            if (!(Time.time - lastUpdate > UPTInterval)) return;
            lastUpdate = Time.time;
            FetchEngineData();

            List<string> namelist = new List<string>();
            namelist = engineData.Keys.ToList();

            if (namelist.Count > 0)
            {
                EngineList = namelist.ToArray();
            }
            else
            {
                EngineList = new string[] { "None Available" };
            }
            updatedisplayedData();

            for (int i = 0; i < stationKeepData.resources.Count(); i++)
            {
                float ratio1 = 10 * stationKeepData.ratios[i];
                for (int j = 0; j < stationKeepData.resources.Count(); j++)
                {
                    float ratio2 = 10 * stationKeepData.ratios[j];
                    if (stationKeepData.amounts[i] / ratio1 < stationKeepData.amounts[j] / ratio2)
                        stationKeepData.amounts[j] = stationKeepData.amounts[i] / ratio1 * ratio2;
                    /*equalizing fuel amount to comply with consumption ratios
                         * without mutliplying ratios by 10 result is acurate only to 4th position after digital point 
                         * or 7th position in total for huge amounts of fuel
                         * 179.999991330234 instead of 180 - tremendous error, i know ;)
                         * tried casting double type to each variable in equasion
                         * bu multiplying ratios seems to be only working solution
                         * its a math issue/limitation encountered in multiple compilers
                         * *************************************************************/
                }
            }

            ISP = stationKeepData.ISP;

            StationKeepResources = string.Join(" ", stationKeepData.resources);
            amounts = string.Join(" ", stationKeepData.amounts.Select(v => v.ToString("F3")));
        }


        private static double fetchPartResource(Part part, int Id, ResourceFlowMode flowMode)
        {
            List<PartResource> Resources = new List<PartResource>();

            part.GetConnectedResourceTotals(Id, out double amount, out double MaxAmount);

            if (Resources.Count <= 0) return amount;
            foreach (PartResource Res in Resources)
            {
                amount += Res.amount;
            }
            return amount;
        }





    }
    [Serializable]
    public class StationKeepData : IConfigNode
    {
        [SerializeField]
        public bool IsStationKeeping;
        [SerializeField]
        public string engine = "";
        [SerializeField]
        public string[] resources = { "" };
        [SerializeField]
        public double[] amounts = { 0 };
        [SerializeField]
        public double fuelLost;
        [SerializeField]
        public float[] ratios = { 0 };
        [SerializeField]
        public float ISP;
        [SerializeField]
        public double Area = 0;

        public void Load(ConfigNode node)
        {
            IsStationKeeping = node.SafeLoad("IsStationKeeping", false);

            resources = node.SafeLoad("resources", "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            amounts = node.SafeLoad("amounts", "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)   // split on whitespace
                .Select(token =>
                {
                    double v;
                    return double.TryParse(token, out v) ? v : 0f;
                }).ToArray();

            ratios = node.SafeLoad("ratios", "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)   // split on whitespace
                .Select(token =>
                {
                    float v;
                    return float.TryParse(token, out v) ? v : 0f;
                }).ToArray();

            fuelLost = node.SafeLoad("fuelLost", 0);
            ISP = node.SafeLoad("ISP", 0);
            engine = node.SafeLoad("engine", "");
        }

        public void Save(ConfigNode node)
        {
            node.AddValue("resources", string.Join(" ", resources));
            node.AddValue("amounts", amounts.Select(v => v.ToString()));
            node.AddValue("ratios", ratios.Select(v => v.ToString()));
            node.AddValue("fuelLost", fuelLost);
            node.AddValue("ISP", ISP);
            node.AddValue("engine", engine);
            node.AddValue("IsStationKeeping", IsStationKeeping);
        }
    }
}




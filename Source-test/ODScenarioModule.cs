using System;
using UnityEngine;

namespace WhitecatIndustries
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.EDITOR, GameScenes.FLIGHT, GameScenes.TRACKSTATION, GameScenes.SPACECENTER)]
    internal class ODScenarioModule : ScenarioModule
    {
        public override void OnSave(ConfigNode node)
        {
            try
            {
                ConfigNode savedVesselsInfo = new ConfigNode("Vessels");

                foreach (var vdc in VesselData.VesselInformationDict.Values)
                {
                    savedVesselsInfo.AddNode("VESSEL", vdc.GetConfigNode());
                }
#if false
                foreach (ConfigNode nod in VesselData.VesselInformation.GetNodes("VESSEL"))
                {
                    savedVesselsInfo.AddNode(nod);
                }
#endif
                node.AddNode(savedVesselsInfo);
                base.OnSave(node);
                print("scenario saved, ship count : " + VesselData.VesselInformationDict.Count.ToString());

            }
            catch (Exception e)
            {
                Debug.LogError("[OrbitalDecay] OnSave(): " + e.ToString());

            }
        }

        public override void OnLoad(ConfigNode node)
        {
            try
            {
                base.OnLoad(node);
                if (node.HasNode("Vessels"))
                {
                    ConfigNode vNode = node.GetNode("Vessels");
                    foreach (var n in vNode.GetNodes("VESSEL"))
                    {
                        VesselDataClass vdc = new VesselDataClass(n);
                        VesselData.VesselInformationDict.Add(vdc.id, vdc);
                    }
#if false
                    VesselData.VesselInformation = node.GetNode("Vessels");
#endif
                    print("scenario loaded, ship count : " + VesselData.VesselInformationDict.Count.ToString());
                    
                }
                VesselData.VesselsLoaded = true;

            }
            catch (Exception e)
            {
                Debug.LogError("[OrbitalDecay] OnLoad(): " + e.ToString());
                


            }
        }
    }
}
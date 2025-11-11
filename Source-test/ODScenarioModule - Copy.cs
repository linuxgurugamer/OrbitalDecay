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
                foreach (ConfigNode nod in VesselData.VesselInformation.GetNodes("VESSEL"))
                {
                    savedVesselsInfo.AddNode(nod);
                }
                node.AddNode(savedVesselsInfo);
                base.OnSave(node);
                print("scenario saved, ship count : " + VesselData.VesselInformation.CountNodes.ToString());

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
                    VesselData.VesselInformation = node.GetNode("Vessels");
                    print("scenario loaded, ship count : " + VesselData.VesselInformation.CountNodes.ToString());
                    
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
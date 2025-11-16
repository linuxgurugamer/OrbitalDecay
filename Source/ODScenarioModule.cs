using System;
using UnityEngine;

using static OrbitalDecay.RegisterToolbar;

namespace OrbitalDecay
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.EDITOR, GameScenes.FLIGHT, GameScenes.TRACKSTATION, GameScenes.SPACECENTER)]
    internal class ODScenarioModule : ScenarioModule
    {
        public override void OnSave(ConfigNode node)
        {
            try
            {
                //ConfigNode savedVesselsInfo = new ConfigNode("Vessels");
                ConfigNode savedVesselsInfo = Vessel_Information.Save(VesselData.VesselInfo, "");
                //node.AddNode(savedVesselsInfo);
#if false
                foreach (ConfigNode nod in VesselData.VesselInformation.GetNodes("VESSEL"))
                {
                    savedVesselsInfo.AddNode(nod);
                }
                node.AddNode(savedVesselsInfo);
#endif
                base.OnSave(node);
                print("scenario saved, ship count : " + VesselData.VesselInfo.Count.ToString());
                //print("scenario saved, ship count : " + VesselData.VesselInformation.CountNodes.ToString());

            }
            catch (Exception e)
            {
                Debug.LogError("[OrbitalDecay] OnSave(): " + e.ToString());

            }
        }

        public override void OnLoad(ConfigNode node)
        {
            VesselData.VesselInfo.Clear();
            try
            {
                base.OnLoad(node);
#if false
                if (node.HasNode("Vessels"))
                {
                    var configNode = node.GetNode("Vessels");
                    foreach (var n in configNode.GetNodes("VESSEL"))
                    {
                        var vi = Vessel_Information.Load(n);
                        VesselData.VesselInfo[vi.id] = vi;
                    }

                    //VesselData.VesselInformation = node.GetNode("Vessels");
                    //print("scenario loaded, ship count : " + VesselData.VesselInformation.CountNodes.ToString());
                    
                }
#endif
                VesselData.VesselsLoaded = true;

            }
            catch (Exception e)
            {
                Debug.LogError("[OrbitalDecay] OnLoad(): " + e.ToString());
                


            }
        }
    }
}
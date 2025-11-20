using System;
using UnityEngine;

using static OrbitalDecay.RegisterToolbar;

namespace OrbitalDecay
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.EDITOR, GameScenes.FLIGHT, GameScenes.TRACKSTATION, GameScenes.SPACECENTER)]
    internal class ODScenarioModule : ScenarioModule
    {
        static bool _initted = false;
        public static bool Initted { get { return _initted; } set { _initted = value; } }
        public override void OnSave(ConfigNode node)
        {
            try
            {
                ConfigNode savedVesselsInfo = Vessel_Information.Save(VesselData.VesselInfo, "");
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
            Log.Info("ODScenarioModule.OnLoad, VesselInfo.Clear()");
            try
            {
                base.OnLoad(node);
                VesselData.VesselsLoaded = true;

            }
            catch (Exception e)
            {
                Debug.LogError("[OrbitalDecay] OnLoad(): " + e.ToString());             
            }

            RegisterToolbar.UpdateWindowSizes();
            Initted = true;
        }
    }
}
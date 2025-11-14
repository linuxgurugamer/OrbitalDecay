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

using static OrbitalDecay.RegisterToolbar;

namespace OrbitalDecay
{
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class DecayManager : MonoBehaviour
    {
        #region Declared Variables

        private float _uptInterval = 1.0f;
        private float _lastUpdate;
        private float _lastUpdaten = 0.0f;
        private float _lastUpdate2 = 0.0f;

        public static double DecayValue;
        public static double MaxDecayValue;
        public static bool VesselDied;
        public static float EstimatedTimeUntilDeorbit;
        public static bool GuiToggled;
        public static Dictionary<Vessel, bool> MessageDisplayed = new Dictionary<Vessel, bool>();
        public static double VesselCount;
        public static Vessel ActiveVessel = new Vessel();
        public static bool ActiveVesselOnOrbit;
        public static bool EvaActive = false;

        public static bool CatchupResourceMassAreaDataComplete;

        public static bool QuickloadKeyDown;
        public static KeyCode QuickloadKeyWindows = KeyCode.F9;
        public static KeyCode QuickloadKeyMac = KeyCode.F6;
        public static float UpdateTimer;

        #endregion

        #region Unity Scene Subroutines

        public void Start()
        {
            if (!HighLogic.LoadedSceneIsGame || HighLogic.LoadedScene == GameScenes.LOADING || HighLogic.LoadedScene == GameScenes.LOADINGBUFFER || HighLogic.LoadedScene == GameScenes.MAINMENU) return;
            CatchupResourceMassAreaDataComplete = false;
            // GameEvents -- //

            GameEvents.onVesselWillDestroy.Add(ClearVesselOnDestroy); // Vessel destroy checks 1.1.0
            GameEvents.onVesselWasModified.Add(UpdateActiveVesselInformation); // Resource level change 1.3.0
            GameEvents.onStageSeparation.Add(UpdateActiveVesselInformationEventReport); // Resource level change 1.3.0
            GameEvents.onNewVesselCreated.Add(UpdateVesselSpawned); // New Vessel Checks 1.4.2

            //GameEvents.onTimeWarpRateChanged.Add(NBodyManager.TimewarpShift); // Timewarp checks for 1.6.0

            if (HighLogic.LoadedScene == GameScenes.FLIGHT)//CheckSceneStateFlight(HighLogic.LoadedScene)) // 1.3.1
            {
                GameEvents.onPartActionUIDismiss.Add(UpdateActiveVesselInformationPart); // Resource level change 1.3.0
                GameEvents.onPartActionUIDismiss.Add(SetGuiToggledFalse);
                GameEvents.onPartActionUICreate.Add(UpdateActiveVesselInformationPart);
            }

            // -- GameEvents //
            CatchUpVessels(Vessel.Situations.ORBITING, Vessel.Situations.SUB_ORBITAL);   // 1.4.2
        }

        internal static void CatchUpVessels(Vessel.Situations situation)
        {
            IEnumerator<Vessel> vessels = FlightGlobals.Vessels.AsEnumerable().GetEnumerator();
            while (vessels.MoveNext())
            {
                if (vessels.Current == null) continue;
                if (vessels.Current.situation == situation)
                {
                    CatchUpOrbit(vessels.Current);
                }
            }
            vessels.Dispose();
        }

        internal static void CatchUpVessels(Vessel.Situations situation1, Vessel.Situations situation2)
        {
            IEnumerator<Vessel> vessels = FlightGlobals.Vessels.AsEnumerable().GetEnumerator();
            while (vessels.MoveNext())
            {
                if (vessels.Current == null) continue;
                if (vessels.Current.situation == situation1 ||
                    vessels.Current.situation == situation2)
                {
                    CatchUpOrbit(vessels.Current);
                }
            }
            vessels.Dispose();
        }

        #region Update Subroutines

        public void UpdateActiveVesselInformationEventReport(EventReport report) // 1.3.0
        {
            if (HighLogic.LoadedScene == GameScenes.FLIGHT) // 1.3.1
            {
                VesselData.UpdateActiveVesselData(FlightGlobals.ActiveVessel);
            }
        }

        public void UpdateActiveVesselInformationPart(Part part) // Until eventdata OnPartResourceFlowState works! // 1.3.0
        {
            if (part.vessel != FlightGlobals.ActiveVessel || TimeWarp.CurrentRate != 0) return;
            if (HighLogic.LoadedScene != GameScenes.FLIGHT || GuiToggled) return;
            VesselData.UpdateActiveVesselData(FlightGlobals.ActiveVessel);
            GuiToggled = true;
        }

        public void SetGuiToggledFalse(Part part)
        {
            GuiToggled = false;
        }

        public void UpdateActiveVesselInformation(Vessel vessel)
        {
            if (HighLogic.LoadedScene == GameScenes.EDITOR) return;
            if (vessel == FlightGlobals.ActiveVessel)
            {
                VesselData.UpdateActiveVesselData(vessel);
            }
        }

        public void UpdateVesselSpawned(Vessel vessel)
        {
            if (vessel.situation == Vessel.Situations.ORBITING)
            {
                VesselData.WriteVesselData(vessel);
            }
        }

        public void QuickSaveUpdate(ConfigNode node)
        {
            VesselData.OnQuickSave();
        } // 1.5.0 QuickSave functionality // Thanks zajc3w!

        public void QuickLoadUpdate()
        {
            VesselData.OnQuickLoad(); // 1.5.3 Fixes

            VesselData.VesselInfo.Clear();
            //VesselData.VesselInformation.ClearNodes();
            print("WhitecatIndustries - OrbitalDecay - Vessel Information lost OnQuickLoadUpdate");
            //string filePath = KSPUtil.ApplicationRootPath + "GameData/WhitecatIndustries/OrbitalDecay/PluginData/VesselData.cfg";
#if false
            ConfigNode fileM = new ConfigNode();
            ConfigNode fileN = new ConfigNode("VESSEL");
            fileN.AddValue("name", "WhitecatsDummyVessel");
            fileN.AddValue("id", "000");
            fileN.AddValue("persistence", "WhitecatsDummySaveFileThatNoOneShouldNameTheirSave");
            fileM.AddNode(fileN);
            VesselData.VesselInformation.AddNode(fileM);
#endif
            VesselData.VesselInfo[Guid.Empty] = new Vessel_Information("WhitecatsDummyVessel", Guid.Empty);

            VesselData.OnQuickSave();

        }

        public void ClearVesselOnDestroy(Vessel vessel)
        {
            VesselData.ClearVesselData(vessel);
            print("Vessel destroyed:" + vessel.GetName());
        }
#endregion

        #region Check Subroutines 

        public static bool CheckSceneStateMain(GameScenes scene)
        {
            if (scene != GameScenes.LOADING && scene != GameScenes.LOADINGBUFFER && HighLogic.LoadedSceneIsGame)
            {
                return true;
            }

            else
            {
                return false;
            }
        }

        public static bool CheckSceneStateMainNotSpaceCentre(GameScenes scene)
        {
            return scene != GameScenes.LOADING && scene != GameScenes.LOADINGBUFFER && HighLogic.LoadedSceneIsGame && scene != GameScenes.SPACECENTER;
        }

        public static bool CheckSceneStateFlight(GameScenes scene)
        {
            return scene == GameScenes.FLIGHT && scene != GameScenes.LOADING && scene != GameScenes.LOADINGBUFFER && HighLogic.LoadedSceneIsGame;
        }

        public static bool CheckVesselState(Vessel vessel)
        {
            return vessel.situation == Vessel.Situations.ORBITING;
        }

        public static bool CheckVesselStateOrbEsc(Vessel vessel)
        {
            return vessel.situation == Vessel.Situations.ORBITING;
        }

        public static bool CheckVesselStateActive(Vessel vessel)
        {
            return vessel.situation == Vessel.Situations.ORBITING && vessel == FlightGlobals.ActiveVessel;
        }

        public static bool CheckVesselActiveInScene(Vessel vessel)
        {
            return vessel.situation == Vessel.Situations.ORBITING && vessel == FlightGlobals.ActiveVessel;
        }

        public static bool CheckVesselProximity(Vessel vessel)
        {
            bool close = false;

            if (!HighLogic.LoadedSceneIsFlight) return close;
            double distance;
            try
            {
                distance = Vector3d.Distance(vessel.GetWorldPos3D(), FlightGlobals.ActiveVessel.GetWorldPos3D());
            }
            catch (NullReferenceException)
            {
                distance = 100001;
            }

            if (distance < 100000)
            {
                close = true;
            }

            if (vessel != FlightGlobals.ActiveVessel) return close;
            foreach (Vessel v in FlightGlobals.Vessels)
            {
                if (v.packed || v == vessel) continue;
                close = true;
                break;
            }

            return close;
        }

        #endregion

        public void FixedUpdate()
        {
            if (Time.time - _lastUpdate2 > _uptInterval / 10.0)
            {

                if (Input.GetKeyDown(QuickloadKeyWindows)) // Quick load request check
                {
                    UpdateTimer = UpdateTimer + _uptInterval / 10.0f;

                    if (UpdateTimer > 0.05f)
                    {
                        QuickloadKeyDown = true;
                        UpdateTimer = 0.0f;
                    }

                    if (QuickloadKeyDown)
                    {
                        print("F9 Held");
                        QuickLoadUpdate();
                    }
                }
            }


            if (Time.timeSinceLevelLoad > 0.4 && HighLogic.LoadedSceneIsFlight && CatchupResourceMassAreaDataComplete == false && (FlightGlobals.ActiveVessel.situation == Vessel.Situations.ORBITING || FlightGlobals.ActiveVessel.situation == Vessel.Situations.SUB_ORBITAL))
            {
                if (FlightGlobals.ActiveVessel.isActiveAndEnabled) // Vessel is ready
                {
                    if (VesselData.FetchFuelLost() > 0)
                    {
                        ResourceManager.RemoveResources(FlightGlobals.ActiveVessel, VesselData.FetchFuelLost());
                        VesselData.SetFuelLost(0);

                    }

                    if (FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleOrbitalDecay>().Any())
                    {
                        if (VesselData.FetchFuelLost() > 0)
                        {
                            ResourceManager.RemoveResources(FlightGlobals.ActiveVessel, VesselData.FetchFuelLost());
                            VesselData.SetFuelLost(0);

                        }
                    }

                    VesselData.UpdateActiveVesselData(FlightGlobals.ActiveVessel);
                    print("WhitecatIndustries - OrbitalDecay - Updating Fuel Levels for: " + FlightGlobals.ActiveVessel.GetName());
                    CatchupResourceMassAreaDataComplete = true;
                }
            }
            /*
            if (Time.timeSinceLevelLoad > 0.45) // NBody predictions
            {
                if (HighLogic.LoadedSceneIsGame && (HighLogic.LoadedScene != GameScenes.LOADING && HighLogic.LoadedScene != GameScenes.LOADINGBUFFER && HighLogic.LoadedScene != GameScenes.MAINMENU))
                {
                    if ((Time.time - lastUpdaten) > UPTInterval)
                    {
                        lastUpdaten = Time.time;

                        if (Settings.ReadNB())
                        {
                            NBodyManager.ManageOrbitalPredictons();
                        }
                    }
                }
            }
             */

            if (Time.timeSinceLevelLoad <= 0.5) return;
            if (!HighLogic.LoadedSceneIsGame || HighLogic.LoadedScene == GameScenes.LOADING || HighLogic.LoadedScene == GameScenes.LOADINGBUFFER || HighLogic.LoadedScene == GameScenes.MAINMENU) return;
            if (!(Time.time - _lastUpdate > _uptInterval)) return;
            _lastUpdate = Time.time;

            if (HighLogic.LoadedScene != GameScenes.SPACECENTER && HighLogic.LoadedScene != GameScenes.TRACKSTATION &&
                HighLogic.LoadedScene != GameScenes.FLIGHT) return;
            for (int i = 0; i < FlightGlobals.Vessels.Count; i++)
            {
                Vessel vessel = FlightGlobals.Vessels[i];

                if (vessel.situation != Vessel.Situations.ORBITING &&
                    (vessel.situation != Vessel.Situations.SUB_ORBITAL || vessel == FlightGlobals.ActiveVessel ||
                     vessel != vessel.packed)) continue;
                if (VesselData.FetchStationKeeping(vessel) == false)
                {
                    if (VesselData.FetchSMA(vessel) > 0)
                    {
                        if (!vessel.packed)
                        {
                            if (Settings.ReadRD())
                            {
                                ActiveDecayRealistic(vessel); // 1.2.0 Realistic Active Decay fixes
                            }
                            else
                            {
                                ActiveDecayStock(vessel);
                            }
                        }
                        else
                        {
                            RealisticDecaySimulator(vessel);
                        }
                    }

                    if (HighLogic.LoadedScene != GameScenes.TRACKSTATION || Settings.ReadPT() != true) continue;
                    if (Settings.ReadDT())
                    {
                        CatchUpOrbit(vessel);
                    }
                    else if (Settings.ReadDT() == false && vessel.vesselType != VesselType.Debris)
                    {
                        CatchUpOrbit(vessel);
                    }
                    else
                    {
                    }
                }
                else
                {
                    StationKeepingManager.FuelManager(vessel);
                }
            }
        }

        public void Save()
        {
            if (!HighLogic.LoadedSceneIsGame || HighLogic.LoadedScene == GameScenes.FLIGHT ||
                HighLogic.LoadedScene == GameScenes.LOADING &&
                (HighLogic.LoadedScene == GameScenes.LOADINGBUFFER ||
                 HighLogic.LoadedScene == GameScenes.MAINMENU)) return;

            // Set Vessel Orbits;
            CatchUpVessels(Vessel.Situations.ORBITING);
        }

        public void OnDestroy()
        {
            if (!HighLogic.LoadedSceneIsGame || HighLogic.LoadedScene == GameScenes.LOADING || HighLogic.LoadedScene == GameScenes.LOADINGBUFFER || HighLogic.LoadedScene == GameScenes.MAINMENU) return;
            GameEvents.onVesselWillDestroy.Remove(ClearVesselOnDestroy);
            GameEvents.onVesselWasModified.Remove(UpdateActiveVesselInformation); // 1.3.0 Resource Change
            GameEvents.onStageSeparation.Remove(UpdateActiveVesselInformationEventReport); // 1.3.0
            GameEvents.onNewVesselCreated.Remove(UpdateVesselSpawned); // 1.4.2 

            //GameEvents.onTimeWarpRateChanged.Remove(NBodyManager.TimewarpShift); // 1.6.0 

            if (HighLogic.LoadedScene == GameScenes.FLIGHT) // 1.3.1
            {
                GameEvents.onPartActionUIDismiss.Remove(UpdateActiveVesselInformationPart); // 1.3.0
                GameEvents.onPartActionUIDismiss.Remove(SetGuiToggledFalse);
                GameEvents.onPartActionUICreate.Remove(UpdateActiveVesselInformationPart);
            }

            // Set Vessel Orbits
            CatchUpVessels(Vessel.Situations.ORBITING);
        }

#endregion

        #region Active Specific Subroutines

        public void ActiveVesselOrbitManage()
        {
            // Redundant in 1.1.0

            if (ActiveVesselOnOrbit) return;
            if (FlightGlobals.ActiveVessel.situation != Vessel.Situations.ORBITING) return;
            ActiveVesselOnOrbit = true;
            VesselData.WriteVesselData(FlightGlobals.ActiveVessel);

        } // Redundant in 1.1.0

        public static void CatchUpOrbit(Vessel vessel)
        {
            if (vessel.situation == Vessel.Situations.PRELAUNCH || vessel.situation == Vessel.Situations.LANDED) return;
            if (!(VesselData.FetchSMA(vessel) < vessel.GetOrbitDriver().orbit.semiMajorAxis) ||
                CheckVesselProximity(vessel)) return;
            try
            {
                OrbitPhysicsManager.HoldVesselUnpack(60);
            }
            catch (NullReferenceException)
            {
            }
            for (int i = 0; i < FlightGlobals.Vessels.Count; i++)
            {
                Vessel ship = FlightGlobals.Vessels[i];
                if (ship.packed)
                {
                    ship.GoOnRails();
                }
            }

            if (HighLogic.LoadedScene == GameScenes.FLIGHT && vessel.situation != Vessel.Situations.PRELAUNCH)
            {
                if (vessel == FlightGlobals.ActiveVessel)
                {
                    vessel.GoOnRails();
                }
            }

            if (VesselData.FetchSMA(vessel) == 0) return;
            CelestialBody oldBody = vessel.orbitDriver.orbit.referenceBody;
            Orbit orbit = vessel.orbitDriver.orbit;
            orbit.inclination = VesselData.FetchINC(vessel);
            orbit.eccentricity = VesselData.FetchECC(vessel);
            orbit.semiMajorAxis = VesselData.FetchSMA(vessel);
            orbit.LAN = VesselData.FetchLAN(vessel);
            orbit.argumentOfPeriapsis = VesselData.FetchLPE(vessel);
            //orbit.meanAnomalyAtEpoch = VesselData.FetchMNA(vessel);
            orbit.epoch = vessel.orbit.epoch;
            orbit.referenceBody = vessel.orbit.referenceBody;
            orbit.Init();

            orbit.UpdateFromUT(HighLogic.CurrentGame.UniversalTime);
            vessel.orbitDriver.pos = vessel.orbit.pos.xzy; // Possibly remove these for NBody
            vessel.orbitDriver.vel = vessel.orbit.vel; // Possibly remove these for NBody

            CelestialBody newBody = vessel.orbitDriver.orbit.referenceBody;
            if (newBody == oldBody) return;
            GameEvents.HostedFromToAction<Vessel, CelestialBody> evnt = new GameEvents.HostedFromToAction<Vessel, CelestialBody>(vessel, oldBody, newBody);
            GameEvents.onVesselSOIChanged.Fire(evnt);
            VesselData.UpdateBody(vessel, newBody);
        } // Main Orbit Set

        #endregion

        #region Misc Calculation Subroutines

        public static double CalculateNewEccentricity(double oldEccentricity, double oldSma, double newSma) // 1.4.0 needs balancing maybe
        {
            double newEccentricity = 0.0;
            double fixedSemiMinorAxis = oldSma * Math.Sqrt(1.0 - Math.Pow(oldEccentricity, 2.0));
            newEccentricity = Math.Sqrt(1.0 - Math.Pow(fixedSemiMinorAxis, 2.0) / Math.Pow(newSma, 2.0)); //
            return newEccentricity;
        }

        #endregion

        #region Decay Simulator

        public static bool CheckReferenceBody(Vessel vessel) // 1.6.0 Body Checks
        {
            bool validBody = false;
            CelestialBody body = vessel.orbitDriver.orbit.referenceBody;

            if (body.Radius < 1095700000) // Checks the body radius (prevents issues with Galacitc Cores.. hopefully)
            {
                validBody = true;
            }

            return validBody;

        }

        public static bool CheckNBodyAltitude(Vessel vessel)
        {
            bool beyondSafeArea = Math.Abs(vessel.orbitDriver.orbit.altitude) > 2.0 * vessel.orbitDriver.orbit.referenceBody.Radius;

            return beyondSafeArea;
        }


        public static void RealisticDecaySimulator(Vessel vessel) // 1.4.0 Cleanup
        {
            Orbit orbit = vessel.orbitDriver.orbit;
            CelestialBody body = orbit.referenceBody;

#if false
            if (Settings.ReadNB() && CheckNBodyAltitude(vessel))
            {
                if (vessel.situation == Vessel.Situations.ORBITING) // For the moment
                {
                    #region NBody debugging
                    /*
                    print("Pos: " + vessel.orbitDriver.orbit.getRelativePositionAtUT(HighLogic.CurrentGame.UniversalTime));
                    print("PosAtUT: " + vessel.orbitDriver.orbit.getPositionAtUT(HighLogic.CurrentGame.UniversalTime));
                    print("PosAlternate: " + vessel.orbitDriver.orbit.getRelativePositionAtUT(HighLogic.CurrentGame.UniversalTime));
                    print("DifferenceBetween Pos & PosAlt: " + (vessel.orbitDriver.orbit.pos - vessel.orbitDriver.orbit.getRelativePositionAtUT(HighLogic.CurrentGame.UniversalTime)));

                    print("Vel: " + vessel.orbitDriver.orbit.vel.magnitude);
                    print("VelAt: " + vessel.orbitDriver.orbit.getOrbitalSpeedAt(HighLogic.CurrentGame.UniversalTime));
                    print("VelAtAlt: " + vessel.orbitDriver.orbit.getOrbitalSpeedAtRelativePos(vessel.orbitDriver.orbit.getRelativePositionAtUT(HighLogic.CurrentGame.UniversalTime)));

                    print("Energy: " + vessel.orbitDriver.orbit.orbitalEnergy);
                    print("Energy Calculated: " + (((Math.Pow(vessel.orbit.vel.magnitude, 2.0)) / 2.0) - (vessel.orbitDriver.orbit.referenceBody.gravParameter / (vessel.orbitDriver.orbit.altitude + vessel.orbit.referenceBody.Radius))));
                    */
                    #endregion

                     // NBodyManager.ManageVessel(vessel); // 1.6.0 N-Body master reference maybe 1.7.0?
                }
            }
#endif

            if (!CheckReferenceBody(vessel)) return;
            RealisticGravitationalPertubationDecay(vessel); // 1.5.0
            RealisticRadiationDragDecay(vessel); // 1.5.0 Happens everywhere now
            RealisticYarkovskyEffectDecay(vessel); // 1.5.0 // Partial, full for 1.6.0

            if (body.atmosphere)
            {
                if (Settings.ReadRD())
                {
                    RealisticAtmosphericDragDecay(vessel);
                }
                else
                {
                    StockAtmosphericDragDecay(vessel);
                }
            }

            CheckVesselSurvival(vessel);
        }
        #endregion

        #region Decay Simulator Subroutines

        public static void RealisticAtmosphericDragDecay(Vessel vessel)
        {
            Orbit orbit = vessel.orbitDriver.orbit;
            CelestialBody body = orbit.referenceBody;

            double initialSemiMajorAxis = VesselData.FetchSMA(vessel);
            double maxInfluence = body.Radius * 1.5;

            if (!(initialSemiMajorAxis < maxInfluence)) return;
            double standardGravitationalParameter = body.gravParameter;
            double cartesianPositionVectorMagnitude = orbit.getRelativePositionAtT(HighLogic.CurrentGame.UniversalTime).magnitude; // Planetarium to HighLogic
            double equivalentAltitude = initialSemiMajorAxis - body.Radius;

            // Eccentricity updating
            double newEccentricity = VesselData.FetchECC(vessel);
            // Still having problems here!
            // NewEccentricity = CalculateNewEccentricity(VesselData.FetchECC(vessel), InitialSemiMajorAxis, (InitialSemiMajorAxis - (DecayRateRealistic(vessel) / 10)));
            VesselData.UpdateVesselECC(vessel, newEccentricity);

            double eccentricity = newEccentricity;

            if (eccentricity > 0.085)
            {
                double altitudeAp = initialSemiMajorAxis * (1 + eccentricity) - body.Radius;
                double altitudePe = initialSemiMajorAxis * (1 - eccentricity) - body.Radius;
                equivalentAltitude = altitudePe + 900.0 * Math.Pow(eccentricity, 0.6);
            }

            double initialOrbitalVelocity = orbit.vel.magnitude;
            double initialDensity = body.atmDensityASL;
            double boltzmannConstant = Math.Pow(1.380 * 10, -23);
            double altitude = vessel.altitude;
            double gravityAsl = body.GeeASL;
            double atmosphericMolarMass = body.atmosphereMolarMass;

            double vesselArea = VesselData.FetchArea(vessel);
            if (vesselArea == 0)
            {
                vesselArea = 1.0;
            }

            double distanceTravelled = initialOrbitalVelocity; // Meters
            double vesselMass = VesselData.FetchMass(vessel);   // Kg
            if (vesselMass == 0)
            {
                vesselMass = 100.0; // Default is 100kg
            }

            equivalentAltitude = equivalentAltitude / 1000.0;

            double molecularMass = 27.0 - 0.0012 * (equivalentAltitude - 200.0);
            double f107Flux = WhitecatIndustries.SCSManager.FetchCurrentF107();
            double geomagneticIndex = WhitecatIndustries.SCSManager.FetchCurrentAp();

            double exothericTemperature = 900.0 + 2.5 * (f107Flux - 70.0) + 1.5 * geomagneticIndex;
            double scaleHeight = exothericTemperature / molecularMass;
            double atmosphericDensity = 6.0 * Math.Pow(10.0, -10.0) * Math.Pow(Math.E, -((equivalentAltitude - 175.0f) / scaleHeight));

            double deltaPeriod = 3 * Math.PI * (initialSemiMajorAxis * atmosphericDensity) * (vesselArea * 2.2 / vesselMass); // Unitless
            double initialPeriod = 2 * Math.PI * Math.Sqrt(Math.Pow(initialSemiMajorAxis, 3.0) / standardGravitationalParameter);
            double finalPeriod = initialPeriod - deltaPeriod;
            double finalSemiMajorAxis = Math.Pow(Math.Pow(finalPeriod / (2 * Math.PI), 2.0) * standardGravitationalParameter, 1.0 / 3.0);
            double decayValue = initialSemiMajorAxis - finalSemiMajorAxis;

            double multipliers = TimeWarp.CurrentRate * Settings.ReadDecayDifficulty();

            VesselData.UpdateVesselSMA(vessel, initialSemiMajorAxis - decayValue * multipliers);
            VesselData.UpdateVesselLAN(vessel, VesselData.FetchLAN(vessel));

            // Possibly update vessel LAN too? - 1.5.0
        } // Requires SCS

        public static void StockAtmosphericDragDecay(Vessel vessel)
        {
            Orbit orbit = vessel.orbitDriver.orbit;
            CelestialBody body = orbit.referenceBody;

            double initialSemiMajorAxis = VesselData.FetchSMA(vessel);
            double maxInfluence = body.Radius * 1.5;

            if (!(initialSemiMajorAxis < maxInfluence)) return;
            double standardGravitationalParameter = body.gravParameter;
            double cartesianPositionVectorMagnitude = orbit.getRelativePositionAtT(HighLogic.CurrentGame.UniversalTime).magnitude; // Planetarium to HighLogic
            double equivalentAltitude = initialSemiMajorAxis - body.Radius;

            // Eccentricity updating
            double newEccentricity = VesselData.FetchECC(vessel);
            // Still having problems here!
            // NewEccentricity = CalculateNewEccentricity(VesselData.FetchECC(vessel), InitialSemiMajorAxis, (InitialSemiMajorAxis - (DecayRateRealistic(vessel) / 10)));
            VesselData.UpdateVesselECC(vessel, newEccentricity);

            double eccentricity = newEccentricity;

            if (eccentricity > 0.085)
            {
                double altitudeAp = initialSemiMajorAxis * (1 + eccentricity) - body.Radius;
                double altitudePe = initialSemiMajorAxis * (1 - eccentricity) - body.Radius;
                equivalentAltitude = altitudePe + 900.0 * Math.Pow(eccentricity, 0.6);
            }

            double initialOrbitalVelocity = orbit.vel.magnitude;
            double initialDensity = body.atmDensityASL;
            double boltzmannConstant = Math.Pow(1.380 * 10, -23);
            double altitude = vessel.altitude;
            double gravityAsl = body.GeeASL;
            double atmosphericMolarMass = body.atmosphereMolarMass;

            double vesselArea = VesselData.FetchArea(vessel);
            if (vesselArea == 0)
            {
                vesselArea = 5.0;
            }

            double distanceTravelled = initialOrbitalVelocity; // Meters
            double vesselMass = VesselData.FetchMass(vessel);   // Kg
            if (vesselMass == 0)
            {
                vesselMass = 1000.0; // Default is 100kg
            }

            equivalentAltitude = equivalentAltitude / 1000.0;

            double atmosphericDensity = 1.020 * Math.Pow(10.0, 7.0) * Math.Pow(equivalentAltitude + 70.0, -7.172);
            double deltaPeriod = 3 * Math.PI * (initialSemiMajorAxis * atmosphericDensity) * (vesselArea * 2.2 / vesselMass); // Unitless
            double initialPeriod = 2 * Math.PI * Math.Sqrt(Math.Pow(initialSemiMajorAxis, 3.0) / standardGravitationalParameter);
            double finalPeriod = initialPeriod - deltaPeriod;
            double finalSemiMajorAxis = Math.Pow(Math.Pow(finalPeriod / (2 * Math.PI), 2.0) * standardGravitationalParameter, 1.0 / 3.0);
            double decayValue = initialSemiMajorAxis - finalSemiMajorAxis;
            double multipliers = TimeWarp.CurrentRate * Settings.ReadDecayDifficulty();

            VesselData.UpdateVesselSMA(vessel, initialSemiMajorAxis - decayValue * multipliers);
            VesselData.UpdateVesselLAN(vessel, VesselData.FetchLAN(vessel));
        } // 1.4.0

        public static void RealisticRadiationDragDecay(Vessel vessel)
        {
            Orbit orbit = vessel.orbitDriver.orbit;
            CelestialBody body = orbit.referenceBody;

            double solarEnergy = Math.Pow(3.86 * 10.0, 26); // W
            double solarDistance = 0.0;
            solarDistance = vessel.orbitDriver.orbit.referenceBody == Sun.Instance.sun ?
                vessel.orbitDriver.orbit.altitude :
                vessel.orbitDriver.orbit.referenceBody.orbit.altitude;

            double solarConstant = solarEnergy / (4.0 * Math.PI * Math.Pow(solarDistance, 2.0)); // W/m^2
            double initialSemiMajorAxis = VesselData.FetchSMA(vessel);
            double standardGravitationalParameter = body.gravParameter;
            double meanAngularVelocity = Math.Sqrt(standardGravitationalParameter / Math.Pow(initialSemiMajorAxis, 3.0));
            double speedOfLight = Math.Pow(3.0 * 10.0, 8.0);

            double vesselArea = VesselData.FetchArea(vessel);
            if (vesselArea == 0)
            {
                vesselArea = 1.0;
            }

            double vesselMass = VesselData.FetchMass(vessel);   // Kg
            if (vesselMass == 0)
            {
                vesselMass = 100.0;
            }

            double vesselRadius = Math.Sqrt(vesselArea / Math.PI);
            double immobileAccelleration = Math.PI * (vesselRadius * vesselRadius) * solarConstant / (vesselMass * speedOfLight * (solarDistance * solarDistance));
            double changeInSemiMajorAxis = -(6.0 * Math.PI * immobileAccelleration * initialSemiMajorAxis) / (meanAngularVelocity * speedOfLight);
            double finalSemiMajorAxis = initialSemiMajorAxis + changeInSemiMajorAxis;

            VesselData.UpdateVesselSMA(vessel, initialSemiMajorAxis + changeInSemiMajorAxis * TimeWarp.CurrentRate * Settings.ReadDecayDifficulty());
        }

        public static void RealisticGravitationalPertubationDecay(Vessel vessel) // 1.5.0 
        {
            Orbit orbit = vessel.orbitDriver.orbit;
            CelestialBody body = orbit.referenceBody;

            double gravitationalConstant = 6.67408 * Math.Pow(10.0, -11.0); // G [Newton Meter Squared per Square Kilogram]
            double forceAtSurface = gravitationalConstant * VesselData.FetchMass(vessel) * vessel.orbitDriver.orbit.referenceBody.Mass;
            double forceAtDistance = gravitationalConstant * VesselData.FetchMass(vessel) * vessel.orbitDriver.orbit.referenceBody.Mass / Math.Pow(vessel.orbitDriver.orbit.altitude, 2.0);
            if (!(forceAtDistance > 0.0000000000001 * forceAtSurface)) return;
            if (TimeWarp.CurrentRate < 100)
            {
                if (!MasConData.CheckMasConProximity(vessel)) return;
                VesselData.UpdateVesselSMA(vessel, VesselData.FetchSMA(vessel) + MasConManager.GetCalculatedSMAChange(vessel, orbit.LAN, orbit.meanAnomalyAtEpoch, orbit.argumentOfPeriapsis, orbit.eccentricity, orbit.inclination, orbit.semiMajorAxis, orbit.epoch) * TimeWarp.CurrentRate);
                VesselData.UpdateVesselINC(vessel, VesselData.FetchINC(vessel) + MasConManager.GetCalculatedINCChange(vessel, orbit.LAN, orbit.meanAnomalyAtEpoch, orbit.argumentOfPeriapsis, orbit.eccentricity, orbit.inclination, orbit.semiMajorAxis, orbit.epoch));
                VesselData.UpdateVesselECC(vessel, VesselData.FetchECC(vessel) + MasConManager.GetCalculatedINCChange(vessel, orbit.LAN, orbit.meanAnomalyAtEpoch, orbit.argumentOfPeriapsis, orbit.eccentricity, orbit.inclination, orbit.semiMajorAxis, orbit.epoch));
                VesselData.UpdateVesselLAN(vessel, VesselData.FetchLAN(vessel) + MasConManager.GetCalculatedLANChange(vessel, orbit.LAN, orbit.meanAnomalyAtEpoch, orbit.argumentOfPeriapsis, orbit.eccentricity, orbit.inclination, orbit.semiMajorAxis, orbit.epoch));
                //print("Change In MNA from Mascon: " + MasConManager.GetCalculatedMNAChange(vessel, orbit.LAN, orbit.meanAnomalyAtEpoch, orbit.argumentOfPeriapsis, orbit.eccentricity, orbit.inclination, orbit.semiMajorAxis, orbit.epoch));
            }

            else
            {
                if (!MasConData.CheckMasConProximity(vessel)) return;
                VesselData.UpdateVesselSMA(vessel, VesselData.FetchSMA(vessel) + MasConManager.GetSecularSMAChange(vessel, orbit.LAN, orbit.meanAnomalyAtEpoch, orbit.argumentOfPeriapsis, orbit.eccentricity, orbit.inclination, orbit.semiMajorAxis, orbit.epoch));
                VesselData.UpdateVesselINC(vessel, VesselData.FetchINC(vessel) + MasConManager.GetSecularIncChange(vessel, orbit.LAN, orbit.meanAnomalyAtEpoch, orbit.argumentOfPeriapsis, orbit.eccentricity, orbit.inclination, orbit.semiMajorAxis, orbit.epoch));
                VesselData.UpdateVesselECC(vessel, VesselData.FetchECC(vessel) + MasConManager.GetSecularECCChange(vessel, orbit.LAN, orbit.meanAnomalyAtEpoch, orbit.argumentOfPeriapsis, orbit.eccentricity, orbit.inclination, orbit.semiMajorAxis, orbit.epoch));
                VesselData.UpdateVesselLAN(vessel, MasConManager.GetSecularLANChange(vessel, orbit.LAN, orbit.meanAnomalyAtEpoch, orbit.argumentOfPeriapsis, orbit.eccentricity, orbit.inclination, orbit.semiMajorAxis, orbit.epoch));
            }
        }

        public static void RealisticYarkovskyEffectDecay(Vessel vessel) // 1.5.0 
        {
            VesselData.UpdateVesselSMA(vessel, VesselData.FetchSMA(vessel) - -1.0 * YarkovskyEffect.FetchDeltaSMA(vessel));
        }


        #endregion

        #region Old Stock 

        /*
        public static void StockDecaySimulator(Vessel vessel)
        {
            double BodyGravityConstant = vessel.orbitDriver.orbit.referenceBody.GeeASL;
            double AtmosphereMultiplier;
            double MaxDecayInfluence = vessel.orbitDriver.orbit.referenceBody.Radius * 10;
            var oldBody = vessel.orbitDriver.orbit.referenceBody;

            if (vessel.orbitDriver.orbit.referenceBody.atmosphere)
            {
                AtmosphereMultiplier = vessel.orbitDriver.orbit.referenceBody.atmospherePressureSeaLevel / 101.325;
            }
            else
            {
                AtmosphereMultiplier = 0.5;
            }

            if (vessel.GetOrbitDriver().orbit.semiMajorAxis + 50 < MaxDecayInfluence)
            {
                double Lambda = 0.000000000133913;
                double Sigma = MaxDecayInfluence - vessel.orbitDriver.orbit.altitude;
                double Area = VesselData.FetchArea(vessel);
                if (Area == 0)
                {
                    Area = 1.0;
                }
                double Mass = VesselData.FetchMass(vessel);
                if (Mass == 0)
                {
                    Mass = 100.0; // Default 100Kg
                }

                double DistanceMultiplier = Math.Pow(Math.E, ((vessel.orbitDriver.orbit.referenceBody.atmosphereDepth/1000) / ((VesselData.FetchSMA(vessel) - vessel.orbitDriver.orbit.referenceBody.Radius) / 1000)));

                DecayValue = TimeWarp.CurrentRate * AtmosphereMultiplier * vessel.orbitDriver.orbit.referenceBody.GeeASL * 0.5 * (1.0 / (Mass / 1000.0)) * Area * DistanceMultiplier;
                //DecayValue = (double)TimeWarp.CurrentRate * Sigma * BodyGravityConstant * AtmosphereMultiplier * Lambda * Area * (Mass) * (2.509 * Math.Pow(10.0, -4.0)) * DistanceMultiplier; // 1.0.9 Update
            }
            else
            {
                DecayValue = 0.0;
            }

            double DecayRateModifier = 0.0;
            DecayRateModifier = Settings.ReadDecayDifficulty();

            DecayValue = DecayValue * DecayRateModifier;// Decay Rate Modifier from Settings 
            VesselDied = false;
            CheckVesselSurvival(vessel);

            if (VesselDied == false)         // Just Incase the vessel is destroyed part way though the check.
            {
                if (vessel.orbitDriver.orbit.referenceBody.GetInstanceID() != 0 || vessel.GetOrbitDriver().orbit.semiMajorAxis > vessel.orbitDriver.orbit.referenceBody.Radius + 5)
                {
                    VesselData.UpdateVesselSMA(vessel, ((float)VesselData.FetchSMA(vessel) - (float)DecayValue));
                }
            }
            CheckVesselSurvival(vessel);
        }
        */

        #endregion

        #region Survival Checks 
        public static void CheckVesselSurvival(Vessel vessel)
        {
            VesselDied = false;
            if (vessel.situation == Vessel.Situations.SUB_ORBITAL) return;
            if (vessel.orbitDriver.orbit.referenceBody.atmosphere) // Big problem ( Jool, Eve, Duna, Kerbin, Laythe)
            {
                if (!MessageDisplayed.Keys.Contains(vessel))
                {
                    if (VesselData.FetchSMA(vessel) < vessel.orbitDriver.orbit.referenceBody.Radius + vessel.orbitDriver.referenceBody.atmosphereDepth)
                    {
                        TimeWarp.SetRate(0, false);
                        print("Warning: " + vessel.vesselName + " is approaching " + vessel.orbitDriver.referenceBody.name + "'s hard atmosphere");
                        ScreenMessages.PostScreenMessage("Warning: " + vessel.vesselName + " is approaching " + vessel.orbitDriver.referenceBody.name + "'s hard atmosphere");
                        MessageDisplayed.Add(vessel, true);
                    }
                }

                if (VesselData.FetchSMA(vessel) < vessel.orbitDriver.orbit.referenceBody.Radius + vessel.orbitDriver.referenceBody.atmosphereDepth / 2.0) // 1.5.0 Increased Tolerance
                {
                    VesselDied = true;
                }
            }
            else // Moon Smaller Problem
            {
                if (MessageDisplayed.Keys.Contains(vessel))
                {
                    if (VesselData.FetchSMA(vessel) < vessel.orbitDriver.orbit.referenceBody.Radius + 5000)
                    {
                        TimeWarp.SetRate(0, false);
                        print("Warning: " + vessel.vesselName + " is approaching " + vessel.orbitDriver.referenceBody.name + "'s surface");
                        ScreenMessages.PostScreenMessage("Warning: " + vessel.vesselName + " is approaching " + vessel.orbitDriver.referenceBody.name + "'s surface");
                        MessageDisplayed.Add(vessel, true);
                    }
                }

                if (VesselData.FetchSMA(vessel) < vessel.orbitDriver.orbit.referenceBody.Radius + 100)
                {
                    VesselDied = true;
                }
            }

            if (VesselDied != true) return;
            if (vessel != FlightGlobals.ActiveVessel)
            {
                print(vessel.vesselName + " entered " + vessel.orbitDriver.referenceBody.name + "'s atmosphere and was destroyed");
                ScreenMessages.PostScreenMessage(vessel.vesselName + " entered " + vessel.orbitDriver.referenceBody.name + "'s atmosphere and was destroyed");
                if (MessageDisplayed.ContainsKey(vessel))
                {
                    MessageDisplayed.Remove(vessel);
                }
                VesselData.ClearVesselData(vessel);
                print("WhitecatIndustries - OrbitalDecay - Vessel died");
                vessel.Die();
            }
            VesselDied = false;
        }
        #endregion

        #region Active Decay Subroutines

        public static void ActiveDecayRealistic(Vessel vessel)            // 1.4.0 Use Rigidbody.addForce
        {
            if (!CheckReferenceBody(vessel)) return;
            double readTime = HighLogic.CurrentGame.UniversalTime;
            double decayValue = DecayRateTotal(vessel);
            double initialVelocity = vessel.orbitDriver.orbit.getOrbitalVelocityAtUT(readTime).magnitude;
            double calculatedFinalVelocity = 0.0;
            Orbit newOrbit = vessel.orbitDriver.orbit;
            //newOrbit.semiMajorAxis = (VesselData.FetchSMA(vessel) - DecayValue);
            double newSemiMajorAxis = VesselData.FetchSMA(vessel) - decayValue;
            calculatedFinalVelocity = newOrbit.getOrbitalVelocityAtUT(readTime).magnitude;

            double deltaVelocity = initialVelocity - calculatedFinalVelocity;
            double decayForce = deltaVelocity * (VesselData.FetchMass(vessel) * 1000);
            GameObject thisVessel = new GameObject();

            Log.Info($"ActiveDecayRealistic, TimeWarp.CurrentRate: {TimeWarp.CurrentRate}  TimeWarp.WarpMode: {TimeWarp.WarpMode}");

            if (TimeWarp.CurrentRate == 0 || TimeWarp.CurrentRate > 0 && TimeWarp.WarpMode == TimeWarp.Modes.LOW)
            {
                Log.Info($"ActiveDecayRealistic 2");
                if (vessel.vesselType == VesselType.EVA) return;
#if false
                foreach (Part p in vessel.parts)
                {
                    if (p.physicalSignificance == Part.PhysicalSignificance.FULL &&
                        p.Rigidbody != null)
                    {
                        // NBody Active
                        // p.Rigidbody.AddForce(Vector3d.back * (decayForce)); // 1.5.0
                                
                    }
                }
#endif
                VesselData.UpdateVesselSMA(vessel, newSemiMajorAxis);
            }

            else if (TimeWarp.CurrentRate > 0 && TimeWarp.WarpMode == TimeWarp.Modes.HIGH) // 1.3.0 Timewarp Fix
            {
                Log.Info($"ActiveDecayRealistic 3");
                bool multipleLoadedSceneVessels = false; // 1.4.0 Debris warp fix
                multipleLoadedSceneVessels = CheckVesselProximity(vessel);

                if (multipleLoadedSceneVessels) return;
                if (vessel.vesselType == VesselType.EVA) return;
                //NBodyManager.ManageVessel(vessel); // 1.6.0 NBody

                VesselData.UpdateVesselSMA(vessel, VesselData.FetchSMA(vessel) - decayValue);
                CatchUpOrbit(vessel);
            }
        }

        public static void ActiveDecayStock(Vessel vessel)
        {
            Log.Info($"ActiveDecayStock 1");
            if (!CheckReferenceBody(vessel)) return;
            double readTime = HighLogic.CurrentGame.UniversalTime;
            double decayValue = DecayRateTotal(vessel);
            double initialVelocity = vessel.orbitDriver.orbit.getOrbitalVelocityAtUT(readTime).magnitude;
            double calculatedFinalVelocity = 0.0;
            Orbit newOrbit = vessel.orbitDriver.orbit;
            //newOrbit.semiMajorAxis = (VesselData.FetchSMA(vessel) - DecayValue);
            double newSemiMajorAxis = VesselData.FetchSMA(vessel) - decayValue;
            calculatedFinalVelocity = newOrbit.getOrbitalVelocityAtUT(readTime).magnitude;
            double deltaVelocity = initialVelocity - calculatedFinalVelocity;
            double decayForce = deltaVelocity * (VesselData.FetchMass(vessel) / 1000.0);
            GameObject thisVessel = new GameObject();

            Log.Info($"ActiveDecayStock, TimeWarp.CurrentRate: {TimeWarp.CurrentRate}  TimeWarp.WarpMode: {TimeWarp.WarpMode}");

            if (TimeWarp.CurrentRate == 0 || TimeWarp.CurrentRate > 0 && TimeWarp.WarpMode == TimeWarp.Modes.LOW)
            {
                if (vessel.vesselType == VesselType.EVA) return;
                Log.Info($"ActiveDecayStock 2");
#if false
                foreach (Part p in vessel.parts)
                {
                    if (p.physicalSignificance == Part.PhysicalSignificance.FULL &&
                        p.Rigidbody != null)
                    {
                        // p.Rigidbody.AddForce((Vector3d.back * (decayForce))); // 1.5.0 Too Fast Still
                    }
                }
#endif
                VesselData.UpdateVesselSMA(vessel, newSemiMajorAxis);

            }

            else if (TimeWarp.CurrentRate > 0 && TimeWarp.WarpMode == TimeWarp.Modes.HIGH) // 1.3.0 Timewarp Fix
            {
                Log.Info($"ActiveDecayStock 3");
                // 1.4.0 Debris warp fix
                bool multipleLoadedSceneVessels = CheckVesselProximity(vessel);
                if (multipleLoadedSceneVessels) return;
                if (vessel.vesselType == VesselType.EVA) return;
                VesselData.UpdateVesselSMA(vessel, VesselData.FetchSMA(vessel) - decayValue);
                CatchUpOrbit(vessel);
            }
        }

        #endregion

        #region Simulation Decay Rate Subroutines

        public static double DecayRateRadiationPressure(Vessel vessel)
        {
            CelestialBody body = vessel.orbitDriver.orbit.referenceBody;
            double solarEnergy = Math.Pow(3.86 * 10.0, 26.0); // W
            double solarDistance = vessel.orbitDriver.orbit.referenceBody == Sun.Instance.sun ?
                vessel.orbitDriver.orbit.altitude :
                vessel.orbitDriver.orbit.referenceBody.orbit.altitude;

            double solarConstant = solarEnergy / (4.0 * Math.PI * Math.Pow(solarDistance, 2.0));
            double initialSemiMajorAxis = VesselData.FetchSMA(vessel);
            double standardGravitationalParameter = body.gravParameter;
            double meanAngularVelocity = Math.Sqrt(standardGravitationalParameter / Math.Pow(initialSemiMajorAxis, 3.0));
            double speedOfLight = Math.Pow(3.0 * 10.0, 8.0);

            double vesselArea = VesselData.FetchArea(vessel);
            if (vesselArea == 0)
            {
                vesselArea = 1.0;
            }

            double vesselMass = VesselData.FetchMass(vessel);   // Kg
            if (vesselMass == 0)
            {
                vesselMass = 100.0;
            }

            double vesselRadius = Math.Sqrt(vesselArea / Math.PI);
            double immobileAccelleration = Math.PI * (vesselRadius * vesselRadius) * solarConstant / (vesselMass * speedOfLight * (solarDistance * solarDistance));
            double changeInSemiMajorAxis = -(6.0 * Math.PI * immobileAccelleration * initialSemiMajorAxis) / (meanAngularVelocity * speedOfLight);

            double decayRateModifier = 0.0;
            decayRateModifier = Settings.ReadDecayDifficulty();
            double decayRate = changeInSemiMajorAxis * TimeWarp.CurrentRate * decayRateModifier;

            return decayRate;
        }

        public static double DecayRateAtmosphericDrag(Vessel vessel) // Removed floats 
        {
            double decayRate = 0.0;
            Orbit orbit = vessel.orbitDriver.orbit;
            CelestialBody body = orbit.referenceBody;

            if (Settings.ReadRD())
            {
                if (body.atmosphere != true) return decayRate;
                double initialSemiMajorAxis = VesselData.FetchSMA(vessel);
                double maxInfluence = body.Radius * 1.5;

                if (!(initialSemiMajorAxis < maxInfluence)) return decayRate;
                double standardGravitationalParameter = body.gravParameter;
                double cartesianPositionVectorMagnitude = orbit.getRelativePositionAtT(HighLogic.CurrentGame.UniversalTime).magnitude; // Planetarium
                double equivalentAltitude = initialSemiMajorAxis - body.Radius;
                double eccentricity = VesselData.FetchECC(vessel);

                if (eccentricity > 0.085)
                {
                    double altitudeAp = initialSemiMajorAxis * (1 + eccentricity) - body.Radius;
                    double altitudePe = initialSemiMajorAxis * (1 - eccentricity) - body.Radius;
                    equivalentAltitude = altitudePe + 900 * Math.Pow(eccentricity, 0.6);
                }
                double initialOrbitalVelocity = orbit.vel.magnitude;
                double initialDensity = body.atmDensityASL;
                double boltzmannConstant = Math.Pow(1.380 * 10, -23);
                double altitude = vessel.altitude;
                double gravityAsl = body.GeeASL;
                double atmosphericMolarMass = body.atmosphereMolarMass;
                double vesselArea = VesselData.FetchArea(vessel);
                if (vesselArea == 0)
                {
                    vesselArea = 1.0;
                }

                double distanceTravelled = initialOrbitalVelocity; // Meters
                double vesselMass = VesselData.FetchMass(vessel);   // Kg
                if (vesselMass == 0)
                {
                    vesselMass = 100.0; // Default is 100kg
                }

                equivalentAltitude = equivalentAltitude / 1000.0;

                double molecularMass = 27 - 0.0012 * (equivalentAltitude - 200);
                double f107Flux = WhitecatIndustries.SCSManager.FetchCurrentF107();
                double geomagneticIndex = WhitecatIndustries.SCSManager.FetchCurrentAp();

                double exothericTemperature = 900.0 + 2.5 * (f107Flux - 70.0) + 1.5 * geomagneticIndex;
                double scaleHeight = exothericTemperature / molecularMass;
                double atmosphericDensity = 6.0 * Math.Pow(10.0, -10.0) * Math.Pow(Math.E, -((equivalentAltitude - 175.0f) / scaleHeight));

                double deltaPeriod = 3 * Math.PI * (initialSemiMajorAxis * atmosphericDensity) * (vesselArea * 2.2 / vesselMass); // Unitless
                double initialPeriod = 2 * Math.PI * Math.Sqrt(Math.Pow(initialSemiMajorAxis, 3) / standardGravitationalParameter);
                double finalPeriod = initialPeriod - deltaPeriod;
                double finalSemiMajorAxis = Math.Pow(Math.Pow(finalPeriod / (2.0 * Math.PI), 2.0) * standardGravitationalParameter, 1.0 / 3.0);

                double decayRateModifier = Settings.ReadDecayDifficulty();

                decayRate = (initialSemiMajorAxis - finalSemiMajorAxis) * TimeWarp.CurrentRate * decayRateModifier;
            }
            else
            {
                if (body.atmosphere != true) return decayRate;
                double initialSemiMajorAxis = VesselData.FetchSMA(vessel);
                double maxInfluence = body.Radius * 1.5;

                if (!(initialSemiMajorAxis < maxInfluence)) return decayRate;
                double standardGravitationalParameter = body.gravParameter;
                double cartesianPositionVectorMagnitude = orbit.getRelativePositionAtT(HighLogic.CurrentGame.UniversalTime).magnitude; // Planetarium
                double equivalentAltitude = initialSemiMajorAxis - body.Radius;
                double eccentricity = VesselData.FetchECC(vessel);

                if (eccentricity > 0.085)
                {
                    double altitudeAp = initialSemiMajorAxis * (1 + eccentricity) - body.Radius;
                    double altitudePe = initialSemiMajorAxis * (1 - eccentricity) - body.Radius;
                    equivalentAltitude = altitudePe + 900 * Math.Pow(eccentricity, 0.6);
                }
                double initialOrbitalVelocity = orbit.vel.magnitude;
                double initialDensity = body.atmDensityASL;
                double boltzmannConstant = Math.Pow(1.380 * 10, -23);
                double altitude = vessel.altitude;
                double gravityAsl = body.GeeASL;
                double atmosphericMolarMass = body.atmosphereMolarMass;
                double vesselArea = VesselData.FetchArea(vessel);
                if (vesselArea == 0)
                {
                    vesselArea = 5.0;
                }

                double distanceTravelled = initialOrbitalVelocity; // Meters
                double vesselMass = VesselData.FetchMass(vessel);   // Kg
                if (vesselMass == 0)
                {
                    vesselMass = 1000.0; // Default is 100kg
                }

                equivalentAltitude = equivalentAltitude / 1000.0;

                double atmosphericDensity = 1.020 * (Math.Pow(10, 7.0) * Math.Pow(equivalentAltitude + 70.0, -7.172)); // Kg/m^3 // *1
                double deltaPeriod = 3 * Math.PI * (initialSemiMajorAxis * atmosphericDensity) * (vesselArea * 2.2 / vesselMass); // Unitless
                double initialPeriod = 2 * Math.PI * Math.Sqrt(Math.Pow(initialSemiMajorAxis, 3) / standardGravitationalParameter);
                double finalPeriod = initialPeriod - deltaPeriod;
                double finalSemiMajorAxis = Math.Pow(Math.Pow(finalPeriod / (2.0 * Math.PI), 2.0) * standardGravitationalParameter, 1.0 / 3.0);

                double decayRateModifier = Settings.ReadDecayDifficulty();

                decayRate = (initialSemiMajorAxis - finalSemiMajorAxis) * TimeWarp.CurrentRate * decayRateModifier;
            }

            return decayRate;
        }

        public static double DecayRateGravitationalPertubation(Vessel vessel)
        {
            Orbit orbit = vessel.orbitDriver.orbit;
            CelestialBody body = orbit.referenceBody;
            double decayRate = 0.0;

            double gravitationalConstant = 6.67408 * Math.Pow(10.0, -11.0); // G [Newton Meter Squared per Square Kilogram]
            double forceAtSurface = gravitationalConstant * VesselData.FetchMass(vessel) * vessel.orbitDriver.orbit.referenceBody.Mass;
            double forceAtDistance = gravitationalConstant * VesselData.FetchMass(vessel) * vessel.orbitDriver.orbit.referenceBody.Mass / Math.Pow(vessel.orbitDriver.orbit.altitude, 2.0);
            if (!(forceAtDistance > 0.0000000000001 * forceAtSurface)) return decayRate;
            if (vessel.isActiveVessel)
            {
                if (MasConData.CheckMasConProximity(vessel))
                {
                    decayRate = MasConManager.GetCalculatedSMAChange(vessel, orbit.LAN, orbit.meanAnomalyAtEpoch, orbit.argumentOfPeriapsis, orbit.eccentricity, orbit.inclination, orbit.semiMajorAxis, orbit.epoch) * TimeWarp.CurrentRate;
                }
            }
            else
            {
                if (MasConData.CheckMasConProximity(vessel))
                {
                    decayRate = MasConManager.GetSecularSMAChange(vessel, orbit.LAN, orbit.meanAnomalyAtEpoch, orbit.argumentOfPeriapsis, orbit.eccentricity, orbit.inclination, orbit.semiMajorAxis, orbit.epoch);
                }
            }

            return decayRate;
        }

        public static double DecayRateYarkovskyEffect(Vessel vessel)
        {
            double decayRate = YarkovskyEffect.FetchDeltaSMA(vessel);
            return decayRate;
        }

        public static double DecayRateNBodyPerturbation(Vessel vessel)
        {
            double decayRate = 0;
            //decayRate = NBodyManager.CalculateSMA(vessel, vessel.orbitDriver.orbit.getOrbitalSpeedAtRelativePos(vessel.orbitDriver.orbit.getRelativePositionAtUT(HighLogic.CurrentGame.UniversalTime)), HighLogic.CurrentGame.UniversalTime, 1.0);

            // Work this out.

            return decayRate;
        }

        public static double DecayRateTotal(Vessel vessel)
        {
            double total = DecayRateAtmosphericDrag(vessel) + DecayRateGravitationalPertubation(vessel) + DecayRateRadiationPressure(vessel) + DecayRateYarkovskyEffect(vessel);
            return total;
        } // Total for 1.5.0

        #endregion

        #region Editor Decay Rate Subroutines

        public static double EditorDecayRateRadiationPressure(double mass, double area, double sma, double eccentricity, CelestialBody body)
        {
            double solarEnergy = Math.Pow(3.86 * 10.0, 26.0); // W
            double solarDistance;
            if (body == Sun.Instance.sun) // Checks for the sun
            {
                solarDistance = sma - body.Radius;
            }
            else
            {
                solarDistance = body.orbitDriver.orbit.altitude;
            }

            double solarConstant = solarEnergy / (4.0 * Math.PI * Math.Pow(solarDistance, 2.0));
            double initialSemiMajorAxis = sma;
            double standardGravitationalParameter = body.gravParameter;
            double meanAngularVelocity = Math.Sqrt(standardGravitationalParameter / Math.Pow(initialSemiMajorAxis, 3.0));
            double speedOfLight = Math.Pow(3.0 * 10.0, 8.0);

            double vesselArea = area;
            if (vesselArea == 0)
            {
                vesselArea = 1.0;
            }

            double vesselMass = mass;   // Kg
            if (vesselMass == 0)
            {
                vesselMass = 100.0;
            }

            double vesselRadius = Math.Sqrt(vesselArea / Math.PI);
            double immobileAccelleration = Math.PI * (vesselRadius * vesselRadius) * solarConstant / (vesselMass * speedOfLight * (solarDistance * solarDistance));
            double changeInSemiMajorAxis = -(6.0 * Math.PI * immobileAccelleration * initialSemiMajorAxis) / (meanAngularVelocity * speedOfLight);

            double decayRateModifier = Settings.ReadDecayDifficulty();
            double decayRate = changeInSemiMajorAxis * decayRateModifier;

            return decayRate;
        } // 1.6.0

        public static double EditorDecayRateAtmosphericDrag(double mass, double area, double sma, double eccentricity, CelestialBody body)
        {
            double decayRate = 0.0;

            if (Settings.ReadRD())
            {
                if (body.atmosphere != true) return decayRate;
                double initialSemiMajorAxis = sma;
                double maxInfluence = body.Radius * 1.5;

                if (!(initialSemiMajorAxis < maxInfluence)) return decayRate;
                double standardGravitationalParameter = body.gravParameter;
                double equivalentAltitude = initialSemiMajorAxis - body.Radius;
                //double eccentricity = eccentricity;

                if (eccentricity > 0.085)
                {
                    double altitudeAp = initialSemiMajorAxis * (1 + eccentricity) - body.Radius;
                    double altitudePe = initialSemiMajorAxis * (1 - eccentricity) - body.Radius;
                    equivalentAltitude = altitudePe + 900 * Math.Pow(eccentricity, 0.6);
                }
                double initialDensity = body.atmDensityASL;
                double boltzmannConstant = Math.Pow(1.380 * 10, -23);
                double altitude = sma - body.Radius;
                double gravityAsl = body.GeeASL;
                double atmosphericMolarMass = body.atmosphereMolarMass;
                double vesselArea = area;
                if (vesselArea == 0)
                {
                    vesselArea = 1.0;
                }

                double vesselMass = mass;   // Kg
                if (vesselMass == 0)
                {
                    vesselMass = 100.0; // Default is 100kg
                }

                equivalentAltitude = equivalentAltitude / 1000.0;

                double molecularMass = 27 - 0.0012 * (equivalentAltitude - 200);
                double f107Flux = WhitecatIndustries.SCSManager.FetchCurrentF107();
                double geomagneticIndex = WhitecatIndustries.SCSManager.FetchCurrentAp();

                double exothericTemperature = 900.0 + 2.5 * (f107Flux - 70.0) + 1.5 * geomagneticIndex;
                double scaleHeight = exothericTemperature / molecularMass;
                double atmosphericDensity = 6.0 * Math.Pow(10.0, -10.0) * Math.Pow(Math.E, -((equivalentAltitude - 175.0f) / scaleHeight));

                double deltaPeriod = 3 * Math.PI * (initialSemiMajorAxis * atmosphericDensity) * (vesselArea * 2.2 / vesselMass); // Unitless
                double initialPeriod = 2 * Math.PI * Math.Sqrt(Math.Pow(initialSemiMajorAxis, 3) / standardGravitationalParameter);
                double finalPeriod = initialPeriod - deltaPeriod;
                double finalSemiMajorAxis = Math.Pow(Math.Pow(finalPeriod / (2.0 * Math.PI), 2.0) * standardGravitationalParameter, 1.0 / 3.0);

                double decayRateModifier = Settings.ReadDecayDifficulty();

                decayRate = (initialSemiMajorAxis - finalSemiMajorAxis) * TimeWarp.CurrentRate * decayRateModifier;
            }
            else
            {
                if (!body.atmosphere) return decayRate;
                double initialSemiMajorAxis = sma;
                double maxInfluence = body.Radius * 1.5;

                if (!(initialSemiMajorAxis < maxInfluence)) return decayRate;
                double standardGravitationalParameter = body.gravParameter;
                double equivalentAltitude = initialSemiMajorAxis - body.Radius;
                //double eccentricity = eccentricity;

                if (eccentricity > 0.085)
                {
                    double altitudeAp = initialSemiMajorAxis * (1 + eccentricity) - body.Radius;
                    double altitudePe = initialSemiMajorAxis * (1 - eccentricity) - body.Radius;
                    equivalentAltitude = altitudePe + 900 * Math.Pow(eccentricity, 0.6);
                }
                double initialDensity = body.atmDensityASL;
                double boltzmannConstant = Math.Pow(1.380 * 10, -23);
                double altitude = sma - body.Radius;
                double gravityAsl = body.GeeASL;
                double atmosphericMolarMass = body.atmosphereMolarMass;
                double vesselArea = area;
                if (vesselArea == 0)
                {
                    vesselArea = 5.0;
                }

                double vesselMass = mass; // Kg
                if (vesselMass == 0)
                {
                    vesselMass = 1000.0; // Default is 100kg
                }

                equivalentAltitude = equivalentAltitude / 1000.0;

                double atmosphericDensity = 1.020 * (Math.Pow(10, 7.0) * Math.Pow(equivalentAltitude + 70.0, -7.172)); // Kg/m^3 // *1
                double deltaPeriod = 3 * Math.PI * (initialSemiMajorAxis * atmosphericDensity) * (vesselArea * 2.2 / vesselMass); // Unitless
                double initialPeriod = 2 * Math.PI * Math.Sqrt(Math.Pow(initialSemiMajorAxis, 3) / standardGravitationalParameter);
                double finalPeriod = initialPeriod - deltaPeriod;
                double finalSemiMajorAxis = Math.Pow(Math.Pow(finalPeriod / (2.0 * Math.PI), 2.0) * standardGravitationalParameter, 1.0 / 3.0);

                double decayRateModifier = Settings.ReadDecayDifficulty();

                decayRate = (initialSemiMajorAxis - finalSemiMajorAxis) * TimeWarp.CurrentRate * decayRateModifier;
            }

            return decayRate;
        } // 1.6.0


        #endregion


        #region Timing Subroutines

        public static double DecayTimePredictionExponentialsVariables(Vessel vessel)
        {
            double initialSemiMajorAxis = VesselData.FetchSMA(vessel);
            Orbit orbit = vessel.GetOrbitDriver().orbit;
            CelestialBody body = vessel.orbitDriver.orbit.referenceBody;
            double initialPeriod = Math.PI * 2.0 * Math.Sqrt(initialSemiMajorAxis * initialSemiMajorAxis * initialSemiMajorAxis / body.gravParameter);

            double equivalentAltitude = initialSemiMajorAxis - body.Radius;
            if (orbit.eccentricity > 0.085)
            {
                double altitudeAp = initialSemiMajorAxis * (1 - orbit.eccentricity) - body.Radius;
                double altitudePe = initialSemiMajorAxis * (1 + orbit.eccentricity) - body.Radius;
                equivalentAltitude = altitudePe + 900 * Math.Pow(orbit.eccentricity, 0.6);
            }

            double baseAltitude = body.atmosphereDepth / 1000;

            double molecularMass = 27 - 0.0012 * (equivalentAltitude / 1000 - 200);
            double f107Flux = WhitecatIndustries.SCSManager.FetchAverageF107();
            double geomagneticIndex = WhitecatIndustries.SCSManager.FetchAverageAp();

            double exothericTemperature = 900.0 + 2.5 * (f107Flux - 70) + 1.5 * geomagneticIndex;
            double scaleHeight = exothericTemperature / molecularMass;
            double atmosphericDensity = 0;

            if (Settings.ReadRD() == false)
            {
                atmosphericDensity = 1.020 * Math.Pow(10.0, 7.0) * Math.Pow(equivalentAltitude / 1000.0 + 70.0, -7.172); // 1.4.2
            }
            else if (Settings.ReadRD())
            {
                atmosphericDensity = 6 * Math.Pow(10, -10) * Math.Pow(Math.E, -((equivalentAltitude / 1000 - 175.0f) / scaleHeight));
            }

            double beta = 1.0 / scaleHeight;

            double vesselArea = VesselData.FetchArea(vessel);
            if (vesselArea == 0)
            {
                vesselArea = 5.0;
            }

            double vesselMass = VesselData.FetchMass(vessel);   // Kg
            if (vesselMass == 0)
            {
                vesselMass = 1000.0;
            }

            equivalentAltitude = equivalentAltitude + body.Radius;


            double time1 = initialPeriod / (60.0 * 60.0) / 4.0 * Math.PI * ((2.0 * beta * equivalentAltitude + 1.0) / (atmosphericDensity * (beta * beta) * (equivalentAltitude * equivalentAltitude * equivalentAltitude)));
            double time2 = time1 * (vesselMass / (2.2 * vesselArea)) * (1 - Math.Pow(Math.E, beta * (baseAltitude - (equivalentAltitude - body.Radius) / 1000)));

            double daysUntilDecay = time2;

            return daysUntilDecay;
        } // 1.4.0

        public static double DecayTimePredictionEditor(double area, double mass, double sma, double eccentricity, CelestialBody body)
        {
            double initialSemiMajorAxis = sma;

            double initialPeriod = Math.PI * 2.0 * Math.Sqrt(initialSemiMajorAxis * initialSemiMajorAxis * initialSemiMajorAxis / body.gravParameter);

            double equivalentAltitude = initialSemiMajorAxis - body.Radius;
            if (eccentricity > 0.085)
            {
                double altitudeAp = initialSemiMajorAxis * (1 - eccentricity) - body.Radius;
                double altitudePe = initialSemiMajorAxis * (1 + eccentricity) - body.Radius;
                equivalentAltitude = altitudePe + 900 * Math.Pow(eccentricity, 0.6);
            }

            double baseAltitude = body.atmosphereDepth / 1000;

            double molecularMass = 27 - 0.0012 * (equivalentAltitude / 1000 - 200);
            double f107Flux = WhitecatIndustries.SCSManager.FetchAverageF107();
            double geomagneticIndex = WhitecatIndustries.SCSManager.FetchAverageAp();

            double exothericTemperature = 900.0 + 2.5 * (f107Flux - 70) + 1.5 * geomagneticIndex;
            double scaleHeight = exothericTemperature / molecularMass;
            double atmosphericDensity = 0;

            if (Settings.ReadRD() == false)
            {
                atmosphericDensity = 1.020 * Math.Pow(10.0, 7.0) * Math.Pow(equivalentAltitude / 1000.0 + 70.0, -7.172); // 1.4.2
            }
            else if (Settings.ReadRD())
            {
                atmosphericDensity = 6 * Math.Pow(10, -10) * Math.Pow(Math.E, -((equivalentAltitude / 1000 - 175.0f) / scaleHeight));
            }

            double beta = 1.0 / scaleHeight;

            double vesselArea = area;
            if (vesselArea == 0)
            {
                vesselArea = 5.0;
            }

            double vesselMass = mass;   // Kg
            if (vesselMass == 0)
            {
                vesselMass = 1000.0;
            }

            equivalentAltitude = equivalentAltitude + body.Radius;


            double time1 = initialPeriod / (60.0 * 60.0) / 4.0 * Math.PI * ((2.0 * beta * equivalentAltitude + 1.0) / (atmosphericDensity * (beta * beta) * (equivalentAltitude * equivalentAltitude * equivalentAltitude)));
            double time2 = time1 * (vesselMass / (2.2 * vesselArea)) * (1 - Math.Pow(Math.E, beta * (baseAltitude - (equivalentAltitude - body.Radius) / 1000)));

            double daysUntilDecay = time2;

            return daysUntilDecay;
        }

        public static double DecayTimePredictionLinearVariables(Vessel vessel)
        {
            double decayRateVariables = Math.Abs(DecayRateRadiationPressure(vessel)) + Math.Abs(DecayRateYarkovskyEffect(vessel)); //+ Math.Abs(DecayRateGravitationalPertubation(vessel));
            double timewarpRate;
            if (TimeWarp.CurrentRate == 0)
            {
                timewarpRate = 1;
            }
            else
            {
                timewarpRate = TimeWarp.CurrentRate;
            }
            // Time Until Impact
            return Math.Abs((VesselData.FetchSMA(vessel) - vessel.orbitDriver.orbit.referenceBody.Radius) / (decayRateVariables / timewarpRate));
        }

        #endregion
    }
}
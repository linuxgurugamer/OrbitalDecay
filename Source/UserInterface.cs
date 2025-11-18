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

using ClickThroughFix;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OrbitalDecay
{
    [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
    internal class UserInterface : MonoBehaviour
    {
        private static int currentTab;
        private static string[] tabs = { "Vessels", "Settings" };
        private static Rect MainwindowPosition = new Rect(150, 50, 300, 400);
        private static Rect DecayBreakdownwindowPosition = new Rect(0, 0, 450, 150);
        private static bool DecayBreakdownwindowPositionInitted = false;
        private static GUIStyle windowStyle = new GUIStyle(HighLogic.Skin.window);
        private static Color tabUnselectedColor = new Color(0.0f, 0.0f, 0.0f);
        private static Color tabSelectedColor = new Color(0.0f, 0.0f, 0.0f);
        private static Color tabUnselectedTextColor = new Color(0.0f, 0.0f, 0.0f);
        private static Color tabSelectedTextColor = new Color(0.0f, 0.0f, 0.0f);
        private GUISkin skins = HighLogic.Skin;
        private int id = Guid.NewGuid().GetHashCode();
        //public static ApplicationLauncherButton ToolbarButton;

        //public static bool Visible = false;
        //private static bool Hidden = false;
        //public static bool DecayBreakdownVisible;

        public static List<VesselType> FilterTypes = new List<VesselType>();

        //public static Texture launcher_icon;

        private Vector2 scrollPosition1 = Vector2.zero;
        private Vector2 scrollPosition2 = Vector2.zero;
        private Vector2 scrollPosition3 = Vector2.zero;
        private float MultiplierValue = 5.0f;
        private float MultiplierValue2 = 5.0f;

        private Vessel subwindowVessel = new Vessel();

        private void Awake()
        {
            GameEvents.onGUIApplicationLauncherDestroyed.Add(DestroyEvent);
            //GameEvents.onHideUI.Add(onHideUI);
            //GameEvents.onShowUI.Add(onShowUI);
        }

        public void OnDestroy()
        {
            GameEvents.onGUIApplicationLauncherDestroyed.Remove(DestroyEvent);
            //GameEvents.onHideUI.Remove(onHideUI);
            //GameEvents.onShowUI.Remove(onShowUI);
            DestroyEvent();
        }

        public void DestroyEvent()
        {
            //Visible = false;
            ToolbarInterface.GuiOff();
            ToolbarInterface.DecayBreakdownVisible = false;
        }

        public void OnGUI()
        {
            if (HighLogic.LoadedSceneIsEditor || !ToolbarInterface.Visible)
                return;
            MainwindowPosition = ClickThruBlocker.GUILayoutWindow(id, MainwindowPosition, MainWindow, "Orbital Decay Manager", windowStyle);
            if (ToolbarInterface.DecayBreakdownVisible)
            {
                if (ToolbarInterface.Visible && (!DecayBreakdownwindowPositionInitted || HighLogic.CurrentGame.Parameters.CustomParams<OD3>().snapBreakdownWindow))
                {
                    DecayBreakdownwindowPosition.x = MainwindowPosition.x + MainwindowPosition.width;
                    DecayBreakdownwindowPosition.y = MainwindowPosition.y;
                    DecayBreakdownwindowPositionInitted = true;
                }

                DecayBreakdownwindowPosition = ClickThruBlocker.GUILayoutWindow(8989, DecayBreakdownwindowPosition, DecayBreakdownWindow, "Orbital Decay Breakdown Display", windowStyle);
            }
        }

        public void MainWindow(int windowID)
        {
            if (GUI.Button(new Rect(MainwindowPosition.width - 22, 3, 19, 19), "x"))
            {
                //if (ToolbarButton != null)
                //    ToolbarButton.toggleButton.Value = false;
                //toolbarControl.SetFalse(true);
                ToolbarInterface.GuiOff();
            }
            GUILayout.BeginVertical();
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();

            for (int i = 0; i < tabs.Length; ++i)
            {
                if (GUILayout.Button(tabs[i]))
                {
                    currentTab = i;
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            switch (currentTab)
            {
                case 0:
                    InformationTab();
                    break;
                case 1:
                    SettingsTab();
                    break;

                default:
                    break;
            }
            GUILayout.EndVertical();
            GUI.DragWindow();
            MainwindowPosition.x = Mathf.Clamp(MainwindowPosition.x, 0f, Screen.width - MainwindowPosition.width);
            MainwindowPosition.y = Mathf.Clamp(MainwindowPosition.y, 0f, Screen.height - MainwindowPosition.height);
        }

        const float INFO_WIDTH = 330f;
        string filterString = "";
        bool showActiveVessel = false;
        public void InformationTab()
        {
            GUILayout.BeginHorizontal();
            GUI.skin.label.fontSize = 16;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUILayout.Label("Vessel Information Filters", GUILayout.Width(INFO_WIDTH));
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            GUI.skin.label.fontSize = 12;
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical();

            // 1.5.2 Filtering // 
            using (new GUILayout.HorizontalScope())
            {
                //GUILayout.FlexibleSpace();
                GUILayout.Label("Filter: ");
                filterString = GUILayout.TextField(filterString, GUILayout.Width(150));
                GUILayout.FlexibleSpace();
                if (HighLogic.LoadedSceneIsFlight)
                {
                    showActiveVessel = GUILayout.Toggle(showActiveVessel, "");
                    GUILayout.Label("Show Active Vessel");
                }
            }
            GUILayout.Space(3);

            using (new GUILayout.HorizontalScope())
            {
                GUIContent guiContent;

                guiContent = FilterTypes.Contains(VesselType.Probe) ?
                    new GUIContent(GameDatabase.Instance.GetTexture("WhitecatIndustries/OrbitalDecay/Icon/Icon_Unmanned_highlighted", false), "Unmanned") :
                    new GUIContent(GameDatabase.Instance.GetTexture("WhitecatIndustries/OrbitalDecay/Icon/Icon_Unmanned", false), "Unmanned");

                if (GUILayout.Button(guiContent))
                {
                    if (FilterTypes.Contains(VesselType.Probe))
                        FilterTypes.Remove(VesselType.Probe);
                    else
                        FilterTypes.Add(VesselType.Probe);
                }

                GUILayout.Space(3);
                guiContent = FilterTypes.Contains(VesselType.Relay) ?
                    new GUIContent(GameDatabase.Instance.GetTexture("WhitecatIndustries/OrbitalDecay/Icon/Icon_Relay_highlighted", false), "Relay") :
                    new GUIContent(GameDatabase.Instance.GetTexture("WhitecatIndustries/OrbitalDecay/Icon/Icon_Relay", false), "Relay");

                if (GUILayout.Button(guiContent))
                {
                    if (FilterTypes.Contains(VesselType.Relay))
                        FilterTypes.Remove(VesselType.Relay);
                    else
                        FilterTypes.Add(VesselType.Relay);
                }

                GUILayout.Space(3);

                guiContent = FilterTypes.Contains(VesselType.Ship) ?
                    new GUIContent(GameDatabase.Instance.GetTexture("WhitecatIndustries/OrbitalDecay/Icon/Icon_Ship_highlighted", false), "Ship") :
                    new GUIContent(GameDatabase.Instance.GetTexture("WhitecatIndustries/OrbitalDecay/Icon/Icon_Ship", false), "Ship");

                if (GUILayout.Button(guiContent))
                {
                    if (FilterTypes.Contains(VesselType.Ship))
                        FilterTypes.Remove(VesselType.Ship);
                    else
                        FilterTypes.Add(VesselType.Ship);
                }

                GUILayout.Space(3);
                guiContent = FilterTypes.Contains(VesselType.Station) ?
                    new GUIContent(GameDatabase.Instance.GetTexture("WhitecatIndustries/OrbitalDecay/Icon/Icon_Station_highlighted", false), "Station") :
                    new GUIContent(GameDatabase.Instance.GetTexture("WhitecatIndustries/OrbitalDecay/Icon/Icon_Station", false), "Station");
                if (GUILayout.Button(guiContent))
                {
                    if (FilterTypes.Contains(VesselType.Station))
                        FilterTypes.Remove(VesselType.Station);
                    else
                        FilterTypes.Add(VesselType.Station);
                }

                GUILayout.Space(3);

                guiContent = FilterTypes.Contains(VesselType.SpaceObject) ?
                    new GUIContent(GameDatabase.Instance.GetTexture("WhitecatIndustries/OrbitalDecay/Icon/Icon_Spaceobject_highlighted", false), "Space Object") :
                    new GUIContent(GameDatabase.Instance.GetTexture("WhitecatIndustries/OrbitalDecay/Icon/Icon_Spaceobject", false), "Space Object");

                if (GUILayout.Button(guiContent))
                {
                    if (FilterTypes.Contains(VesselType.SpaceObject))
                        FilterTypes.Remove(VesselType.SpaceObject);
                    else
                        FilterTypes.Add(VesselType.SpaceObject);

                    if (FilterTypes.Contains(VesselType.Unknown))
                        FilterTypes.Remove(VesselType.Unknown);
                    else
                        FilterTypes.Add(VesselType.Unknown);
                }

                GUILayout.Space(3);

                guiContent = FilterTypes.Contains(VesselType.Debris) ?
                    new GUIContent(GameDatabase.Instance.GetTexture("WhitecatIndustries/OrbitalDecay/Icon/Icon_Debris_highlighted", false), "Debris") :
                    new GUIContent(GameDatabase.Instance.GetTexture("WhitecatIndustries/OrbitalDecay/Icon/Icon_Debris", false), "Debris");

                if (GUILayout.Button(guiContent))
                {
                    if (FilterTypes.Contains(VesselType.Debris))
                        FilterTypes.Remove(VesselType.Debris);
                    else
                        FilterTypes.Add(VesselType.Debris);
                }

                GUILayout.Space(3);
            }

            GUILayout.Space(3);
            GUILayout.Label("_______________________________________");
            GUILayout.EndVertical();

            scrollPosition1 = GUILayout.BeginScrollView(scrollPosition1, GUILayout.Width(INFO_WIDTH), GUILayout.Height(350));
            bool Realistic = Settings.ReadRD();
            bool ClockType = Settings.Read24Hr();
            //151var Resource = Settings.ReadStationKeepingResource();
            var filterString1 = filterString.ToLower();
            foreach (Vessel vessel in FlightGlobals.Vessels)
            {
                if ((showActiveVessel && vessel == FlightGlobals.ActiveVessel) ||
                    (vessel.vesselName.ToLower().Contains(filterString1) && FilterTypes.Contains(vessel.vesselType)))
                {

                    if (vessel.situation == Vessel.Situations.ORBITING)
                    {
                        string StationKeeping = VesselData.FetchStationKeeping(vessel).ToString();
                        string StationKeepingFuelRemaining = ResourceManager.GetResources(vessel).ToString("F3");
                        string Resource = ResourceManager.GetResourceNames(vessel);
                        string ButtonText = "";
                        double HoursInDay = 6.0;

                        double DaysInYear = 0;
                        bool KerbinTime = GameSettings.KERBIN_TIME;

                        if (KerbinTime)
                        {
                            DaysInYear = 9203545 / (60 * 60 * HoursInDay);
                        }
                        else
                        {
                            DaysInYear = 31557600 / (60 * 60 * HoursInDay);
                        }


                        if (StationKeeping == "True")
                        {
                            ButtonText = "Disable Station Keeping";
                        }
                        else
                        {
                            ButtonText = "Enable Station Keeping";
                        }

                        if (ClockType)
                        {
                            HoursInDay = 24.0;
                        }
                        else
                        {
                            HoursInDay = 6.0;
                        }

                        GUILayout.BeginVertical();
                        //   GUILayout.Label("Vessels count" + VesselData.VesselInformation.CountNodes.ToString());
                        GUILayout.Label("Vessel Name: <B>" + vessel.vesselName + "</B>");
                        //    GUILayout.Label("Vessel Area: " + VesselData.FetchArea(vessel).ToString());
                        //    GUILayout.Label("Vessel Mass: " + VesselData.FetchMass(vessel).ToString());
                        GUILayout.Space(2);
                        GUILayout.Label("Orbiting Body: " + vessel.orbitDriver.orbit.referenceBody.GetName());
                        GUILayout.Space(2);

                        if (StationKeeping == "True")
                        {
                            GUILayout.Label("Current Total Decay Rate: Vessel is Station Keeping");
                            GUILayout.Space(2);
                        }
                        else
                        {
                            double TotalDecayRatePerSecond = Math.Abs(DecayManager.DecayRateAtmosphericDrag(vessel)) + Math.Abs(DecayManager.DecayRateRadiationPressure(vessel)) + Math.Abs(DecayManager.DecayRateYarkovskyEffect(vessel)); //+ Math.Abs(DecayManager.DecayRateGravitationalPertubation(vessel));
                            double ADDR = DecayManager.DecayRateAtmosphericDrag(vessel);
                            double GPDR = DecayManager.DecayRateGravitationalPertubation(vessel);
                            double PRDR = DecayManager.DecayRateRadiationPressure(vessel);
                            double YEDR = DecayManager.DecayRateYarkovskyEffect(vessel);

                            GUILayout.Label("Current Total Decay Rate: " + FormatDecayRateToString(TotalDecayRatePerSecond));
                            GUILayout.Space(2);

#if DEBUG
                            GUILayout.Label("AP: " + vessel.orbit.ApA.ToString("F1"));
                            GUILayout.Label($"PE: " + vessel.orbit.PeA.ToString("F1"));
                            GUILayout.Space(2);
#endif

                            double TimeUntilDecayInUnits = 0.0;
                            string TimeUntilDecayInDays = "";

                            if (ADDR != 0)
                            {
                                TimeUntilDecayInUnits = DecayManager.DecayTimePredictionExponentialsVariables(vessel);
                                TimeUntilDecayInDays = FormatTimeUntilDecayInDaysToString(TimeUntilDecayInUnits);
                            }
                            else
                            {
                                TimeUntilDecayInUnits = DecayManager.DecayTimePredictionLinearVariables(vessel);
                                TimeUntilDecayInDays = FormatTimeUntilDecayInSecondsToString(TimeUntilDecayInUnits);
                            }

                            GUILayout.Label("Approximate Time Until Decay: " + TimeUntilDecayInDays);
                            GUILayout.Space(2);
                        }

                        GUILayout.Label("Station Keeping: " + StationKeeping);
                        GUILayout.Space(2);
                        GUILayout.Label("Station Keeping Fuel Remaining: " + StationKeepingFuelRemaining);
                        GUILayout.Space(2);
                        GUILayout.Label("Using Fuel Type: " + Resource);//151
                        GUILayout.Space(2); //151

                        if (GUILayout.Button("Toggle Decay Rate Breakdown")) // Display a new window here?
                        {
                            subwindowVessel = vessel;
                            ToolbarInterface.DecayBreakdownVisible = !ToolbarInterface.DecayBreakdownVisible;
                        }

                        if (StationKeeping == "True")
                        {
                            double DecayRateSKL = 0;

                            DecayRateSKL = DecayManager.DecayRateAtmosphericDrag(vessel) + DecayManager.DecayRateRadiationPressure(vessel) + DecayManager.DecayRateYarkovskyEffect(vessel);


                            double StationKeepingLifetime = double.Parse(StationKeepingFuelRemaining) / (DecayRateSKL / TimeWarp.CurrentRate * VesselData.FetchEfficiency(vessel) /*ResourceManager.GetEfficiency(Resource)*/ * Settings.ReadResourceRateDifficulty()) / (60 * 60 * HoursInDay);

                            if (StationKeepingLifetime < -5) // SRP Fixes
                            {
                                GUILayout.Label("Station Keeping Fuel Lifetime: > 1000 years.");
                            }

                            else
                            {
                                if (StationKeepingLifetime > 365000 && HoursInDay == 24)
                                {
                                    GUILayout.Label("Station Keeping Fuel Lifetime: > 1000 years.");
                                }

                                else if (StationKeepingLifetime > 425000 && HoursInDay == 6)
                                {
                                    GUILayout.Label("Station Keeping Fuel Lifetime: > 1000 years.");
                                }

                                else
                                {
                                    if (StationKeepingLifetime > 425 && HoursInDay == 6)
                                    {
                                        GUILayout.Label("Station Keeping Fuel Lifetime: " + (StationKeepingLifetime / 425).ToString("F1") + " years.");
                                    }

                                    else if (StationKeepingLifetime > 365 && HoursInDay == 24)
                                    {
                                        GUILayout.Label("Station Keeping Fuel Lifetime: " + (StationKeepingLifetime / 365).ToString("F1") + " years.");
                                    }

                                    else
                                    {
                                        GUILayout.Label("Station Keeping Fuel Lifetime: " + StationKeepingLifetime.ToString("F1") + " days.");
                                    }
                                }
                            }
                            GUILayout.Space(3);
                        }

                        if (GUILayout.Button(ButtonText))
                        {
                            if (StationKeeping == "True")
                            {
                                VesselData.UpdateStationKeeping(vessel, false);
                                ScreenMessages.PostScreenMessage("Vessel: " + vessel.vesselName + ": Station Keeping Disabled");

                            }

                            if (StationKeeping == "False")
                            {
                                if (StationKeepingManager.EngineCheck(vessel))
                                {
                                    if (double.Parse(StationKeepingFuelRemaining) > 0.01) // Good enough...
                                    {

                                        VesselData.UpdateStationKeeping(vessel, true);
                                        ScreenMessages.PostScreenMessage("Vessel: " + vessel.vesselName + ": Station Keeping Enabled");
                                    }
                                    else
                                    {
                                        ScreenMessages.PostScreenMessage("Vessel: " + vessel.vesselName + " has no fuel to Station Keep!");
                                    }
                                }
                                else
                                {
                                    ScreenMessages.PostScreenMessage("Vessel: " + vessel.vesselName + " has no Engines or RCS modules on board!");
                                }
                            }
                        }
                        GUILayout.Space(2);
                        GUILayout.Label("_____________________________________");
                        GUILayout.Space(3);
                        GUILayout.EndVertical();
                    }

                }
            }
            GUILayout.EndScrollView();
            // Optionally display the tooltip near the mouse cursor
            // if (HighLogic.CurrentGame.Parameters.CustomParams<MissionPlannerSettings>().showTooltips)
            {
                if (!string.IsNullOrEmpty(GUI.tooltip))
                {
                    Vector2 mouse = Event.current.mousePosition;
                    GUIStyle style = GUI.skin.box;
                    Vector2 size = style.CalcSize(new GUIContent(GUI.tooltip));

                    // Small offset so it doesn’t overlap the cursor
                    Rect tipRect = new Rect(mouse.x + 16f, mouse.y + 16f, size.x + 8f, size.y + 4f);
                    GUI.Box(tipRect, GUI.tooltip);
                }
            }
        }

        public void SettingsTab()
        {

            GUILayout.BeginHorizontal();
            GUI.skin.label.fontSize = 16;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUILayout.Label("Control Panel", GUILayout.Width(INFO_WIDTH));
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            GUI.skin.label.fontSize = 12;
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical();
            GUILayout.Label("_________________________________________");
            GUILayout.Space(3);

            double DecayDifficulty = HighLogic.CurrentGame.Parameters.CustomParams<OD>().DecayDifficulty;
            double ResourceDifficulty = Settings.ReadResourceRateDifficulty();

            GUILayout.Space(2);
            if (GUILayout.Button("Toggle Kerbin Day (6 hour) / Earth Day (24 hour)"))
            {
                Settings.Write24H(!Settings.Read24Hr());
                if (Settings.Read24Hr())
                {
                    ScreenMessages.PostScreenMessage("Earth Day (24 hour) set.");
                }
                else
                {
                    ScreenMessages.PostScreenMessage("Kerbin Day (6 hour) set.");
                }

            }
            GUILayout.Space(3);
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            MultiplierValue = GUILayout.HorizontalSlider(MultiplierValue, 0.5f, 50.0f);
            GUILayout.Space(2);
            GUILayout.Label("Current Decay multiplier: " + DecayDifficulty.ToString("F1"));
            GUILayout.Space(2);
            GUILayout.Label("New Decay multiplier: " + (MultiplierValue / 5).ToString("F1"));
            GUILayout.Space(2);

            if (GUILayout.Button("Set Multiplier"))
            {
                Settings.WriteDifficulty(MultiplierValue / 5);
                ScreenMessages.PostScreenMessage("Decay Multiplier set to: " + (MultiplierValue / 5).ToString("F2"));
            }

            GUILayout.Space(2);
            GUILayout.Label("_________________________________________");
            GUILayout.Space(3);
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            MultiplierValue2 = GUILayout.HorizontalSlider(MultiplierValue2, 0.5f, 50.0f);
            GUILayout.Space(2);
            GUILayout.Label("Resource drain rate multiplier: " + ResourceDifficulty.ToString("F1"));
            GUILayout.Space(2);
            GUILayout.Label("New Resource drain rate multiplier: " + (MultiplierValue2 / 5).ToString("F1"));
            GUILayout.Space(2);

            if (GUILayout.Button("Set Multiplier"))
            {
                Settings.WriteResourceRateDifficulty(MultiplierValue2 / 5);
                ScreenMessages.PostScreenMessage("Resource drain rate multiplier: " + (MultiplierValue2 / 5).ToString("F1"));
            }

            GUILayout.Space(2);
            GUILayout.EndVertical();
        }

        public void DecayBreakdownWindow(int id)
        {
            if (GUI.Button(new Rect(DecayBreakdownwindowPosition.width - 22, 3, 19, 19), "x"))
            {
                if (ToolbarInterface.DecayBreakdownVisible)
                    ToolbarInterface.DecayBreakdownVisible = false;
            }
            GUILayout.BeginVertical();
            GUILayout.Space(10);
            try
            {
                Vessel vessel = subwindowVessel;
                double ADDR = DecayManager.DecayRateAtmosphericDrag(vessel);
                double GPDR = DecayManager.DecayRateGravitationalPertubation(vessel);
                double GPIDR = MasConManager.GetSecularIncChange(vessel, vessel.orbitDriver.orbit.LAN, vessel.orbitDriver.orbit.meanAnomaly, vessel.orbitDriver.orbit.argumentOfPeriapsis, vessel.orbitDriver.orbit.eccentricity, vessel.orbitDriver.orbit.inclination, vessel.orbitDriver.orbit.semiMajorAxis, vessel.orbitDriver.orbit.epoch);
                double GPLANDR = MasConManager.GetSecularLANChange(vessel, vessel.orbitDriver.orbit.LAN, vessel.orbitDriver.orbit.meanAnomaly, vessel.orbitDriver.orbit.argumentOfPeriapsis, vessel.orbitDriver.orbit.eccentricity, vessel.orbitDriver.orbit.inclination, vessel.orbitDriver.orbit.semiMajorAxis, vessel.orbitDriver.orbit.epoch);
                double PRDR = DecayManager.DecayRateRadiationPressure(vessel);
                double YEDR = DecayManager.DecayRateYarkovskyEffect(vessel);

                GUILayout.Label("Vessel: " + vessel.GetName());
                GUILayout.Space(4);
                GUILayout.Label("Atmospheric Drag Decay Rate (Delta SMA): " + FormatDecayRateToString(ADDR));
                GUILayout.Space(2);
                GUILayout.Label("Radiation Pressure Decay Rate (Delta SMA): " + FormatDecayRateToString(PRDR));
                GUILayout.Space(2);
                GUILayout.Label("Gravitational Effect Decay Rate (Delta SMA): " + FormatDecayRateToString(GPDR));
                GUILayout.Space(2);
                GUILayout.Label("Gravitational Effect Decay Rate (Delta INC): " + FormatDecayRateDegreesToString(GPIDR));
                GUILayout.Space(2);
                GUILayout.Label("Gravitational Effect Decay Rate (Delta LAN): " + FormatDecayRateDegreesToString(GPLANDR));
                GUILayout.Space(2);
                GUILayout.Label("Yarkovsky Effect Decay Rate (Delta SMA): " + FormatDecayRateSmallToString(YEDR));
                GUILayout.Space(2);
                GUILayout.Label("Note: Prediction estimates accurate to +/- 10% per day.");
            }
            catch (ArgumentNullException)
            {
                GUILayout.Label("WARNING: Error detected, this is a MasCon issue, this will not affect gameplay.");
                GUILayout.Space(2);
                GUILayout.Label("Please make a note of your vessel latitude and longitude and reference body and post in the Orbital Decay Forum.");
                GUILayout.Space(2);
                GUILayout.Label("Thanks, Whitecat106");
            }
            GUILayout.EndVertical();
            GUI.DragWindow();
            //MainwindowPosition.x = Mathf.Clamp(MainwindowPosition.x, 0f, Screen.width - MainwindowPosition.width);
            //MainwindowPosition.y = Mathf.Clamp(MainwindowPosition.y, 0f, Screen.height - MainwindowPosition.height);
        }

        public static string FormatDecayRateToString(double DecayRate)
        {
            double TimewarpRate = 0;

            DecayRate = Math.Abs(DecayRate);

            if (TimeWarp.CurrentRate == 0)
            {
                TimewarpRate = 1;
            }
            else
            {
                TimewarpRate = TimeWarp.CurrentRate;
            }

            string DecayRateString = $"none, DecayRate: {DecayRate}";
            DecayRate = DecayRate / TimewarpRate;
            double SecondsInYear = 0.0;
            double HoursInDay = 0.0;

            bool KerbinTime = GameSettings.KERBIN_TIME;

            if (KerbinTime)
            {
                SecondsInYear = 9203545;
                HoursInDay = 6;
            }
            else
            {
                SecondsInYear = 31557600;
                HoursInDay = 24;
            }

            double DecayRatePerDay = DecayRate * 60 * 60 * HoursInDay;
            double DecayRatePerYear = DecayRate * SecondsInYear;

            // Daily Rates //

            if (DecayRatePerDay > 1000.0)
            {
                DecayRateString = (DecayRatePerDay / 1000.0).ToString("F1") + "Km per day.";
            }

            else if (DecayRatePerDay <= 1000.0 && DecayRatePerDay >= 1.0)
            {
                DecayRateString = DecayRatePerDay.ToString("F1") + "m per day.";
            }

            else if (DecayRatePerDay < 1.0 && DecayRatePerDay >= 0.01)
            {
                DecayRateString = (DecayRatePerDay * 10).ToString("F1") + "cm per day.";
            }

            else if (DecayRatePerDay < 0.01 && DecayRatePerDay >= 0.001)
            {
                DecayRateString = (DecayRatePerDay * 100).ToString("F1") + "mm per day.";
            }

            else if (DecayRatePerDay < 0.001)
            {
                if (DecayRatePerYear > 1000.0)
                {
                    DecayRateString = (DecayRatePerYear / 1000.0).ToString("F1") + "Km per year.";
                }

                else if (DecayRatePerYear <= 1000.0 && DecayRatePerYear >= 1.0)
                {
                    DecayRateString = DecayRatePerYear.ToString("F1") + "m per year.";
                }

                else if (DecayRatePerYear < 1.0 && DecayRatePerYear >= 0.01)
                {
                    DecayRateString = (DecayRatePerYear * 10).ToString("F1") + "cm per year.";
                }

                else if (DecayRatePerYear < 0.01 && DecayRatePerYear >= 0.001)
                {
                    DecayRateString = (DecayRatePerYear * 100).ToString("F1") + "mm per year.";
                }

                else
                {
                    DecayRateString = "Negligible.";
                }
            }


            return DecayRateString;
        }

        public static string FormatDecayRateSmallToString(double DecayRate)
        {
            double TimewarpRate = 0;

            DecayRate = Math.Abs(DecayRate);

            if (TimeWarp.CurrentRate == 0)
            {
                TimewarpRate = 1;
            }
            else
            {
                TimewarpRate = TimeWarp.CurrentRate;
            }

            DecayRate = DecayRate / TimewarpRate;
            string DecayRateString = "";
            double SecondsInYear = 0.0;
            double HoursInDay = 0.0;

            bool KerbinTime = GameSettings.KERBIN_TIME;

            if (KerbinTime)
            {
                SecondsInYear = 9203545;
                HoursInDay = 6;
            }
            else
            {
                SecondsInYear = 31557600;
                HoursInDay = 24;
            }

            double DecayRatePerDay = DecayRate * 60 * 60 * HoursInDay;
            double DecayRatePerYear = DecayRate * SecondsInYear;

            // Daily Rates //

            if (DecayRatePerDay > 1000.0)
            {
                DecayRateString = (DecayRatePerDay / 1000.0).ToString("F1") + "Km per day.";
            }

            else if (DecayRatePerDay <= 1000.0 && DecayRatePerDay >= 1.0)
            {
                DecayRateString = DecayRatePerDay.ToString("F1") + "m per day.";
            }

            else if (DecayRatePerDay < 1.0 && DecayRatePerDay >= 0.01)
            {
                DecayRateString = (DecayRatePerDay * 10).ToString("F1") + "cm per day.";
            }

            else if (DecayRatePerDay < 0.01 && DecayRatePerDay >= 0.001)
            {
                DecayRateString = (DecayRatePerDay * 100).ToString("F1") + "mm per day.";
            }

            else if (DecayRatePerDay < 0.001)
            {
                if (DecayRatePerYear > 1.0)
                {
                    DecayRateString = (DecayRatePerYear / 1000.0).ToString("F1") + "m per year.";
                }

                else if (DecayRatePerYear <= 1.0 && DecayRatePerYear >= 0.01)
                {
                    DecayRateString = DecayRatePerYear.ToString("F1") + "m per year.";
                }

                else if (DecayRatePerYear < 0.01 && DecayRatePerYear >= 0.001)
                {
                    DecayRateString = (DecayRatePerYear * 10).ToString("F1") + "cm per year.";
                }

                else if (DecayRatePerYear < 0.001 && DecayRatePerYear >= 0.0001)
                {
                    DecayRateString = (DecayRatePerYear * 100).ToString("F1") + "µm per year.";
                }

                else if (DecayRatePerYear < 0.0001 && DecayRatePerYear >= 0.0000001)
                {
                    DecayRateString = (DecayRatePerYear * 100).ToString("F1") + "nm per year.";
                }

                else if (DecayRatePerYear < 0.0000001 && DecayRatePerYear >= 0.0000000001)
                {
                    DecayRateString = (DecayRatePerYear * 100).ToString("F1") + "pm per year.";
                }

                else if (DecayRatePerYear < 0.0000000001 && DecayRatePerYear >= 0.0000000000001)
                {
                    DecayRateString = (DecayRatePerYear * 100).ToString("F1") + "fm per year.";
                }

                else if (DecayRatePerYear < 0.0000000000001 && DecayRatePerYear >= 0.0000000000000001)
                {
                    DecayRateString = (DecayRatePerYear * 100).ToString("F1") + "am per year.";
                }

                else if (DecayRatePerYear < 0.0000000000000001 && DecayRatePerYear >= 0.0000000000000000001)
                {
                    DecayRateString = (DecayRatePerYear * 100).ToString("F1") + "zm per year.";
                }

                else if (DecayRatePerYear < 0.0000000000000000001 && DecayRatePerYear >= 0.0000000000000000000001)
                {
                    DecayRateString = (DecayRatePerYear * 100).ToString("F1") + "ym per year.";
                }

                else
                {
                    DecayRateString = "< 1.0 yocto-meter per year.";
                }
            }
            return DecayRateString;
        }

        public string FormatDecayRateDegreesToString(double DecayRate)
        {
            double TimewarpRate = 0;

            DecayRate = Math.Abs(DecayRate);

            if (TimeWarp.CurrentRate == 0)
            {
                TimewarpRate = 1;
            }
            else
            {
                TimewarpRate = TimeWarp.CurrentRate;
            }

            DecayRate = DecayRate / TimewarpRate;
            string DecayRateString = "";
            double SecondsInYear = 0.0;
            double HoursInDay = 0.0;

            bool KerbinTime = GameSettings.KERBIN_TIME;

            if (KerbinTime)
            {
                SecondsInYear = 9203545;
                HoursInDay = 6;
            }
            else
            {
                SecondsInYear = 31557600;
                HoursInDay = 24;
            }

            double DecayRatePerDay = DecayRate * 60 * 60 * HoursInDay;
            double DecayRatePerYear = DecayRate * SecondsInYear;

            // Daily Rates //

            if (DecayRatePerDay > 360.0)
            {
                DecayRateString = "Vessel too close to a Mass Concentration.";
            }

            else if (DecayRatePerDay <= 360.0 && DecayRatePerDay >= 1.0)
            {
                DecayRateString = DecayRatePerDay.ToString("F1") + "degrees per day.";
            }

            else if (DecayRatePerDay < 1.0 && DecayRatePerDay >= 1.0 / 60.0)
            {
                DecayRateString = (DecayRatePerDay * 10).ToString("F1") + "arc-minutes per day.";
            }

            else if (DecayRatePerDay < 1.0 / 60.0 && DecayRatePerDay >= 1.0 / 60.0 / 60.0)
            {
                DecayRateString = (DecayRatePerDay * 100).ToString("F1") + "arc-seconds per day.";
            }

            else if (DecayRatePerDay < 1.0 / 60.0 / 60.0)
            {
                if (DecayRatePerYear > 360.0)
                {
                    DecayRateString = "Vessel too close to a Mass Concentration.";
                }

                else if (DecayRatePerYear <= 360.0 && DecayRatePerYear >= 1.0)
                {
                    DecayRateString = DecayRatePerYear.ToString("F1") + "degrees per year.";
                }

                else if (DecayRatePerYear < 1.0 && DecayRatePerYear >= 1.0 / 60.0)
                {
                    DecayRateString = (DecayRatePerYear * 60).ToString("F1") + "arc-minutes per year.";
                }

                else if (DecayRatePerYear < 1.0 / 60.0 && DecayRatePerYear >= 1.0 / 60.0 / 60.0)
                {
                    DecayRateString = (DecayRatePerYear * 60 * 60).ToString("F1") + "arc-seconds per year.";
                }

                else
                {
                    DecayRateString = "Negligible.";
                }
            }


            return DecayRateString;
        }

        public static string FormatTimeUntilDecayInDaysToString(double TimeUntilDecayInDays)
        {
            TimeUntilDecayInDays = Math.Abs(TimeUntilDecayInDays);

            string DecayTimeString = "";
            double SecondsInYear = 0.0;
            double HoursInDay = 0.0;
            double DaysInYear = 0.0;

            bool KerbinTime = GameSettings.KERBIN_TIME;

            if (KerbinTime)
            {
                SecondsInYear = 9203545;
                HoursInDay = 6;
            }
            else
            {
                SecondsInYear = 31557600;
                HoursInDay = 24;
            }

            DaysInYear = SecondsInYear / (HoursInDay * 60 * 60);

            if (TimeUntilDecayInDays > DaysInYear)
            {
                if (TimeUntilDecayInDays / DaysInYear > 1000)
                {
                    DecayTimeString = "> 1000 years.";
                }
                else
                {
                    DecayTimeString = (TimeUntilDecayInDays / DaysInYear).ToString("F1") + " years.";
                }
            }

            else
            {
                if (TimeUntilDecayInDays > 1.0)
                {
                    DecayTimeString = TimeUntilDecayInDays.ToString("F1") + " days.";
                }

                else
                {
                    if (TimeUntilDecayInDays * HoursInDay > 1.0)
                    {
                        DecayTimeString = (TimeUntilDecayInDays * HoursInDay).ToString("F1") + " hours.";
                    }

                    else
                    {
                        DecayTimeString = "Decay Imminent.";
                    }
                }
            }


            return DecayTimeString;
        }

        public string FormatTimeUntilDecayInSecondsToString(double TimeUntilDecayInSeconds)
        {
            TimeUntilDecayInSeconds = Math.Abs(TimeUntilDecayInSeconds);

            string DecayTimeString = "";
            double SecondsInYear = 0.0;

            bool KerbinTime = GameSettings.KERBIN_TIME;

            if (KerbinTime)
            {
                SecondsInYear = 9203545;
            }
            else
            {
                SecondsInYear = 31557600;
            }

            try
            {
                DecayTimeString = KSPUtil.dateTimeFormatter.PrintTime(Math.Abs(TimeUntilDecayInSeconds), 2, false);
            }
            catch (ArgumentOutOfRangeException)
            {
                DecayTimeString = "Error!";
            }
            catch (OverflowException)
            {
                DecayTimeString = "Error!";
            }


            if (TimeUntilDecayInSeconds > 1000 * SecondsInYear)
            {
                DecayTimeString = "> 1000 years.";
            }

            return DecayTimeString;
        }
    }
}

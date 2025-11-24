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
using static OrbitalDecay.RegisterToolbar;

namespace OrbitalDecay
{
    [KSPAddon(KSPAddon.Startup.FlightAndKSC, false)]
    internal class UserInterfaceFlightAndKSP : UserInterface
    {

    }

    [KSPAddon(KSPAddon.Startup.TrackingStation, false)]
    internal class UserInterfaceTrackingStation : UserInterface
    {

    }

    internal class UserInterface : MonoBehaviour
    {
        const string ICON_FOLDER_BASE = "WhitecatIndustries/OrbitalDecay/Icon/";
        static string ICON_FOLDER = ICON_FOLDER_BASE;
        private static int currentTab;
        private static string[] tabs = { "Vessels", "Settings" };
        private static bool DecayBreakdownwindowPositionInitted = false;
        private static Color tabUnselectedColor = new Color(0.0f, 0.0f, 0.0f);
        private static Color tabSelectedColor = new Color(0.0f, 0.0f, 0.0f);
        private static Color tabUnselectedTextColor = new Color(0.0f, 0.0f, 0.0f);
        private static Color tabSelectedTextColor = new Color(0.0f, 0.0f, 0.0f);
        private GUISkin skins = HighLogic.Skin;
        private int id = Guid.NewGuid().GetHashCode();

        public static List<VesselType> FilterTypes = new List<VesselType>();

        private Vector2 scrollPosition1 = Vector2.zero;
        private Vector2 scrollPosition2 = Vector2.zero;
        private Vector2 scrollPosition3 = Vector2.zero;
        private float MultiplierValue = 5.0f;
        private float MultiplierValue2 = 5.0f;
        public static float windowScaling2;
        private Vessel subwindowVessel = new Vessel();

        private void Awake()
        {
            GameEvents.onGUIApplicationLauncherDestroyed.Add(DestroyEvent);
            windowScaling2 = (float)HighLogic.CurrentGame.Parameters.CustomParams<OD3>().windowScaling;
            UpdateIconFolder();
        }
        public static void UpdateIconFolder()
        {
            ICON_FOLDER = ICON_FOLDER_BASE + (HighLogic.CurrentGame.Parameters.CustomParams<OD3>().useFlatIcons ? "Flat/" : "Shaded/");
        }
        public void OnDestroy()
        {
            GameEvents.onGUIApplicationLauncherDestroyed.Remove(DestroyEvent);
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

        void DrawLine(float spaceBefore = 4, float spaceAfter = 4f)
        {
            GUILayout.Space(spaceBefore);
            var rect = GUILayoutUtility.GetRect(GUIContent.none,
                                                GUIStyle.none,
                                                GUILayout.Height(2),
                                                GUILayout.ExpandWidth(true));
            GUI.DrawTexture(rect, lineTex);
            GUILayout.Space(spaceAfter);
        }

        public void MainWindow(int windowID)
        {
            if (GUI.Button(new Rect(MainwindowPosition.width - 22, 3, 19, 19), "x"))
            {
                ToolbarInterface.GuiOff();
            }
            using (new GUILayout.VerticalScope())
            {
                GUILayout.Space(10);
                using (new GUILayout.HorizontalScope())
                {
                    for (int i = 0; i < tabs.Length; ++i)
                    {
                        if (GUILayout.Button(tabs[i]))
                        {
                            currentTab = i;
                            windowScaling2 = (float)HighLogic.CurrentGame.Parameters.CustomParams<OD3>().windowScaling;
                        }
                    }
                }
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
            }
            GUI.DragWindow();
            MainwindowPosition.x = Mathf.Clamp(MainwindowPosition.x, 0f, Screen.width - MainwindowPosition.width);
            MainwindowPosition.y = Mathf.Clamp(MainwindowPosition.y, 0f, Screen.height - MainwindowPosition.height);
        }

        string filterString = "";
        static bool showActiveVessel = false;

        const float BUTTON_WIDTH_BASE = 34;
        const float BUTTON_HEIGHT_BASE = 32;

        static float BUTTON_WIDTH = BUTTON_WIDTH_BASE;
        static float BUTTON_HEIGHT = BUTTON_HEIGHT_BASE;

        static Dictionary<Guid, bool> displayedVessel = new Dictionary<Guid, bool>();


        static Texture2D Icon_Debris,
            Icon_Debris_highlighted,
            Icon_Relay,
            Icon_Relay_highlighted,
            Icon_Ship,
            Icon_Ship_highlighted,
            Icon_Spaceobject,
            Icon_Spaceobject_highlighted,
            Icon_Station,
            Icon_Station_highlighted,
            Icon_Unmanned,
            Icon_Unmanned_highlighted;


        public static void UpdateTextures()
        {
            Icon_Debris = GameDatabase.Instance.GetTexture(ICON_FOLDER + "Icon_Debris", false);
            Icon_Debris_highlighted = GameDatabase.Instance.GetTexture(ICON_FOLDER + "Icon_Debris_highlighted", false);
            Icon_Relay = GameDatabase.Instance.GetTexture(ICON_FOLDER + "Icon_Relay", false);
            Icon_Relay_highlighted = GameDatabase.Instance.GetTexture(ICON_FOLDER + "Icon_Relay_highlighted", false);
            Icon_Ship = GameDatabase.Instance.GetTexture(ICON_FOLDER + "Icon_Ship", false);
            Icon_Ship_highlighted = GameDatabase.Instance.GetTexture(ICON_FOLDER + "Icon_Ship_highlighted", false);
            Icon_Spaceobject = GameDatabase.Instance.GetTexture(ICON_FOLDER + "Icon_Spaceobject", false);
            Icon_Spaceobject_highlighted = GameDatabase.Instance.GetTexture(ICON_FOLDER + "Icon_Spaceobject_highlighted", false);
            Icon_Station = GameDatabase.Instance.GetTexture(ICON_FOLDER + "Icon_Station", false);
            Icon_Station_highlighted = GameDatabase.Instance.GetTexture(ICON_FOLDER + "Icon_Station_highlighted", false);
            Icon_Unmanned = GameDatabase.Instance.GetTexture(ICON_FOLDER + "Icon_Unmanned", false);
            Icon_Unmanned_highlighted = GameDatabase.Instance.GetTexture(ICON_FOLDER + "Icon_Unmanned_highlighted", false);

            Icon_Debris = ResizeTexture(Icon_Debris, BUTTON_WIDTH * windowScaling2, BUTTON_HEIGHT * windowScaling2);
            Icon_Debris_highlighted = ResizeTexture(Icon_Debris_highlighted, BUTTON_WIDTH * windowScaling2, BUTTON_HEIGHT * windowScaling2);
            Icon_Relay = ResizeTexture(Icon_Relay, BUTTON_WIDTH * windowScaling2, BUTTON_HEIGHT * windowScaling2);
            Icon_Relay_highlighted = ResizeTexture(Icon_Relay_highlighted, BUTTON_WIDTH * windowScaling2, BUTTON_HEIGHT * windowScaling2);
            Icon_Ship = ResizeTexture(Icon_Ship, BUTTON_WIDTH * windowScaling2, BUTTON_HEIGHT * windowScaling2);
            Icon_Ship_highlighted = ResizeTexture(Icon_Ship_highlighted, BUTTON_WIDTH * windowScaling2, BUTTON_HEIGHT * windowScaling2);
            Icon_Spaceobject = ResizeTexture(Icon_Spaceobject, BUTTON_WIDTH * windowScaling2, BUTTON_HEIGHT * windowScaling2);
            Icon_Spaceobject_highlighted = ResizeTexture(Icon_Spaceobject_highlighted, BUTTON_WIDTH * windowScaling2, BUTTON_HEIGHT * windowScaling2);
            Icon_Station = ResizeTexture(Icon_Station, BUTTON_WIDTH * windowScaling2, BUTTON_HEIGHT * windowScaling2);
            Icon_Station_highlighted = ResizeTexture(Icon_Station_highlighted, BUTTON_WIDTH * windowScaling2, BUTTON_HEIGHT * windowScaling2);
            Icon_Unmanned = ResizeTexture(Icon_Unmanned, BUTTON_WIDTH * windowScaling2, BUTTON_HEIGHT * windowScaling2);
            Icon_Unmanned_highlighted = ResizeTexture(Icon_Unmanned_highlighted, BUTTON_WIDTH * windowScaling2, BUTTON_HEIGHT * windowScaling2);
        }

        public static Texture2D ResizeTexture(Texture2D source, float newWidth1, float newHeight1)
        {
            int newWidth = (int)Math.Round(newWidth1);
            int newHeight = (int)Math.Round(newHeight1);
            // Create a render target
            RenderTexture rt = new RenderTexture(newWidth, newHeight, 0, RenderTextureFormat.ARGB32);
            rt.filterMode = FilterMode.Bilinear;

            // Copy source → RT (scaled)
            RenderTexture.active = rt;
            Graphics.Blit(source, rt);

            // Read pixels back into a new texture
            Texture2D newTex = new Texture2D(newWidth, newHeight, TextureFormat.ARGB32, false);
            newTex.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
            newTex.Apply();

            // Cleanup
            RenderTexture.active = null;
            rt.Release();

            return newTex;
        }

        public void InformationTab()
        {
            using (new GUILayout.VerticalScope())
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUI.skin.label.fontSize = (int)LARGEFONTSIZE;
                    GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                    GUILayout.Label("Vessel Information Filters");
                    GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                    GUI.skin.label.fontSize = (int)FONTSIZE;
                    GUI.skin.button.fontSize = (int)FONTSIZE;

                    RegisterToolbar.noPadStyle.fixedHeight = BUTTON_WIDTH * windowScaling2;
                    RegisterToolbar.noPadStyle.fixedWidth = BUTTON_HEIGHT * windowScaling2;
                }

                // 1.5.2 Filtering // 
                using (new GUILayout.HorizontalScope())
                {
                    //GUILayout.FlexibleSpace();
                    GUILayout.Label("Filter: ");
                    filterString = GUILayout.TextField(filterString, GUILayout.Width(150));
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("+ all", GUILayout.Width(40)))
                    {
                        foreach (var v in FlightGlobals.Vessels)
                            displayedVessel[v.id] = true;
                    }
                    if (GUILayout.Button("- all", GUILayout.Width(40)))
                    {
                        foreach (var v in FlightGlobals.Vessels)
                            displayedVessel[v.id] = false;
                    }

                }
                if (HighLogic.LoadedSceneIsFlight)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        showActiveVessel = GUILayout.Toggle(showActiveVessel, "");
                        GUILayout.Label("Show Active Vessel");
                        if (showActiveVessel)
                            displayedVessel[FlightGlobals.ActiveVessel.id] = true;
                    }
                }
                     else
                        showActiveVessel = false;
               GUILayout.Space(3);

                using (new GUILayout.HorizontalScope())
                {
                    GUIContent guiContent;

                    guiContent = FilterTypes.Contains(VesselType.Probe) ?
                                            new GUIContent(Icon_Unmanned_highlighted, "Probe") :
                                            new GUIContent(Icon_Unmanned, "Probe");
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(guiContent, RegisterToolbar.noPadStyle))
                    {
                        if (FilterTypes.Contains(VesselType.Probe))
                            FilterTypes.Remove(VesselType.Probe);
                        else
                            FilterTypes.Add(VesselType.Probe);
                    }

                    guiContent = FilterTypes.Contains(VesselType.Relay) ?
                        new GUIContent(Icon_Relay_highlighted, "Relay") :
                        new GUIContent(Icon_Relay, "Relay");

                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(guiContent, RegisterToolbar.noPadStyle))
                    {
                        if (FilterTypes.Contains(VesselType.Relay))
                            FilterTypes.Remove(VesselType.Relay);
                        else
                            FilterTypes.Add(VesselType.Relay);
                    }


                    guiContent = FilterTypes.Contains(VesselType.Ship) ?
                        new GUIContent(Icon_Ship_highlighted, "Ship") :
                        new GUIContent(Icon_Ship, "Ship");

                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(guiContent, RegisterToolbar.noPadStyle))
                    {
                        if (FilterTypes.Contains(VesselType.Ship))
                            FilterTypes.Remove(VesselType.Ship);
                        else
                            FilterTypes.Add(VesselType.Ship);
                    }

                    guiContent = FilterTypes.Contains(VesselType.Station) ?
                        new GUIContent(Icon_Station_highlighted, "Station") :
                        new GUIContent(Icon_Station, "Station");
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(guiContent, RegisterToolbar.noPadStyle))
                    {
                        if (FilterTypes.Contains(VesselType.Station))
                            FilterTypes.Remove(VesselType.Station);
                        else
                            FilterTypes.Add(VesselType.Station);
                    }

                    guiContent = FilterTypes.Contains(VesselType.SpaceObject) ?
                        new GUIContent(Icon_Spaceobject_highlighted, "Space Object") :
                        new GUIContent(Icon_Spaceobject, "Space Object");

                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(guiContent, RegisterToolbar.noPadStyle))
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

                    guiContent = FilterTypes.Contains(VesselType.Debris) ?
                        new GUIContent(Icon_Debris_highlighted, "Debris") :
                        new GUIContent(Icon_Debris, "Debris");

                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(guiContent, RegisterToolbar.noPadStyle))
                    {
                        if (FilterTypes.Contains(VesselType.Debris))
                            FilterTypes.Remove(VesselType.Debris);
                        else
                            FilterTypes.Add(VesselType.Debris);
                    }

                    GUILayout.FlexibleSpace();
                }

                DrawLine(4, 4);
            }

            scrollPosition1 = GUILayout.BeginScrollView(scrollPosition1,
                                                        GUILayout.Width(INFO_WIDTH),
                                                        GUILayout.Height(SCROLLVIEW_HEIGHT));
            bool Realistic = Settings.ReadRD();
            bool ClockType = Settings.Read24Hr();
            var filterString1 = filterString.ToLower();

            foreach (Vessel vessel in FlightGlobals.Vessels)
            {
                if (vessel != null)
                {
                    if (!displayedVessel.ContainsKey(vessel.id))
                        displayedVessel[vessel.id] = false;

                    if ((showActiveVessel && vessel.id == FlightGlobals.ActiveVessel.id) ||
                        (!showActiveVessel && vessel.vesselName.ToLower().Contains(filterString1) && FilterTypes.Contains(vessel.vesselType))
                        )
                    {

                        using (new GUILayout.VerticalScope())
                        {
                            if (vessel.situation == Vessel.Situations.ORBITING)
                            {
                                bool StationKeeping = VesselData.FetchStationKeeping(vessel);
                                double StationKeepingFuelRemaining = ResourceManager.GetResources(vessel);
                                string Resource = ResourceManager.GetResourceNames(vessel);
                                double HoursInDay = 6.0;

                                double DaysInYear = 0;
                                bool KerbinTime = GameSettings.KERBIN_TIME;

                                DaysInYear = KerbinTime ? (9203545 / (60 * 60 * HoursInDay)) : (31557600 / (60 * 60 * HoursInDay));

                                HoursInDay = ClockType ? 24d : 6d;

                                //   GUILayout.Label("Vessels count" + VesselData.VesselInformation.CountNodes.ToString());
                                using (new GUILayout.HorizontalScope())
                                {
                                    if (GUILayout.Button(displayedVessel[vessel.id] ? "-" : "+", GUILayout.Width(20)))
                                    {
                                        displayedVessel[vessel.id] = !displayedVessel[vessel.id];
                                    }
                                    GUILayout.Label("Vessel Name: <B>" + vessel.vesselName + "</B>");
                                }
                                using (new GUILayout.HorizontalScope())
                                {
                                    GUILayout.Space(30);
                                    var result = OrbitFromAE.ComputeApPe(VesselData.FetchSMA(vessel), VesselData.FetchECC(vessel), vessel.mainBody);
                                    GUILayout.Label($"AP: {result.ApAltitude.ToString("F1")}");
                                    GUILayout.Space(20);
                                    GUILayout.Label($"PE: {result.PeAltitude.ToString("F1")}");
                                }

                                if (displayedVessel[vessel.id])
                                {
                                    //    GUILayout.Label("Vessel Area: " + VesselData.FetchArea(vessel).ToString());
                                    //    GUILayout.Label("Vessel Mass: " + VesselData.FetchMass(vessel).ToString());
                                    GUILayout.Space(2);
                                    using (new GUILayout.HorizontalScope())
                                    {
                                        GUILayout.Label("Orbiting Body: " + vessel.orbitDriver.orbit.referenceBody.GetName());
                                        GUILayout.FlexibleSpace();
                                        var semimajor = VesselData.FetchSMA(vessel);
                                        var mna = VesselData.FetchMNA(vessel);
                                    }
                                    GUILayout.Space(2);

                                    if (StationKeeping == true)
                                    {
                                        GUILayout.Label("Current Total Decay Rate: Vessel is Station Keeping");
                                        GUILayout.Space(2);
                                    }
                                    else
                                    {
                                        double ADDR = DecayManager.DecayRateAtmosphericDrag(vessel);
                                        double GPDR = DecayManager.DecayRateGravitationalPertubation(vessel);
                                        double PRDR = DecayManager.DecayRateRadiationPressure(vessel);
                                        double YEDR = DecayManager.DecayRateYarkovskyEffect(vessel);

                                        double TotalDecayRatePerSecond = Math.Abs(ADDR) + /* Math.Abs(GPDR) + */ Math.Abs(PRDR) + Math.Abs(YEDR);
                                        //Math.Abs(DecayManager.DecayRateAtmosphericDrag(vessel)) + 
                                        Log.Info($" Vessel: {vessel.vesselName}  ADDR: {ADDR} GPDR: {GPDR}  PRDR: {PRDR}  YEDR: {YEDR}");

                                        GUILayout.Label("Current Total Decay Rate: " + FormatDecayRateToString(TotalDecayRatePerSecond));
                                        GUILayout.Space(2);

                                        double TimeUntilDecayInUnits = 0.0;
                                        string TimeUntilDecayInDays = "";
                                        double days = 0;

                                        if (ADDR != 0)
                                        {
                                            TimeUntilDecayInUnits = DecayManager.DecayTimePredictionExponentialsVariables(vessel);
                                            TimeUntilDecayInDays = FormatTimeUntilDecayIn_Days_ToString(TimeUntilDecayInUnits);
                                            days = TimeUntilDecayInUnits;
                                        }
                                        else
                                        {
                                            TimeUntilDecayInUnits = DecayManager.DecayTimePredictionLinearVariables(vessel);
                                            TimeUntilDecayInDays = FormatTimeUntilDecayIn_Seconds_ToString(TimeUntilDecayInUnits);
                                            days = TimeUntilDecayInUnits / (3600 * HoursInDay);
                                        }


                                        if (days < 100000)
                                            GUILayout.Label("Approximate Time Until Decay: " + TimeUntilDecayInDays + " (" + days.ToString("N0") + " days)");
                                        else
                                            GUILayout.Label("Approximate Time Until Decay: " + TimeUntilDecayInDays);
                                        GUILayout.Space(2);
                                    }

                                    GUILayout.Label("Station Keeping: " + StationKeeping);
                                    GUILayout.Space(2);
                                    GUILayout.Label("Station Keeping Fuel Remaining: " + StationKeepingFuelRemaining.ToString("F3"));
                                    GUILayout.Space(2);
                                    GUILayout.Label("Using Fuel Type: " + Resource);//151
                                    GUILayout.Space(2); //151


                                    if (StationKeeping == true)
                                    {
                                        double DecayRateSKL = 0;

                                        DecayRateSKL = DecayManager.DecayRateAtmosphericDrag(vessel) + DecayManager.DecayRateRadiationPressure(vessel) + DecayManager.DecayRateYarkovskyEffect(vessel);


                                        double StationKeepingLifetime = StationKeepingFuelRemaining / (DecayRateSKL / TimeWarp.CurrentRate * VesselData.FetchEfficiency(vessel) /*ResourceManager.GetEfficiency(Resource)*/ * Settings.ReadResourceRateDifficulty()) / (60 * 60 * HoursInDay);

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

                                    using (new GUILayout.HorizontalScope())
                                    {
                                        if (GUILayout.Button((ToolbarInterface.DecayBreakdownVisible ? "Hide" : "Show") + " Decay Rate Detail")) // Display a new window here?
                                        {
                                            subwindowVessel = vessel;
                                            ToolbarInterface.DecayBreakdownVisible = !ToolbarInterface.DecayBreakdownVisible;
                                        }

                                        if (GUILayout.Button((StationKeeping ? "Disable" : "Enable") + " Station Keeping"))
                                        {
                                            if (StationKeeping == true)
                                            {
                                                VesselData.UpdateStationKeeping(vessel, false);
                                                ScreenMessages.PostScreenMessage("Vessel: " + vessel.vesselName + ": Station Keeping Disabled");
                                            }

                                            if (StationKeeping == false)
                                            {
                                                if (StationKeepingManager.EngineCheck(vessel))
                                                {
                                                    if (StationKeepingFuelRemaining > 0.01) // Good enough...
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
                                    }

                                    if (HighLogic.LoadedSceneIsFlight && vessel.id != FlightGlobals.ActiveVessel.id)
                                    {
                                        if (GUILayout.Button("Switch to vessel"))
                                        {
                                            VesselSwitcher.SwitchToGuid(vessel.id);
                                        }
                                    }
                                }

                                DrawLine(4, 4);
                            }
                            else
                            {
                                GUILayout.Label($"Vessel Name: <B>{vessel.vesselName}</B>");
                                GUILayout.Label($"Situation: {vessel.situation.ToString()}");
                                DrawLine(4, 4);
                            }
                        }
                    }
                }
            }
            GUILayout.EndScrollView();

            // Optionally display the tooltip near the mouse cursor
            if (HighLogic.CurrentGame.Parameters.CustomParams<OD3>().showTooltips)
            {
                if (!string.IsNullOrEmpty(GUI.tooltip))
                {
                    Vector2 mouse = Event.current.mousePosition;
                    var content = new GUIContent(GUI.tooltip);
                    Vector2 size = GUI.skin.label.CalcSize(content);

                    // Small offset so it doesn’t overlap the cursor
                    Rect tipRect = new Rect(mouse.x + 16f, mouse.y + 16f,
                                            (float)(size.x + 8f),
                                            (float)(size.y + 4f));
                    GUI.Box(tipRect, content);
                }
            }
        }

        public void SettingsTab()
        {
            using (new GUILayout.HorizontalScope())
            {
                GUI.skin.label.fontSize = (int)LARGEFONTSIZE;
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                GUILayout.Label("Control Panel", GUILayout.Width(INFO_WIDTH));
                GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                GUI.skin.label.fontSize = (int)FONTSIZE;
            }

            using (new GUILayout.VerticalScope())
            {
                DrawLine(4, 4);
                //using (new GUILayout.HorizontalScope())
                //{
                //GUILayout.FlexibleSpace();
                //GUILayout.Label("_________________________________________");
                //GUILayout.FlexibleSpace();
                //}
                //GUILayout.Space(3);

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

                //GUILayout.Space(2);
                DrawLine(4, 4);
                //GUILayout.Label("_________________________________________");
                //GUILayout.Space(3);
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

                //GUILayout.Space(2);
                DrawLine(4, 4);
                //GUILayout.Label("_________________________________________");
                //GUILayout.Space(3);
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                windowScaling2 = GUILayout.HorizontalSlider(windowScaling2, 0.8f, 2f);
                GUILayout.Space(2);
                GUILayout.Label("Window Scaling: " + HighLogic.CurrentGame.Parameters.CustomParams<OD3>().windowScaling.ToString("F1"));
                GUILayout.Space(2);
                GUILayout.Label("New Window Scaling: " + windowScaling2.ToString("F1"));
                GUILayout.Space(2);

                if (GUILayout.Button("Set Window Scaling"))
                {
                    HighLogic.CurrentGame.Parameters.CustomParams<OD3>().windowScaling = windowScaling2;
                    RegisterToolbar.UpdateWindowSizes();
                }
                GUILayout.Space(2);
            }
        }

        public void DecayBreakdownWindow(int id)
        {
            if (GUI.Button(new Rect(DecayBreakdownwindowPosition.width - 22, 3, 19, 19), "x"))
            {
                if (ToolbarInterface.DecayBreakdownVisible)
                    ToolbarInterface.DecayBreakdownVisible = false;
            }
            using (new GUILayout.VerticalScope())
            {
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

                    //GUILayout.Label("Vessel: " + vessel.GetName());
                    GUILayout.Label("Vessel: <B>" + vessel.vesselName + "</B>");
                    var result = OrbitFromAE.ComputeApPe(VesselData.FetchSMA(vessel), VesselData.FetchECC(vessel), vessel.mainBody);
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(30);
                        GUILayout.Label("AP: " + result.ApAltitude.ToString("F1"));
                    }
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(30);
                        GUILayout.Label("PE: " + result.PeAltitude.ToString("F1"));
                    }
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(30);
                        GUILayout.Label("Mass: " + VesselData.FetchMass(vessel).ToString("F1"));
                    }

                    GUILayout.Space(8);
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
            }
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

        public static string FormatTimeUntilDecayIn_Days_ToString(double TimeUntilDecayInDays)
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

        public string FormatTimeUntilDecayIn_Seconds_ToString(double TimeUntilDecayInSeconds)
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

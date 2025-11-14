using ClickThroughFix;
using KSP.UI.Screens;
using System;
using System.Collections.Generic;
using ToolbarControl_NS;
using UnityEngine;
using static KSP.UI.Screens.ApplicationLauncher;
using static OrbitalDecay.RegisterToolbar;

namespace OrbitalDecay
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    internal class EditorUserInterface : MonoBehaviour
    {
        const int WINWIDTH = 300;

        //private static int currentTab;
        //private static string[] tabs = { "Orbital Decay Utilities" };
        private static Rect MainwindowPosition = new Rect(250, 0, WINWIDTH + 40, 500);
        //private static Rect DecayBreakdownwindowPosition = new Rect(0, 0, 450, 150);
        //private static Rect NBodyManagerwindowPosition = new Rect(0, 0, 450, 150);
        private static GUIStyle windowStyle;
        //private static Color tabUnselectedColor = new Color(0.0f, 0.0f, 0.0f);
        //private static Color tabSelectedColor = new Color(0.0f, 0.0f, 0.0f);
        //private static Color tabUnselectedTextColor = new Color(0.0f, 0.0f, 0.0f);
        //private static Color tabSelectedTextColor = new Color(0.0f, 0.0f, 0.0f);
        // private GUISkin skin;
        private int id;

        //public bool Visible = false;
        //public bool Hidden = false;

        //public static Texture launcher_icon;
        private float AltitudeValue = 70000f;
        float MaxDisplayValue = 2100000;
        private static CelestialBody ReferenceBody;

        private void Awake()
        {
            Log.Info("{.Awake");
            id = Guid.NewGuid().GetHashCode();
            windowStyle = new GUIStyle(HighLogic.Skin.window);
            //skins = HighLogic.Skin;
            //ReferenceBody = FlightGlobals.GetHomeBody();

            //GameEvents.onGUIApplicationLauncherReady.Add(ReadyEvent);
            GameEvents.onGUIApplicationLauncherDestroyed.Add(DestroyEvent);
            //GameEvents.onHideUI.Add(onHideUI);
            //GameEvents.onShowUI.Add(onShowUI);


            foreach (CelestialBody body in FlightGlobals.Bodies)
            {
                if (body.isHomeWorld)
                {
                    ReferenceBody = body;
                    break;
                }
            }
        }

#if false
        public void ReadyEvent()
        {
            if (ApplicationLauncher.Ready && ToolbarButton == null)
            {
                ApplicationLauncher.AppScenes Scenes = ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH;
                launcher_icon = GameDatabase.Instance.GetTexture("WhitecatIndustries/OrbitalDecay/Icon/Icon_Toolbar", false);
                ToolbarButton = ApplicationLauncher.Instance.AddModApplication(GuiOn, GuiOff, null, null, null, null, Scenes, launcher_icon);
            }
        }
#endif

        public void OnDestroy()
        {
            GameEvents.onGUIApplicationLauncherDestroyed.Remove(DestroyEvent);
            //GameEvents.onGUIApplicationLauncherReady.Remove(ReadyEvent);
            //GameEvents.onHideUI.Remove(onHideUI);
            //GameEvents.onShowUI.Remove(onShowUI);
            DestroyEvent();
        }

        public void DestroyEvent()
        {
            //if (ToolbarButton == null) return;
            //ApplicationLauncher.Instance.RemoveModApplication(ToolbarButton);
            //ToolbarButton = null;
            ToolbarInterface.GuiOff(); // Visible = false;
        }

#if false
        private void GuiOn()        {            Visible = true;        }

        private void GuiOff()        {            Visible = false;        }


        private void onHideUI() { Hidden = true; }
        private void onShowUI() { Hidden = false; }
#endif

        public void OnGUI()
        {
            if (ToolbarInterface.Visible) //&& !Hidden)
            {
                if (!HighLogic.CurrentGame.Parameters.CustomParams<OD3>().useAltSkin)
                GUI.skin = HighLogic.Skin;

                MainwindowPosition = ClickThruBlocker.GUILayoutWindow(id, MainwindowPosition, MainWindow, "Orbital Decay Utilities", windowStyle);
            }
        }

        static GUIStyle centeredBigLabel = null, centeredLabel;
        Vector2 scrollPos;
        static GUIStyle hSmallScrollBar;

        public void MainWindow(int windowID)
        {
            if (centeredBigLabel == null)
            {
                centeredBigLabel = new GUIStyle(GUI.skin.label);
                centeredBigLabel.fontSize = 16;
                centeredBigLabel.alignment = TextAnchor.MiddleCenter;

                centeredLabel = new GUIStyle(GUI.skin.label);
                centeredLabel.alignment = TextAnchor.MiddleCenter;
                hSmallScrollBar = new GUIStyle(GUI.skin.horizontalScrollbar);
                hSmallScrollBar.fixedHeight = 0f;
            }

            if (GUI.Button(new Rect(MainwindowPosition.width - 22, 3, 19, 19), "x"))
            {
                //if (ToolbarButton != null)
                //    ToolbarButton.toggleButton.Value = false;
                ToolbarInterface.GuiOff();
                //T.SetFalse(true);

            }
            GUILayout.BeginVertical();
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();

#if false
            for (int i = 0; i < tabs.Length; ++i)
            {
                if (GUILayout.Button(tabs[i]))
                {
                    currentTab = i;
                }
            }
#endif

            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            scrollPos = GUILayout.BeginScrollView(scrollPos, false, false, hSmallScrollBar, GUI.skin.verticalScrollbar);

            InformationTab();
            GUILayout.EndScrollView();

            GUILayout.EndVertical();
            GUI.DragWindow();
            MainwindowPosition.height = Math.Min(MainwindowPosition.height, Screen.height - 100);
            MainwindowPosition.x = Mathf.Clamp(MainwindowPosition.x, 0f, Screen.width - MainwindowPosition.width);
            MainwindowPosition.y = Mathf.Clamp(MainwindowPosition.y, 0f, Screen.height - MainwindowPosition.height);
        }

        void SetMaxDisplayValue(CelestialBody ReferenceBody)
        {
            MaxDisplayValue = (float)ReferenceBody.atmosphereDepth * 30f;
            if (ReferenceBody == Sun.Instance.sun)
            {
                MaxDisplayValue = (float)ReferenceBody.atmosphereDepth * 100000f;
            }
        }

        public void InformationTab()
        {
            Log.Info("InformationTab 1");
            double VesselMass = CalculateMass();
            double VesselArea = CalculateArea();

            GUILayout.BeginHorizontal(GUILayout.Width(WINWIDTH));
            GUI.skin.label.fontSize = 16;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUILayout.Label("Vessel Information", GUILayout.Width(290));
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            GUI.skin.label.fontSize = 12;
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical();

            Log.Info("InformationTab 2");

            GUILayout.Space(3);
            GUILayout.Label("Vessel Information:");
            //GUILayout.Space(2);
            GUILayout.Label("_________________________________________");
            //GUILayout.Space(2);
            GUILayout.Label("Mass: " + (CalculateMass() * 1000).ToString("F2") + " Kg");
            //GUILayout.Space(2);
            GUILayout.Label("Prograde Area: " + CalculateArea().ToString("F2") + " Square Meters");
            //GUILayout.Space(2);
            GUILayout.Label("Total Area: " + (CalculateArea() * 4).ToString("F2") + " Square Meters");
            //GUILayout.Space(2);
            GUILayout.Label("_________________________________________");
            //GUILayout.Space(3);

            GUILayout.Label("Decay Information:");
            //GUILayout.Space(2);
            //GUILayout.Label("_________________________________________");
            //GUILayout.Space(2);
            GUILayout.Label("Reference Body: " + ReferenceBody.name);
            GUILayout.Space(2);

            for (int b = 0; b < FlightGlobals.Bodies.Count; b++)
            {
                CelestialBody body = FlightGlobals.Bodies[b];
                if (body.atmosphere)
                {
                    if (GUILayout.Button(body.name, GUILayout.Width(WINWIDTH)))
                    {
                        ReferenceBody = body;
                        SetMaxDisplayValue(ReferenceBody);
                        AltitudeValue = Math.Max((float)ReferenceBody.atmosphereDepth, AltitudeValue);
                        AltitudeValue = Math.Min(AltitudeValue, MaxDisplayValue);
                    }
                    GUILayout.Space(2);
                }
            }
            Log.Info("InformationTab 4");
            GUILayout.EndHorizontal();
            GUILayout.Space(2);
            GUILayout.Label("Reference Altitude:");
            GUILayout.Space(2);
            AltitudeValue = GUILayout.HorizontalSlider(AltitudeValue, (float)ReferenceBody.atmosphereDepth, MaxDisplayValue);
            GUILayout.Space(2);
            GUILayout.Label("Altitude set: " + (AltitudeValue / 1000).ToString("F1") + "Km.");
            //GUILayout.Space(2);
            GUILayout.Label("Decay Rate (Atmospheric Drag): " + UserInterface.FormatDecayRateToString(
                DecayManager.EditorDecayRateAtmosphericDrag(CalculateMass() * 1000, CalculateArea(), ReferenceBody.Radius + AltitudeValue, 0, ReferenceBody)
                ));
            //GUILayout.Space(2);
            GUILayout.Label("Decay Rate (Radiation Pressure): " + UserInterface.FormatDecayRateSmallToString(DecayManager.EditorDecayRateRadiationPressure(CalculateMass() * 1000, CalculateArea(), ReferenceBody.Radius + AltitudeValue, 0, ReferenceBody)));
            //GUILayout.Space(2);
            GUILayout.Label("Estimated Orbital Lifetime: " + UserInterface.FormatTimeUntilDecayInDaysToString(DecayManager.DecayTimePredictionEditor(CalculateArea(), CalculateMass() * 1000, ReferenceBody.Radius + AltitudeValue, 0, ReferenceBody)));
            GUILayout.Space(2);
            GUILayout.Label("_________________________________________");
            GUILayout.Space(3);
            Log.Info("InformationTab 5");

            GUILayout.Label("Station Keeping Information:");
            //GUILayout.Space(2);
            GUILayout.Label("_________________________________________");
            GUILayout.Space(2);
            GUILayout.Label("Total Fuel: " + (GetFuel() * 1000).ToString("F1") + " Kg.");
            //GUILayout.Space(2);
            GUILayout.Label("Useable Resources: ");

            Dictionary<string, double> ResourceQuantites = new Dictionary<string, double>();
            //   double tempHold = 0;
            for (int i = 0; i < EditorLogic.SortedShipList.Count; i++)
            {
                Part part = EditorLogic.SortedShipList[i];
                for (int r = 0; r < part.Resources.Count; r++)
                {
                    PartResource res = part.Resources[r];
                    if (ResourceQuantites.ContainsKey(res.resourceName))
                        ResourceQuantites[res.resourceName] += res.maxAmount;
                    else
                        ResourceQuantites[res.resourceName] = res.maxAmount;
                    //GUILayout.Label(res.resourceName + ": " + res.maxAmount);
                }
            }

            GetMaximumPossibleLifetime();
            foreach (var resource in AllUsableFuels)
            {
                GUILayout.Label(resource.Key + " : " + ResourceQuantites[resource.Key].ToString("F0"));
            }




            GUILayout.Space(2);
            GUILayout.Label("Maximum possible Station Keeping fuel lifetime: " + (UserInterface.FormatTimeUntilDecayInDaysToString(GetMaximumPossibleLifetime())));
            GUILayout.Space(2);
#if true
            GUILayout.Label("Maximum possible lifetime: " + (UserInterface.FormatTimeUntilDecayInDaysToString(DecayManager.DecayTimePredictionEditor(CalculateArea(), CalculateMass() * 1000, ReferenceBody.Radius + AltitudeValue, 0, ReferenceBody) +
              +DecayManager.EditorDecayRateAtmosphericDrag(CalculateMass() * 1000, CalculateArea(), ReferenceBody.Radius + AltitudeValue, 0, ReferenceBody)
             + GetMaximumPossibleLifetime())));
#endif
            GUILayout.Space(2);
            GUILayout.Label("_________________________________________");
            GUILayout.Space(3);

            //GUILayout.EndVertical();
        }

        public double CalculateMass()
        {
            return EditorLogic.fetch.ship.GetTotalMass();
        }

        public double CalculateArea()
        {
            double Area = 0;
            Area = EditorLogic.fetch.ship.shipSize.y * EditorLogic.fetch.ship.shipSize.z / 4;

            return Area;
        }

        public double GetFuel()
        {
            double Total = 0;
            float EmptyMass = 0;
            float FuelMass = 0;

            EditorLogic.fetch.ship.GetShipMass(out EmptyMass, out FuelMass);

            Total = FuelMass;

            return Total;
        }

        Dictionary<string, double> AllUsableFuels = new Dictionary<string, double>();

        public double GetMaximumPossibleLifetime()
        {
            double Lifetime = 0;

            //List<Part> constructPartsList = EditorLogic.RootPart.FindChildParts<Part>().ToList();
            //constructPartsList.AddUnique(EditorLogic.RootPart);
            var constructParts = EditorLogic.SortedShipList.ToArray();
            //Part[] constructParts = EditorLogic.RootPart.FindChildParts<Part>();
            //constructParts.AddUnique(EditorLogic.RootPart);

            bool ClockType = Settings.Read24Hr();
            double HoursInDay = 6;

            if (ClockType)
            {
                HoursInDay = 24.0;
            }
            else
            {
                HoursInDay = 6.0;
            }
            AllUsableFuels.Clear();

            var allResources = GetResources(constructParts);
            for (int pi = 0; pi < constructParts.Length; pi++)
            {
                Part p = constructParts[pi];

                Dictionary<string, double> UsableFuels = new Dictionary<string, double>();
                Dictionary<string, double> FuelRatios = new Dictionary<string, double>();

                var moduleEngines = p.FindModuleImplementing<ModuleEngines>();
                if (moduleEngines != null)
                {
                    double engEfficiency = moduleEngines.atmosphereCurve.Evaluate(0);

                    if (moduleEngines != null)
                    {
                        for (int mep = 0; mep < moduleEngines.propellants.Count; mep++)
                        {
                            Propellant pro = moduleEngines.propellants[mep];

                            //if (pro.ToString() == GetResources(constructParts, pro.name)[i])
                            if (allResources.ContainsKey(pro.name))
                            {
                                // Blah blah blah 
                                if (!UsableFuels.ContainsKey(pro.name))
                                {
                                    UsableFuels.Add(pro.name, pro.totalResourceCapacity);
                                    FuelRatios.Add(pro.name, pro.ratio);
                                }
                                if (!AllUsableFuels.ContainsKey(pro.name))
                                    AllUsableFuels.Add(pro.name, pro.totalResourceCapacity);

                                double ResEff = 1.0 / engEfficiency;

                                Lifetime = Lifetime + allResources[pro.name] /
                                    (DecayManager.EditorDecayRateRadiationPressure(CalculateMass() * 1000, CalculateArea(), ReferenceBody.Radius + AltitudeValue, 0, ReferenceBody) * ResEff * Settings.ReadResourceRateDifficulty() +
                                        DecayManager.EditorDecayRateAtmosphericDrag(CalculateMass() * 1000, CalculateArea(), ReferenceBody.Radius + AltitudeValue, 0, ReferenceBody)
                                    ) / (60 * 60 * HoursInDay);
                            }
                        }
                    }
                }
                var moduleRCS = p.FindModuleImplementing<ModuleRCS>();
                if (moduleRCS != null)
                {
                    var rcsEfficiency = moduleRCS.atmosphereCurve.Evaluate(0);
                    if (moduleRCS != null)
                    {
                        for (int mep = 0; mep < moduleRCS.propellants.Count; mep++)
                        {
                            Propellant pro = moduleRCS.propellants[mep];
                            if (allResources.ContainsKey(pro.name))
                            {
                                // Blah blah blah 
                                if (!UsableFuels.ContainsKey(pro.name))
                                {
                                    UsableFuels.Add(pro.name, pro.totalResourceCapacity);
                                    FuelRatios.Add(pro.name, pro.ratio);
                                }
                                if (!AllUsableFuels.ContainsKey(pro.name))
                                    AllUsableFuels.Add(pro.name, pro.totalResourceCapacity);

                                double ResEff = 1.0 / rcsEfficiency;

                                Lifetime = Lifetime + allResources[pro.name] /
                                    (DecayManager.EditorDecayRateRadiationPressure(CalculateMass() * 1000, CalculateArea(), ReferenceBody.Radius + AltitudeValue, 0, ReferenceBody) * ResEff * Settings.ReadResourceRateDifficulty() +
                                    DecayManager.EditorDecayRateAtmosphericDrag(CalculateMass() * 1000, CalculateArea(), ReferenceBody.Radius + AltitudeValue, 0, ReferenceBody)
                                    ) / (60 * 60 * HoursInDay);
                            }
                        }
                    }
                }
            }

            return Lifetime;
        }

        public double GetPropellants(Propellant prop)
        {
            return 0;
        }

        public string GetEngines()
        {
            string Engines = "";

            return Engines;
        }

        public Dictionary<string, double> GetResources(Part[] constructParts)
        {
            Dictionary<string, double> reslist = new Dictionary<string, double>();
            //string res = "";
            for (int pi = 0; pi < constructParts.Length; pi++)
            {
                Part p = constructParts[pi];

                if (p.Resources.Count != 0)
                {
                    for (int pr = 0; pr < p.Resources.Count; pr++)
                    {
                        PartResource pRes = p.Resources[pr];

                        if (pRes.resourceName != "IntakeAir" && pRes.resourceName != "ElectricCharge")
                        {
                            if (reslist.ContainsKey(pRes.resourceName))
                            {
                                reslist[pRes.resourceName] += pRes.maxAmount;
                            }
                            else
                            {
                                reslist[pRes.resourceName] = pRes.maxAmount;
                                //res += pRes.resourceName + ", ";
                            }
                        }
                    }
                }

                /*
                ProtoPartSnapshot protopart = p.protoPartSnapshot;
             
                {
                    foreach (ProtoPartModuleSnapshot protopartmodulesnapshot in protopart.modules)
                    {
                        if (protopartmodulesnapshot.moduleName == "ModuleOrbitalDecay")
                        {
                            ConfigNode node = protopartmodulesnapshot.moduleValues.GetNode("stationKeepData");
                            reslist.Add(node.GetValue("resources"));
                            break;
                        }
                    }
                }
                 */
            }

            return reslist;
        }
    }
}

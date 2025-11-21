using ClickThroughFix;
using System;
using System.Collections.Generic;
using UnityEngine;
using static OrbitalDecay.RegisterToolbar;

namespace OrbitalDecay
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    internal class EditorUserInterface : MonoBehaviour
    {
        private int id;

        private float AltitudeValue = 70000f;
        float MaxDisplayValue = 2100000;
        private static CelestialBody ReferenceBody;

        private void Awake()
        {
            id = Guid.NewGuid().GetHashCode();
            GameEvents.onGUIApplicationLauncherDestroyed.Add(DestroyEvent);

            foreach (CelestialBody body in FlightGlobals.Bodies)
            {
                if (body.isHomeWorld)
                {
                    ReferenceBody = body;
                    break;
                }
            }
        }

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

        public void OnGUI()
        {
            if (ToolbarInterface.Visible) //&& !Hidden)
            {
                if (!HighLogic.CurrentGame.Parameters.CustomParams<OD3>().useAltSkin)
                    GUI.skin = HighLogic.Skin;

                MainwindowPosition = ClickThruBlocker.GUILayoutWindow(id, MainwindowPosition, MainWindow, "Orbital Decay Utilities", windowStyle);
            }
        }

        Vector2 scrollPos;

        public void MainWindow(int windowID)
        {

            if (GUI.Button(new Rect(MainwindowPosition.width - 22, 3, 19, 19), "x"))
            {
                ToolbarInterface.GuiOff();
            }
            using (new GUILayout.VerticalScope())
            {
                GUILayout.Space(20);

                scrollPos = GUILayout.BeginScrollView(scrollPos, false, false, hSmallScrollBar, GUI.skin.verticalScrollbar);
                InformationTab();
                GUILayout.EndScrollView();
            }
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
            double VesselMass = CalculateMass();
            double VesselArea = CalculateArea();

            using (new GUILayout.HorizontalScope(GUILayout.Width(WINWIDTH)))
            {
                GUI.skin.label.fontSize = (int)Math.Round(LARGEFONTSIZE);
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                GUILayout.Label("Vessel Information", GUILayout.Width(VI_BUTTON));
                GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                GUI.skin.label.fontSize = (int)Math.Round(FONTSIZE);
            }

            using (new GUILayout.VerticalScope())
            {
                GUILayout.Space(3);
                GUILayout.Label("Vessel Information:");
                GUILayout.Label("_________________________________________");
                GUILayout.Label("Mass: " + (CalculateMass() * 1000).ToString("F2") + " Kg");
                GUILayout.Label("Prograde Area: " + CalculateArea().ToString("F2") + " Square Meters");
                GUILayout.Label("Total Area: " + (CalculateArea() * 4).ToString("F2") + " Square Meters");
                GUILayout.Label("_________________________________________");

                GUILayout.Label("Decay Information:");
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

                GUILayout.Label("Maximum possible lifetime: " + (UserInterface.FormatTimeUntilDecayInDaysToString(DecayManager.DecayTimePredictionEditor(CalculateArea(), CalculateMass() * 1000, ReferenceBody.Radius + AltitudeValue, 0, ReferenceBody) +
                  +DecayManager.EditorDecayRateAtmosphericDrag(CalculateMass() * 1000, CalculateArea(), ReferenceBody.Radius + AltitudeValue, 0, ReferenceBody)
                 + GetMaximumPossibleLifetime())));

                GUILayout.Space(2);
                GUILayout.Label("_________________________________________");
                GUILayout.Space(3);

            }
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

            var constructParts = EditorLogic.SortedShipList.ToArray();

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

                ModuleEngines moduleEngines = null;
                ModuleEnginesFX m = p.FindModuleImplementing<ModuleEnginesFX>();
                if (m != null)
                {
                    moduleEngines = m as ModuleEngines;
                }
                else
                    moduleEngines = p.FindModuleImplementing<ModuleEngines>();
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
                ModuleRCS moduleRCS = null;
                var moduleRCSFX = p.FindModuleImplementing<ModuleRCSFX>();
                if (moduleRCSFX != null)
                {
                    moduleRCS = moduleRCSFX as ModuleRCS;
                }
                else
                    moduleRCS = p.FindModuleImplementing<ModuleRCS>();

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
            }

            return reslist;
        }
    }
}

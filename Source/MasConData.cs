/*
 * Whitecat Industries Orbital Decay for Kerbal Space Program. for Kerbal Space Program. 
 * 
 * Written by Whitecat106 (Marcus Hehir).
 * 
 * Kerbal Space Program is Copyright (C) 2016 Squad. See http://kerbalspaceprogram.com/. This
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

namespace OrbitalDecay
{
    internal class Mascon
    {
        internal int id;
        internal double centreGal;
        internal double centreLat;
        internal double centreLong;
        internal double radiusm;
        internal double diamdeg;

        internal Mascon()
        {
            id = 99;
            centreGal = 0;
            centreLat = 0;
            centreLong = 0;
            radiusm = 0;
            diamdeg = 360;
        }
        internal Mascon(ConfigNode cn)
        {
            cn.TryGetValue("id", ref id);
            cn.TryGetValue("centreGal", ref centreGal);
            cn.TryGetValue("centreLat", ref centreLat);
            cn.TryGetValue("centreLong", ref centreLong);
            cn.TryGetValue("radiusm", ref radiusm);
            cn.TryGetValue("diamdeg", ref diamdeg);
        }
    }
    internal class GravityMap
    {
        internal string body;
        internal bool alternate;
        internal double meanG;
        internal double meanGal;
        internal double minGal;
        internal double maxGal;
        internal double minASL;
        internal double maxASL;

        internal Mascon[] masconAr;

        internal GravityMap(ConfigNode GMSData)
        {
            body = "";
            Load(GMSData);
        }
        internal void Load(ConfigNode GMSData)
        {
            GMSData.TryGetValue("body", ref body);
            GMSData.TryGetValue("alternate", ref alternate);
            GMSData.TryGetValue("meanG", ref meanG);
            GMSData.TryGetValue("meanGal", ref meanGal);
            GMSData.TryGetValue("minGal", ref minGal);
            GMSData.TryGetValue("maxGal", ref maxGal);
            GMSData.TryGetValue("minASL", ref minASL);
            GMSData.TryGetValue("maxASL", ref maxASL);

            ConfigNode[] mc = GMSData.GetNodes("MASCON");
            List<Mascon> mcList = new List<Mascon>();
            foreach (var node in mc)
            {
                mcList.Add(new Mascon(node));
            }
            masconAr = mcList.ToArray();
        }
    }

    [KSPAddon(KSPAddon.Startup.Instantly, false)]
    public class MasConDataInit : MonoBehaviour
    {

        public void Start()
        {
            MasConData.LoadData();
        }
    }

    public class MasConData : MonoBehaviour
    {
        static Dictionary<string, GravityMap> gravityMapDict = null;

        public static string MasConDataFilePath;
        //public static ConfigNode GMSData;

        public static void LoadData()
        {
            MasConDataFilePath = KSPUtil.ApplicationRootPath + "GameData/WhitecatIndustries/OrbitalDecay/PluginData/MasConData.cfg";

            if (gravityMapDict == null)
            {
                gravityMapDict = new Dictionary<string, GravityMap>();
                var node = ConfigNode.Load(MasConDataFilePath);
                foreach (var n in node.GetNodes("GRAVITYMAP"))
                {
                    GravityMap gm = new GravityMap(n);
                    gravityMapDict.Add(gm.body, gm);

                }
            }

        }

        private static GravityMap ThisGravityMap(string Body)
        {
            if (gravityMapDict.ContainsKey(Body))
                return gravityMapDict[Body];
            else return null;
        }


        public static bool IsBetween(double item, double min, double max)
        {
            return Enumerable.Range(Math.Abs((int)min), Math.Abs((int)max)).Contains(Math.Abs((int)item));
        }


        internal static Mascon LocalMasCon(Vessel vessel)
        {
            GravityMap LocalGravityMap = ThisGravityMap(vessel.orbitDriver.orbit.referenceBody.GetName());

            Mascon Local = new Mascon();

            if (LocalGravityMap.masconAr.Count() > 0)
            {
                double VesselLat = vessel.latitude;
                double VesselLong = vessel.longitude;

                bool LatitudeWithin = false;
                bool LongitudeWithin = false;

                foreach (Mascon MasCon in LocalGravityMap.masconAr)
                {
                    int idx = MasCon.id;
                    double CentreGal = MasCon.centreGal;
                    double CentreLat = MasCon.centreLat;
                    double CentreLong = MasCon.centreLong;
                    double DegDiam = MasCon.diamdeg;
                    double DegRad = DegDiam / 2.0;

                    double UpperBoundLat = CentreLat + DegRad;
                    double LowerBoundLat = CentreLat - DegRad;
                    double UpperBoundLong = CentreLong + DegRad;
                    double LowerBoundLong = CentreLong - DegRad;

                    if (UpperBoundLat > 90) { UpperBoundLat = Math.Abs(UpperBoundLat - 180); }

                    if (LowerBoundLat < 90) { LowerBoundLat = -1 * (UpperBoundLat + 180); }

                    if (UpperBoundLong > 180) { UpperBoundLong = UpperBoundLong - 360; }

                    if (LowerBoundLong < -180) { LowerBoundLong = LowerBoundLong + 360; }

                    if (IsBetween(VesselLat, UpperBoundLat, LowerBoundLat)) { LatitudeWithin = true; }

                    if (IsBetween(VesselLong, UpperBoundLong, LowerBoundLong)) { LongitudeWithin = true; }

                    if (LatitudeWithin && LongitudeWithin)
                    {
                        Local = MasCon;
                        break;
                    }
                }

            }

            return Local;
        }

        public static bool CheckMasConProximity(Vessel vessel)
        {
            bool WithinEffectRange = false;

            if (vessel.orbitDriver.orbit.referenceBody.GetName() == "Earth" || vessel.orbitDriver.orbit.referenceBody.GetName() == "Kerbin" || vessel.orbitDriver.orbit.referenceBody.GetName() == "Moon") // 
            {
                GravityMap LocalGravityMap = ThisGravityMap(vessel.orbitDriver.orbit.referenceBody.GetName());


                if (LocalGravityMap.masconAr.Count() > 0)
                {
                    double VesselLat = vessel.latitude;
                    double VesselLong = vessel.longitude;

                    bool LatitudeWithin = false;
                    bool LongitudeWithin = false;

                    foreach (var MasCon in LocalGravityMap.masconAr)
                    {
                        double CentreGal = MasCon.centreGal;
                        double CentreLat = MasCon.centreLat;
                        double CentreLong = MasCon.centreLong;
                        double DegDiam = MasCon.diamdeg;
                        double DegRad = DegDiam / 2.0;

                        double UpperBoundLat = CentreLat + DegRad;
                        double LowerBoundLat = CentreLat - DegRad;
                        double UpperBoundLong = CentreLong + DegRad;
                        double LowerBoundLong = CentreLong - DegRad;

                        if (UpperBoundLat > 90) { UpperBoundLat = Math.Abs(UpperBoundLat - 180); }

                        if (LowerBoundLat < 90) { LowerBoundLat = -1 * (UpperBoundLat + 180); }

                        if (UpperBoundLong > 180) { UpperBoundLong = UpperBoundLong - 360; }

                        if (UpperBoundLong < -180) { UpperBoundLong = UpperBoundLong + 360; }

                        if (IsBetween(VesselLat, UpperBoundLat, LowerBoundLat)) { LatitudeWithin = true; }

                        if (IsBetween(VesselLong, UpperBoundLong, LowerBoundLong)) { LongitudeWithin = true; }


                        if (LatitudeWithin && LongitudeWithin)
                        {
                            break;
                        }
                    }

                    if (LatitudeWithin && LongitudeWithin)
                    {
                        WithinEffectRange = true;
                    }
                }
            }

            return WithinEffectRange;
        }

        public static double GalAtPosition(Vessel vessel)
        {
            GravityMap LocalMap = ThisGravityMap(vessel.orbitDriver.orbit.referenceBody.GetName());
            double meanGal = LocalMap.meanGal;

            Mascon MasCon = LocalMasCon(vessel);
            if (MasCon != null)
            {
                double CentreGal = MasCon.centreGal;
                double CentreLat = MasCon.centreLat;
                double CentreLong = MasCon.centreLong;
                double radiusdeg = MasCon.diamdeg / 2;
                double EdgeLat = CentreLat + radiusdeg;
                double EdgeLong = CentreLong + radiusdeg;

                double GalAtDistance = 0.0;

                double R = vessel.orbitDriver.orbit.referenceBody.Radius;
                double A = ToRadians(CentreLat);
                double B = ToRadians(EdgeLat);
                double C = ToRadians(EdgeLat) - ToRadians(CentreLat);
                double D = ToRadians(EdgeLong) - ToRadians(CentreLong);
                double E = Math.Sin(C / 2) * Math.Sin(C / 2) +
                        Math.Cos(A) * Math.Cos(B) *
                        Math.Sin(D / 2) * Math.Sin(D / 2);
                double F = 2 * Math.Atan2(Math.Sqrt(E), Math.Sqrt(1 - E));
                double Edgedistance = R * F;

                double φ1 = ToRadians(CentreLat);
                double φ2 = ToRadians(vessel.latitude);
                double Δφ = ToRadians(vessel.latitude) - ToRadians(CentreLat);
                double Δλ = ToRadians(vessel.longitude) - ToRadians(CentreLong);
                double a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) +
                        Math.Cos(φ1) * Math.Cos(φ2) *
                        Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);
                double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                double Vesseldistance = R * c;

                GalAtDistance = Math.Abs(CentreGal) / Edgedistance * Vesseldistance; // Work out negative push mascons for 1.6.0 removed absolute CentreGal

                /*
                double GravitationalConstant = 6.67408 * Math.Pow(10.0, -11.0); // G [Newton Meter Squared per Square Kilograms] 
                double AccelerationToGal = 100.0;
                double GalAtSurface = ((GravitationalConstant * vessel.orbitDriver.orbit.referenceBody.Mass * vessel.GetTotalMass()));
                double GalAtVerticalDistance = ((GravitationalConstant * vessel.orbitDriver.orbit.referenceBody.Mass * vessel.GetTotalMass()) / (Math.Pow(vessel.orbitDriver.orbit.altitude,2.0))) * AccelerationToGal;
                GalAtDistance = GalAtDistance + GalAtVerticalDistance;
                */

                return GalAtDistance;
            }
            return 0;
        }

        public static double ToRadians(double val)
        {
            return Math.PI / 180.0 * val;
        }

        public static double ToDegrees(double val)
        {
            return 57.3 * val;
        }

        #region Old Subs

        public static double CalculateRightAscension(Vector3d direction)
        {
            double RAAN = 0.0;
            Vector3d NormalisedVector = Vector3d.Normalize(direction);
            double l = direction.x / NormalisedVector.magnitude;
            double m = direction.y / NormalisedVector.magnitude;
            double n = direction.z / NormalisedVector.magnitude;
            double alpha = 0.0;
            double delta = 0.0;
            delta = Math.Asin(n) * 180.0 / Math.PI;

            if (m > 0)
            {
                alpha = Math.Acos(l / Math.Cos(delta)) * 180.0 / Math.PI;
            }
            else
            {
                alpha = 360 - Math.Acos(l / Math.Cos(delta)) * 180.0 / Math.PI;
            }

            RAAN = alpha;

            return RAAN;
        }

        public static double CalculateDeclination(Vector3d direction)
        {
            double DEC = 0.0;

            Vector3d NormalisedVector = Vector3d.Normalize(direction);
            double l = direction.x / NormalisedVector.magnitude;
            double m = direction.y / NormalisedVector.magnitude;
            double n = direction.z / NormalisedVector.magnitude;

            double delta = 0.0;
            delta = Math.Asin(n) * 180.0 / Math.PI;

            DEC = delta;

            return DEC;
        }

        #endregion
    }
}
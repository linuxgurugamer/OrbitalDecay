using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhitecatIndustries
{
    public static class VesselExtension
    {

        public static double GetCorrectVesselMass(this Vessel v)
        {

            if (v.loaded)
                return v.GetTotalMass();
            return GetUnloadedVesselMass(v.name, v.protoVessel);

        }
        private static  double GetUnloadedVesselMass(string name, ProtoVessel protoVessel)
        {
            double num = 0.0;
            for (int i = 0; i < protoVessel.protoPartSnapshots.Count; i++)
            {
                ProtoPartSnapshot protoPartSnapshot = protoVessel.protoPartSnapshots[i];
                num += (double)protoPartSnapshot.mass;
                for (int j = 0; j < protoPartSnapshot.resources.Count; j++)
                {
                    ProtoPartResourceSnapshot protoPartResourceSnapshot = protoPartSnapshot.resources[j];
                    if (protoPartResourceSnapshot != null)
                    {
                        if (protoPartResourceSnapshot.definition != null)
                        {
                            num += protoPartResourceSnapshot.amount * (double)protoPartResourceSnapshot.definition.density;
                        }
                        //else
                        //    Debug.Log("Vessel: " + name + ", resource: " + protoPartResourceSnapshot.resourceName + ", no definition");
                    }
                }
            }
            return num;
        }

        public static double CalculateVesselArea(this Vessel vessel)
        {
            double Area = 0;
            Area = FindVesselArea(vessel);
            return Area;
        }

        private static double FindVesselArea(Vessel vessel)
        {
            double Area = 0.0;
            ProtoVessel vesselImage = vessel.protoVessel;
            List<ProtoPartSnapshot> PartSnapshots = vesselImage.protoPartSnapshots;
            foreach (ProtoPartSnapshot part in PartSnapshots)
            {
                if (vessel == FlightGlobals.ActiveVessel)
                {
                    Area = Area + part.partRef.radiativeArea;
                }
                else
                {
                    Area = Area + part.partInfo.partSize * 2.0 * Math.PI;
                }
            }

            return Area / 4.0; // only one side facing prograde
        }


    }
}

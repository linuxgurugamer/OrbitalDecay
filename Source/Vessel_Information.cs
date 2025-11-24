using System;
using System.Collections.Generic;

namespace OrbitalDecay
{
    public class Vessel_Information
    {
        public string name;
        public Guid id;
        public string code;
        public float Mass;
        public double Area;
        public string ReferenceBody;

        public double SMA;
        public double SemiMinorAxis;
        public double ECC;
        public double INC;
        public double LPE;
        public double LAN;
        public double MNA;
        public double EPH;
        public double Fuel = 0;

        Vessel_Information() { }
        public Vessel_Information(string name, Guid id)
        {
            this.name = name;
            this.id = id;
        }

        public Vessel_Information(string name, Guid id, string code, float Mass,
            double Area, string ReferenceBody, double SMA,
            double ECC, double INC, double LPE, double LAN,
            double MNA, double EPH)
        {
            this.name = name;
            this.id = id;
            this.code = code;
            this.Mass = Mass;
            this.Area = Area;
            this.ReferenceBody = ReferenceBody;
            this.SMA = SMA;
            this.ECC = ECC;
            this.INC = INC;
            this.LPE = LPE;
            this.LAN = LAN;
            this.MNA = MNA;
            this.EPH = EPH;
        }

        static public ConfigNode Save(Dictionary<Guid, Vessel_Information> vi, string filename)
        {
            ConfigNode file = new ConfigNode();
            ConfigNode configNode = new ConfigNode("Vessels");
            file.AddNode(configNode);
            foreach (var v in vi.Values)
            {
                ConfigNode node = new ConfigNode("VESSEL");
                node.AddValue("name", v.name);
                node.AddValue("id", v.id);
                node.AddValue("code", v.code);
                node.AddValue("Mass", v.Mass);
                node.AddValue("Area", v.Area);
                node.AddValue("ReferenceBody", v.ReferenceBody);
                node.AddValue("SMA", v.SMA);
                node.AddValue("ECC", v.ECC);
                node.AddValue("INC", v.INC);
                node.AddValue("LPE", v.LPE);
                node.AddValue("LAN", v.LAN);
                node.AddValue("MNA", v.MNA);
                node.AddValue("EPH", v.EPH);
                node.AddValue("Fuel", v.Fuel);

                configNode.AddNode(node);
            }
            if (!string.IsNullOrEmpty(filename))
                file.Save(filename);
            return configNode;
        }

    }

}

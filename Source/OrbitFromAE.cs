public static class OrbitFromAE
{
    public struct ApPe
    {
        public double ApAltitude;   // Apoapsis altitude above sea level
        public double PeAltitude;   // Periapsis altitude above sea level
        public double Eccentricity;

        public ApPe(double apAlt, double peAlt, double e)
        {
            ApAltitude = apAlt;
            PeAltitude = peAlt;
            Eccentricity = e;
        }
    }

    /// <summary>
    /// Compute apoapsis and periapsis altitudes given semimajor axis (a)
    /// and eccentricity (e).
    /// </summary>
    public static ApPe ComputeApPe(double semimajorAxis, double eccentricity, CelestialBody body)
    {
        // Using:
        //   PeR = a (1 - e)
        //   ApR = a (1 + e)

        double peRadius = semimajorAxis * (1.0 - eccentricity);
        double apRadius = semimajorAxis * (1.0 + eccentricity);

        // Convert to altitudes above body radius
        double peAlt = peRadius - body.Radius;
        double apAlt = apRadius - body.Radius;

        return new ApPe(apAlt, peAlt, eccentricity);
    }
}

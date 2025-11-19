using System.Reflection;
using UnityEngine;

using static OrbitalDecay.RegisterToolbar;

public static class RCSUtils
{
    // Cache the FieldInfo for performance 
    private static readonly FieldInfo currentThrustField = typeof(ModuleRCS).GetField("currentThrustForce",
        BindingFlags.Instance | BindingFlags.NonPublic);

    // Also works on ModuleRCSFX since it inherits from ModuleRCS
    private static readonly FieldInfo currentThrustFieldFX = typeof(ModuleRCSFX).GetField("currentThrustForce",
        BindingFlags.Instance | BindingFlags.NonPublic) ?? currentThrustField;

    public static float GetCurrentThrust(this ModuleRCS rcs)
    {
        if (rcs == null) return 0f;

        var field = rcs is ModuleRCSFX ? currentThrustFieldFX : currentThrustField;
        if (field != null)
        {
            return (float)field.GetValue(rcs);
        }

        return 0f;
    }

    // Convenience extension for Part
    public static bool IsFiring(this Part part)
    {
        for (int i = 0; i < part.Modules.Count; i++)
        {
            var rcs = part.Modules[i] as ModuleRCS;
            if (rcs != null && rcs.GetCurrentThrust() > 0.001f)
                return true;

            var rcsFX = part.Modules[i] as ModuleRCSFX;
            if (rcsFX != null && rcsFX.GetCurrentThrust() > 0.001f)
                return true;
        }
        return false;
    }

    // Or per-thruster check
    public static bool IsRCSThrusterActuallyFiring(this PartModule pm)
    {
        if (pm is ModuleRCS rcs) return rcs.GetCurrentThrust() > 0.001f;
        if (pm is ModuleRCSFX rcsFX) return rcsFX.GetCurrentThrust() > 0.001f;
        return false;
    }
}
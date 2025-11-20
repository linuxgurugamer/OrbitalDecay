using System;
using System.Linq;
using UnityEngine;

public static class VesselSwitcher
{
    /// <summary>
    /// Switch to a vessel by its persistentId (uint).
    /// Works for loaded and unloaded vessels.
    /// </summary>
    public static bool SwitchToPersistentId(uint persistentId)
    {
        Vessel v = FlightGlobals.Vessels
            .FirstOrDefault(x => x != null && x.persistentId == persistentId);

        if (v == null)
        {
            Debug.Log("[VesselSwitcher] No vessel found with persistentId: " + persistentId);
            return false;
        }

        SafeSwitch(v);
        return true;
    }

    /// <summary>
    /// Switch to a vessel by its vesselID (Guid).
    /// Works for loaded and unloaded vessels.
    /// </summary>
    public static bool SwitchToGuid(Guid vesselGuid)
    {
        Vessel v = FlightGlobals.Vessels
            .FirstOrDefault(x => x != null && x.id == vesselGuid);

        if (v == null)
        {
            Debug.Log("[VesselSwitcher] No vessel found with Guid: " + vesselGuid);
            return false;
        }

        SafeSwitch(v);
        return true;
    }


    /// <summary>
    /// Ensures switching happens when physics and the scene are stable.
    /// Uses ForceSetActiveVessel so distance does not matter.
    /// </summary>
    private static void SafeSwitch(Vessel v)
    {
        // Switch after FixedUpdate to ensure stability
        v.StartCoroutine(SwitchCoroutine(v));
    }

    private static System.Collections.IEnumerator SwitchCoroutine(Vessel v)
    {
        // Wait a physics tick to ensure scene stability
        yield return new WaitForFixedUpdate();

        Debug.Log("[VesselSwitcher] Switching to vessel: " + v.GetName());
        FlightGlobals.ForceSetActiveVessel(v);
    }
}

using ToolbarControl_NS;
using UnityEngine;

namespace WhitecatIndustries
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RegisterToolbarOrbitalDecay : MonoBehaviour
    {
        public static KSP_Log.Log Log;


        void Start()
        {
            ToolbarControl.RegisterMod(UserInterface.MODID, UserInterface.MODNAME);
            ToolbarControl.RegisterMod(EditorUserInterface.MODID, UserInterface.MODNAME);

            ToolbarControl.RegisterMod(ToolbarInterface.MODID, ToolbarInterface.MODNAME);

            Log = new KSP_Log.Log("OrbitalDecay"
#if DEBUG
    , KSP_Log.Log.LEVEL.DETAIL
#endif
        );

        }
#if false
        bool initted = false;
        void OnGUI()
        {
            if (!initted)
            {
                InitStyle();
                initted = true;
            }
        }
        internal static void InitStyle()
        {
        }
#endif
    }
}

#if false
using UnityEngine;
using ToolbarControl_NS;

namespace WhitecatIndustries
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RegisterToolbar : MonoBehaviour
    {
        void Start()
        {
            ToolbarControl.RegisterMod(ToolbarInterface.MODID, ToolbarInterface.MODNAME);
        }
    }
}
#endif
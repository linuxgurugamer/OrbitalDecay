using ToolbarControl_NS;
using UnityEngine;

namespace OrbitalDecay
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RegisterToolbar : MonoBehaviour
    {
        const float WINWIDTH_CONST = 300f;
        const float VI_BUTTON_CONST = 290f;
        const float INFO_WIDTH_CONST = 350;
        const float FONTSIZE_CONST = 12f;
        const float LARGEFONTSIZE_CONST = 16f;

        const float SCROLLVIEW_HEIGHT_CONST = 350f;

        public static float WINWIDTH = WINWIDTH_CONST;
        public static float VI_BUTTON = VI_BUTTON_CONST;
        public static float INFO_WIDTH = INFO_WIDTH_CONST;

        public static float FONTSIZE = FONTSIZE_CONST;
        public static float LARGEFONTSIZE = LARGEFONTSIZE_CONST;

        public static float SCROLLVIEW_HEIGHT = SCROLLVIEW_HEIGHT_CONST;


        public static KSP_Log.Log Log;
        public static GUIStyle noPadStyle = null;

        public static GUIStyle hSmallScrollBar;
        public static GUIStyle windowStyle;
        public static GUIStyle comboBoxStyle;

        public static Rect MainwindowPosition = new Rect(150, 75, WINWIDTH + 40, 500);
        public static Rect DecayBreakdownwindowPosition = new Rect(0, 0, 450, 150);

        public static Texture2D lineTex;

        void Start()
        {
            ToolbarControl.RegisterMod(ToolbarInterface.MODID, ToolbarInterface.MODNAME);

            Log = new KSP_Log.Log("OrbitalDecay"
#if DEBUG
    , KSP_Log.Log.LEVEL.DETAIL
#endif
        );

        }

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
            noPadStyle = new GUIStyle(GUIStyle.none)
            {
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0),
                border = new RectOffset(0, 0, 0, 0),
                overflow = new RectOffset(0, 0, 0, 0),
                imagePosition = ImagePosition.ImageOnly,
            };
            // IMPORTANT: remove background textures so Unity can't add its own box
            noPadStyle.normal.background = null;
            noPadStyle.hover.background = null;
            noPadStyle.active.background = null;
            noPadStyle.focused.background = null;

            hSmallScrollBar = new GUIStyle(GUI.skin.horizontalScrollbar);
            hSmallScrollBar.fixedHeight = 0f;

            windowStyle = new GUIStyle(HighLogic.Skin.window);
            comboBoxStyle = GUI.skin.box;

            UpdateWindowSizes();
            InitLine();
        }

        static void InitLine()
        {
            if (lineTex == null)
            {
                lineTex = new Texture2D(1, 1);
                lineTex.SetPixel(0, 0, Color.white);
                lineTex.Apply();
            }
        }
        public static void UpdateWindowSizes()
        {
            if (HighLogic.CurrentGame != null)
            {
                WINWIDTH = (float)(WINWIDTH_CONST * HighLogic.CurrentGame.Parameters.CustomParams<OD3>().windowScaling);
                VI_BUTTON = (float)(VI_BUTTON_CONST * HighLogic.CurrentGame.Parameters.CustomParams<OD3>().windowScaling);
                INFO_WIDTH = (float)(INFO_WIDTH_CONST * HighLogic.CurrentGame.Parameters.CustomParams<OD3>().windowScaling);

                MainwindowPosition = new Rect(MainwindowPosition.x, MainwindowPosition.y,
                    (float)(WINWIDTH + 40f * HighLogic.CurrentGame.Parameters.CustomParams<OD3>().windowScaling),
                    (float)(500 * HighLogic.CurrentGame.Parameters.CustomParams<OD3>().windowScaling));

                DecayBreakdownwindowPosition = new Rect(DecayBreakdownwindowPosition.x, DecayBreakdownwindowPosition.y,
                    (float)(450 * HighLogic.CurrentGame.Parameters.CustomParams<OD3>().windowScaling),
                    (float)(150 * HighLogic.CurrentGame.Parameters.CustomParams<OD3>().windowScaling));

                FONTSIZE = (float)(FONTSIZE_CONST * HighLogic.CurrentGame.Parameters.CustomParams<OD3>().windowScaling);
                LARGEFONTSIZE = Mathf.Round((float)(LARGEFONTSIZE_CONST * HighLogic.CurrentGame.Parameters.CustomParams<OD3>().windowScaling));
                SCROLLVIEW_HEIGHT = Mathf.Round((float)(SCROLLVIEW_HEIGHT_CONST * HighLogic.CurrentGame.Parameters.CustomParams<OD3>().windowScaling));

                UserInterface.UpdateTextures();
            }
        }
    }
}

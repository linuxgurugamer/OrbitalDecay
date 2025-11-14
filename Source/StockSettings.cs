
using System.Collections;
using System.Reflection;

namespace OrbitalDecay
{
    // http://forum.kerbalspaceprogram.com/index.php?/topic/147576-modders-notes-for-ksp-12/#comment-2754813
    // search for "Mod integration into Stock Settings

    // HighLogic.CurrentGame.Parameters.CustomParams<OD>().useAltSkin
    public class OD : GameParameters.CustomParameterNode
    {
        public override string Title { get { return "Orbital Decay"; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "OrbitalDecay"; } }
        public override string DisplaySection { get { return "Orbital Decay"; } }
        public override int SectionOrder { get { return 1; } }
        public override bool HasPresets { get { return true; } }

        [GameParameters.CustomParameterUI("Realistic Decay ",
                  toolTip = "Use realistic values for decay")]
        public bool RealisticDecay = false;

        [GameParameters.CustomParameterUI("24HourClock ")]
        public bool _24HourClock = false;

        [GameParameters.CustomFloatParameterUI("DecayDifficulty ", minValue = 0.5f, maxValue = 50f, stepCount =101, toolTip = "Higher means harder")]
        public double DecayDifficulty = 1f;

        [GameParameters.CustomParameterUI("Planetarium Tracking ", toolTip = "")]
        public bool PlanetariumTracking = true;

        [GameParameters.CustomParameterUI("Planetarium Debris Tracking ",
                  toolTip = "")]
        public bool PlanetariumDebrisTracking = true;

        public override void SetDifficultyPreset(GameParameters.Preset preset) { }
        public override bool Enabled(MemberInfo member, GameParameters parameters) { return true; }
        public override bool Interactible(MemberInfo member, GameParameters parameters) { return true; }
        public override IList ValidValues(MemberInfo member) { return null; }
    }

    // HighLogic.CurrentGame.Parameters.CustomParams<OD2>().useAltSkin
    public class OD2 : GameParameters.CustomParameterNode
    {
        public override string Title { get { return "Resources"; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "OrbitalDecay"; } }
        public override string DisplaySection { get { return "Orbital Decay"; } }
        public override int SectionOrder { get { return 2; } }
        public override bool HasPresets { get { return true; } }


        [GameParameters.CustomFloatParameterUI("Resource Rate Difficulty ", minValue = 0.1f, maxValue = 10f,
                  toolTip = "")]
        public double ResourceRateDifficulty = 1f;



        public override void SetDifficultyPreset(GameParameters.Preset preset) { }
        public override bool Enabled(MemberInfo member, GameParameters parameters) { return true; }
        public override bool Interactible(MemberInfo member, GameParameters parameters) { return true; }
        public override IList ValidValues(MemberInfo member) { return null; }
    }


    // HighLogic.CurrentGame.Parameters.CustomParams<OD3>().useAltSkin
    public class OD3 : GameParameters.CustomParameterNode
    {
        public override string Title { get { return "Misc"; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "OrbitalDecay"; } }
        public override string DisplaySection { get { return "Orbital Decay"; } }
        public override int SectionOrder { get { return 3; } }
        public override bool HasPresets { get { return true; } }

        [GameParameters.CustomParameterUI("Snap Decay Rate Breakdown window to manager",
                  toolTip = "Make the Decay Rate Breakdown window snap to the upper-right of the main window")]
        public bool snapBreakdownWindow = true;

        [GameParameters.CustomParameterUI("Use alt skin ",
                  toolTip = "Use an alternate skin")]
        public bool useAltSkin = true;



        public override void SetDifficultyPreset(GameParameters.Preset preset) { }
        public override bool Enabled(MemberInfo member, GameParameters parameters) { return true; }
        public override bool Interactible(MemberInfo member, GameParameters parameters) { return true; }
        public override IList ValidValues(MemberInfo member) { return null; }
    }


}
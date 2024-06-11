using UnityEngine;

namespace BeautifyEffect {

    public partial class Beautify : MonoBehaviour {

        public static class ShaderParams {

            public static int BokehData = Shader.PropertyToID("_BokehData");
            public static int BokehData3 = Shader.PropertyToID("_BokehData3");
            public static int BokehData2 = Shader.PropertyToID("_BokehData2");
            public static int Sharpen = Shader.PropertyToID("_Sharpen");
            public static int Bloom = Shader.PropertyToID("_Bloom");
            public static int BloomTexture = Shader.PropertyToID("_BloomTex");
            public static int BloomTexture1 = Shader.PropertyToID("_BloomTex1");
            public static int BloomTexture2 = Shader.PropertyToID("_BloomTex2");
            public static int BloomTexture3 = Shader.PropertyToID("_BloomTex3");
            public static int BloomTexture4 = Shader.PropertyToID("_BloomTex4");
            public static int BloomWeights2 = Shader.PropertyToID("_BloomWeights2");
            public static int BloomWeights = Shader.PropertyToID("_BloomWeights");
            public static int BloomZDepthBias = Shader.PropertyToID("_BloomLayerZBias");
            public static int BloomTint = Shader.PropertyToID("_BloomTint");
            public static int BloomTint0 = Shader.PropertyToID("_BloomTint0");
            public static int BloomTint1 = Shader.PropertyToID("_BloomTint1");
            public static int BloomTint2 = Shader.PropertyToID("_BloomTint2");
            public static int BloomTint3 = Shader.PropertyToID("_BloomTint3");
            public static int BloomTint4 = Shader.PropertyToID("_BloomTint4");
            public static int BloomTint5 = Shader.PropertyToID("_BloomTint5");
            public static int BloomDepthNearThreshold = Shader.PropertyToID("_BloomNearThreshold");
            public static int BloomDepthThreshold = Shader.PropertyToID("_BloomDepthThreshold");
            public static int BloomSourceTexture = Shader.PropertyToID("_BloomSourceTex");
            public static int BloomSourceDepthTexture = Shader.PropertyToID("_BloomSourceDepth");
            public static int BloomSourceRightEyeDepthTexture = Shader.PropertyToID("_BloomSourceDepthRightEye");
            public static int BloomSourceRightEyeTexture = Shader.PropertyToID("_BloomSourceTexRightEye");
            public static int Purkinje = Shader.PropertyToID("_Purkinje");
            public static int EyeAdaptation = Shader.PropertyToID("_EyeAdaptation");
            public static int CompareData = Shader.PropertyToID("_CompareParams");
            public static int CompareTexture = Shader.PropertyToID("_CompareTex");
            public static int DoFDepthBias = Shader.PropertyToID("_BeautifyDepthBias");
            public static int DoFExclusionCullMode = Shader.PropertyToID("_BeautifyDoFExclusionCullMode");
            public static int DoFTransparencyCullMode = Shader.PropertyToID("_BeautifyDoFTransparencyCullMode");
            public static int DoFTexture = Shader.PropertyToID("_DoFTex");
            public static int DoFExclusionTexture = Shader.PropertyToID("_DofExclusionTexture");
            public static int DoFBokehRT = Shader.PropertyToID("_DoFBokeh");
            public static int DepthTexture = Shader.PropertyToID("_DepthTexture");
            public static int AFTint = Shader.PropertyToID("_AFTint");
            public static int OverlayTexture = Shader.PropertyToID("_OverlayTex");
            public static int SFMainTexture = Shader.PropertyToID("_SFMainTex");
            public static int SFHalo = Shader.PropertyToID("_SunHalo");
            public static int SFSunTint = Shader.PropertyToID("_SunTint");
            public static int SFGhosts4 = Shader.PropertyToID("_SunGhosts4");
            public static int SFGhosts3 = Shader.PropertyToID("_SunGhosts3");
            public static int SFGhosts2 = Shader.PropertyToID("_SunGhosts2");
            public static int SFGhosts1 = Shader.PropertyToID("_SunGhosts1");
            public static int SFCoronaRays1 = Shader.PropertyToID("_SunCoronaRays1");
            public static int SFCoronaRays2 = Shader.PropertyToID("_SunCoronaRays2");
            public static int SFSunData = Shader.PropertyToID("_SunData");
            public static int SFSunPos = Shader.PropertyToID("_SunPos");
            public static int SFSunPosRightEye = Shader.PropertyToID("_SunPosRightEye");
            public static int Frame = Shader.PropertyToID("_Frame");
            public static int FrameMaskTexture = Shader.PropertyToID("_FrameMask");
            public static int FrameData = Shader.PropertyToID("_FrameData");
            public static int OutlineColor = Shader.PropertyToID("_Outline");
            public static int OutlineIntensityMultiplier = Shader.PropertyToID("_OutlineIntensityMultiplier");
            public static int OutlineMinDepthThreshold = Shader.PropertyToID("_OutlineMinDepthThreshold");
            public static int VignetteAspectRatio = Shader.PropertyToID("_VignettingAspectRatio");
            public static int Vignette = Shader.PropertyToID("_Vignetting");
            public static int VignetteMaskTexture = Shader.PropertyToID("_VignettingMask");
            public static int FXData = Shader.PropertyToID("_FXData");
            public static int FXColor = Shader.PropertyToID("_FXColor");
            public static int HardLight = Shader.PropertyToID("_HardLight");
            public static int ColorBoost = Shader.PropertyToID("_ColorBoost");
            public static int ColorBoost2 = Shader.PropertyToID("_ColorBoost2");
            public static int AntialiasData = Shader.PropertyToID("_AntialiasData");
            public static int Dither = Shader.PropertyToID("_Dither");
            public static int Dirt = Shader.PropertyToID("_Dirt");
            public static int ScreenLum = Shader.PropertyToID("_ScreenLum");
            public static int TintColor = Shader.PropertyToID("_TintColor");
            public static int LUT = Shader.PropertyToID("_LUTTex");
            public static int LUT3D = Shader.PropertyToID("_LUT3DTex");
            public static int LUT3DParams = Shader.PropertyToID("_LUT3DParams");
            public static int BlurScale = Shader.PropertyToID("_BlurScale");
            public static int EAHist = Shader.PropertyToID("_EAHist");
            public static int EALumSrc = Shader.PropertyToID("_EALumSrc");
            public static int FlareTexture = Shader.PropertyToID("_FlareTex");
            public static int ChromaticAberration = Shader.PropertyToID("_ChromaticAberrationData");
            public static int LUTPreview = Shader.PropertyToID("_LUTPreview");
            public static int LUTTex = Shader.PropertyToID("_LUTTex");

            // Shader keywords
            public const string SKW_BLOOM = "BEAUTIFY_BLOOM";
            public const string SKW_BLOOM_CONSERVATIVE_THRESHOLD = "BEAUTIFY_BLOOM_PROP_THRESHOLDING";
            public const string SKW_LUT = "BEAUTIFY_LUT";
            public const string SKW_LUT3D = "BEAUTIFY_LUT3D";
            public const string SKW_NIGHT_VISION = "BEAUTIFY_NIGHT_VISION";
            public const string SKW_THERMAL_VISION = "BEAUTIFY_THERMAL_VISION";
            public const string SKW_OUTLINE = "BEAUTIFY_OUTLINE";
            public const string SKW_FRAME = "BEAUTIFY_FRAME";
            public const string SKW_FRAME_MASK = "BEAUTIFY_FRAME_MASK";
            public const string SKW_DALTONIZE = "BEAUTIFY_DALTONIZE";
            public const string SKW_DIRT = "BEAUTIFY_DIRT";
            public const string SKW_VIGNETTING = "BEAUTIFY_VIGNETTING";
            public const string SKW_VIGNETTING_MASK = "BEAUTIFY_VIGNETTING_MASK";
            public const string SKW_DEPTH_OF_FIELD = "BEAUTIFY_DEPTH_OF_FIELD";
            public const string SKW_DEPTH_OF_FIELD_TRANSPARENT = "BEAUTIFY_DEPTH_OF_FIELD_TRANSPARENT";
            public const string SKW_EYE_ADAPTATION = "BEAUTIFY_EYE_ADAPTATION";
            public const string SKW_TONEMAP_ACES = "BEAUTIFY_TONEMAP_ACES";
            public const string SKW_PURKINJE = "BEAUTIFY_PURKINJE";
            public const string SKW_BLOOM_USE_DEPTH = "BEAUTIFY_BLOOM_USE_DEPTH";
            public const string SKW_BLOOM_USE_LAYER = "BEAUTIFY_BLOOM_USE_LAYER";
            public const string SKW_CHROMATIC_ABERRATION = "BEAUTIFY_CHROMATIC_ABERRATION";
        }
    }



}
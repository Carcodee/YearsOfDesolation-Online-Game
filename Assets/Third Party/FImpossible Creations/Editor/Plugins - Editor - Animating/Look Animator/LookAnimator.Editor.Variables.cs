using FIMSpace.FEditor;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FLook
{
    public partial class FLookAnimator_Editor
    {
        #region Foldouts, helpers etc.



        static bool drawNewInspector = true;

        static bool drawMain = true;
        static bool drawOptional = false;


        Transform previousHead = null;

        int compensationBonesCount = 0;
        protected bool animatorDetected = false;
        protected bool animatorAnimPhys = false;

        private float bothX = 70f;
        private float lastBothX = 70f;
        private float bothY = 60f;
        private float lastBothY = 60f;

        // We checking difference in keys for curve because we can't detect change in curve by preCurve != falloffCurve
        Keyframe[] preKeyframes = new Keyframe[0];

        #endregion


        #region Serialized properties

        protected SerializedProperty sp_tofollow;
        protected SerializedProperty sp_lookBones;
        protected SerializedProperty sp_FollowOffset;
        protected SerializedProperty sp_FollowMode;
        protected SerializedProperty sp_leadbone;
        protected SerializedProperty sp_AutoBackbonesWeights;
        protected SerializedProperty sp_basetr;
        protected SerializedProperty sp_rotspd;
        protected SerializedProperty sp_usmooth;
        protected SerializedProperty sp_backbcount;
        protected SerializedProperty sp_falloff;
        //protected SerializedProperty sp_CustomWeights;
        protected SerializedProperty sp_clamph;
        protected SerializedProperty sp_clampv;
        protected SerializedProperty sp_maxdiff;
        protected SerializedProperty sp_chretspeed;
        //protected SerializedProperty sp_animwithsource;
        protected SerializedProperty sp_angoff;
        protected SerializedProperty sp_axesmul;
        protected SerializedProperty sp_AnimationStyle;

        protected SerializedProperty sp_blend;
        protected SerializedProperty sp_eyespos;
        protected SerializedProperty sp_ancheyes;
        protected SerializedProperty sp_anchrefr;
        protected SerializedProperty sp_leadblend;
        //protected SerializedProperty sp_rotaoff;
        protected SerializedProperty sp_monitor;

        protected SerializedProperty sp_compensblend;
        protected SerializedProperty sp_compensblendB;
        protected SerializedProperty sp_poscompens;
        protected SerializedProperty sp_poscompensB;
        protected SerializedProperty sp_weighmul;
        //protected SerializedProperty sp_DetectZeroKeyframes;

        protected SerializedProperty sp_auto;
        protected SerializedProperty sp_manfromax;
        protected SerializedProperty sp_mantoax;
        protected SerializedProperty sp_lookdirection;

        protected SerializedProperty sp_autooff;
        protected SerializedProperty sp_autofrom;
        protected SerializedProperty sp_fixpres;

        protected SerializedProperty sp_elasticX;
        protected SerializedProperty sp_elasticY;
        protected SerializedProperty sp_starttpose;
        protected SerializedProperty sp_dray;

        protected SerializedProperty sp_usecurve;
        protected SerializedProperty sp_fallvall;
        protected SerializedProperty sp_backoff;
        //protected SerializedProperty sp_180prev;
        protected SerializedProperty sp_maxdist;
        protected SerializedProperty sp_distMeasureOffset;
        protected SerializedProperty sp_dist2D;

        protected SerializedProperty sp_bird;
        protected SerializedProperty sp_birdlag;
        protected SerializedProperty sp_birddel;
        protected SerializedProperty sp_birmaxdist;
        protected SerializedProperty sp_birdfreq;
        //protected SerializedProperty sp_birdspd;
        //protected SerializedProperty sp_birdlagprog;
        protected SerializedProperty sp_birddelgospeed;
        protected SerializedProperty sp_animphys;
        protected SerializedProperty sp_LookWhenAbove;
        protected SerializedProperty sp_LookWhenAboveVertical;
        //protected SerializedProperty sp_LookAboveCurve;
        //protected SerializedProperty sp_LookAboveEnvelope;
        protected SerializedProperty sp_DeltaAcc;
        protected SerializedProperty sp_OptimizeWithMesh;
        protected SerializedProperty sp_NoddingTransitions;
        protected SerializedProperty sp_BackBonesNod;
        protected SerializedProperty sp_NodAxis;
        protected SerializedProperty sp_HoldRotateToOppositeUntil;
        //protected SerializedProperty sp_LookState;

        protected SerializedProperty sp_ModelForwardAxis;
        protected SerializedProperty sp_ModelUpAxis;
        protected SerializedProperty sp_DetectionFactor;
        protected SerializedProperty sp_LookAtPositionSmoother;
        protected SerializedProperty sp_DestroyMomentTransformOnMaxDistance;
        protected SerializedProperty sp_DeltaType;
        protected SerializedProperty sp_SimulationSpeed;
        protected SerializedProperty sp_StartLookElasticRangeX;
        protected SerializedProperty sp_StartLookElasticRangeY;
        protected SerializedProperty sp_MaxRotationSpeed;

        protected SerializedProperty sp_WhenAboveEraseAfter;
        protected SerializedProperty sp_WhenAboveEraseAfterVertical;
        protected SerializedProperty sp_WhenAboveGoBackDuration;
        protected SerializedProperty sp_BaseRotationCompensation;

        protected SerializedProperty sp_CompensationWeightB;
        protected SerializedProperty sp_CompensatePositionsB;
        protected SerializedProperty sp_BigAngleAutomation;
        protected SerializedProperty sp_BigAngleAutomationCompensation;
        protected SerializedProperty sp_ConstantParentalAxisUpdate;

        #endregion


        #region Eyes Animator


        protected SerializedProperty sp_eyeL;
        protected SerializedProperty sp_eyeLInv;
        protected SerializedProperty sp_eyeR;
        protected SerializedProperty sp_eyeRInv;
        protected SerializedProperty sp_head;
        protected SerializedProperty sp_eyesTarget;
        protected SerializedProperty sp_eyesSpeed;
        protected SerializedProperty sp_EyesOffsetRotation;
        protected SerializedProperty sp_eyesBlend;

        protected SerializedProperty sp_EyesXRange;
        protected SerializedProperty sp_EyesYRange;
        protected SerializedProperty sp_EyesNoKeyframes;


        private float eyesbothX = 70f;
        private float eyeslastBothX = 70f;
        private float eyesbothY = 60f;
        private float eyeslastBothY = 60f;


        #endregion



        //ModelForwardAxis


        protected virtual void OnEnable()
        {
            sp_tofollow = serializedObject.FindProperty("ObjectToFollow");
            sp_lookBones = serializedObject.FindProperty("LookBones");
            sp_FollowOffset = serializedObject.FindProperty("FollowOffset");
            sp_FollowMode = serializedObject.FindProperty("FollowMode");
            sp_leadbone = serializedObject.FindProperty("LeadBone");
            sp_AutoBackbonesWeights = serializedObject.FindProperty("AutoBackbonesWeights");
            sp_basetr = serializedObject.FindProperty("BaseTransform");
            sp_rotspd = serializedObject.FindProperty("RotationSpeed");
            sp_usmooth = serializedObject.FindProperty("UltraSmoother");
            sp_backbcount = serializedObject.FindProperty("BackBonesCount");
            sp_falloff = serializedObject.FindProperty("BackBonesFalloff");
            //sp_CustomWeights = serializedObject.FindProperty("CustomWeights");
            sp_clamph = serializedObject.FindProperty("XRotationLimits");
            sp_clampv = serializedObject.FindProperty("YRotationLimits");
            sp_maxdiff = serializedObject.FindProperty("StopLookingAbove");
            sp_chretspeed = serializedObject.FindProperty("ChangeTargetSmoothing");
            //sp_animwithsource = serializedObject.FindProperty("SyncWithAnimator");
            sp_angoff = serializedObject.FindProperty("RotationOffset");
            sp_axesmul = serializedObject.FindProperty("RotCorrectionMultiplier");
            sp_AnimationStyle = serializedObject.FindProperty("AnimationStyle");
            sp_DeltaAcc = serializedObject.FindProperty("QuickerRotateAbove");
            //sp_LookState = serializedObject.FindProperty("LookState");

            sp_blend = serializedObject.FindProperty("LookAnimatorAmount");
            sp_eyespos = serializedObject.FindProperty("StartLookPointOffset");
            sp_ancheyes = serializedObject.FindProperty("AnchorStartLookPoint");
            sp_anchrefr = serializedObject.FindProperty("RefreshStartLookPoint");
            sp_leadblend = serializedObject.FindProperty("LeadBoneWeight");
            //sp_rotaoff = serializedObject.FindProperty("SyncWithAnimator");
            sp_monitor = serializedObject.FindProperty("MonitorAnimator");

            sp_compensblend = serializedObject.FindProperty("CompensationWeight");
            sp_compensblendB = serializedObject.FindProperty("CompensationWeightB");
            sp_poscompens = serializedObject.FindProperty("CompensatePositions");
            sp_poscompensB = serializedObject.FindProperty("CompensatePositionsB");
            sp_weighmul = serializedObject.FindProperty("WeightsMultiplier");
            //sp_DetectZeroKeyframes = serializedObject.FindProperty("DetectZeroKeyframes");

            sp_auto = serializedObject.FindProperty("AutomaticFix");
            sp_manfromax = serializedObject.FindProperty("ManualFromAxis");
            sp_mantoax = serializedObject.FindProperty("ManualToAxis");
            sp_lookdirection = serializedObject.FindProperty("LookDirection");

            sp_autooff = serializedObject.FindProperty("OffsetAuto");
            sp_autofrom = serializedObject.FindProperty("FromAuto");
            sp_fixpres = serializedObject.FindProperty("FixingPreset");

            sp_elasticX = serializedObject.FindProperty("XElasticRange");
            sp_elasticY = serializedObject.FindProperty("YElasticRange");
            sp_starttpose = serializedObject.FindProperty("StartAfterTPose");
            sp_dray = serializedObject.FindProperty("DebugRays");

            sp_usecurve = serializedObject.FindProperty("CurveSpread");
            sp_fallvall = serializedObject.FindProperty("FaloffValue");
            sp_backoff = serializedObject.FindProperty("BackBonesAddOffset");
            //sp_180prev = serializedObject.FindProperty("Fix180");
            sp_maxdist = serializedObject.FindProperty("MaximumDistance");
            sp_distMeasureOffset = serializedObject.FindProperty("DistanceMeasurePoint");
            sp_dist2D = serializedObject.FindProperty("Distance2D");

            sp_bird = serializedObject.FindProperty("BirdMode");
            sp_birdlag = serializedObject.FindProperty("LagRotation");
            sp_birdfreq = serializedObject.FindProperty("LagEvery");
            //sp_birdspd = serializedObject.FindProperty("LaggySpeed");
            sp_birddel = serializedObject.FindProperty("DelayPosition");
            sp_birmaxdist = serializedObject.FindProperty("DelayMaxDistance");
            sp_birddelgospeed = serializedObject.FindProperty("DelayGoSpeed");
            sp_animphys = serializedObject.FindProperty("AnimatePhysics");

            sp_LookWhenAbove = serializedObject.FindProperty("LookWhenAbove");
            sp_LookWhenAboveVertical = serializedObject.FindProperty("LookWhenAboveVertical");
            //sp_LookAboveCurve = serializedObject.FindProperty("StartLookCurve");
            //sp_LookAboveEnvelope = serializedObject.FindProperty("StartLook");

            sp_OptimizeWithMesh = serializedObject.FindProperty("OptimizeWithMesh");
            sp_NoddingTransitions = serializedObject.FindProperty("NoddingTransitions");
            sp_BackBonesNod = serializedObject.FindProperty("BackBonesNod");
            sp_NodAxis = serializedObject.FindProperty("NodAxis");
            sp_HoldRotateToOppositeUntil = serializedObject.FindProperty("HoldRotateToOppositeUntil");

            sp_ModelForwardAxis = serializedObject.FindProperty("ModelForwardAxis");
            sp_ModelUpAxis = serializedObject.FindProperty("ModelUpAxis");
            sp_DetectionFactor = serializedObject.FindProperty("MaxOutDistanceFactor");
            sp_LookAtPositionSmoother = serializedObject.FindProperty("LookAtPositionSmoother");
            sp_DestroyMomentTransformOnMaxDistance = serializedObject.FindProperty("DestroyMomentTargetOnMaxDistance");
            sp_DeltaType = serializedObject.FindProperty("DeltaType");
            sp_SimulationSpeed = serializedObject.FindProperty("SimulationSpeed");

            sp_StartLookElasticRangeX = serializedObject.FindProperty("StartLookElasticRangeX");
            sp_StartLookElasticRangeY = serializedObject.FindProperty("StartLookElasticRangeY");
            sp_MaxRotationSpeed = serializedObject.FindProperty("MaxRotationSpeed");
            sp_WhenAboveEraseAfter = serializedObject.FindProperty("WhenAboveGoBackAfter");
            sp_WhenAboveEraseAfterVertical = serializedObject.FindProperty("WhenAboveGoBackAfterVertical");
            sp_WhenAboveGoBackDuration = serializedObject.FindProperty("WhenAboveGoBackDuration");
            sp_BaseRotationCompensation = serializedObject.FindProperty("BaseRotationCompensation");


            CheckForComponents();

            sp_CompensationWeightB = serializedObject.FindProperty("CompensationWeightB");
            sp_CompensatePositionsB = serializedObject.FindProperty("CompensatePositionsB");
            sp_BigAngleAutomation = serializedObject.FindProperty("BigAngleAutomation");
            sp_BigAngleAutomationCompensation = serializedObject.FindProperty("BigAngleAutomationCompensation");
            sp_ConstantParentalAxisUpdate = serializedObject.FindProperty("ConstantParentalAxisUpdate");


            // EYES

            sp_eyeL = serializedObject.FindProperty("LeftEye");
            sp_eyeLInv = serializedObject.FindProperty("InvertLeftEye");
            sp_eyeR = serializedObject.FindProperty("RightEye");
            sp_eyeRInv = serializedObject.FindProperty("InvertRightEye");
            sp_head = serializedObject.FindProperty("HeadReference");
            sp_eyesTarget = serializedObject.FindProperty("EyesTarget");
            sp_eyesSpeed = serializedObject.FindProperty("EyesSpeed");
            sp_eyesBlend = serializedObject.FindProperty("EyesBlend");
            sp_EyesOffsetRotation = serializedObject.FindProperty("EyesOffsetRotation");


            sp_EyesXRange = serializedObject.FindProperty("EyesXRange");
            sp_EyesYRange = serializedObject.FindProperty("EyesYRange");
            sp_EyesNoKeyframes = serializedObject.FindProperty("EyesNoKeyframes");

            if (Get.CompensationBones == null) Get.CompensationBones = new List<FLookAnimator.CompensationBone>();

            // Checking which tabs should be opened for faster workflow

            //if ( Get.BackBonesCount == 0 )
            //{
            //    drawSetup = true;
            //    drawBackBones = true;
            //}

            if (Get.LeadBone == null || Get.BaseTransform == null)
            { }
            else
            {
                if (Get.BackBonesCount == 0) {  drawBackBones = true; }
            }

            if (Get.UseEyes)
            {
                drawEyesSettings = true;

                if (Get.LeftEye != null && Get.HeadReference != null)
                {
                    drawEyesSetup = false;
                }
                else
                    drawEyesSetup = true;
            }


            FGUI_Finders.ResetFinders(false);

            SetupLangs();
        }




    }
}
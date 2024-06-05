using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FLook
{

    /// <summary>
    /// FC: Class which controlls behaviour of head bone and back bones
    /// In this file are defined all variables visible inside inspector window (serialized in editor class)
    /// In this class file all base methods are called.
    /// </summary>
    [AddComponentMenu("FImpossible Creations/Look Animator 2")]
    [DefaultExecutionOrder(-10)]
    public partial class FLookAnimator : MonoBehaviour, UnityEngine.EventSystems.IDropHandler, IFHierarchyIcon
    {
        // THIS IS PARTIAL CLASS: REST OF THE CODE INSIDE "Scripts" directory


        #region Inspector Variables ---------------------------


        [Tooltip("Lead / Head bone - head of look chain")]
        public Transform LeadBone;
        //private Quaternion initLeadBoneRotation;

        [Tooltip("Base root transform - object which moves / rotates - character transform / game object")]
        public Transform BaseTransform;


        [Tooltip("Faloff value of how weight of animation should be spread over bones")]
        //[Range(0f, 2.5f)]
        public float FaloffValue = 0.35f;
        public float FaloffValueB = 1.1f;
        //[Tooltip("When your character will look far behind you can dynamically adjust Weights Faloff to be bigger\n\n0: Not changing weights faloff\nSuggesting settings here large values larger than 1")]

        [Tooltip("When character is looking far back in big angle or far high, you can automate weights falloff value")]
        public bool BigAngleAutomation = false;

        [Tooltip("When character is looking far back in big angle or far high, you can automate compensation values")]
        public bool BigAngleAutomationCompensation = false;

        [Tooltip("If bone weights spread should be computed automatically or by hand")]
        public bool AutoBackbonesWeights = true;
        [Tooltip("When you want use curve for more custom falloff or define it by simple slider - 'FaloffValue'")]
        public bool CurveSpread = false;
        //[Tooltip("When you want use custom values for bone weights, or curve for more custom falloff or define it by simple slider - 'FaloffValue'")]
        //public bool CustomWeights = false;

        [Tooltip("Configurable rotation weight placed over back bones - when you will use for example spine bones, here you can define how much will they rotate towards target in reference to other animated bones")]
        public AnimationCurve BackBonesFalloff = AnimationCurve.Linear(0f, 1f, 1f, .1f);



        [Header("If you don't want arms to be rotated when spine", order = 1)]
        [Header("bone is rotated by script (drag & drop here)", order = 3)]
        public List<CompensationBone> CompensationBones = new List<CompensationBone>();

        [Range(0f, 1f)]
        public float CompensationWeight = 0.5f;
        [Range(0f, 1f)]
        public float CompensationWeightB = 0.5f;
        [Range(0f, 1f)]
        public float CompensatePositions = 0f;
        [Range(0f, 1f)]
        public float CompensatePositionsB = 0f;

        private float targetCompensationWeight = 0.5f;
        private float targetCompensationPosWeight = 0.0f;


        [Tooltip("Making script start after first frame so initialization will not catch TPose initial bones rotations, which can cause some wrong offsets for rotations")]
        public bool StartAfterTPose = true;

        [Tooltip("Update with waiting for fixed update clock")]
        public bool AnimatePhysics = false;
        [Tooltip("If you want look animator to stop computing when choosed mesh is not visible in any camera view (editor's scene camera is detecting it too)")]
        public Renderer OptimizeWithMesh = null;



        [Tooltip("Object which will be main target of look.\n\nYou can use feature called 'Moment Target' to look at other object for a moment then look back on ObjectToFollow - check LookAnimator.SetMomentLookTarget()")]
        public Transform ObjectToFollow;

        /// <summary> Offset for followed object, if selected follow mode "Just Position" this variable is treated as target position </summary>
        [Tooltip("Position offset on 'ObjectToFollow'")]
        public Vector3 FollowOffset;
        [Tooltip("If 'FollowOffset' should be world position translation\n\nor target object local space translation\n\nor we don't want to use ObjectToFollow and use just 'FollowOffset' position.")]
        public EFFollowMode FollowMode = EFFollowMode.FollowObject;



        [Range(0.0f, 2.5f)]
        [Tooltip("How fast character should rotate towards focus direction.\n\nRotationSpeed = 2.5 -> Instant rotation\n\nIt is speed of transition for look direction (no bones rotations smoothing)")]
        public float RotationSpeed = .65f;
        private bool instantRotation = false;

        [Range(0.0f, 1f)]
        //[Tooltip("This variable is making rotation animation become very smooth (but also slower).\nBeware because it can provide errors in some extreme values - Will be upgraded in future versions to prevent of happening this errors")]
        [Tooltip("This variable is making rotation animation become very smooth (but also slower).\nIt is enabling smooth rotation transition in bone rotations")]
        public float UltraSmoother = 0f;

        //[Tooltip("For now it's toggle, but later it might be default option - Preventing from head rotating around when using high value of ultra smoother and target object going crazy around character")]
        //public bool Fix180 = false;



        [Header("Look forward if this angle is exceeded", order = 1)]
        [Range(25f, 180f)]
        [Tooltip("If target is too much after transform's back we smooth rotating head back to default animation's rotation")]
        public float StopLookingAbove = 180f;

        [Tooltip("If object in rotation range should be detected only when is nearer than 'StopLookingAbove' to avoid stuttery target changes")]
        [Range(0.1f, 1f)]
        public float StopLookingAboveFactor = 1f;

        //[Range(25f, 90f)]
        //[Tooltip("If head have to rotate more than this value it's animation speed for rotating increases, slight touch on detailing animation (old name 'DeltaAccelerateRange')")]
        //public float QuickerRotateAbove = 50f;

        [Range(0.0f, 1f)]
        [Tooltip("If your character moves head too fast when loosing / changing target, here you can adjust it")]
        public float ChangeTargetSmoothing = 0f;

        [Tooltip("Switch to enable advanced settings for back bones falloff")]
        public bool AdvancedFalloff = false;

        [Tooltip("Max distance to target object to lost interest in it.\nValue = 0 -> Not using distance limits.\nWhen you have moment target - after exceeding distance moment target will be forgotten!")]
        public float MaximumDistance = 0f;

        [Tooltip("When Character is looking at something on his back but more on his right he look to right, when target suddenly goes more on his left and again to right very frequently you can set with this variable range from which rotating head to opposide shoulder side should be triggered to prevent strange looking behaviour when looking at dynamic objects")]
        [FPD_Suffix(0, 45, FPD_SuffixAttribute.SuffixMode.FromMinToMaxRounded, "°")]
        public float HoldRotateToOppositeUntil = 0f;

        [Tooltip("If object in range should be detected only when is nearer than 'MaxDistance' to avoid stuttery target changes")]
        [Range(0.0f, 1f)]
        public float MaxOutDistanceFactor = 0f;

        [Tooltip("If distance should be measured not using Up (y) axis")]
        public bool Distance2D = false;

        [Tooltip("Offsetting point from which we want to measure distance to target")]
        public Vector3 DistanceMeasurePoint;

        [Tooltip("Minimum angle needed to trigger head follow movement. Can be useful to make eyes move first and then head when angle is bigger")]
        [FPD_Suffix(0f, 45f, FPD_SuffixAttribute.SuffixMode.FromMinToMaxRounded, "°")]
        public float LookWhenAbove = 0;
        /// <summary> Supporting look when above go back animation </summary>
        private float animatedLookWhenAbove = 0f;
        [Tooltip("Separated start look angle for vertical look axis\n\nWhen Zero it will have same value as 'LookWhenAbove'")]
        [FPD_Suffix(0, 45, FPD_SuffixAttribute.SuffixMode.FromMinToMaxRounded, "°")]
        public float LookWhenAboveVertical = 0;
        /// <summary> Supporting look when above go back animation </summary>
        private float animatedLookWhenAboveVertical = 0f;

        [Tooltip("Head going back looking in front of target after this amount of seconds")]
        [FPD_Suffix(0f, 3f, FPD_SuffixAttribute.SuffixMode.FromMinToMax, "sec")]
        public float WhenAboveGoBackAfter = 0;
        [Tooltip("Head going back looking in front of target after this amount of seconds")]
        [FPD_Suffix(0f, 3f, FPD_SuffixAttribute.SuffixMode.FromMinToMax, "sec")]
        public float WhenAboveGoBackAfterVertical = 0;
        [Tooltip("Head going back looking in front of target after this amount of seconds")]
        [FPD_Suffix(0.05f, 1f, FPD_SuffixAttribute.SuffixMode.FromMinToMax, "sec")]
        public float WhenAboveGoBackDuration = .2f;

        [Tooltip("Rotating towards target slower when target don't need much angle to look at")]
        [FPD_Suffix(0f, 90f, FPD_SuffixAttribute.SuffixMode.FromMinToMaxRounded, "°")]
        public float StartLookElasticRangeX = 0;
        [Tooltip("Separated elastic start angle for vertical look axis\n\nIf zero then value will be same like 'StartLookElasticRange'")]
        [FPD_Suffix(0f, 90f, FPD_SuffixAttribute.SuffixMode.FromMinToMaxRounded, "°")]
        public float StartLookElasticRangeY = 0;

        [Header("Limits for rotation | Horizontal: X Vertical: Y")]
        public Vector2 XRotationLimits = new Vector2(-80f, 80f);

        [Tooltip("Making clamp ranges elastic, so when it starts to reach clamp value it slows like muscles needs more effort")]
        [FPD_Suffix(0f, 60f, FPD_SuffixAttribute.SuffixMode.FromMinToMaxRounded, "°")]
        public float XElasticRange = 20f;

        [Tooltip("When head want go back to default state of looking, it will blend with default animation instead of changing values of rotation variables to go back")]
        public bool LimitHolder = true;

        public Vector2 YRotationLimits = new Vector2(-50f, 50f);

        [Tooltip("Making clamp ranges elastic, so when it starts to reach clamp value it slows like muscles needs more effort")]
        [FPD_Suffix(0f, 45f, FPD_SuffixAttribute.SuffixMode.FromMinToMaxRounded, "°")]
        public float YElasticRange = 15f;



        [FPD_Percentage(0f, 1f)]
        [Tooltip("You can use this variable to blend intensity of look animator motion over skeleton animation\n\nValue = 1: Animation with Look Animator motion\nValue = 0: Only skeleton animation")]
        public float LookAnimatorAmount = 1f;

        //[FPD_Percentage(0f, 1f)]
        //[Tooltip("If you don't want first / head bone to be rotated toward target in 100%\n\nWhen you look at something you mostly rotate head just partially and then move your eyes to see full object")]
        //public float LeadBoneWeight = 1f;



        [Tooltip("If head look seems to be calculated like it is not looking from center of head but far from bottom or over it - you can adjust it - check scene view gizmos")]
        public Vector3 StartLookPointOffset;

        [Tooltip("Freezes reference start look position in x and z axes to avoid re-reaching max rotation limits when hips etc. are rotating in animation clip.\n\nIf your character is crouching or so, you would like to have this parameter disabled")]
        public bool AnchorStartLookPoint = true;
        [Tooltip("In some cases you'll want to refresh anchor position during gameplay to make it more fitting to character's animation poses")]
        public bool RefreshStartLookPoint = true;



        [Tooltip("[When some of your bones are rotating making circles]\n\nDon't set hard rotations for bones, use animation rotation and add rotation offset to bones so animation's rotations are animated correctly (useful when using attack animations for example)")]
        public bool SyncWithAnimator = true;

        /// <summary> If we're syncing with animator we use bone offsets instead of hard target rotations </summary>
        public bool UseBoneOffsetRotation { get { return SyncWithAnimator; } }

        [Tooltip("When using above action, we need to keep remembered rotations of animation clip from first frame, with monitoring we will remember root rotations from each new animation played")]
        public bool MonitorAnimator = false;
        private Quaternion rootStaticRotation;

        [FPD_Percentage(0f, 3f, true)]
        [Tooltip("When you want create strange effects - this variable will overrotate bones")]
        public float WeightsMultiplier = 1f;
        [Range(0.1f, 2.5f)]
        [Tooltip("If speed of looking toward target should be limited then lower this value")]
        public float MaxRotationSpeed = 2.5f;

        [Range(0.0f, 1f)]
        [Tooltip("When character is rotating and head is rotating with it instead of keep focusing on target, change this value higher")]
        public float BaseRotationCompensation = 0f;

        [Tooltip("If your skeleton have not animated keyframes in animation clip then bones would start doing circles with this option disabled\n\nIn most cases all keyframes are filled, if you're sure for baked keyframes you can disable this option to avoid some not needed calculations")]
        public bool DetectZeroKeyframes = true;

        [Range(0f, 1f)]
        [Tooltip("Target position to look can be smoothed out instead of immediate position changes")]
        public float LookAtPositionSmoother = 0f;

        [Tooltip("Delta Time for Look Animator calculations")]
        public EFDeltaType DeltaType = EFDeltaType.DeltaTime;
        [Tooltip("Multiplier for delta time resulting in changed speed of calculations for Look Animator")]
        public float SimulationSpeed = 1f;

        [Tooltip("It will make head animation stiff but perfectly looking at target")]
        [Range(0f, 1f)]
        public float OverrideHeadForPerfectLookDirection = 0f;

        [Tooltip("Resetting bones before animators update to avoid bones twisting if bones are not animated using unity animator")]
        public bool Calibration = true;

        [Tooltip("With crazy flipped axes from models done in different modelling softwares, sometimes you have to change axes order for Quaternion.LookRotation to work correctly")]
        public EFAxisFixOrder FixingPreset = EFAxisFixOrder.Parental;

        [Tooltip("If your model is not facing 'Z' axis (blue) you can adjust it with this value")]
        public Vector3 ModelForwardAxis = Vector3.forward;
        [Tooltip("If your model is not pointing up 'Y' axis (green) you can adjust it with this value")]
        public Vector3 ModelUpAxis = Vector3.up;

        [Tooltip("Defines model specific bones orientation in order to fix Quaternion.LookRotation axis usage")]
        public Vector3 ManualFromAxis = Vector3.forward;
        public Vector3 ManualToAxis = Vector3.forward;

        public Vector3 FromAuto;
        public Vector3 OffsetAuto;

        public Vector3 parentalReferenceLookForward;
        public Vector3 parentalReferenceUp;
        public Vector3 DynamicReferenceUp;



        [Tooltip("Additional degrees of rotations for head look - for simple correction, sometimes you have just to rotate head in y axis by 90 degrees")]
        public Vector3 RotationOffset = new Vector3(0f, 0f, 0f);
        [Tooltip("Additional degrees of rotations for backones - for example when you have wolf and his neck is going up in comparison to keyfarmed animation\nVariable name 'BackBonesAddOffset'")]
        public Vector3 BackBonesAddOffset = new Vector3(0f, 0f, 0f);

        [Tooltip("[ADVANCED] Axes multiplier for custom fixing flipped armature rotations")]
        public Vector3 RotCorrectionMultiplier = new Vector3(1f, 1f, 1f);

        [Tooltip("View debug rays in scene window")]
        public bool DebugRays = false;

        [Tooltip("Animation curve mode for rotating toward target")]
        public EFAnimationStyle AnimationStyle = EFAnimationStyle.SmoothDamp;


        // Deprecated support
        [System.Obsolete("Use LookAnimatorAmount instead, but remember that it works in reversed way -> LookAnimatorAmount 1 = BlendToOriginal 0  and  LookAnimatorAmount 0 = BlendToOriginal 1, simply you can replace it by using '1 - LookAnimatorAmount'")]
        public float BlendToOriginal { get { return 1f - LookAnimatorAmount; } set { LookAnimatorAmount = 1f - value; } }

        [System.Obsolete("Now using StartLookPointOffset as more responsive naming")]
        public Vector3 LookReferenceOffset { get { return StartLookPointOffset; } set { StartLookPointOffset = value; } }
        [System.Obsolete("Now using AnchorStartLookPoint as more responsive naming")]
        public bool AnchorReferencePoint { get { return AnchorStartLookPoint; } set { AnchorStartLookPoint = value; } }
        [System.Obsolete("Now using RefreshStartLookPoint as more responsive naming")]
        public bool RefreshAnchor { get { return RefreshStartLookPoint; } set { RefreshStartLookPoint = value; } }
        [System.Obsolete("Now using LookWhenAbove as more responsive naming")]
        public float MinHeadLookAngle { get { return LookWhenAbove; } set { LookWhenAbove = value; } }
        [System.Obsolete("Now using StopLookingAbove as more responsive naming")]
        public float MaxRotationDiffrence { get { return StopLookingAbove; } set { StopLookingAbove = value; } }

        [System.Obsolete("Now using SyncWithAnimator as more responsive naming")]
        public bool AnimateWithSource { get { return SyncWithAnimator; } set { SyncWithAnimator = value; } }

        [Tooltip("Updating reference axis for parental look rotation mode every frame")]
        public bool ConstantParentalAxisUpdate = true;

        #endregion


        // THIS IS PARTIAL CLASS: REST OF THE CODE INSIDE "Scripts" directory

        
        private void Reset() { FindBaseTransform(); }

        private void Awake() { _LOG_NoRefs(); }

        protected virtual void Start()
        {
            initialized = false;

            if (!StartAfterTPose) InitializeBaseVariables(); else startAfterTPoseCounter = 0;
        }

        private void OnDisable()
        {
            wasUpdating = false;
            animatePhysicsWorking = false;
        }

        public void ResetLook()
        {
            ResetBones();
            finalMotionWeight = 0f;
            _velo_animatedMotionWeight = 0f;
            animatedMotionWeight = 0f;
        }

        bool updateLookAnimator = true;
        bool wasUpdating = false;
        void Update()
        {
            #region Conditions to do any calculations for Look Animator

            if (!initialized)
            {
                if (StartAfterTPose)
                {
                    startAfterTPoseCounter++;
                    if (startAfterTPoseCounter > 6) InitializeBaseVariables();
                }

                updateLookAnimator = false;
                return;
            }

            if (OptimizeWithMesh != null) if (OptimizeWithMesh.isVisible == false) { updateLookAnimator = false; wasUpdating = false; return; }

            if (wasUpdating == false)
            {
                ResetLook();
                wasUpdating = true;
            }


            #region Triggering Animate Physics Support

            if (AnimatePhysics)
            {
                if (!animatePhysicsWorking) StartCoroutine(AnimatePhysicsClock());
                if (!triggerAnimatePhysics) { updateLookAnimator = false; return; } else triggerAnimatePhysics = false;
            }

            #endregion

            if (finalMotionWeight < 0.01f)
            {
                animatedLookAngles = Vector3.zero;
                //ResetBones();
                // If we blend animation to 0 we don't do anything
                if (LookAnimatorAmount <= 0f) { updateLookAnimator = false; return; }
            }

            UpdateCorrectionMatrix();

            updateLookAnimator = true;

            #endregion

            if (!AnimatePhysics) PreCalibrateBones();
        }

        void FixedUpdate()
        {
            if (!updateLookAnimator) return;
            if (AnimatePhysics) PreCalibrateBones();
        }


        public virtual void LateUpdate()
        {
            if (!updateLookAnimator) return;

            CalibrateBones();
            TargetingUpdate();
            BeginStateCheck();

            UpdateSmoothing();
            MaxDistanceCalculations();
            NoddingCalculations();

            CalculateLookAnimation();

            UpdateLookAnimatorAmountWeight();
            ChangeBonesRotations();

            _Debug_Rays();

            if (BirdMode) CalculateBirdMode();
            if (UseEyes) UpdateEyesLogics();

            EndUpdate();

            PostAnimatingTweaks();
        }

    }
}
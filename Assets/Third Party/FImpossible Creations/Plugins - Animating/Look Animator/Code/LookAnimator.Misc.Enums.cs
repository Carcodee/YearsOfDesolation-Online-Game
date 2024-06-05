namespace FIMSpace.FLook
{
    /// <summary>
    /// FC: In this partial class we storing enums
    /// </summary>
    public partial class FLookAnimator
    {
        public enum EFAxisFixOrder { Parental, FromBased, FullManual, ZYX }
        public enum EFHeadLookState { Null, Following, OutOfMaxRotation, ClampedAngle, OutOfMaxDistance }
        public enum EFFollowMode { FollowObject, LocalOffset, WorldOffset, ToFollowSpaceOffset, FollowJustPosition }
        public enum EFDeltaType { DeltaTime, SmoothDeltaTime, UnscaledDeltaTime, FixedDeltaTime }
        public enum EFAnimationStyle { SmoothDamp, FastLerp, Linear }//, Easing }

    }
}
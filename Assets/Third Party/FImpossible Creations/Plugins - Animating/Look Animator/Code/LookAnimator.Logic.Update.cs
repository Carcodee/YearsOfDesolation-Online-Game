using UnityEngine;

namespace FIMSpace.FLook
{
    /// <summary>
    /// FC: 
    /// </summary>
    public partial class FLookAnimator
    {
        private void BeginStateCheck()
        {
            // Checking if we lost follow target
            if (activeLookTarget == null) LookState = EFHeadLookState.Null;
            else
                if (LookState == EFHeadLookState.Null) LookState = EFHeadLookState.Following;

            previousState = LookState;

            isLooking = !(LookState == EFHeadLookState.OutOfMaxDistance || LookState == EFHeadLookState.OutOfMaxRotation);
        }


        /// <summary> Progressing look when above to look forward </summary>
        private float whenAboveGoBackDuration = 0f;
        private float whenAboveGoBackTimer = 0f;

        private float _whenAboveGoBackVelo = 0f;
        private float _whenAboveGoBackVerticalVelo = 0f;

        private Vector2 whenAboveGoBackAngles;

        private void LookWhenAboveGoBackCalculations()
        {
            if (whenAboveGoBackDuration > 0f)
            {
                if (WhenAboveGoBackAfter > 0f)
                {

                    //if (WhenAboveGoBackAfterVertical > 0f) //{//}//else//{
                    animatedLookWhenAbove = Mathf.SmoothDamp(animatedLookWhenAbove, 0f, ref _whenAboveGoBackVelo, whenAboveGoBackDuration, Mathf.Infinity, delta);
                    if (animatedLookWhenAbove <= 0.001f) whenAboveGoBackDuration = 0f;

                    if (LookWhenAboveVertical <= 0f) animatedLookWhenAboveVertical = animatedLookWhenAbove;
                    else
                    {
                        animatedLookWhenAboveVertical = Mathf.SmoothDamp(animatedLookWhenAboveVertical, 0f, ref _whenAboveGoBackVerticalVelo, whenAboveGoBackDuration, Mathf.Infinity, delta);
                    }
                }
            }
            else
            {

                if (animatedLookWhenAbove < LookWhenAbove) animatedLookWhenAbove = Mathf.SmoothDamp(animatedLookWhenAbove, LookWhenAbove, ref _whenAboveGoBackVelo, whenAboveGoBackDuration, Mathf.Infinity, delta);

                if (LookWhenAboveVertical <= 0f) animatedLookWhenAboveVertical = animatedLookWhenAbove;
                else
                if (animatedLookWhenAboveVertical < LookWhenAboveVertical) animatedLookWhenAboveVertical = Mathf.SmoothDamp(animatedLookWhenAboveVertical, LookWhenAboveVertical, ref _whenAboveGoBackVerticalVelo, whenAboveGoBackDuration, Mathf.Infinity, delta);


                if (WhenAboveGoBackAfter > 0f)
                {
                    //if (WhenAboveGoBackAfterVertical > 0f) //{ //}
                    //else //{
                    float diff = Mathf.Abs(_preLookAboveLookAngles.x - whenAboveGoBackAngles.x) + Mathf.Abs(_preLookAboveLookAngles.y - whenAboveGoBackAngles.y);

                    whenAboveGoBackTimer += delta * Mathf.Lerp(0.0f, 1f, Mathf.InverseLerp(LookWhenAbove / 5f, LookWhenAbove, diff));

                    if (whenAboveGoBackTimer > WhenAboveGoBackAfter)
                    {
                        whenAboveGoBackTimer = 0f;
                        whenAboveGoBackDuration = WhenAboveGoBackDuration;
                    }
                }
            }



        }
    }
}
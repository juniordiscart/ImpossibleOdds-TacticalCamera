using UnityEngine;

namespace ImpossibleOdds.TacticalCamera
{
    [CreateAssetMenu(fileName = "TacticalCameraSettings", menuName = "Impossible Odds/Tactical Camera/new Tactical Camera Settings")]
    public class TacticalCameraSettings : ScriptableObject, ITacticalCameraSettings
    {
        [SerializeField, Tooltip("Range of speed maximums depending on the camera's height.")]
        internal ValueRange movementSpeedRange = new ValueRange(5f, 20f);
        [SerializeField, Tooltip("The transition of the maximum speed values from low altitude to high altitude.")]
        internal AnimationCurve movementSpeedTransition = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField, Tooltip("Fade curve for movement.")]
        internal AnimationCurve movementFade = AnimationCurve.Linear(0f, 1f, 1f, 0f);
        [SerializeField, Min(0f), Tooltip("Time for the movement to linger and fade out. (In seconds)")]
        internal float movementFadeTime = 0.2f;
        [SerializeField, Min(0f), Range(0f, 1f), Tooltip("Smoothing time when the camera is moving to its focus point.")]
        internal float moveToTargetSmoothingTime = 0.2f;
        
        [SerializeField, Min(0f), Tooltip("Maximum speed at which the camera will rotate around its origin. (In degrees per second)")]
        internal float maxRotationSpeed = 180f;
        [SerializeField, Tooltip("Fade curve for rotation.")]
        internal AnimationCurve rotationalFade = AnimationCurve.Linear(0f, 1f, 1f, 0f);
        [SerializeField, Min(0f), Tooltip("Time for the rotational fade to linger and fade out. (In seconds)")]
        internal float rotationalFadeTime = 0.2f;
        [SerializeField, Tooltip("When attempting to orbit around a point, but no valid point could be found, is the camera allowed to pivot instead?")]
        internal bool allowPivotOnFailedOrbit = true;
        
        [SerializeField, Tooltip("The range of height values the camera can operate in.")]
        internal ValueRange absoluteHeightRange = new ValueRange(0f, 20f);
        
        [SerializeField, Tooltip("Range in which the camera can tilt when it is at its lowest operating position. (In degrees)")]
        internal ValueRange tiltRangeLow = new ValueRange(-10f, 30f);
        [SerializeField, Tooltip("Range in which the camera can tilt when it is at its highest operating position. (In degrees)")]
        internal ValueRange tiltRangeHigh = new ValueRange(10f, 75f);
        [SerializeField, Tooltip("Transition of tilt ranges from low to high.")]
        internal AnimationCurve tiltRangeTransition = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        
        [SerializeField, Tooltip("Should the tactical camera apply a dynamic field-of-view based on its height range?")]
        internal bool useDynamicFieldOfView = true;
        [SerializeField, Tooltip("The range of field-of-view values for the camera, depending on the height it's operating at.")]
        internal ValueRange dynamicFieldOfViewRange = new ValueRange(45f, 80f);
        [SerializeField, Tooltip("The transition of the field-of-view value from the lowest operating height to the highest operating height.")]
        internal AnimationCurve dynamicFieldOfViewTransition = AnimationCurve.Linear(0f, 0f, 1, 1f);
        
        [SerializeField, Tooltip("Layers that are to used to interact with the camera, e.g. when lowering it towards the ground.")]
        internal LayerMask interactionMask = 1;
        [SerializeField, Min(0f), Tooltip("Distance the camera can interact with the world, e.g. ray cast distance.")]
        internal float interactionDistance = 1000f;
        [SerializeField, Min(0f), Tooltip("The radius of the camera's collider, used to avoid collisions and clipping with the environment.")]
        internal float interactionBubbleRadius = 1f;

        /// <inheritdoc />
        public float Epsilon => 0.001f;

        /// <inheritdoc />
        public ValueRange MovementSpeedRange
        {
            get => movementSpeedRange;
            set => movementSpeedRange = value;
        }

        /// <inheritdoc />
        public float MovementFadeTime
        {
            get => movementFadeTime;
            set => movementFadeTime = value;
        }

        /// <inheritdoc />
        public float MoveToTargetSmoothingTime
        {
            get => moveToTargetSmoothingTime;
            set => moveToTargetSmoothingTime = value;
        }

        /// <inheritdoc />
        public ValueRange AbsoluteHeightRange
        {
            get => absoluteHeightRange;
            set => absoluteHeightRange = value;
        }

        /// <inheritdoc />
        public float MaxRotationalSpeed
        {
            get => maxRotationSpeed;
            set => maxRotationSpeed = value;
        }

        /// <inheritdoc />
        public float RotationalFadeTime
        {
            get => rotationalFadeTime;
            set => rotationalFadeTime = value;
        }

        /// <inheritdoc />
        public ValueRange TiltRangeHigh
        {
            get => tiltRangeHigh;
            set => tiltRangeHigh = value;
        }

        /// <inheritdoc />
        public ValueRange TiltRangeLow
        {
            get => tiltRangeLow;
            set => tiltRangeLow = value;
        }

        /// <inheritdoc />
        public bool UseDynamicFieldOfView
        {
            get => useDynamicFieldOfView;
            set => useDynamicFieldOfView = value;
        }

        /// <inheritdoc />
        public ValueRange DynamicFieldOfViewRange
        {
            get => dynamicFieldOfViewRange;
            set => dynamicFieldOfViewRange = value;
        }

        /// <inheritdoc />
        public LayerMask InteractionMask
        {
            get => interactionMask;
            set => interactionMask = value;
        }

        /// <inheritdoc />
        public float InteractionDistance
        {
            get => interactionDistance;
            set => interactionDistance = value;
        }

        /// <inheritdoc />
        public float InteractionBubbleRadius
        {
            get => interactionBubbleRadius;
            set => interactionBubbleRadius = value;
        }

        /// <inheritdoc />
        public bool AllowPivotOnFailedOrbit
        {
            get => allowPivotOnFailedOrbit;
            set => allowPivotOnFailedOrbit = value;
        }

        public AnimationCurve MovementSpeedTransition
        {
            get => movementSpeedTransition;
            set => movementSpeedTransition = value;
        }

        public AnimationCurve MovementFadeCurve
        {
            get => movementFade;
            set => movementFade = value;
        }

        public AnimationCurve RotationalFadeCurve
        {
            get => rotationalFade;
            set => rotationalFade = value;
        }

        public AnimationCurve TiltRangeTransition
        {
            get => tiltRangeTransition;
            set => tiltRangeTransition = value;
        }

        public AnimationCurve DynamicFieldOfViewTransition
        {
            get => dynamicFieldOfViewTransition;
            set => dynamicFieldOfViewTransition = value;
        }

        private void OnValidate()
        {
            // For axis-flipping reasons, the tilt ranges should remain within the -90 to 90 degrees range.
            tiltRangeHigh.Set(Mathf.Clamp(tiltRangeHigh.Min, -90f, 90f), Mathf.Clamp(tiltRangeHigh.Max, -90f, 90f));
            tiltRangeLow.Set(Mathf.Clamp(tiltRangeLow.Min, -90f, 90f), Mathf.Clamp(tiltRangeLow.Max, -90f, 90f));
        }

        /// <inheritdoc />
        public float EvaluateMovementFadeOut(float t)
        {
            return movementFade.Evaluate(t);
        }

        /// <inheritdoc />
        public float EvaluateRotationFadeOut(float t)
        {
            return rotationalFade.Evaluate(t);
        }

        /// <inheritdoc />
        public float EvaluateMovementTransition(float t)
        {
            return movementSpeedTransition.Evaluate(t);
        }

        /// <inheritdoc />
        public float EvaluateTiltTransition(float t)
        {
            return tiltRangeTransition.Evaluate(t);
        }

        /// <inheritdoc />
        public float EvaluateFieldOfViewTransition(float t)
        {
            return dynamicFieldOfViewTransition.Evaluate(t);
        }
    }
}

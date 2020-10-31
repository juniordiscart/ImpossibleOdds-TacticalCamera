namespace ImpossibleOdds.TacticalCamera
{
	using UnityEngine;

	[CreateAssetMenu(fileName = "TacticalCameraSettings", menuName = "Impossible Odds/Tactical Camera/Settings")]
	public class TacticalCameraSettings : ScriptableObject
	{
		[Header("Movement Settings")]
		[SerializeField, Tooltip("Curve that defines the speed of the camera depending on its altitude.")]
		private AnimationCurve movementSpeedTransition = AnimationCurve.Linear(0f, 0f, 1f, 1f);
		[SerializeField, Tooltip("Fade curve for movement.")]
		private AnimationCurve movementFade = AnimationCurve.Linear(0f, 1f, 1f, 0f);
		[SerializeField, Min(0f), Tooltip("Time for the movement to linger and fade out. (In seconds)")]
		private float movementFadeTime = 0.2f;
		[SerializeField, Min(0f), Range(0f, 1f), Tooltip("Smoothing time when the camera is moving to its focus point.")]
		private float moveToTargetSmoothingTime = 0.2f;
		[SerializeField, Tooltip("The range of height values the camera can operate in.")]
		private ValueRange absoluteHeightRange = new ValueRange();

		[Header("Rotation Settings")]
		[SerializeField, Min(0f), Tooltip("Maximum speed at which the camera will rotate around its origin. (In degrees per second)")]
		private float maxRotationSpeed = 180f;
		[SerializeField, Tooltip("Fade curve for rotation.")]
		private AnimationCurve rotationalFade = AnimationCurve.Linear(0f, 1f, 1f, 0f);
		[SerializeField, Min(0f), Tooltip("Time for the rotational fade to linger and fade out. (In seconds)")]
		private float rotationalFadeTime = 0.2f;

		[Header("Tilt Settings")]
		[SerializeField, Tooltip("Range in which the camera can tilt when it is at its lowest operating position. (In degrees)")]
		private ValueRange tiltRangeLow = new ValueRange();
		[SerializeField, Tooltip("Range in which the camera can tilt when it is at its heighest operating position. (In degrees)")]
		private ValueRange tiltRangeHigh = new ValueRange();
		[SerializeField, Tooltip("Transition of tilt ranges from low to high.")]
		private AnimationCurve tiltRangeTransition = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		[Header("Field of View")]
		[SerializeField, Tooltip("Should the tactical camera apply a dynamic field-of-view based on its height range?")]
		private bool useDynamicFieldOfView = true;
		[SerializeField, Tooltip("The range of field-of-view values for the camera, depending on the height it's operating at.")]
		private ValueRange dynamicFieldOfViewRange = new ValueRange();
		[SerializeField, Tooltip("The transition of the field-of-view value from the lowest operating height to the highest operating height.")]
		private AnimationCurve dynamicFieldOfViewTransition = AnimationCurve.Linear(0f, 0f, 1, 1f);

		[Header("World Interaction Settings")]
		[SerializeField, Tooltip("Layers that are to used to interact with the camera, e.g. when lowering it towards the ground.")]
		private LayerMask interactionMask = 0;
		[SerializeField, Min(0f), Tooltip("Distance the camera can interact with the world, e.g. raycast distance.")]
		private float interactionDistance = 1000f;

		/// <summary>
		/// Curve that defines the speed of the camera depending on its altitude.
		/// </summary>
		public AnimationCurve MovementSpeedTransition
		{
			get { return movementSpeedTransition; }
			set { movementSpeedTransition = value; }
		}

		/// <summary>
		/// Fade curve for movement.
		/// </summary>
		public AnimationCurve MovementFadeCurve
		{
			get { return movementFade; }
			set { movementFade = value; }
		}

		/// <summary>
		/// Time for the movement to linger and fade out. (In seconds)
		/// </summary>
		public float MovementFadeTime
		{
			get { return movementFadeTime; }
			set { movementFadeTime = value; }
		}

		/// <summary>
		/// Smoothing time when the camera is moving to its focus point.
		/// </summary>
		public float MoveToTargetSmoothingTime
		{
			get { return moveToTargetSmoothingTime; }
			set { moveToTargetSmoothingTime = value; }
		}

		/// <summary>
		/// The minimum height above a surface (defined by the interaction mask) and maximum height the camera can go.
		/// </summary>
		public ValueRange AbsoluteHeightRange
		{
			get { return absoluteHeightRange; }
			set { absoluteHeightRange = value; }
		}

		/// <summary>
		/// Maximum speed at which the camera will pivot around its origin. (In degrees per second)
		/// </summary>
		public float MaxRotationalSpeed
		{
			get { return maxRotationSpeed; }
			set { maxRotationSpeed = value; }
		}

		/// <summary>
		/// Fade curve for pivotal rotation.
		/// </summary>
		public AnimationCurve RotationalFadeCurve
		{
			get { return rotationalFade; }
			set { rotationalFade = value; }
		}

		/// <summary>
		/// Time for the rotation to linger and fade out. (In seconds)
		/// </summary>
		public float RotationalFadeTime
		{
			get { return rotationalFadeTime; }
			set { rotationalFadeTime = value; }
		}

		/// <summary>
		/// Range in which the camera can tilt when it is up high. (In degrees)
		/// </summary>
		public ValueRange TiltRangeHigh
		{
			get { return tiltRangeHigh; }
			set { tiltRangeHigh = value; }
		}

		/// <summary>
		/// Range in which the camera can tilt when it is down low. (In degrees)
		/// </summary>
		public ValueRange TiltRangeLow
		{
			get { return tiltRangeLow; }
			set { tiltRangeLow = value; }
		}

		/// <summary>
		/// Transition of tilt ranges from low to high.
		/// </summary>
		public AnimationCurve TiltRangeTransition
		{
			get { return tiltRangeTransition; }
			set { tiltRangeTransition = value; }
		}

		/// <summary>
		/// Should the tactical camera apply a dynamic field-of-view based on its height range?
		/// </summary>
		public bool UseDynamicFieldOfView
		{
			get { return useDynamicFieldOfView; }
			set { useDynamicFieldOfView = value; }
		}

		/// <summary>
		/// The range of field-of-view values for the camera, depending on the height it's operating at.
		/// </summary>
		public ValueRange DynamicFieldOfViewRange
		{
			get { return dynamicFieldOfViewRange; }
			set { dynamicFieldOfViewRange = value; }
		}

		/// <summary>
		/// The transition of the field-of-view value from the lowest operating height to the highest operating height.
		/// </summary>
		public AnimationCurve DynamicFieldOfViewTransition
		{
			get { return dynamicFieldOfViewTransition; }
			set { dynamicFieldOfViewTransition = value; }
		}

		/// <summary>
		/// Layers that are to used to interact with the camera movement.
		/// </summary>
		public LayerMask InteractionMask
		{
			get { return interactionMask; }
			set { interactionMask = value; }
		}

		/// <summary>
		/// Distance the camera can interact with the world, e.g. raycast distance.
		/// </summary>
		public float InteractionDistance
		{
			get { return interactionDistance; }
			set { interactionDistance = value; }
		}
	}
}

namespace ImpossibleOdds.TacticalCamera
{
	using UnityEngine;

	[CreateAssetMenu(fileName = "TacticalCameraSettings", menuName = "Impossible Odds/Tactical Camera/Settings")]
	public class TacticalCameraSettings : ScriptableObject
	{
		[Header("Movement Settings")]
		[SerializeField, Tooltip("Curve that defines the speed of the camera depending on its altitude.")]
		private AnimationCurve movementSpeedTransition = AnimationCurve.Linear(0f, 10f, 1f, 1f);
		[SerializeField, Tooltip("Rolloff curve for movement.")]
		private AnimationCurve movementRolloff = AnimationCurve.Linear(0f, 1f, 1f, 0f);
		[SerializeField, Min(0f), Tooltip("Time for the movement to linger and fade out. (In seconds)")]
		private float movementRolloffTime = 0.2f;
		[SerializeField, Min(0f), Range(0f, 1f), Tooltip("Smoothing time when the camera is moving to its focus point.")]
		private float moveToTargetSmoothingTime = 0.2f;
		[SerializeField, Min(0f), Tooltip("The minimum height above a surface (defined by the interaction mask) and maximum height the camera can go.")]
		private ValueRange heightRange = new ValueRange();

		[Header("Pivot Settings")]
		[SerializeField, Min(0f), Tooltip("Maximum speed at which the camera will pivot around its origin. (In degrees per second)")]
		private float maxPivotalSpeed = 180f;
		[SerializeField, Tooltip("Rolloff curve for pivotal rotation.")]
		private AnimationCurve pivotalRolloff = AnimationCurve.Linear(0f, 1f, 1f, 0f);
		[SerializeField, Min(0f), Tooltip("Time for the pivotal rolloff to linger and fade out. (In seconds)")]
		private float pivotalRolloffTime = 0.2f;

		[Header("Orbit Settings")]
		[SerializeField, Min(0f), Tooltip("Maximum speed at which the camera will orbit around its focus point. (In degrees per second)")]
		private float maxOrbitalSpeed = 90f;
		[SerializeField, Tooltip("Rolloff curve for orbital movement.")]
		private AnimationCurve orbitalRolloff = AnimationCurve.Linear(0f, 1f, 1f, 0f);
		[SerializeField, Min(0f), Tooltip("Time for the orbital rolloff to linger and fade out. (In seconds)")]
		private float orbitalRolloffTime = 0.2f;

		[Header("Pitch Settings")]
		[SerializeField, Tooltip("Range in which the camera can pitch when it is up high. (In degrees)")]
		private ValueRange pitchRangeHigh = new ValueRange();
		[SerializeField, Tooltip("Range in which the camera can pitch when it is down low. (In degrees)")]
		private ValueRange pitchRangeLow = new ValueRange();
		[SerializeField, Tooltip("Transition of pitch ranges from high to low.")]
		private AnimationCurve pitchRangeTransition = AnimationCurve.Linear(0f, 1f, 1f, 0f);

		[Header("World Interaction Settings")]
		[SerializeField, Tooltip("Layers that are to used to interact with the camera movement.")]
		private LayerMask interactionMask = 0;
		[SerializeField, Tooltip("Maximum length of rays that are used for interacting with the world.")]
		private float maxInteractionRayLength = 1000f;

		/// <summary>
		/// Curve that defines the speed of the camera depending on its altitude.
		/// </summary>
		public AnimationCurve MovementSpeedTransition
		{
			get { return movementSpeedTransition; }
			set { movementSpeedTransition = value; }
		}

		/// <summary>
		/// Rolloff curve for movement.
		/// </summary>
		public AnimationCurve MovementRolloff
		{
			get { return movementRolloff; }
			set { movementRolloff = value; }
		}

		/// <summary>
		/// Time for the movement to linger and fade out. (In seconds)
		/// </summary>
		public float MovementRolloffTime
		{
			get { return movementRolloffTime; }
			set { movementRolloffTime = value; }
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
		/// Maximum speed at which the camera will pivot around its origin. (In degrees per second)
		/// </summary>
		public float MaxPivotalSpeed
		{
			get { return maxPivotalSpeed; }
			set { maxPivotalSpeed = value; }
		}

		/// <summary>
		/// Rolloff curve for pivotal rotation.
		/// </summary>
		public AnimationCurve PivotalRolloff
		{
			get { return pivotalRolloff; }
			set { pivotalRolloff = value; }
		}

		/// <summary>
		/// Time for the pivotal rolloff to linger and fade out. (In seconds)
		/// </summary>
		public float PivotalRolloffTime
		{
			get { return pivotalRolloffTime; }
			set { pivotalRolloffTime = value; }
		}

		/// <summary>
		/// Maximum speed at which the camera will orbit around its focus point. (In degrees per second)
		/// </summary>
		public float MaxOrbitalSpeed
		{
			get { return maxOrbitalSpeed; }
			set { maxOrbitalSpeed = value; }
		}

		/// <summary>
		/// Rolloff curve for orbital movement.
		/// </summary>
		public AnimationCurve OrbitalRolloff
		{
			get { return orbitalRolloff; }
			set { orbitalRolloff = value; }
		}

		/// <summary>
		/// Time for the orbital rolloff to linger and fade out. (In seconds)
		/// </summary>
		public float OrbitalRolloffTime
		{
			get { return orbitalRolloffTime; }
			set { orbitalRolloffTime = value; }
		}

		/// <summary>
		/// Range in which the camera can pitch when it is up high. (In degrees)
		/// </summary>
		public ValueRange PitchRangeHigh
		{
			get { return pitchRangeHigh; }
			set { pitchRangeHigh = value; }
		}

		/// <summary>
		/// Range in which the camera can pitch when it is down low. (In degrees)
		/// </summary>
		public ValueRange PitchRangeLow
		{
			get { return pitchRangeLow; }
			set { pitchRangeLow = value; }
		}

		/// <summary>
		/// Transition of pitch ranges from high to low.
		/// </summary>
		public AnimationCurve PitchRangeTransition
		{
			get { return pitchRangeTransition; }
			set { pitchRangeTransition = value; }
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
		/// Maximum length of rays that are used for interacting with the world.
		/// </summary>
		public float MaxInteractionRayLength
		{
			get { return maxInteractionRayLength; }
			set { maxInteractionRayLength = value; }
		}

		/// <summary>
		/// The minimum height above a surface (defined by the interaction mask) and maximum height the camera can go.
		/// </summary>
		public ValueRange HeightRange
		{
			get { return heightRange; }
			set { heightRange = value; }
		}
	}
}

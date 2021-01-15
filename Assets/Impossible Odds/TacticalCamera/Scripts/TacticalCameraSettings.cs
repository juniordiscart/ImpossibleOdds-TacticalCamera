namespace ImpossibleOdds.TacticalCamera
{
	using System;
	using UnityEngine;

	[CreateAssetMenu(fileName = "TacticalCameraSettings", menuName = "Impossible Odds/Tactical Camera/Settings")]
	public class TacticalCameraSettings : ScriptableObject, ITacticalCameraSettings
	{
		[Header("Movement Settings")]
		[SerializeField, Tooltip("Curve that defines the speed of the camera depending on its height.")]
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
		[SerializeField, Min(0f), Tooltip("The radius of the camera's collider, used to avoid collisions and clipping with the environment.")]
		private float interactionBubbleRadius = 1f;

		/// <inheritdoc />
		public event Action onSettingsUpdated;

		/// <inheritdoc />
		public float Epsilon
		{
			get { return 0.001f; }
		}

		/// <inheritdoc />
		public AnimationCurve MovementSpeedTransition
		{
			get { return movementSpeedTransition; }
			set
			{
				movementSpeedTransition = value;
				onSettingsUpdated.InvokeIfNotNull();
			}
		}

		/// <inheritdoc />
		public AnimationCurve MovementFadeCurve
		{
			get { return movementFade; }
			set
			{
				movementFade = value;
				onSettingsUpdated.InvokeIfNotNull();
			}
		}

		/// <inheritdoc />
		public float MovementFadeTime
		{
			get { return movementFadeTime; }
			set
			{
				movementFadeTime = value;
				onSettingsUpdated.InvokeIfNotNull();
			}
		}

		/// <inheritdoc />
		public float MoveToTargetSmoothingTime
		{
			get { return moveToTargetSmoothingTime; }
			set
			{
				moveToTargetSmoothingTime = value;
				onSettingsUpdated.InvokeIfNotNull();
			}
		}

		/// <inheritdoc />
		public ValueRange AbsoluteHeightRange
		{
			get { return absoluteHeightRange; }
			set
			{
				absoluteHeightRange = value;
				onSettingsUpdated.InvokeIfNotNull();
			}
		}

		/// <inheritdoc />
		public float MaxRotationalSpeed
		{
			get { return maxRotationSpeed; }
			set
			{
				maxRotationSpeed = value;
				onSettingsUpdated.InvokeIfNotNull();
			}
		}

		/// <inheritdoc />
		public AnimationCurve RotationalFadeCurve
		{
			get { return rotationalFade; }
			set
			{
				rotationalFade = value;
				onSettingsUpdated.InvokeIfNotNull();
			}
		}

		/// <inheritdoc />
		public float RotationalFadeTime
		{
			get { return rotationalFadeTime; }
			set
			{
				rotationalFadeTime = value;
				onSettingsUpdated.InvokeIfNotNull();
			}
		}

		/// <inheritdoc />
		public ValueRange TiltRangeHigh
		{
			get { return tiltRangeHigh; }
			set
			{
				tiltRangeHigh = value;
				onSettingsUpdated.InvokeIfNotNull();
			}
		}

		/// <inheritdoc />
		public ValueRange TiltRangeLow
		{
			get { return tiltRangeLow; }
			set
			{
				tiltRangeLow = value;
				onSettingsUpdated.InvokeIfNotNull();
			}
		}

		/// <inheritdoc />
		public AnimationCurve TiltRangeTransition
		{
			get { return tiltRangeTransition; }
			set
			{
				tiltRangeTransition = value;
				onSettingsUpdated.InvokeIfNotNull();
			}
		}

		/// <inheritdoc />
		public bool UseDynamicFieldOfView
		{
			get { return useDynamicFieldOfView; }
			set
			{
				useDynamicFieldOfView = value;
				onSettingsUpdated.InvokeIfNotNull();
			}
		}

		/// <inheritdoc />
		public ValueRange DynamicFieldOfViewRange
		{
			get { return dynamicFieldOfViewRange; }
			set
			{
				dynamicFieldOfViewRange = value;
				onSettingsUpdated.InvokeIfNotNull();
			}
		}

		/// <inheritdoc />
		public AnimationCurve DynamicFieldOfViewTransition
		{
			get { return dynamicFieldOfViewTransition; }
			set
			{
				dynamicFieldOfViewTransition = value;
				onSettingsUpdated.InvokeIfNotNull();
			}
		}

		/// <inheritdoc />
		public LayerMask InteractionMask
		{
			get { return interactionMask; }
			set
			{
				interactionMask = value;
				onSettingsUpdated.InvokeIfNotNull();
			}
		}

		/// <inheritdoc />
		public float InteractionDistance
		{
			get { return interactionDistance; }
			set
			{
				interactionDistance = value;
				onSettingsUpdated.InvokeIfNotNull();
			}
		}

		/// <inheritdoc />
		public float InteractionBubbleRadius
		{
			get { return interactionBubbleRadius; }
			set
			{
				interactionBubbleRadius = value;
				onSettingsUpdated.InvokeIfNotNull();
			}
		}

		private void OnValidate()
		{
			// For reasons axis-flipping reasons, the tilt ranges should remain within the -90 to 90 degrees range.
			tiltRangeHigh.Set(Mathf.Clamp(tiltRangeHigh.Min, -90f, 90f), Mathf.Clamp(tiltRangeHigh.Max, -90f, 90f));
			tiltRangeLow.Set(Mathf.Clamp(tiltRangeLow.Min, -90f, 90f), Mathf.Clamp(tiltRangeLow.Max, -90f, 90f));
		}
	}
}

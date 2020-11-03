namespace ImpossibleOdds.TacticalCamera
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	using ImpossibleOdds.DependencyInjection;

	[RequireComponent(typeof(CharacterController), typeof(Camera))]
	public class TacticalCamera : MonoBehaviour
	{
		[SerializeField, Tooltip("(Optional) - Initial settings if no other settings are provided.")]
		private TacticalCameraSettings initialSettings = null;
		[SerializeField, Tooltip("(Optional) - Initial input provider if no other input provider is provided.")]
		private AbstractTacticalCameraInputProvider initialInputProvider = null;
		[SerializeField, Tooltip("(Optional) - Initial camera bounds if no other bounds are provided.")]
		private AbstractTacticalCameraBounds initialCameraBounds = null;

		private ITacticalCameraSettings settings = null;
		private ITacticalCameraInputProvider inputProvider = null;
		private ITacticalCameraBounds bounds = null;

		private ValueRange operatingHeightRange = new ValueRange();
		private ValueRange operatingTiltRange = new ValueRange();
		private float tiltFactor = 0f;  // Factor that keeps track of the tilt in the operating tilt range. For smooth angle adjustment when moving vertically.

		private new Camera camera = null;
		private CharacterController characterController = null;

		private Coroutine moveToPositionHandle = null;
		private Coroutine monitorTiltAngleHandle = null;
		private Coroutine dynamicFieldOfViewHandle = null;

		private FadeState moveForwardsState = new FadeState();
		private FadeState moveSidewaysState = new FadeState();
		private FadeState moveUpwardsState = new FadeState();
		private FadeState tiltState = new FadeState();
		private FadeState rotationState = new FadeState();
		private List<FadeState> fadeStates = new List<FadeState>();

		/// <summary>
		/// (Injectable) Settings that define how the camera should behave when moving or rotating.
		/// </summary>
		[Inject]
		public ITacticalCameraSettings Settings
		{
			get { return settings; }
			set { ApplySettings(value); }
		}

		/// <summary>
		/// (Injectable) Input provider that should tell where the camera to move to or look at.
		/// </summary>
		[Inject]
		public ITacticalCameraInputProvider InputProvider
		{
			get { return inputProvider; }
			set { inputProvider = value; }
		}

		/// <summary>
		/// (Injectable) Object that defines the positional bounds of the camera.
		/// If no bounds are set, then the camera can move anywhere.
		/// </summary>
		[Inject]
		public ITacticalCameraBounds Bounds
		{
			get { return bounds; }
			set { bounds = value; }
		}

		/// <summary>
		/// Is the camera moving to a focus point.
		/// </summary>
		public bool IsMovingToFocusPoint
		{
			get { return moveToPositionHandle != null; }
		}

		/// <summary>
		/// The current maximum movement speed of the camera based on it's height.
		/// </summary>
		public float MaxMovementSpeed
		{
			get
			{
				if (settings == null)
				{
					return 0f;
				}

				float speedT = Mathf.Clamp01(operatingHeightRange.InverseLerp(CurrentHeight));
				return settings.MovementSpeedTransition.Evaluate(speedT);
			}
		}

		/// <summary>
		/// Shortcut to retrieve the y-value of the transform's position in world-space.
		/// </summary>
		public float CurrentHeight
		{
			get { return transform.position.y; }
		}

		/// <summary>
		/// The current tilt angle. (In Degrees)
		/// Defines the tilt of the camera in the range [-180, 180], with 0 being level with the horizon.
		/// </summary>
		public float CurrentTiltAngle
		{
			get
			{
				Vector3 forward = transform.forward;
				Vector3 up = transform.up;
				float x = transform.localEulerAngles.x;

				// Depending on the quadrant the up-vector and forward-verctor are,
				// a modifier is applied to map it between -180 and 180 degrees.
				if (forward.y <= 0)
				{
					return (up.y >= 0f) ? x : (180f - x);
				}
				else
				{
					return (up.y >= 0f) ? (x - 360f) : (180f - x);
				}
			}
			private set
			{
				float tiltAngle = value;
				Vector3 forward = transform.forward;
				Vector3 up = transform.up;
				if (forward.y <= 0)
				{
					tiltAngle = (up.y >= 0f) ? tiltAngle : (180f + tiltAngle);
				}
				else
				{
					tiltAngle = (up.y >= 0f) ? (tiltAngle + 360f) : (180f + tiltAngle);
				}

				Vector3 localEulerAngles = transform.localEulerAngles;
				localEulerAngles.x = tiltAngle;
				transform.localEulerAngles = localEulerAngles;
			}
		}

		private void Awake()
		{
			characterController = GetComponent<CharacterController>();
			camera = GetComponent<Camera>();

			if ((settings == null) && (initialSettings != null))
			{
				Settings = initialSettings;
			}

			if ((inputProvider == null) && (initialInputProvider != null))
			{
				InputProvider = initialInputProvider;
			}

			if ((bounds == null) && (initialCameraBounds != null))
			{
				Bounds = initialCameraBounds;
			}

			fadeStates = new List<FadeState>()
			{
				moveForwardsState,
				moveSidewaysState,
				moveUpwardsState,
				tiltState,
				rotationState,
			};
		}

		private void OnEnable()
		{
			if ((inputProvider == null) || (settings == null))
			{
				return;
			}

			UpdateOperatingHeightRange();
			UpdateOperatingTiltRange();
			UpdateTiltFactor();

			if (settings.UseDynamicFieldOfView)
			{
				float t = operatingHeightRange.InverseLerp(CurrentHeight);
				camera.fieldOfView = settings.DynamicFieldOfViewRange.Lerp(settings.DynamicFieldOfViewTransition.Evaluate(t));
			}

			if (monitorTiltAngleHandle == null)
			{
				monitorTiltAngleHandle = StartCoroutine(RoutineMonitorTilt());
			}

			if (dynamicFieldOfViewHandle == null)
			{
				dynamicFieldOfViewHandle = StartCoroutine(RoutineMonitorFieldOfView());
			}
		}

		private void OnDisable()
		{
			// By setting the time to 0, we basically reset them.
			fadeStates.ForEach(fs => fs.Reset());
			StopRoutine(ref moveToPositionHandle);
			StopRoutine(ref monitorTiltAngleHandle);
			StopRoutine(ref dynamicFieldOfViewHandle);
		}

		private void LateUpdate()
		{
			if ((inputProvider == null) || (settings == null))
			{
				return;
			}

			UpdateStates();
			UpdateMovement();
			UpdateBounds();
			UpdateRotation();
		}

		private void ApplySettings(ITacticalCameraSettings settings)
		{
			if (this.settings != null)
			{
				this.settings.PurgeDelegatesOf(this);
			}

			this.settings = settings;

			if (settings == null)
			{
				return;
			}

			moveForwardsState.ApplySettings(settings.MovementFadeTime, settings.MovementFadeCurve);
			moveSidewaysState.ApplySettings(settings.MovementFadeTime, settings.MovementFadeCurve);
			moveUpwardsState.ApplySettings(settings.MovementFadeTime, settings.MovementFadeCurve);
			tiltState.ApplySettings(settings.RotationalFadeTime, settings.RotationalFadeCurve);
			rotationState.ApplySettings(settings.RotationalFadeTime, settings.RotationalFadeCurve);
			characterController.radius = settings.InteractionBubbleRadius;
			characterController.height = settings.InteractionBubbleRadius;



			if (monitorTiltAngleHandle == null)
			{
				monitorTiltAngleHandle = StartCoroutine(RoutineMonitorTilt());
			}

			if (dynamicFieldOfViewHandle == null)
			{
				dynamicFieldOfViewHandle = StartCoroutine(RoutineMonitorFieldOfView());
			}
		}

		private void UpdateStates()
		{
			if (inputProvider == null)
			{
				return;
			}

			// Movement
			UpdateState(inputProvider.MoveForward, moveForwardsState);
			UpdateState(inputProvider.MoveSideways, moveSidewaysState);
			UpdateState(inputProvider.MoveUp, moveUpwardsState);

			// Rotating
			UpdateState(inputProvider.TiltDelta, tiltState);
			UpdateState(inputProvider.RotationDelta, rotationState);
		}

		private void UpdateState(float inputValue, FadeState state)
		{
			if (Mathf.Approximately(inputValue, 0f))
			{
				state.Tick();
			}
			else
			{
				// If the outcome is not 0, then they are of opposite sign
				// and we substitute the new value, or we check whether
				// the new input is stronger than the current fading value.
				float value = state.Value;
				if (((Math.Sign(inputValue) + Math.Sign(value)) != 0) || (Mathf.Abs(inputValue) > Mathf.Abs(value)))
				{
					state.FadeValue = inputValue;
				}
				else
				{
					state.Tick();
				}
			}
		}

		private void UpdateMovement()
		{
			// If moving to a focus point and the animation is requested to be cancelled.
			if ((moveToPositionHandle != null) && inputProvider.CancelMoveToTarget)
			{
				StopCoroutine(moveToPositionHandle);
				moveToPositionHandle = null;
			}
			else if (inputProvider.MoveToTarget)
			{
				MoveToTarget();
			}

			Vector3 movement = Vector3.zero;

			// If the camera isn't moving towards its focus point,
			// then we apply the forward and sideways movement
			if (!IsMovingToFocusPoint)
			{
				Vector3 direction = Vector3.zero;
				if (moveForwardsState.IsActive)
				{
					Vector3 projectedForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

					// If the projected forward vector is close to 0, then that means the camera is facing down,
					// Then we use the up-vector as an indicator to go forward.
					if (projectedForward.sqrMagnitude <= settings.Epsilon)
					{
						projectedForward = Vector3.ProjectOnPlane(transform.up, Vector3.up).normalized;
					}

					direction += projectedForward * moveForwardsState.Value;
				}

				if (moveSidewaysState.IsActive)
				{
					Vector3 projectedRight = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
					direction += projectedRight * moveSidewaysState.Value;
				}

				if (direction.sqrMagnitude > settings.Epsilon)
				{
					movement += direction.normalized * MaxMovementSpeed * Time.deltaTime;
				}
			}

			// Vertical movement - zoom in/out
			if (moveUpwardsState.IsActive)
			{
				Vector3 direction = Vector3.up * moveUpwardsState.Value;
				if (direction.sqrMagnitude > settings.Epsilon)
				{
					movement += direction.normalized * MaxMovementSpeed * Time.deltaTime;
				}
			}

			float distance = movement.magnitude;

			// If hardly any movement is generated, then it can stop here.
			// No need for advanced extra processing.
			if (distance < settings.Epsilon)
			{
				return;
			}

			// Restrict the height movement
			Vector3 targetPosition = transform.position + movement;
			UpdateOperatingHeightRange(targetPosition);
			targetPosition.y = operatingHeightRange.Clamp(targetPosition.y);

			characterController.Move(targetPosition - transform.position);
		}

		private void UpdateRotation()
		{
			if (rotationState.IsActive)
			{
				if (inputProvider.OrbitAroundTarget && Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, settings.InteractionDistance, settings.InteractionMask))
				{
					// Rotation - Rotate horizontally around the focus point
					transform.RotateAround(hit.point, Vector3.up, rotationState.Value * settings.MaxRotationalSpeed * Time.deltaTime);
				}
				else
				{
					// Rotation - Rotate around the world up vector
					transform.Rotate(Vector3.up, rotationState.Value * settings.MaxRotationalSpeed * Time.deltaTime, Space.World);
				}
			}

			// Tilt - Rotate around the local right vector
			if (tiltState.IsActive)
			{
				float currentTiltAngle = CurrentTiltAngle;
				currentTiltAngle += tiltState.Value * settings.MaxRotationalSpeed * Time.deltaTime;
				if (!operatingTiltRange.InRange(currentTiltAngle))
				{
					currentTiltAngle = operatingTiltRange.Clamp(currentTiltAngle);
				}

				CurrentTiltAngle = currentTiltAngle;
				UpdateTiltFactor();
			}

			// Dirty fix for now: keep the z-value of the local Euler angles 0 to prevent drifting.
			Vector3 localEulerAngles = transform.localEulerAngles;
			localEulerAngles.z = 0f;
			transform.localEulerAngles = localEulerAngles;
		}

		private void UpdateOperatingHeightRange()
		{
			UpdateOperatingHeightRange(transform.position);
		}

		private void UpdateOperatingHeightRange(Vector3 origin)
		{
			float targetMin = settings.AbsoluteHeightRange.Min;
			float targetMax = settings.AbsoluteHeightRange.Max;

			// Define a possibly height operational minimum.
			if (Physics.Raycast(origin, Vector3.down, out RaycastHit minHit, settings.AbsoluteHeightRange.Range, settings.InteractionMask))
			{
				targetMin = Mathf.Max(minHit.point.y, targetMin);
			}

			// Define a possibly lower operational maximum. This one will most likely
			// never occur, unless we enter some kind of cave or structure with overhanging objects.
			if (Physics.Raycast(origin, Vector3.up, out RaycastHit maxHit, settings.AbsoluteHeightRange.Range, settings.InteractionMask))
			{
				targetMax = Mathf.Min(maxHit.point.y, targetMax);
			}

			operatingHeightRange.Set(targetMin, targetMax);
		}

		private void UpdateOperatingTiltRange()
		{
			operatingTiltRange = ValueRange.Lerp(settings.TiltRangeLow, settings.TiltRangeHigh, operatingHeightRange.InverseLerp(CurrentHeight));
		}

		private void UpdateBounds()
		{
			if (bounds != null)
			{
				bounds.Apply(this);
			}
		}

		private void UpdateTiltFactor()
		{
			tiltFactor = operatingTiltRange.InverseLerp(CurrentTiltAngle);
		}

		private void ApplyTiltFactor()
		{
			CurrentTiltAngle = operatingTiltRange.Lerp(tiltFactor);
		}

		private void MoveToTarget()
		{
			if (moveToPositionHandle != null)
			{
				StopCoroutine(moveToPositionHandle);
				moveToPositionHandle = null;
			}

			RaycastHit hitInfo;
			Ray direction = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (!Physics.Raycast(direction, out hitInfo, float.MaxValue, settings.InteractionMask, QueryTriggerInteraction.Ignore))
			{
				return;
			}

			Vector3 targetPosition = hitInfo.point;
			moveToPositionHandle = StartCoroutine(RoutineMoveToPosition(targetPosition));
		}

		private void StopRoutine(ref Coroutine handle)
		{
			if (handle != null)
			{
				StopCoroutine(handle);
				handle = null;
			}
		}

		private IEnumerator RoutineMoveToPosition(Vector3 endPosition)
		{
			Vector3 velocity = Vector3.zero;
			do
			{
				endPosition.y = CurrentHeight;
				Vector3 target = Vector3.SmoothDamp(transform.position, endPosition, ref velocity, settings.MoveToTargetSmoothingTime);
				characterController.Move(target - transform.position);
				yield return null;

			} while (velocity.sqrMagnitude > settings.Epsilon);

			moveToPositionHandle = null;
		}

		private IEnumerator RoutineMonitorTilt()
		{
			float tiltLowVelocity = 0f;
			float tiltHighVelocity = 0f;

			WaitForEndOfFrame waitHandle = new WaitForEndOfFrame();
			yield return waitHandle;

			while (true)
			{
				// Adapt the operating tilt range based on the operating height range, and if it's
				// over its limits, then move it towards it's target range.
				float t = operatingHeightRange.InverseLerp(CurrentHeight);
				t = Mathf.Clamp01(t);
				t = settings.TiltRangeTransition.Evaluate(t);
				ValueRange targetRange = ValueRange.Lerp(settings.TiltRangeLow, settings.TiltRangeHigh, t);
				operatingTiltRange.Set(
					Mathf.SmoothDampAngle(operatingTiltRange.Min, targetRange.Min, ref tiltLowVelocity, 0.2f),
					Mathf.SmoothDampAngle(operatingTiltRange.Max, targetRange.Max, ref tiltHighVelocity, 0.2f));

				ApplyTiltFactor();
				yield return waitHandle;
			}
		}

		private IEnumerator RoutineMonitorFieldOfView()
		{
			float velocity = 0f;
			WaitForEndOfFrame waitHandle = new WaitForEndOfFrame();
			yield return waitHandle;

			while (true)
			{
				if ((settings != null) && settings.UseDynamicFieldOfView)
				{
					float t = operatingHeightRange.InverseLerp(CurrentHeight);
					float target = settings.DynamicFieldOfViewRange.Lerp(settings.DynamicFieldOfViewTransition.Evaluate(t));
					camera.fieldOfView = Mathf.SmoothDamp(camera.fieldOfView, target, ref velocity, 0.2f);
				}

				yield return waitHandle;
			}
		}
	}
}

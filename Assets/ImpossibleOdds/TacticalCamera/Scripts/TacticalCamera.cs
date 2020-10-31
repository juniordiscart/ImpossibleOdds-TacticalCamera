namespace ImpossibleOdds.TacticalCamera
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	using ImpossibleOdds.DependencyInjection;

	[RequireComponent(typeof(CharacterController))]
	public class TacticalCamera : MonoBehaviour
	{
		private const float Epsilon = 0.001f;

		[SerializeField, Inject, Tooltip("(Injectable) Settings that define the camera's behaviour.")]
		private TacticalCameraSettings settings = null;
		[SerializeField, Inject, Tooltip("(Injectable) The camera it is operating on.")]
		private new Camera camera = null;
		[Inject, Tooltip("(Injectable) Input provider.")]
		private ITacticalCameraInputProvider inputProvider = null;
		[Inject, Tooltip("(Injectable) Forces the camera to remain within a certain area.")]
		private ITacticalCameraBounds bounds = null;

		private ValueRange operatingHeightRange = new ValueRange();
		private ValueRange operatingTiltRange = new ValueRange();
		private float tiltFactor = 0f;  // Factor that keeps track of the tilt in the operating tilt range. For smooth angle adjustment when moving vertically.

		private CharacterController characterController = null;

		private Coroutine moveToPositionHandle = null;
		private Coroutine restrictTiltAngleHandle = null;
		private Coroutine dynamicFieldOfViewHandle = null;

		private FadeState moveForwardsState = new FadeState();
		private FadeState moveSidewaysState = new FadeState();
		private FadeState moveUpwardsState = new FadeState();
		private FadeState tiltState = new FadeState();
		private FadeState rotationState = new FadeState();
		private List<FadeState> fadeStates = new List<FadeState>();

		/// <summary>
		/// Settings that define how the camera should behave when moving or rotating.
		/// </summary>
		public TacticalCameraSettings Settings
		{
			get { return settings; }
			set { ApplySettings(value); }
		}

		/// <summary>
		/// Input provider that should tell where the camera to move to or look at.
		/// </summary>
		public ITacticalCameraInputProvider InputProvider
		{
			get { return inputProvider; }
			set { inputProvider = value; }
		}

		/// <summary>
		/// Object that defines the positional bounds of the camera.
		/// If no bounds are set, then the camera can move anywhere.
		/// </summary>
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

				float speedT = Mathf.Clamp01(Mathf.InverseLerp(operatingHeightRange.Min, operatingHeightRange.Max, CurrentHeight));
				return settings.MovementSpeedTransition.Evaluate(speedT);
			}
		}

		public float CurrentHeight
		{
			get { return transform.position.y; }
		}

		private void Awake()
		{
			characterController = GetComponent<CharacterController>();

			ApplySettings(settings);
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
			UpdateTiltFactor();

			if (restrictTiltAngleHandle == null)
			{
				restrictTiltAngleHandle = StartCoroutine(RoutineMonitorTilt());
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
			StopRoutine(ref restrictTiltAngleHandle);
			StopRoutine(ref dynamicFieldOfViewHandle);
		}

		private void LateUpdate()
		{
			if ((inputProvider == null) || (settings == null))
			{
				return;
			}

			UpdateStates(); // Process the inputs and fade the states
			UpdateMovement();
			UpdateRotation();
			UpdateOperatingHeightRange();

			ApplyHeightRestriction();

			if (bounds != null)
			{
				bounds.Apply();
			}
		}

		private void ApplySettings(TacticalCameraSettings settings)
		{
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

			if (restrictTiltAngleHandle == null)
			{
				restrictTiltAngleHandle = StartCoroutine(RoutineMonitorTilt());
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
				// the new input is stronger than the current rolling value.
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

			Vector3 direction = Vector3.zero;

			// If the camera isn't moving towards its focus point,
			// then we apply the forward and sideways movement
			if (!IsMovingToFocusPoint)
			{
				if (moveForwardsState.IsActive)
				{
					Vector3 projectedForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

					// If the projected forward vector is close to 0, then that means the camera is facing down,
					// Then we use the up-vector as an indicator to go forward.
					if (projectedForward.sqrMagnitude <= Epsilon)
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
			}

			if (moveUpwardsState.IsActive)
			{
				direction.y = moveUpwardsState.Value;
			}

			direction.Normalize();
			Vector3 movement = direction * MaxMovementSpeed * Time.deltaTime;
			float distance = movement.magnitude;

			// If hardly any movement is generated, just quit here then
			if (distance < Epsilon)
			{
				return;
			}

			characterController.Move(movement);
		}

		private void UpdateRotation()
		{
			if (rotationState.IsActive)
			{
				if (inputProvider.OrbitAroundTarget && Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, settings.InteractionDistance, settings.InteractionMask))
				{
					// Rotation - Rotate horizontally around the focus point
					transform.RotateAround(hit.point, Vector3.up, rotationState.Value * Time.deltaTime);
				}
				else
				{
					// Rotation - Rotate around the world up vector
					transform.Rotate(Vector3.up, rotationState.Value * Time.deltaTime, Space.World);
				}
			}

			// Tilt - Rotate around the local right vector
			if (tiltState.IsActive)
			{
				transform.Rotate(transform.right, tiltState.Value * Time.deltaTime, Space.World);
				Vector3 localAngles = transform.localEulerAngles;
				localAngles.x = Mathf.Clamp(localAngles.x, operatingTiltRange.Min, operatingTiltRange.Max);
				transform.localEulerAngles = localAngles;
				UpdateTiltFactor();
			}

			// Dirty fix for now: keep the z-value of the local Euler angles 0 to prevent drifting.
			Vector3 localEulerAngles = transform.localEulerAngles;
			localEulerAngles.z = 0f;
			transform.localEulerAngles = localEulerAngles;
		}

		private void UpdateTiltFactor()
		{
			// Restrict the camera's tilt (x-value). When the camera's tilt
			// reaches the horizon (0 degrees), it will wrap around back to 360.
			// To counter this, we will do a -360 degrees offset.
			float tilt = transform.localEulerAngles.x;
			tilt = (tilt > 180f) ? (tilt - 360f) : tilt;
			tilt = Mathf.Clamp(tilt, operatingTiltRange.Min, operatingTiltRange.Max);
			tiltFactor = Mathf.InverseLerp(operatingTiltRange.Min, operatingTiltRange.Max, tilt);
		}

		private void UpdateOperatingHeightRange()
		{
			float targetMin = settings.AbsoluteHeightRange.Min;
			float targetMax = settings.AbsoluteHeightRange.Max;

			// Define a possibly height operational minimum.
			if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit minHit, settings.AbsoluteHeightRange.Range, settings.InteractionMask))
			{
				targetMin = Mathf.Max(minHit.point.y, targetMin);
			}

			// Define a possibly lower operational maximum. This one will most likely
			// never occur, unless we enter some kind of cave or structure with overhanging objects.
			if (Physics.Raycast(transform.position, Vector3.up, out RaycastHit maxHit, settings.AbsoluteHeightRange.Range, settings.InteractionMask))
			{
				targetMax = Mathf.Min(maxHit.point.y, targetMax);
			}

			// TODO: fix hitch?
			operatingHeightRange.Set(targetMin, targetMax);
		}

		private void ApplyHeightRestriction()
		{
			Vector3 position = transform.position;
			position.y = Mathf.Clamp(CurrentHeight, operatingHeightRange.Min, operatingHeightRange.Max);
			transform.position = position;
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

			} while (velocity.sqrMagnitude > Epsilon);

			moveToPositionHandle = null;
		}

		private IEnumerator RoutineMonitorTilt()
		{
			float tiltLowVelocity = 0f;
			float tiltHighVelocity = 0f;

			WaitForEndOfFrame waitHandle = new WaitForEndOfFrame();

			while (true)
			{
				// Adapt the operating tilt range based on the operating height range, and if it's
				// over its limits, then move it towards it's target range.
				float t = Mathf.InverseLerp(operatingHeightRange.Min, operatingHeightRange.Max, CurrentHeight);
				t = Mathf.Clamp01(t);
				t = settings.TiltRangeTransition.Evaluate(t);
				ValueRange targetRange = ValueRange.Lerp(settings.TiltRangeLow, settings.TiltRangeHigh, t);
				operatingTiltRange.Set(
					Mathf.SmoothDampAngle(operatingTiltRange.Min, targetRange.Min, ref tiltLowVelocity, 0.2f),
					Mathf.SmoothDampAngle(operatingTiltRange.Max, targetRange.Max, ref tiltHighVelocity, 0.2f));

				// Apply the tilt factor
				Vector3 localAngles = transform.localEulerAngles;
				localAngles.x = Mathf.Lerp(operatingTiltRange.Min, operatingTiltRange.Max, tiltFactor);
				transform.localEulerAngles = localAngles;

				yield return waitHandle;
			}
		}

		private IEnumerator RoutineMonitorFieldOfView()
		{
			float velocity = 0f;
			WaitForEndOfFrame waitHandle = new WaitForEndOfFrame();

			while (true)
			{
				if ((camera != null) && (settings != null) && settings.UseDynamicFieldOfView)
				{
					float t = Mathf.InverseLerp(operatingHeightRange.Min, operatingHeightRange.Max, CurrentHeight);
					float target = Mathf.Lerp(settings.DynamicFieldOfViewRange.Min, settings.DynamicFieldOfViewRange.Max, settings.DynamicFieldOfViewTransition.Evaluate(t));
					camera.fieldOfView = Mathf.SmoothDamp(camera.fieldOfView, target, ref velocity, 0.2f);
				}

				yield return waitHandle;
			}
		}
	}
}

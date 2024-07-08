using System;
using System.Collections;
using System.Collections.Generic;
using ImpossibleOdds.DependencyInjection;
using UnityEngine;

namespace ImpossibleOdds.TacticalCamera
{
	[Injectable, RequireComponent(typeof(CharacterController), typeof(Camera))]
	public class TacticalCamera : MonoBehaviour
	{
		/// <summary>
		/// Called when no valid target position could be determined to move the camera to.
		/// </summary>
		public event Action onMoveToTargetFailed;
		
		[SerializeField, Tooltip("Initial settings if no other settings are provided through other means.")]
		internal TacticalCameraSettings initialSettings;
		[SerializeField, Tooltip("Initial input provider if no other input provider is provided through other means.")]
		internal AbstractTacticalCameraInputProvider initialInputProvider;
		[SerializeField, Tooltip("(Optional) - Initial camera bounds if no other bounds are provided through other means.")]
		internal AbstractTacticalCameraBounds initialCameraBounds;

		private ITacticalCameraSettings settings;

		private Transform cachedTransform;
		private Camera cachedCamera;
		private CharacterController characterController;

		private ValueRange operatingHeightRange;
		private ValueRange operatingTiltRange;
		private float tiltFactor;  // Factor that keeps track of the tilt in the operating tilt range. For smooth angle adjustment when moving vertically.
		private float tiltLowSpeed;
		private float tiltHighSpeed;
		private float fovSpeed;

		private Coroutine moveToPositionHandle;

		private readonly FadeState moveForwardsState = new FadeState();
		private readonly FadeState moveSidewaysState = new FadeState();
		private readonly FadeState moveUpwardsState = new FadeState();
		private readonly FadeState tiltState = new FadeState();
		private readonly FadeState rotationState = new FadeState();
		private List<FadeState> fadeStates;

		/// <summary>
		/// (Injectable) Settings that define how the camera should behave when moving or rotating.
		/// </summary>
		[Inject]
		public ITacticalCameraSettings Settings
		{
			get => settings;
			set => ApplySettings(value);
		}

		/// <summary>
		/// (Injectable) Input provider that should tell where the camera to move to or look at.
		/// </summary>
		[Inject]
		public ITacticalCameraInputProvider InputProvider
		{
			get;
			set;
		}

		/// <summary>
		/// (Injectable) Object that defines the positional bounds of the camera.
		/// If no bounds are set, then the camera can move anywhere.
		/// </summary>
		[Inject]
		public ITacticalCameraBounds Bounds
		{
			get;
			set;
		}

		/// <summary>
		/// Is the camera moving to a focus point.
		/// </summary>
		public bool IsMovingToFocusPoint => moveToPositionHandle != null;

		private float DeltaTime => settings.IgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
		private float TimeScaleFactor => settings.IgnoreTimeScale ? Time.timeScale : 1f;

		/// <summary>
		/// The current maximum movement speed of the camera based on its height.
		/// </summary>
		public float MaxMovementSpeed
		{
			get
			{
				if (settings == null)
				{
					return 0f;
				}

				float t = Mathf.Clamp01(operatingHeightRange.InverseLerp(CurrentHeight));
				t = settings.EvaluateMovementTransition(t);
				return settings.MovementSpeedRange.Lerp(t);
			}
		}

		/// <summary>
		/// Shortcut to retrieve the y-value of the transform's position in world-space.
		/// </summary>
		public float CurrentHeight => cachedTransform.position.y;

		/// <summary>
		/// The current tilt angle. (In Degrees)
		/// Defines the tilt of the camera in the range [-180, 180], with 0 being level with the horizon.
		/// </summary>
		public float CurrentTiltAngle
		{
			get
			{
				Vector3 forward = cachedTransform.forward;
				Vector3 up = cachedTransform.up;
				float x = cachedTransform.localEulerAngles.x;

				// Depending on the quadrant the up-vector and forward-vector are,
				// a modifier is applied to map it between -180 and 180 degrees.
				if (forward.y <= 0)
				{
					return (up.y >= 0f) ? x : (180f - x);
				}
				
				return (up.y >= 0f) ? (x - 360f) : (180f - x);
			}
			private set
			{
				float tiltAngle = value;
				Vector3 forward = cachedTransform.forward;
				Vector3 up = cachedTransform.up;
				if (forward.y <= 0)
				{
					tiltAngle = (up.y >= 0f) ? tiltAngle : (180f + tiltAngle);
				}
				else
				{
					tiltAngle = (up.y >= 0f) ? (tiltAngle + 360f) : (180f + tiltAngle);
				}

				Vector3 localEulerAngles = cachedTransform.localEulerAngles;
				localEulerAngles.x = tiltAngle;
				cachedTransform.localEulerAngles = localEulerAngles;
			}
		}

		/// <summary>
		/// Moves the camera over to the target position. Note that the camera will only move over the XZ-plane.
		/// </summary>
		/// <param name="targetPosition">Target position for the camera to move to.</param>
		public void MoveToTarget(Vector3 targetPosition)
		{
			CancelMoveToTarget();	// If it was running.
			moveToPositionHandle = StartCoroutine(RoutineMoveToTarget(targetPosition));
		}

		/// <summary>
		/// Cancels the move-to-target routine, if it was running.
		/// </summary>
		public void CancelMoveToTarget()
		{
			if (moveToPositionHandle != null)
			{
				StopCoroutine(moveToPositionHandle);
				moveToPositionHandle = null;
			}
		}

		private void Awake()
		{
			characterController = GetComponent<CharacterController>();
			cachedCamera = GetComponent<Camera>();
			cachedTransform = transform;

			if ((settings == null) && (initialSettings != null))
			{
				Settings = initialSettings;
			}

			if ((InputProvider == null) && (initialInputProvider != null))
			{
				InputProvider = initialInputProvider;
			}

			if ((Bounds == null) && (initialCameraBounds != null))
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
			if ((InputProvider == null) || (settings == null))
			{
				return;
			}

			UpdateOperatingHeightRange();
			UpdateOperatingTiltRange();
			UpdateTiltFactor();

			if (settings.UseDynamicFieldOfView)
			{
				float t = operatingHeightRange.InverseLerp(CurrentHeight);
				cachedCamera.fieldOfView = settings.DynamicFieldOfViewRange.Lerp(settings.EvaluateFieldOfViewTransition(t));
			}
		}

		private void OnDisable()
		{
			// By setting the time to 0, we basically reset them.
			fadeStates.ForEach(fs => fs.Reset());
			CancelMoveToTarget();
		}

		private void LateUpdate()
		{
			if ((InputProvider == null) || (settings == null))
			{
				return;
			}

			UpdateStates();
			UpdateMovement();
			UpdateBounds();
			UpdateRotation();
			
			MonitorTilt();
			MonitorFieldOfView();
		}

		private void ApplySettings(ITacticalCameraSettings settings)
		{
			this.settings?.PurgeDelegatesOf(this);
			this.settings = settings;

			if (settings == null)
			{
				return;
			}

			moveForwardsState.ApplySettings(settings.MovementFadeTime, settings.EvaluateMovementFadeOut, () => DeltaTime);
			moveSidewaysState.ApplySettings(settings.MovementFadeTime, settings.EvaluateMovementFadeOut, () => DeltaTime);
			moveUpwardsState.ApplySettings(settings.MovementFadeTime, settings.EvaluateMovementFadeOut, () => DeltaTime);
			tiltState.ApplySettings(settings.RotationalFadeTime, settings.EvaluateRotationFadeOut, () => DeltaTime);
			rotationState.ApplySettings(settings.RotationalFadeTime, settings.EvaluateRotationFadeOut, () => DeltaTime);
			characterController.radius = settings.InteractionBubbleRadius;
			characterController.height = settings.InteractionBubbleRadius;
		}

		private void UpdateStates()
		{
			if (InputProvider == null)
			{
				return;
			}

			// Movement
			UpdateState(InputProvider.MoveForward, moveForwardsState);
			UpdateState(InputProvider.MoveSideways, moveSidewaysState);
			UpdateState(InputProvider.MoveUp, moveUpwardsState);

			// Rotating
			UpdateState(InputProvider.TiltDelta, tiltState);
			UpdateState(InputProvider.RotationDelta, rotationState);
		}

		private void UpdateState(float inputValue, FadeState state)
		{
			if (Mathf.Approximately(inputValue, 0f))
			{
				state.Tick();
			}
			else
			{
				// If the outcome is not 0, then they are of opposite sign, and we substitute the new value,
				// or we check whether the new input is stronger than the current fading value.
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
			if ((moveToPositionHandle != null) && InputProvider.CancelMoveToTarget)
			{
				CancelMoveToTarget();
			}
			else if (InputProvider.MoveToTarget)
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
					Vector3 projectedForward = Vector3.ProjectOnPlane(cachedTransform.forward, Vector3.up).normalized;

					// If the projected forward vector is close to 0, then that means the camera is facing down,
					// Then we use the up-vector as an indicator to go forward.
					if (projectedForward.sqrMagnitude <= settings.Epsilon)
					{
						projectedForward = Vector3.ProjectOnPlane(cachedTransform.up, Vector3.up).normalized;
					}

					direction += projectedForward * moveForwardsState.Value;
				}

				if (moveSidewaysState.IsActive)
				{
					Vector3 projectedRight = Vector3.ProjectOnPlane(cachedTransform.right, Vector3.up).normalized;
					direction += projectedRight * moveSidewaysState.Value;
				}

				if (direction.sqrMagnitude > settings.Epsilon)
				{
					movement += direction.normalized * (MaxMovementSpeed * DeltaTime);
				}
			}

			// Vertical movement - zoom in/out
			if (moveUpwardsState.IsActive)
			{
				Vector3 direction = Vector3.up * moveUpwardsState.Value;
				if (direction.sqrMagnitude > settings.Epsilon)
				{
					movement += direction.normalized * (MaxMovementSpeed * DeltaTime);
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
			Vector3 position = cachedTransform.position;
			Vector3 targetPosition = position + movement;
			UpdateOperatingHeightRange(targetPosition);
			targetPosition.y = operatingHeightRange.Clamp(targetPosition.y);

			characterController.Move(targetPosition - position);
		}

		private void UpdateRotation()
		{
			if (rotationState.IsActive)
			{
				if (InputProvider.OrbitAroundTarget)
				{
					if (Physics.Raycast(cachedTransform.position, cachedTransform.forward, out RaycastHit hit, settings.InteractionDistance, settings.InteractionMask))
					{
						cachedTransform.RotateAround(hit.point, Vector3.up, rotationState.Value * settings.MaxRotationalSpeed * DeltaTime);
					}
					else if (settings.AllowPivotOnFailedOrbit)
					{
						// Rotation - Rotate around the world up vector
						cachedTransform.Rotate(Vector3.up, rotationState.Value * settings.MaxRotationalSpeed * DeltaTime, Space.World);						
					}
				}
				else
				{
					// Rotation - Rotate around the world up vector
					cachedTransform.Rotate(Vector3.up, rotationState.Value * settings.MaxRotationalSpeed * DeltaTime, Space.World);
				}
			}

			// Tilt - Rotate around the local right vector
			if (tiltState.IsActive)
			{
				float currentTiltAngle = CurrentTiltAngle;
				currentTiltAngle += tiltState.Value * settings.MaxRotationalSpeed * DeltaTime;
				if (!operatingTiltRange.InRange(currentTiltAngle))
				{
					currentTiltAngle = operatingTiltRange.Clamp(currentTiltAngle);
				}

				CurrentTiltAngle = currentTiltAngle;
				UpdateTiltFactor();
			}

			// Dirty fix for now: keep the z-value of the local Euler angles 0 to prevent drifting.
			Vector3 localEulerAngles = cachedTransform.localEulerAngles;
			localEulerAngles.z = 0f;
			cachedTransform.localEulerAngles = localEulerAngles;
		}

		private void UpdateOperatingHeightRange()
		{
			UpdateOperatingHeightRange(cachedTransform.position);
		}

		private void UpdateOperatingHeightRange(Vector3 origin)
		{
			float targetMin = settings.AbsoluteHeightRange.Min;
			float targetMax = settings.AbsoluteHeightRange.Max;

			// Define a possible height operational minimum.
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
			Bounds?.Apply(this);
		}

		private void UpdateTiltFactor()
		{
			tiltFactor = operatingTiltRange.InverseLerp(CurrentTiltAngle);
		}
		
		private void MonitorTilt()
		{
			// Adapt the operating tilt range based on the operating height range, and if it's
			// over its limits, then move it towards its target range.
			float t = operatingHeightRange.InverseLerp(CurrentHeight);
			t = Mathf.Clamp01(t);
			t = settings.EvaluateTiltTransition(t);
			ValueRange targetRange = ValueRange.Lerp(settings.TiltRangeLow, settings.TiltRangeHigh, t);
			operatingTiltRange.Set(
				Mathf.SmoothDampAngle(operatingTiltRange.Min, targetRange.Min, ref tiltLowSpeed, 0.2f * TimeScaleFactor),
				Mathf.SmoothDampAngle(operatingTiltRange.Max, targetRange.Max, ref tiltHighSpeed, 0.2f * TimeScaleFactor));

			CurrentTiltAngle = operatingTiltRange.Lerp(tiltFactor);
		}

		private void MonitorFieldOfView()
		{
			if (!(settings is { UseDynamicFieldOfView: true }))
			{
				return;
			}
			
			float t = operatingHeightRange.InverseLerp(CurrentHeight);
			float target = settings.DynamicFieldOfViewRange.Lerp(settings.EvaluateFieldOfViewTransition(t));
			cachedCamera.fieldOfView = Mathf.SmoothDamp(cachedCamera.fieldOfView, target, ref fovSpeed, 0.2f * TimeScaleFactor);
		}
		
		private void MoveToTarget()
		{
			Ray direction = cachedCamera.ScreenPointToRay(InputProvider.MousePosition);
			if (!Physics.Raycast(direction, out RaycastHit hitInfo, float.MaxValue, settings.InteractionMask, QueryTriggerInteraction.Ignore))
			{
				onMoveToTargetFailed.InvokeIfNotNull();
				return;
			}

			Vector3 targetPosition = hitInfo.point;
			MoveToTarget(targetPosition);
		}
		
		private IEnumerator RoutineMoveToTarget(Vector3 targetPosition)
		{
			Vector3 originalPosition = cachedTransform.position;
			float moveToTargetTime = settings.MoveToTargetSmoothingTime;
			float time = 0f;
			
			while (time < moveToTargetTime)
			{
				float t = Mathf.SmoothStep(0f, 1f, time / moveToTargetTime);

				targetPosition.y = CurrentHeight;
				originalPosition.y = CurrentHeight;
				
				characterController.Move(Vector3.Lerp(originalPosition, targetPosition, t) - cachedTransform.position);
				time += DeltaTime;
				
				yield return null;
			}

			targetPosition.y = CurrentHeight;
			characterController.Move(targetPosition - cachedTransform.position);
			moveToPositionHandle = null;
		}
	}
}

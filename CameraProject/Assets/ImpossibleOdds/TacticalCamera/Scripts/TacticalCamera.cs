namespace ImpossibleOdds.TacticalCamera
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	using ValueRange = TacticalCameraSettings.ValueRange;

	public class TacticalCamera : MonoBehaviour
	{
		[SerializeField, Tooltip("Settings that define the camera's behaviour.")]
		private TacticalCameraSettings settings;
		[SerializeField, Tooltip("Input provider.")]
		private ITacticalCameraInputProvider inputProvider;
		[SerializeField, Tooltip("Forces the camera to remain within a certain area.")]
		private ITacticalCameraBounds bounds;

		private ValueRange heightRange = new ValueRange();
		private ValueRange pitchRange = new ValueRange();
		private float pitchFactor = 0f;

		private bool isOrbiting = false;
		private Vector3 orbitPoint = Vector3.zero;

		private Coroutine moveToPositionHandle = null;

		private RolloffState moveForwardsState = new RolloffState();
		private RolloffState moveSidewaysState = new RolloffState();
		private RolloffState moveUpwardsState = new RolloffState();
		private RolloffState pivotPitchState = new RolloffState();
		private RolloffState pivotYawState = new RolloffState();
		private RolloffState orbitPitchState = new RolloffState();
		private RolloffState orbitYawState = new RolloffState();
		private List<RolloffState> rolloffStates = new List<RolloffState>();

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

				float speedT = 1f - Mathf.Clamp01(Mathf.InverseLerp(settings.HeightRange.Min, settings.HeightRange.Max, transform.position.y));
				return settings.MovementSpeedTransition.Evaluate(speedT);
			}
		}

		private void Awake()
		{
			ApplySettings(settings);
			rolloffStates = new List<RolloffState>()
			{
				moveForwardsState,
				moveSidewaysState,
				moveUpwardsState,
				pivotPitchState,
				pivotYawState,
				orbitPitchState,
				orbitYawState,
			};
		}

		private void ApplySettings(TacticalCameraSettings settings)
		{
			this.settings = settings;

			if (settings == null)
			{
				return;
			}

			moveForwardsState.ApplySettings(settings.MovementRolloffTime, settings.MovementRolloff);
			moveSidewaysState.ApplySettings(settings.MovementRolloffTime, settings.MovementRolloff);
			moveUpwardsState.ApplySettings(settings.MovementRolloffTime, settings.MovementRolloff);
			pivotPitchState.ApplySettings(settings.PivotalRolloffTime, settings.PivotalRolloff);
			pivotYawState.ApplySettings(settings.PivotalRolloffTime, settings.PivotalRolloff);
			orbitPitchState.ApplySettings(settings.OrbitalRolloffTime, settings.OrbitalRolloff);
			orbitYawState.ApplySettings(settings.OrbitalRolloffTime, settings.OrbitalRolloff);
		}

		private void OnEnable()
		{
			if ((inputProvider == null) || (settings == null))
			{
				return;
			}

			UpdateHeightRange();
			UpdatePitchFactor();
		}

		private void OnDisable()
		{
			// By setting the time to 0, we basically reset them.
			rolloffStates.ForEach(r => r.Time = 0f);

			if (moveToPositionHandle != null)
			{
				StopCoroutine(moveToPositionHandle);
				moveToPositionHandle = null;
			}
		}

		private void LateUpdate()
		{
			if ((inputProvider == null) || (settings == null))
			{
				return;
			}

			UpdateStates();
			UpdateMovement();
			UpdatePitchRange();
			UpdateRotation();
			UpdateHeightRange();
			ApplyPitchFactor();
			ApplyHeightRestriction();

			if (bounds != null)
			{
				bounds.Apply();
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

			// Rotating - Pivoting versus orbiting
			if (inputProvider.PreferOrbitOverPivot)
			{
				if (!isOrbiting)
				{
					RaycastHit hitInfo;
					if (Physics.Raycast(transform.position, transform.forward, out hitInfo, settings.MaxInteractionRayLength, settings.InteractionMask, QueryTriggerInteraction.Ignore))
					{
						isOrbiting = true;
						orbitPoint = hitInfo.point;
					}
				}

				if (isOrbiting)
				{
					UpdateState(inputProvider.PitchDelta, orbitPitchState);
					UpdateState(inputProvider.YawDelta, orbitYawState);
				}

				pivotPitchState.Update();
				pivotYawState.Update();
			}
			else
			{
				UpdateState(inputProvider.PitchDelta, pivotPitchState);
				UpdateState(inputProvider.YawDelta, pivotYawState);
				orbitPitchState.Update();
				orbitYawState.Update();
				isOrbiting = false;
			}
		}

		private void UpdateState(float inputValue, RolloffState state)
		{
			if (Mathf.Approximately(inputValue, 0f))
			{
				state.Update();
			}
			else
			{
				// If the outcome is not 0, then they are of opposite sign
				// and we substitute the new value, or we check whether
				// the new input is stronger than the current rolling value.
				float value = state.Value;
				if (((Math.Sign(inputValue) + Math.Sign(value)) != 0) || (Mathf.Abs(inputValue) > Mathf.Abs(value)))
				{
					state.RolloffValue = inputValue;
				}
				else
				{
					state.Update();
				}
			}
		}

		private void UpdateMovement()
		{
			if (inputProvider.MoveToFocusPoint)
			{
				MoveToFocusPoint();
			}

			Vector3 movement = Vector3.zero;

			// If the camera isn't moving towards its focus point,
			// then we apply the forward and sideways movement
			if (moveToPositionHandle == null)
			{
				// TODO: an edge-case occurs when the camera is facing 90 degrees down/up. Fix this?
				Vector3 projectedForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
				Vector3 projectedRight = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;

				movement += (projectedForward * moveForwardsState.Value) + (projectedRight * moveSidewaysState.Value);
			}

			movement.y = moveUpwardsState.Value;    // Zooming in/out. Always in world-space.

			// If we are zooming in/out, we keep in mind the current pitch factor.
			// which keeps the camera's pitch around the same level
			if (!Mathf.Approximately(movement.y, 0f))
			{
				ApplyPitchFactor();
			}

			movement = Vector3.ClampMagnitude(movement, MaxMovementSpeed); // Clamp to max speed
			movement *= Time.deltaTime; // Timescaling
			transform.Translate(movement, Space.World);
		}

		private void UpdateRotation()
		{
			// Pivot - Pitch - Rotate around the local right vector
			if (!Mathf.Approximately(0f, pivotPitchState.Value))
			{
				transform.Rotate(Vector3.right, pivotPitchState.Value * Time.deltaTime, Space.Self);
				UpdatePitchFactor();    // We update this one only because pitch is changing
			}

			// Pivot - Yaw - Rotate around the world up vector
			if (!Mathf.Approximately(0f, pivotYawState.Value))
			{
				transform.Rotate(Vector3.up, pivotYawState.Value * Time.deltaTime, Space.World);
			}

			// Orbit - Pitch - Rotate around the local right vector at the focus point
			if (!Mathf.Approximately(0f, orbitPitchState.Value))
			{
				transform.RotateAround(orbitPoint, transform.right, orbitPitchState.Value * Time.deltaTime);
			}

			// Orbit - Yaw - Rotate around the world up vector at the focus point
			if (!Mathf.Approximately(0f, orbitYawState.Value))
			{
				transform.RotateAround(orbitPoint, Vector3.up, orbitYawState.Value * Time.deltaTime);
			}
		}

		private void UpdatePitchRange()
		{
			float t = Mathf.InverseLerp(heightRange.Min, heightRange.Max, transform.position.y);
			t = Mathf.Clamp01(t);
			t = settings.PitchRangeTransition.Evaluate(t);
			pitchRange = ValueRange.Lerp(settings.PitchRangeHigh, settings.PitchRangeLow, t);
		}

		private void UpdatePitchFactor()
		{
			// Restrict the camera's tilt (x-value). When the camera's tilt
			// reaches the horizon (0 degrees), it will wrap around back to 360.
			// To counter this, we will do a -360 degrees offset.
			float pitch = transform.localEulerAngles.x;
			pitch = (pitch > 180f) ? (pitch - 360f) : pitch;
			pitch = Mathf.Clamp(pitch, pitchRange.Min, pitchRange.Max);
			pitchFactor = Mathf.InverseLerp(pitchRange.Min, pitchRange.Max, pitch);
		}

		private void UpdateHeightRange()
		{
			RaycastHit hit;
			if (Physics.Raycast(transform.position, Vector3.down, out hit, settings.MaxInteractionRayLength, settings.InteractionMask))
			{
				// If we're not over the same height minimum anymore,
				// we update the pitch range and factor, as we get a hitch otherwise.
				if (!Mathf.Approximately(heightRange.Min, hit.point.y + settings.HeightRange.Min))
				{
					heightRange.Set(hit.point.y + settings.HeightRange.Min, settings.HeightRange.Max);
					UpdatePitchRange();
					UpdatePitchFactor();
				}
				else
				{
					heightRange.Set(hit.point.y + settings.HeightRange.Min, settings.HeightRange.Max);
				}
			}
			else
			{
				heightRange.Set(settings.HeightRange.Min, settings.HeightRange.Max);
			}
		}

		private void ApplyPitchFactor()
		{
			Vector3 localAngles = transform.localEulerAngles;
			localAngles.x = Mathf.Lerp(pitchRange.Min, pitchRange.Max, pitchFactor);
			transform.localEulerAngles = localAngles;
		}

		private void ApplyHeightRestriction()
		{
			Vector3 position = transform.position;
			position.y = Mathf.Clamp(position.y, heightRange.Min, heightRange.Max);
			transform.position = position;
		}

		private void MoveToFocusPoint()
		{
			if (moveToPositionHandle != null)
			{
				StopCoroutine(moveToPositionHandle);
				moveToPositionHandle = null;
			}

			RaycastHit hitInfo;
			Ray direction = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (!Physics.Raycast(direction, out hitInfo, settings.MaxInteractionRayLength, settings.InteractionMask, QueryTriggerInteraction.Ignore))
			{
				return;
			}

			Vector3 targetPosition = hitInfo.point;
			moveToPositionHandle = StartCoroutine(RoutineMoveToPosition(targetPosition));
		}

		private IEnumerator RoutineMoveToPosition(Vector3 endPosition)
		{
			Vector3 velocity = Vector3.zero;
			do
			{
				endPosition.y = transform.position.y;
				transform.position = Vector3.SmoothDamp(transform.position, endPosition, ref velocity, settings.MoveToTargetSmoothingTime, MaxMovementSpeed);
				yield return null;

			} while (velocity.sqrMagnitude > 0.001f);

			moveToPositionHandle = null;
		}

		/// <summary>
		/// Class to keep track of the current rolloff state of a value. It is controlled by time and a curve.
		/// </summary>
		private class RolloffState
		{
			private float time;
			private float rolloffTime;
			private float rolloffValue;
			private AnimationCurve rolloffCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

			public float Time
			{
				get { return time; }
				set { time = value; }
			}

			public float Value
			{
				get
				{
					float t = Mathf.InverseLerp(rolloffTime, 0f, time);
					return rolloffCurve.Evaluate(t) * rolloffValue;
				}
			}

			public float RolloffTime
			{
				get { return rolloffTime; }
				set
				{
					if (value <= 0f)
					{
						throw new System.ArgumentOutOfRangeException("The rolloff time should be a value larger than 0.");
					}

					rolloffTime = value;
				}
			}

			public float RolloffValue
			{
				get { return rolloffValue; }
				set
				{
					rolloffValue = value;
					time = rolloffTime;
				}
			}

			public AnimationCurve RolloffCurve
			{
				get { return rolloffCurve; }
				set { rolloffCurve = value; }
			}

			public float Update()
			{
				if (time == 0f)
				{
					return 0f;
				}

				time = Mathf.Clamp(time - UnityEngine.Time.deltaTime, 0f, rolloffTime);
				return Value;
			}

			public void ApplySettings(float rolloffTime, AnimationCurve rolloffCurve)
			{
				this.rolloffTime = rolloffTime;
				this.rolloffCurve = rolloffCurve;
				time = 0f;
				rolloffValue = 0f;
			}
		}
	}
}

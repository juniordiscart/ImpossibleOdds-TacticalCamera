namespace ImpossibleOdds.TacticalCamera
{
	using System.Collections;
	using UnityEngine;

	/// <summary>
	/// A sample input implementation to provide input for the TacticalCamera.
	/// The best approach is to implement the input provider interface in your own input manager, and use this class as a sample implementation of expected inputs.
	/// </summary>
	public class TacticalCameraInputProvider : AbstractTacticalCameraInputProvider, ITacticalCameraInputProvider
	{
		public const string DefaultMouseRotationAxis = "Mouse X";
		public const string DefaultMouseTiltAxis = "Mouse Y";

		[Header("Movement")]
		[SerializeField, Tooltip("Key to move the camera forwards.")]
		private KeyCode moveForwardKey = KeyCode.W;
		[SerializeField, Tooltip("Key to move the camera backwards")]
		private KeyCode moveBackwardKey = KeyCode.S;
		[SerializeField, Tooltip("Key to move the camera to the left.")]
		private KeyCode moveLeftKey = KeyCode.A;
		[SerializeField, Tooltip("Key to move the camera to the right.")]
		private KeyCode moveRightKey = KeyCode.D;
		[SerializeField, Tooltip("Key to orbit the camera around it's focus target.")]
		private KeyCode orbitCamera = KeyCode.LeftShift;
		[SerializeField, Range(0f, 0.5f), Tooltip("Screen edge detection for moving the camera. Expressed in percentage of the screen.")]
		private float screenBorderTrigger = 0f;
		[SerializeField, Tooltip("Should movement be triggered by an off-screen mouse cursor?")]
		private bool triggerOffScreen = false;
		[SerializeField, Min(0f), Tooltip("Value to multiply the scroll value with.")]
		private float scrollSensitivityFactor = 100f;

		[Header("Rotation")]
		[SerializeField, Range(0, 1), Min(0), Tooltip("The mouse button that's used to move the camera to a position.")]
		private int mouseMoveToPositionKey = 0;
		[SerializeField, Range(0, 1), Min(0), Tooltip("The mouse button that's used to rotate the camera.")]
		private int mouseRotationKey = 1;
		[SerializeField, Tooltip("The mouse axis that is used for tilting the camera up/down.")]
		private string mouseTiltAxis = DefaultMouseTiltAxis;
		[SerializeField, Tooltip("The mouse axis that is used for rotating the camera in the XZ-plane.")]
		private string mouseRotationAxis = DefaultMouseRotationAxis;

		[Header("Rotation Modifiers")]
		[SerializeField, Tooltip("Invert the tilt input value.")]
		private bool invertTilt = true;
		[SerializeField, Tooltip("Invert the rotation input value.")]
		private bool invertRotation = false;
		[SerializeField, Tooltip("Invert the zoom value.")]
		private bool invertZoom = true;
		[SerializeField, Tooltip("Is the camera always rotating? If enabled, the camera will lock & hide the cursor permanently")]
		private bool alwaysRotating = false;

		private Event latestEvent = null;
		private bool rotationEnabled = false;

		/// <inheritdoc />
		public override bool MoveToTarget
		{
			get
			{
				return
					(latestEvent != null) &&
					latestEvent.isMouse &&
					(latestEvent.type == EventType.MouseDown) &&
					(latestEvent.button == mouseMoveToPositionKey) &&
					(latestEvent.clickCount == 2);
			}
		}

		/// <inheritdoc/>
		public override bool CancelMoveToTarget
		{
			get
			{
				return !Mathf.Approximately(Mathf.Abs(MoveForward), 0f) || !Mathf.Approximately(Mathf.Abs(MoveSideways), 0f) || Input.GetKeyDown(KeyCode.Escape);
			}
		}

		/// <inheritdoc />
		public override bool OrbitAroundTarget
		{
			get { return Input.GetKey(orbitCamera); }
		}

		/// <inheritdoc />
		public override float MoveForward
		{
			get
			{
				float value = 0f;

				// Keyboard
				if (Input.GetKey(moveForwardKey))
				{
					value += 1f;
				}

				if (Input.GetKey(moveBackwardKey))
				{
					value -= 1f;
				}

				// Mouse
				float mouseY = Input.mousePosition.y;

				if ((mouseY >= (Screen.height * (1f - screenBorderTrigger))) &&
					(triggerOffScreen || (mouseY <= Screen.height)))
				{
					value += 1f;
				}

				if ((mouseY <= (Screen.height * screenBorderTrigger)) &&
					(triggerOffScreen || (mouseY >= 0f)))
				{
					value -= 1f;
				}

				value = Mathf.Clamp(value, -1f, 1f);
				return value;
			}
		}

		/// <inheritdoc />
		public override float MoveSideways
		{
			get
			{
				float value = 0f;

				// Keyboard
				if (Input.GetKey(moveLeftKey))
				{
					value -= 1f;
				}

				if (Input.GetKey(moveRightKey))
				{
					value += 1f;
				}

				// Mouse
				float mouseX = Input.mousePosition.x;

				if ((mouseX <= (Screen.width * screenBorderTrigger)) &&
					(triggerOffScreen || (mouseX >= 0f)))
				{
					value -= 1f;
				}

				if ((mouseX >= (Screen.width * (1f - screenBorderTrigger))) &&
					(triggerOffScreen || (mouseX <= Screen.width)))
				{
					value += 1f;
				}

				value = Mathf.Clamp(value, -1f, 1f);
				return value;
			}
		}

		/// <inheritdoc />
		public override float MoveUp
		{
			get
			{
				float value = Input.mouseScrollDelta.y * scrollSensitivityFactor;

				if (invertZoom)
				{
					value *= -1f;
				}

				return value;
			}
		}

		/// <inheritdoc />
		public override float TiltDelta
		{
			get
			{
				float value = 0f;

				if (rotationEnabled)
				{
					value = Mathf.Clamp(Input.GetAxis(mouseTiltAxis), -1f, 1f);
				}

				if (invertTilt)
				{
					value *= -1f;
				}

				return value;
			}
		}

		/// <inheritdoc />
		public override float RotationDelta
		{
			get
			{
				float value = 0f;

				if (rotationEnabled)
				{
					value = Mathf.Clamp(Input.GetAxis(mouseRotationAxis), -1f, 1f);
				}

				if (invertRotation)
				{
					value *= -1f;
				}

				return value;
			}
		}

		private void OnEnable()
		{
			if (alwaysRotating)
			{
				Cursor.visible = false;
				Cursor.lockState = CursorLockMode.Locked;
				rotationEnabled = true;
			}
		}

		private void OnGUI()
		{
			latestEvent = Event.current;
			CheckMouseCursor();
		}

		private void CheckMouseCursor()
		{
			if (alwaysRotating || (latestEvent == null) || !latestEvent.isMouse)
			{
				return;
			}

			if ((latestEvent.type == EventType.MouseDown) && (latestEvent.button == mouseRotationKey))
			{
				Cursor.visible = false;
				Cursor.lockState = CursorLockMode.Locked;
				rotationEnabled = false;
				StartCoroutine(RoutineWaitForDrag());   // Prevent spike in mouse input
			}
			else if ((latestEvent.type == EventType.MouseUp) && (latestEvent.button == mouseRotationKey))
			{
				Cursor.visible = true;
				Cursor.lockState = CursorLockMode.None;
				rotationEnabled = false;
			}
		}

		private IEnumerator RoutineWaitForDrag()
		{
			yield return new WaitWhile(() =>
				(Cursor.lockState == CursorLockMode.Locked) &&
				((Mathf.Abs(Input.GetAxis(mouseTiltAxis)) > 0f) || (Mathf.Abs(Input.GetAxis(mouseRotationAxis)) > 0f)));

			yield return null;
			rotationEnabled = true;
		}
	}
}

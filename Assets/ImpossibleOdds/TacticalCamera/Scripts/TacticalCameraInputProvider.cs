namespace ImpossibleOdds.TacticalCamera
{
	using UnityEngine;

	/// <summary>
	/// A sample input implementation to provide input for the TacticalCamera.
	/// The best approach is to implement the input provider interface in your own input manager, and use this class as a sample implementation of expected inputs.
	/// </summary>
	[RequireComponent(typeof(TacticalCamera))]
	public class TacticalCameraInputProvider : MonoBehaviour, ITacticalCameraInputProvider
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
		[SerializeField, Range(0f, 1f), Min(0f), Tooltip("Time interval between mouse clicks to register it as a double-click.")]
		private float doubleClickTime = 0.1f;
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
		[SerializeField, Tooltip("Should the cursor be hidden when rotating the camera.")]
		private bool hideCursorWhenRotating = true;

		private TacticalCamera tacticalCamera = null;
		private TacticalCameraSettings settings = null;
		private int lastCheckFrameCounter = 0;
		private float firstClickTime = 0f;
		private bool singleClicked = false;
		private bool doubleClicked = false;

		/// <inheritdoc />
		public bool MoveToTarget
		{
			get
			{
				CheckDoubleClick();
				return doubleClicked;
			}
		}

		/// <inheritdoc/>
		public bool CancelMoveToTarget
		{
			get
			{
				return !Mathf.Approximately(Mathf.Abs(MoveForward), 0f) || !Mathf.Approximately(Mathf.Abs(MoveSideways), 0f) || Input.GetKeyDown(KeyCode.Escape);
			}
		}

		/// <inheritdoc />
		public bool OrbitAroundTarget
		{
			get { return Input.GetKey(orbitCamera); }
		}

		/// <inheritdoc />
		public float MoveForward
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

				if (settings != null)
				{
					value *= tacticalCamera.MaxMovementSpeed;
				}

				return value;
			}
		}

		/// <inheritdoc />
		public float MoveSideways
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

				if (settings != null)
				{
					value *= tacticalCamera.MaxMovementSpeed;
				}

				return value;
			}
		}

		/// <inheritdoc />
		public float MoveUp
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
		public float TiltDelta
		{
			get
			{
				float value = 0f;

				if (Input.GetMouseButton(mouseRotationKey))
				{
					value = Input.GetAxis(mouseTiltAxis);
				}

				if (invertTilt)
				{
					value *= -1f;
				}

				if (settings != null)
				{
					value *= settings.MaxRotationalSpeed;
				}

				return value;
			}
		}

		/// <inheritdoc />
		public float RotationDelta
		{
			get
			{
				float value = 0f;

				if (Input.GetMouseButton(mouseRotationKey))
				{
					value = Input.GetAxis(mouseRotationAxis);
				}

				if (invertRotation)
				{
					value *= -1f;
				}

				if (settings != null)
				{
					value *= settings.MaxRotationalSpeed;
				}

				return value;
			}
		}

		private void Start()
		{
			tacticalCamera = GetComponent<TacticalCamera>();
			tacticalCamera.InputProvider = this;
			settings = tacticalCamera.Settings;
		}

		private void Update()
		{
			CheckDoubleClick();
			CheckMouseCursor();
		}

		private void CheckMouseCursor()
		{
			if (!hideCursorWhenRotating)
			{
				return;
			}

			if (Input.GetMouseButtonDown(mouseRotationKey))
			{
				Cursor.visible = false;
				Cursor.lockState = CursorLockMode.Locked;
			}
			else if (Input.GetMouseButtonUp(mouseRotationKey))
			{
				Cursor.visible = true;
				Cursor.lockState = CursorLockMode.None;
			}
		}

		private void CheckDoubleClick()
		{
			if (lastCheckFrameCounter == Time.frameCount)
			{
				return;
			}

			lastCheckFrameCounter = Time.frameCount;

			// If the button has not been pressed
			if (!Input.GetMouseButtonDown(mouseMoveToPositionKey))
			{
				// If we single clicked, but the double click time has already expired, we reset
				if (singleClicked && !ClickedWithinTimeLimit())
				{
					singleClicked = false;
					firstClickTime = 0f;
				}

				if (doubleClicked)
				{
					doubleClicked = false;
				}
			}
			else
			{
				// If we haven't clicked a first time, or not clicked within the time limit, register it as a first click.
				// Else, we've registered a double click.
				if (!singleClicked || !ClickedWithinTimeLimit())
				{
					singleClicked = true;
					firstClickTime = Time.timeSinceLevelLoad;
					doubleClicked = false;
				}
				else
				{
					doubleClicked = true;
				}
			}
		}

		private bool ClickedWithinTimeLimit()
		{
			return (firstClickTime + doubleClickTime) > Time.timeSinceLevelLoad;
		}
	}
}

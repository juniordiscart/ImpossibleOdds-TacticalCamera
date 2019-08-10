/// <summary>
/// A sample input implementation to provide input for the TacticalCamera.
/// The best approach is to implement the input provider interface in your own input manager, and use this class as a sample implementation of expected inputs.
/// </summary>
namespace ImpossibleOdds.TacticalCamera
{
	using UnityEngine;

	[RequireComponent(typeof(TacticalCamera))]
	public class TacticalCameraInputProvider : MonoBehaviour, ITacticalCameraInputProvider
	{
		public const string DefaultMouseYawAxis = "Mouse X";
		public const string DefaultMousePitchAxis = "Mouse Y";

		[SerializeField, Tooltip("Key to move the camera forwards.")]
		private KeyCode moveForwardKey = KeyCode.W;
		[SerializeField, Tooltip("Key to move the camera backwards")]
		private KeyCode moveBackwardKey = KeyCode.S;
		[SerializeField, Tooltip("Key to move the camera to the left.")]
		private KeyCode moveLeftKey = KeyCode.A;
		[SerializeField, Tooltip("Key to move the camera to the right.")]
		private KeyCode moveRightKey = KeyCode.D;
		[SerializeField, Tooltip("Key to switch between pivoting the camera versus orbiting around a focus point.")]
		private KeyCode orbitModifierKey = KeyCode.LeftAlt;
		[SerializeField, Range(0, 1), Min(0), Tooltip("The mouse button that's used to move the camera to a position.")]
		private int mouseMoveToPositionKey = 0;
		[SerializeField, Range(0, 1), Min(0), Tooltip("The mouse button that's used to rotate the camera.")]
		private int mouseRotationKey = 1;
		[SerializeField, Tooltip("The mouse axis that is used for the pitch value during any form of rotation.")]
		private string mousePitchRotationAxis = DefaultMousePitchAxis;
		[SerializeField, Tooltip("The mouse axis that is used for the yaw value during any form of rotation.")]
		private string mouseYawRotationAxis = DefaultMouseYawAxis;
		[SerializeField, Range(0f, 1f), Min(0f), Tooltip("Time interval between mouse clicks to register it as a double-click.")]
		private float doubleClickTime = 0.1f;

		[SerializeField, Tooltip("Invert the pitch input value.")]
		private bool invertPitch = true;
		[SerializeField, Tooltip("Invert the yaw input value.")]
		private bool invertYaw = false;
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

		/// <summary>
		/// Should the camera move towards its focus point.
		/// </summary>
		/// <value></value>
		public bool MoveToFocusPoint
		{
			get
			{
				CheckDoubleClick();
				return doubleClicked;
			}
		}

		/// <summary>
		/// Should the camera move forward or backwards.
		/// </summary>
		/// <value>Positive value to move forward, negative to move backwards.</value>
		public float MoveForward
		{
			get
			{
				float value = 0f;

				if (Input.GetKey(moveForwardKey) || (Input.mousePosition.y >= Screen.height))
				{
					value += 1f;
				}

				if (Input.GetKey(moveBackwardKey) || (Input.mousePosition.y <= 0))
				{
					value -= 1f;
				}

				if (settings != null)
				{
					value *= tacticalCamera.MaxMovementSpeed;
				}

				return value;
			}
		}

		/// <summary>
		/// Should the camera move left or right.
		/// </summary>
		/// <value>Positive value to move right, negative to move left.</value>
		public float MoveSideways
		{
			get
			{
				float value = 0f;

				if (Input.GetKey(moveLeftKey) || (Input.mousePosition.x <= 0))
				{
					value -= 1f;
				}

				if (Input.GetKey(moveRightKey) || (Input.mousePosition.x >= Screen.width))
				{
					value += 1f;
				}

				if (settings != null)
				{
					value *= tacticalCamera.MaxMovementSpeed;
				}

				return value;
			}
		}

		/// <summary>
		/// Should the camera move up or down.
		/// </summary>
		/// <value>Positive value to move up, negative to move down.</value>
		public float MoveUp
		{
			get
			{
				float value = Input.mouseScrollDelta.y;

#if UNITY_STANDALONE_WIN
				value *= 100f;
#endif

				if (invertZoom)
				{
					value *= -1f;
				}

				return value;
			}
		}

		/// <summary>
		/// Should the camera orbit around its focus point instead of pivoting around its origin when requested to rotate.
		/// </summary>
		/// <value>When true, the camera will orbit around its focus point.</value>
		public bool PreferOrbitOverPivot
		{
			get { return Input.GetKey(orbitModifierKey); }
		}

		/// <summary>
		/// Should the camera pitch around its pivot or focus point.
		/// </summary>
		/// <value>Positive value to pitch up, negative to pitch down.</value>
		public float PitchDelta
		{
			get
			{
				float value = 0f;

				if (Input.GetMouseButton(mouseRotationKey))
				{
					value = Input.GetAxis(mousePitchRotationAxis);
				}

				if (invertPitch)
				{
					value *= -1f;
				}

				if (settings != null)
				{
					// Pitch orbiting is a pretty counter-intuitive thing, so setting the value to 0 prevents orbital pitch.
					value *= PreferOrbitOverPivot ? 0f /* settings.MaxOrbitalSpeed */ : settings.MaxPivotalSpeed;
				}

				return value;
			}
		}

		/// <summary>
		/// Should the camera yaw around its pivot or focus point.
		/// </summary>
		/// <value>Positive value to yaw right, negative value to yaw left.</value>
		public float YawDelta
		{
			get
			{
				float value = 0f;

				if (Input.GetMouseButton(mouseRotationKey))
				{
					value = Input.GetAxis(mouseYawRotationAxis);
				}

				if (invertYaw)
				{
					value *= -1f;
				}

				if (settings != null)
				{
					value *= PreferOrbitOverPivot ? settings.MaxOrbitalSpeed : settings.MaxPivotalSpeed;
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

			bool isRotating = Input.GetMouseButton(mouseRotationKey);
			if (Cursor.visible && isRotating)
			{
				Cursor.visible = false;
				Cursor.lockState = CursorLockMode.Locked;
			}
			else if (!Cursor.visible && !isRotating)
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

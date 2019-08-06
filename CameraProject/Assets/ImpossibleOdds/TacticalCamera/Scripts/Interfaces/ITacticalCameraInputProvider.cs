namespace ImpossibleOdds.TacticalCamera
{
	/// <summary>
	/// Provides the necessary inputs to control the camera.
	/// </summary>
	public interface ITacticalCameraInputProvider
	{
		/// <summary>
		/// Should the camera move towards its focus point.
		/// </summary>
		/// <value></value>
		bool MoveToFocusPoint
		{
			get;
		}

		/// <summary>
		/// Should the camera move forward or backwards.
		/// </summary>
		/// <value>Positive value to move forward, negative to move backwards.</value>
		float MoveForward
		{
			get;
		}

		/// <summary>
		/// Should the camera move left or right.
		/// </summary>
		/// <value>Positive value to move right, negative to move left.</value>
		float MoveSideways
		{
			get;
		}

		/// <summary>
		/// Should the camera move up or down.
		/// </summary>
		/// <value>Positive value to move up, negative to move down.</value>
		float MoveUp
		{
			get;
		}

		/// <summary>
		/// Should the camera orbit around its focus point instead of pivoting around its origin when requested to rotate.
		/// </summary>
		/// <value>When true, the camera will orbit around its focus point.</value>
		bool PreferOrbitOverPivot
		{
			get;
		}

		/// <summary>
		/// Should the camera pitch around its pivot or focus point.
		/// </summary>
		/// <value>Positive value to pitch up, negative to pitch down.</value>
		float PitchDelta
		{
			get;
		}

		/// <summary>
		/// Should the camera yaw around its pivot or focus point.
		/// </summary>
		/// <value>Positive value to yaw right, negative value to yaw left.</value>
		float YawDelta
		{
			get;
		}
	}
}

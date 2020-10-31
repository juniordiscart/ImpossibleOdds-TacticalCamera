namespace ImpossibleOdds.TacticalCamera
{
	/// <summary>
	/// Provides the necessary inputs to control the camera.
	/// </summary>
	public interface ITacticalCameraInputProvider
	{
		/// <summary>
		/// Should the camera move towards its target.
		/// </summary>
		/// <value>True if the camera should move to the target under the mouse cursor.</value>
		bool MoveToTarget
		{
			get;
		}

		/// <summary>
		/// Cancel the move-to-target animation.
		/// </summary>
		/// <value>True if the tactical camera should any ongoing move-to animations.</value>
		bool CancelMoveToTarget
		{
			get;
		}

		/// <summary>
		/// Prefer orbiting around the focus target rather than rotating around it's current location.
		/// </summary>
		/// <value>True if orbiting is preferred.</value>
		bool OrbitAroundTarget
		{
			get;
		}

		/// <summary>
		/// Should the camera move forward or backwards.
		/// Expected value range is [-1, 1].
		/// </summary>
		/// <value>Positive value to move forward, negative to move backwards.</value>
		float MoveForward
		{
			get;
		}

		/// <summary>
		/// Should the camera move left or right.
		/// Expected value range is [-1, 1].
		/// </summary>
		/// <value>Positive value to move right, negative to move left.</value>
		float MoveSideways
		{
			get;
		}

		/// <summary>
		/// Should the camera move up or down.
		/// Expected value range is [-1, 1].
		/// </summary>
		/// <value>Positive value to move up, negative to move down.</value>
		float MoveUp
		{
			get;
		}

		/// <summary>
		/// Should the camera tilt around its pivot.
		/// Expected value range is [-1, 1].
		/// </summary>
		/// <value>Positive value to tilt up, negative to tilt down.</value>
		float TiltDelta
		{
			get;
		}

		/// <summary>
		/// Should the camera rotate around its pivot.
		/// Expected value range is [-1, 1].
		/// </summary>
		/// <value>Positive value to rotate right, negative value to rotate left.</value>
		float RotationDelta
		{
			get;
		}
	}
}

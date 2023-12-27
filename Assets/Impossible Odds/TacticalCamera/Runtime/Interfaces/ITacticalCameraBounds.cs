namespace ImpossibleOdds.TacticalCamera
{
	/// <summary>
	/// Interface to restrict the camera's position.
	/// </summary>
	public interface ITacticalCameraBounds
	{
		/// <summary>
		/// Apply the bounds to the camera.
		/// </summary>
		/// <param name="tCamera">The camera unto which the bounds should be applied.</param>
		void Apply(TacticalCamera tCamera);
	}
}

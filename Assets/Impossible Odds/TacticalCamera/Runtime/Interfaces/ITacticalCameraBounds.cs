using UnityEngine;
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

		/// <summary>
		/// Checks whether the provided position is within the bounds. If not, the position is moved to the closest valid location within.
		/// </summary>
		/// <param name="position">The position that will be checked by the bounds.</param>
		/// <returns>A valid position within the bounds.</returns>
		Vector3 Apply(Vector3 position);

		/// <summary>
		/// Checks whether the provided position is within bounds.
		/// </summary>
		/// <param name="position">The position to check for.</param>
		/// <returns>True if the provided position is within bounds, false otherwise.</returns>
		bool IsWithinBounds(Vector3 position);
	}
}

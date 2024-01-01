using UnityEngine;

namespace ImpossibleOdds.TacticalCamera
{
	public abstract class AbstractTacticalCameraBounds : MonoBehaviour, ITacticalCameraBounds
	{
		/// <inheritdoc />
		public abstract void Apply(TacticalCamera tCamera);

		/// <inheritdoc />
		public abstract Vector3 Apply(Vector3 position);

		/// <inheritdoc />
		public abstract bool IsWithinBounds(Vector3 position);
	}
}

using UnityEngine;

namespace ImpossibleOdds.TacticalCamera
{
	public abstract class AbstractTacticalCameraBounds : MonoBehaviour, ITacticalCameraBounds
	{
		/// <inheritdoc />
		public abstract void Apply(TacticalCamera tCamera);
	}
}

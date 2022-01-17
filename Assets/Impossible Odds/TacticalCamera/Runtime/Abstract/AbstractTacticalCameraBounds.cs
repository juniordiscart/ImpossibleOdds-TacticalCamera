namespace ImpossibleOdds.TacticalCamera
{
	using UnityEngine;

	public abstract class AbstractTacticalCameraBounds : MonoBehaviour, ITacticalCameraBounds
	{
		public abstract void Apply(TacticalCamera tCamera);
	}
}

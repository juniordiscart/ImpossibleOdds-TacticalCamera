namespace ImpossibleOdds.TacticalCamera
{
	using UnityEngine;

	public abstract class AbstractTacticalCameraInputProvider : MonoBehaviour, ITacticalCameraInputProvider
	{
		public abstract bool MoveToTarget { get; }
		public abstract bool CancelMoveToTarget { get; }
		public abstract bool OrbitAroundTarget { get; }
		public abstract float MoveForward { get; }
		public abstract float MoveSideways { get; }
		public abstract float MoveUp { get; }
		public abstract float TiltDelta { get; }
		public abstract float RotationDelta { get; }
	}
}

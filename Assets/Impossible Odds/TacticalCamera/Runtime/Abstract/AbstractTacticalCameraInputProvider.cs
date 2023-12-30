using UnityEngine;

namespace ImpossibleOdds.TacticalCamera
{
	public abstract class AbstractTacticalCameraInputProvider : MonoBehaviour, ITacticalCameraInputProvider
	{
		/// <inheritdoc />
		public abstract bool MoveToTarget { get; }
		/// <inheritdoc />
		public abstract bool CancelMoveToTarget { get; }
		/// <inheritdoc />
		public abstract bool OrbitAroundTarget { get; }
		/// <inheritdoc />
		public abstract float MoveForward { get; }
		/// <inheritdoc />
		public abstract float MoveSideways { get; }
		/// <inheritdoc />
		public abstract float MoveUp { get; }
		/// <inheritdoc />
		public abstract float TiltDelta { get; }
		/// <inheritdoc />
		public abstract float RotationDelta { get; }
		/// <inheritdoc />
		public virtual Vector2 MousePosition => Input.mousePosition;
	}
}

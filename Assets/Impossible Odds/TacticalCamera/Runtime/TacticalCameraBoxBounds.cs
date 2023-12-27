using UnityEngine;

namespace ImpossibleOdds.TacticalCamera
{
	/// <summary>
	/// Restricts the camera's position within an axis-aligned bounding box.
	/// </summary>
	public class TacticalCameraBoxBounds : AbstractTacticalCameraBounds, ITacticalCameraBounds
	{
		[SerializeField, Tooltip("Axis-aligned bounding box that determines the camera's area to move in.")]
		private Bounds boundingBox = new Bounds(Vector3.zero, Vector3.one * 100f);
		[SerializeField, Tooltip("Should the bounds be relative to the world position of the object?")]
		private bool relativeToWorldPosition;

		/// <summary>
		/// The bounding box used to restrict the camera.
		/// </summary>
		public Bounds Bounds
		{
			get => boundingBox;
			set => boundingBox = value;
		}

		/// <inheritdoc />
		public override void Apply(TacticalCamera tCamera)
		{
			Vector3 positionToTest = tCamera.transform.position;
			if (relativeToWorldPosition)
			{
				positionToTest -= transform.position;
			}

			if (boundingBox.Contains(positionToTest))
			{
				return;
			}
			
			Vector3 newPosition = boundingBox.ClosestPoint(positionToTest);

			if (relativeToWorldPosition)
			{
				newPosition += transform.position;
			}
				
			tCamera.transform.position = newPosition;
		}

		private void OnDrawGizmosSelected()
		{
			if (!enabled)
			{
				return;
			}

			Color cached = Gizmos.color;
			Gizmos.color = Color.blue;

			if (relativeToWorldPosition)
			{
				Gizmos.DrawWireCube(transform.position + boundingBox.center, boundingBox.size);
			}
			else
			{
				Gizmos.DrawWireCube(boundingBox.center, boundingBox.size);
			}
			
			Gizmos.color = cached;
		}
	}
}

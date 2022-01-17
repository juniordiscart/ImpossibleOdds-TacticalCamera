namespace ImpossibleOdds.TacticalCamera
{
	using UnityEngine;

	/// <summary>
	/// Restricts the camera's position within an axis-aligned bounding box.
	/// </summary>
	public class TacticalCameraBoxBounds : AbstractTacticalCameraBounds, ITacticalCameraBounds
	{
		[SerializeField, Tooltip("Axis-aligned bounding box that determines the camera's area to move in.")]
		private Bounds boundingBox = new Bounds(Vector3.zero, Vector3.one * 100f);

		public Bounds Bounds
		{
			get { return boundingBox; }
			set { boundingBox = value; }
		}

		/// <inheritdoc />
		public override void Apply(TacticalCamera tCamera)
		{
			if (!boundingBox.Contains(tCamera.transform.position))
			{
				tCamera.transform.position = boundingBox.ClosestPoint(tCamera.transform.position);
			}
		}

		private void OnDrawGizmosSelected()
		{
			if (!enabled)
			{
				return;
			}

			Color cached = Gizmos.color;
			Gizmos.color = Color.blue;
			Gizmos.DrawWireCube(boundingBox.center, boundingBox.size);
			Gizmos.color = cached;
		}
	}
}

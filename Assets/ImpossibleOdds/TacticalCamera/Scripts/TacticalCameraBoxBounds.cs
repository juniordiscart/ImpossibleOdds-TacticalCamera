namespace ImpossibleOdds.TacticalCamera
{
	using UnityEngine;

	/// <summary>
	/// Restricts the camera's position within an axis-aligned bounding box.
	/// </summary>
	[RequireComponent(typeof(TacticalCamera))]
	public class TacticalCameraBoxBounds : MonoBehaviour, ITacticalCameraBounds
	{
		[SerializeField, Tooltip("Axis-aligned bounding box that determines the camera's area to move in.")]
		private Bounds boundingBox = new Bounds(Vector3.zero, Vector3.one);

		public Bounds Bounds
		{
			get { return boundingBox; }
			set { boundingBox = value; }
		}

		/// <summary>
		/// Restirct the camera within the defined
		/// </summary>
		public void Apply()
		{
			Vector3 position = transform.position;
			if (!boundingBox.Contains(position))
			{
				transform.position = boundingBox.ClosestPoint(position);
			}
		}

		private void Start()
		{
			GetComponent<TacticalCamera>().Bounds = this;
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

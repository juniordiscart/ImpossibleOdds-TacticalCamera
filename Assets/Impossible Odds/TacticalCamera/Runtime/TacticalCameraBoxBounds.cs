using UnityEngine;
using UnityEngine.Serialization;

namespace ImpossibleOdds.TacticalCamera
{
	/// <summary>
	/// Restricts the camera's position within an axis-aligned bounding box.
	/// </summary>
	public class TacticalCameraBoxBounds : AbstractTacticalCameraBounds
	{
		[SerializeField, Tooltip("Axis-aligned bounding box that determines the camera's area to move in.")]
		private Bounds boundingBox = new Bounds(Vector3.zero, Vector3.one * 100f);
		[SerializeField, Tooltip("Should the bounds follow the game object's position, rotation and scale?")]
		private bool followGameObject;

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
			tCamera.transform.position = Apply(tCamera.transform.position);
		}

		/// <inheritdoc />
		public override Vector3 Apply(Vector3 position)
		{
			if (followGameObject)
			{
				position = transform.InverseTransformPoint(position);
			}

			if (!Bounds.Contains(position))
			{
				position = Bounds.ClosestPoint(position);
			}
			
			if (followGameObject)
			{
				position = transform.TransformPoint(position);
			}

			return position;
		}

		/// <inheritdoc />
		public override bool IsWithinBounds(Vector3 position)
		{
			if (followGameObject)
			{
				position = transform.InverseTransformPoint(position);
			}

			return boundingBox.Contains(position);
		}

		private void OnDrawGizmos()
		{
			if (!enabled)
			{
				return;
			}

			Color cached = Gizmos.color;
			Gizmos.color = Color.blue;

			if (followGameObject)
			{
				Gizmos.matrix = transform.localToWorldMatrix;
				Gizmos.DrawWireCube(boundingBox.center, boundingBox.size);
				Gizmos.matrix = Matrix4x4.identity;
			}
			else
			{
				Gizmos.DrawWireCube(boundingBox.center, boundingBox.size);
			}
			
			Gizmos.color = cached;
		}
	}
}

namespace ImpossibleOdds.TacticalCamera
{
	using System;
	using UnityEngine;

	/// <summary>
	/// Interface to drive the tactical camera's behaviour.
	/// </summary>
	public interface ITacticalCameraSettings
	{
		/// <summary>
		/// Invoked whenever a value is updated.
		/// </summary>
		event Action onSettingsUpdated;

		/// <summary>
		/// Smallest value the camera will use to compare values with to determine certain actions.
		/// </summary>
		float Epsilon
		{
			get;
		}

		/// <summary>
		/// Curve that defines the speed of the camera depending on its altitude.
		/// </summary>
		AnimationCurve MovementSpeedTransition
		{
			get;
			set;
		}

		/// <summary>
		/// Fade curve for movement.
		/// </summary>
		AnimationCurve MovementFadeCurve
		{
			get;
			set;
		}

		/// <summary>
		/// Time for the movement to linger and fade out. (In seconds)
		/// </summary>
		float MovementFadeTime
		{
			get;
			set;
		}

		/// <summary>
		/// Smoothing time when the camera is moving to its focus point.
		/// </summary>
		float MoveToTargetSmoothingTime
		{
			get;
			set;
		}

		/// <summary>
		/// The minimum height above a surface (defined by the interaction mask) and maximum height the camera can go.
		/// </summary>
		ValueRange AbsoluteHeightRange
		{
			get;
			set;
		}

		/// <summary>
		/// Maximum speed at which the camera will pivot around its origin. (In degrees per second)
		/// </summary>
		float MaxRotationalSpeed
		{
			get;
			set;
		}

		/// <summary>
		/// Fade curve for pivotal rotation.
		/// </summary>
		AnimationCurve RotationalFadeCurve
		{
			get;
			set;
		}

		/// <summary>
		/// Time for the rotation to linger and fade out. (In seconds)
		/// </summary>
		float RotationalFadeTime
		{
			get;
			set;
		}

		/// <summary>
		/// Range in which the camera can tilt when it is up high. (In degrees)
		/// </summary>
		ValueRange TiltRangeHigh
		{
			get;
			set;
		}

		/// <summary>
		/// Range in which the camera can tilt when it is down low. (In degrees)
		/// </summary>
		ValueRange TiltRangeLow
		{
			get;
			set;
		}

		/// <summary>
		/// Transition of tilt ranges from low to high.
		/// </summary>
		AnimationCurve TiltRangeTransition
		{
			get;
			set;
		}

		/// <summary>
		/// Should the tactical camera apply a dynamic field-of-view based on its height range?
		/// </summary>
		bool UseDynamicFieldOfView
		{
			get;
			set;
		}

		/// <summary>
		/// The range of field-of-view values for the camera, depending on the height it's operating at.
		/// </summary>
		ValueRange DynamicFieldOfViewRange
		{
			get;
			set;
		}

		/// <summary>
		/// The transition of the field-of-view value from the lowest operating height to the highest operating height.
		/// </summary>
		AnimationCurve DynamicFieldOfViewTransition
		{
			get;
			set;
		}

		/// <summary>
		/// Layers that are to used to interact with the camera movement.
		/// </summary>
		LayerMask InteractionMask
		{
			get;
			set;
		}

		/// <summary>
		/// Distance the camera can interact with the world, e.g. raycast distance.
		/// </summary>
		float InteractionDistance
		{
			get;
			set;
		}

		/// <summary>
		/// The radius of the camera's collider, used to avoid collisions and clipping with the environment.
		/// </summary>
		float InteractionBubbleRadius
		{
			get;
			set;
		}
	}
}

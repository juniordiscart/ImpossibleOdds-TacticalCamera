﻿using UnityEngine;

namespace ImpossibleOdds.TacticalCamera
{
	/// <summary>
	/// Interface to drive the tactical camera's behaviour.
	/// </summary>
	public interface ITacticalCameraSettings
	{
		/// <summary>
		/// Smallest value the camera will use to compare values with to determine certain actions.
		/// </summary>
		float Epsilon
		{
			get;
		}

		/// <summary>
		/// Should the tactical camera use 'real' time, as opposed to 'scaled' time?
		/// </summary>
		bool IgnoreTimeScale
		{
			get;
		}

		/// <summary>
		/// The minimum height above a surface (defined by the interaction mask) and maximum height the camera can go.
		/// </summary>
		ValueRange AbsoluteHeightRange
		{
			get;
		}

		/// <summary>
		/// The range of movement speed values for the camera, depending on the height it's operating at.
		/// </summary>
		ValueRange MovementSpeedRange
		{
			get;
		}

		/// <summary>
		/// Time for the movement to linger and fade out. (In seconds)
		/// </summary>
		float MovementFadeTime
		{
			get;
		}

		/// <summary>
		/// Smoothing time when the camera is moving to its focus point.
		/// </summary>
		float MoveToTargetSmoothingTime
		{
			get;
		}

		/// <summary>
		/// Maximum speed at which the camera will pivot around its origin. (In degrees per second)
		/// </summary>
		float MaxRotationalSpeed
		{
			get;
		}

		/// <summary>
		/// Time for the rotation to linger and fade out. (In seconds)
		/// </summary>
		float RotationalFadeTime
		{
			get;
		}

		/// <summary>
		/// Range in which the camera can tilt when it is down low. (In degrees)
		/// </summary>
		ValueRange TiltRangeLow
		{
			get;
		}

		/// <summary>
		/// Range in which the camera can tilt when it is up high. (In degrees)
		/// </summary>
		ValueRange TiltRangeHigh
		{
			get;
		}

		/// <summary>
		/// Should the tactical camera apply a dynamic field-of-view based on its height range?
		/// </summary>
		bool UseDynamicFieldOfView
		{
			get;
		}

		/// <summary>
		/// The range of field-of-view values for the camera, depending on the height it's operating at.
		/// </summary>
		ValueRange DynamicFieldOfViewRange
		{
			get;
		}

		/// <summary>
		/// Layers that are to used to interact with the camera movement.
		/// </summary>
		LayerMask InteractionMask
		{
			get;
		}

		/// <summary>
		/// Distance the camera can interact with the world, e.g. raycast distance.
		/// </summary>
		float InteractionDistance
		{
			get;
		}

		/// <summary>
		/// The radius of the camera's collider, used to avoid collisions and clipping with the environment.
		/// </summary>
		float InteractionBubbleRadius
		{
			get;
		}

		/// <summary>
		/// When the camera can't determine a suitable orbital pivot point, is it allowed to pivot instead?
		/// </summary>
		bool AllowPivotOnFailedOrbit
		{
			get;
		}

		/// <summary>
		/// Evaluate how the movement fades out when no more input is given.
		/// </summary>
		/// <param name="t">Value between 0 and 1.</param>
		/// <returns>The fade factor to be applied to the movement speed.</returns>
		float EvaluateMovementFadeOut(float t);
		
		/// <summary>
		/// Evaluate how the rotation fades out when no more input is given.
		/// </summary>
		/// <param name="t">Value between 0 and 1.</param>
		/// <returns>The fade factor to be applied to the rotational speed.</returns>
		float EvaluateRotationFadeOut(float t);
		
		/// <summary>
		/// Evaluate how the movement speed should change based on the relative height of the camera,
		/// with 0 defined at the lowest operating point and 1 at the highest.
		/// </summary>
		/// <param name="t">Value between 0 and 1.</param>
		/// <returns>The desired interpolated value for the movement speed transition.</returns>
		float EvaluateMovementTransition(float t);
		
		/// <summary>
		/// Evaluate how the tilt range should change based on the relative height of the camera,
		/// with 0 defined at the lowest operating point and 1 at the highest.
		/// </summary>
		/// <param name="t">Value between 0 and 1.</param>
		/// <returns>The desired interpolated value for the tilt range transition.</returns>
		float EvaluateTiltTransition(float t);
		
		/// <summary>
		/// Evaluate how the field of view should change based on the relative height of the camera,
		/// with 0 defined at the lowest operating point and 1 at the highest.
		/// </summary>
		/// <param name="t">Value between 0 and 1.</param>
		/// <returns>The desired interpolated value for the field of view transition.</returns>
		float EvaluateFieldOfViewTransition(float t);
	}
}

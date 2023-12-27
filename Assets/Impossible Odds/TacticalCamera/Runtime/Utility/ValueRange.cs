using System;
using UnityEngine;

namespace ImpossibleOdds.TacticalCamera
{
	[Serializable]
	public struct ValueRange
	{
		[SerializeField]
		internal float min;
		[SerializeField]
		internal float max;
		
		/// <summary>
		/// The minimum value of the value range.
		/// </summary>
		public float Min => min;

		/// <summary>
		/// The maximum value of the value range.
		/// </summary>
		public float Max => max;

		/// <summary>
		/// Distance between min and max value.
		/// </summary>
		public float Range => Mathf.Abs(max - min);

		public ValueRange(float min, float max)
		{
			this.min = Mathf.Min(min, max);
			this.max = Mathf.Max(min, max);
		}

		public static ValueRange Lerp(ValueRange a, ValueRange b, float t)
		{
			ValueRange result = new ValueRange
			{
				min = Mathf.Lerp(a.min, b.min, t),
				max = Mathf.Lerp(a.max, b.max, t)
			};
			return result;
		}

		public float Lerp(float t)
		{
			return Mathf.Lerp(Min, Max, t);
		}

		/// <summary>
		/// Inverse lerp the given value from Min to Max.
		/// </summary>
		public float InverseLerp(float value)
		{
			return Mathf.InverseLerp(Min, Max, value);
		}

		/// <summary>
		/// Clamps the given value in this value range.
		/// </summary>
		public float Clamp(float value)
		{
			return Mathf.Clamp(value, Min, Max);
		}

		/// <summary>
		/// Set a new minimum and maximum value for this value range.
		/// </summary>
		public void Set(float min, float max)
		{
			this.min = Mathf.Min(min, max);
			this.max = Mathf.Max(min, max);
		}

		/// <summary>
		/// Test whether the given value is within the value range.
		/// </summary>
		public bool InRange(float value)
		{
			return (value >= Min) && (value <= Max);
		}

		public override string ToString()
		{
			return $"Min: {Min:0.000}, Max: {Max:0.000}";
		}
	}
}

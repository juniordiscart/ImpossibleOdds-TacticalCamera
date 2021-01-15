namespace ImpossibleOdds.TacticalCamera
{
	using System;
	using UnityEngine;

	[Serializable]
	public struct ValueRange
	{
		/// <summary>
		/// The minimum value of the value range.
		/// </summary>
		public float Min
		{
			get { return min; }
		}

		/// <summary>
		/// The maximum value of the value range.
		/// </summary>
		public float Max
		{
			get { return max; }
		}

		/// <summary>
		/// Distance between min and max value.
		/// </summary>
		public float Range
		{
			get { return Mathf.Abs(max - min); }
		}

		[SerializeField]
		private float min;
		[SerializeField]
		private float max;

		public static ValueRange Lerp(ValueRange a, ValueRange b, float t)
		{
			ValueRange result = new ValueRange();
			result.min = Mathf.Lerp(a.min, b.min, t);
			result.max = Mathf.Lerp(a.max, b.max, t);
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
			return string.Format("Min: {0}, Max: {1}", Min.ToString("0.000"), Max.ToString("0.000"));
		}
	}
}

namespace ImpossibleOdds.TacticalCamera
{
	using System;
	using UnityEngine;

	[Serializable]
	public struct ValueRange
	{
		public float Min
		{
			get { return Mathf.Min(min, max); }
		}

		public float Max
		{
			get { return Mathf.Max(min, max); }
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

		public float Clamp(float value)
		{
			return Mathf.Clamp(value, Min, Max);
		}

		public void Set(float min, float max)
		{
			this.min = min;
			this.max = max;
		}

		public override string ToString()
		{
			return string.Format("Min: {0}, Max: {1}", Min.ToString("0.000"), Max.ToString("0.000"));
		}
	}
}

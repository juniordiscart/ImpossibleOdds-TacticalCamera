namespace ImpossibleOdds.TacticalCamera
{
	using System;
	using UnityEngine;

	/// <summary>
	/// Class to keep track of the current fade state of a value. It is controlled by time and a curve.
	/// </summary>
	public class FadeState
	{
		private float time = 0f;
		private float fadeTime = 0f;
		private float fadeValue = 0f;
		private Func<float, float> fadeEvaluation = null;

		public float Time
		{
			get { return time; }
			set { time = value; }
		}

		public float Value
		{
			get
			{
				float t = Mathf.InverseLerp(fadeTime, 0f, time);
				return fadeEvaluation(t) * fadeValue;
			}
		}

		public float FadeTime
		{
			get { return fadeTime; }
			set
			{
				if (value <= 0f)
				{
					throw new System.ArgumentOutOfRangeException("The fade time should be a value larger than 0.");
				}

				fadeTime = value;
			}
		}

		public float FadeValue
		{
			get { return fadeValue; }
			set
			{
				fadeValue = value;
				time = fadeTime;
			}
		}

		public Func<float, float> FadeEvaluation
		{
			get { return fadeEvaluation; }
			set { fadeEvaluation = value; }
		}

		public bool IsActive
		{
			get { return time > 0f; }
		}

		public void Reset()
		{
			Time = 0f;
		}

		public float Tick()
		{
			if (time == 0f)
			{
				return 0f;
			}

			time = Mathf.Clamp(time - UnityEngine.Time.deltaTime, 0f, fadeTime);
			return Value;
		}

		public void ApplySettings(float fadeTime, Func<float, float> fadeEvaluation)
		{
			this.fadeTime = fadeTime;
			this.fadeEvaluation = fadeEvaluation;
			time = 0f;
			fadeValue = 0f;
		}
	}
}

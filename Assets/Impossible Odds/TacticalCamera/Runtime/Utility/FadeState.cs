using System;
using UnityEngine;

namespace ImpossibleOdds.TacticalCamera
{
	/// <summary>
	/// Class to keep track of the current fade state of a value. It is controlled by time and a curve.
	/// </summary>
	public class FadeState
	{
		private float fadeTime;
		private float fadeValue;

		public float Time
		{
			get;
			set;
		}

		public float Value
		{
			get
			{
				float t = Mathf.InverseLerp(fadeTime, 0f, Time);
				return FadeEvaluation(t) * fadeValue;
			}
		}

		public float FadeTime
		{
			get => fadeTime;
			set
			{
				if (value <= 0f)
				{
					throw new ArgumentOutOfRangeException(nameof (value),"The fade time should be a value larger than 0.");
				}

				fadeTime = value;
			}
		}

		public float FadeValue
		{
			get => fadeValue;
			set
			{
				fadeValue = value;
				Time = fadeTime;
			}
		}

		public Func<float, float> FadeEvaluation
		{
			get;
			set;
		}

		public Func<float> DeltaTime
		{
			get;
			set;
		} = () => UnityEngine.Time.deltaTime;

		public bool IsActive => Time > 0f;

		public void Reset()
		{
			Time = 0f;
		}

		public float Tick()
		{
			if (Time == 0f)
			{
				return 0f;
			}

			Time = Mathf.Clamp(Time - DeltaTime(), 0f, fadeTime);
			return Value;
		}

		public void ApplySettings(float fadeTime, Func<float, float> fadeEvaluation, Func<float> deltaTime)
		{
			this.fadeTime = fadeTime;
			FadeEvaluation = fadeEvaluation;
			DeltaTime = deltaTime;
			Time = 0f;
			fadeValue = 0f;
		}
	}
}

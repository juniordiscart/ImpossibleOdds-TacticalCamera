namespace ImpossibleOdds.TacticalCamera
{
	using UnityEngine;

	/// <summary>
	/// Class to keep track of the current fade state of a value. It is controlled by time and a curve.
	/// </summary>
	public class FadeState
	{
		private float time = 0f;
		private float fadeTime = 0f;
		private float fadeValue = 0f;
		private AnimationCurve fadeCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

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
				return fadeCurve.Evaluate(t) * fadeValue;
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

		public AnimationCurve FadeCurve
		{
			get { return fadeCurve; }
			set { fadeCurve = value; }
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

		public void ApplySettings(float fadeTime, AnimationCurve fadeCurve)
		{
			this.fadeTime = fadeTime;
			this.fadeCurve = fadeCurve;
			time = 0f;
			fadeValue = 0f;
		}
	}
}

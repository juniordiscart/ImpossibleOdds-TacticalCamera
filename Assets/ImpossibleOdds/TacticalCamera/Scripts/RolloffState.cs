/// <summary>
/// Class to keep track of the current rolloff state of a value. It is controlled by time and a curve.
/// </summary>
namespace ImpossibleOdds.TacticalCamera
{
	using UnityEngine;

	public class RolloffState
	{
		private float time;
		private float rolloffTime;
		private float rolloffValue;
		private AnimationCurve rolloffCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

		public float Time
		{
			get { return time; }
			set { time = value; }
		}

		public float Value
		{
			get
			{
				float t = Mathf.InverseLerp(rolloffTime, 0f, time);
				return rolloffCurve.Evaluate(t) * rolloffValue;
			}
		}

		public float RolloffTime
		{
			get { return rolloffTime; }
			set
			{
				if (value <= 0f)
				{
					throw new System.ArgumentOutOfRangeException("The rolloff time should be a value larger than 0.");
				}

				rolloffTime = value;
			}
		}

		public float RolloffValue
		{
			get { return rolloffValue; }
			set
			{
				rolloffValue = value;
				time = rolloffTime;
			}
		}

		public AnimationCurve RolloffCurve
		{
			get { return rolloffCurve; }
			set { rolloffCurve = value; }
		}

		public float Update()
		{
			if (time == 0f)
			{
				return 0f;
			}

			time = Mathf.Clamp(time - UnityEngine.Time.deltaTime, 0f, rolloffTime);
			return Value;
		}

		public void ApplySettings(float rolloffTime, AnimationCurve rolloffCurve)
		{
			this.rolloffTime = rolloffTime;
			this.rolloffCurve = rolloffCurve;
			time = 0f;
			rolloffValue = 0f;
		}
	}
}

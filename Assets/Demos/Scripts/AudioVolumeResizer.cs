using UnityEngine;

public class AudioVolumeResizer : MonoBehaviour
{
	public enum ELevelType { DB, RMS, Peak };

	[Tooltip("Which audio detector to use (empty: find automatically)")]
	public SentienceLab.AudioDetector Recorder;

	[Tooltip("Use DB or RMS as audio level")]
	public ELevelType LevelType = ELevelType.DB;

	[Tooltip("Minimum audio level for scaling the object")]
	public float MaxLevel = -6;

	[Tooltip("Maximum audio level for scaling the object")]
	public float MinLevel = -80;

	[Tooltip("Scale factors for the object")]
	public Vector3 ScaleFactor = Vector3.zero;

	[Tooltip("Time in s for the scale to revert to normal")]
	public float ShrinkTime  = 1;


	public void Start()
	{
		if (Recorder == null)
		{
			Recorder = FindObjectOfType<SentienceLab.AudioDetector>();
		}
		if (Recorder == null)
		{
			this.enabled = false;
		}
		m_originalScale = transform.localScale;
		m_size = 0;
	}

	public void Update()
	{
		if (Recorder == null) return;

		float vol = 0;
		switch (LevelType)
		{
			case ELevelType.DB:   vol = Recorder.CurrentLevelDB; break;
			case ELevelType.RMS:  vol = Recorder.CurrentLevelRMS; break;
			case ELevelType.Peak: vol = Recorder.CurrentLevelPeak; break;
		}

		vol  = Mathf.Clamp(vol, MinLevel, MaxLevel);
		vol -= MinLevel; 
		vol /= (MaxLevel - MinLevel); // vol is now between 0...1

		m_size  = Mathf.Max(m_size, vol);
		m_size *= 1 - (Time.deltaTime / ShrinkTime); 
		transform.localScale = m_originalScale + ScaleFactor * m_size;
	}


	private Vector3 m_originalScale;
	private float   m_size;
}

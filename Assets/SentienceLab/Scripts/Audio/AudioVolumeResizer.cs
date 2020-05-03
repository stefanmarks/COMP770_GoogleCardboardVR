using UnityEngine;

public class AudioVolumeResizer : MonoBehaviour
{
    public AudioRecorder Recorder;
    public bool          UseDB       = true;
    public float         MaxLevel    = -6;
    public float         MinLevel    = -80;
    public Vector3       ScaleFactor = Vector3.one;
    public float         ShrinkTime  = 1;

    public void Start()
    {
        if (Recorder == null)
        {
            Recorder = FindObjectOfType<AudioRecorder>();
        }
        if (Recorder == null)
        {
            this.enabled = false;
        }
        m_originalScale = transform.localScale;
        m_size = 1;
    }

    public void Update()
    {
        if (Recorder == null) return;

        float vol = UseDB ? Recorder.CurrentVolumeDB : Recorder.CurrentVolumeRMS;
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

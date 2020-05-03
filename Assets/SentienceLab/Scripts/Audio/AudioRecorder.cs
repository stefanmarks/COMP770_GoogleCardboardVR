using System.Text.RegularExpressions;
using UnityEngine;

public class AudioRecorder : MonoBehaviour
{
    [Tooltip("Name of the microphone to use (can be regular expression. empty: use default)")]
    public string MicrophoneName = "";


    void Start()
    {
        SelectMicrophone();
        
        // Start recording audio in 1s clips, looping
        m_clip = Microphone.Start(MicrophoneName, true, 1, 44100);
        if (m_clip == null)
        {
            Debug.LogWarning("Could not open microphone");
            this.enabled = false;
        }
        else
        {
            Debug.LogFormat("Opened microphone '{0}'", MicrophoneName);
        }

        m_data = null;
        m_dataPos = 0;
        m_prevDataPos = 0;
    }


    private void SelectMicrophone()
    {
        string[] microphones = Microphone.devices;
        if (microphones != null && microphones.Length > 0)
        {
            if (MicrophoneName == null || MicrophoneName.Length == 0)
            {
                // no microphone name given > use first one
                MicrophoneName = microphones[0];
            }
            else
            {
                // microphone name given > find exact match
                foreach (string mic in microphones)
                {
                    if (Regex.IsMatch(mic, MicrophoneName))
                    {
                        MicrophoneName = mic;
                        break;
                    }
                }
            }
        }
    }

    
    void Update()
    {
        // allocate audio data buffer if not already done
        if (m_data == null)
        {
            // allocate buffer if not yet done
            m_data = new float[m_clip.samples * m_clip.channels];
        }
        // how much data do we have?
        m_dataPos = Microphone.GetPosition(MicrophoneName);
        if (m_dataPos != m_prevDataPos)
        {
            m_clip.GetData(m_data, 0);

            CurrentVolumeRMS = 0;

            int   idx     = m_prevDataPos;
            int   samples = 0;
            while (idx != m_dataPos)
            {
                CurrentVolumeRMS += m_data[idx] * m_data[idx];
                idx = (idx + 1) % m_data.Length;
                samples++;
            }
            CurrentVolumeRMS /= samples;
            CurrentVolumeRMS = Mathf.Sqrt(CurrentVolumeRMS);
            CurrentVolumeDB = 20 * Mathf.Log10(CurrentVolumeRMS);

            m_prevDataPos = m_dataPos;
        }
    }


    public float CurrentVolumeRMS { get; private set; }
    public float CurrentVolumeDB { get; private set; }


    protected AudioClip m_clip;
    protected float[]   m_data;
    protected int       m_dataPos, m_prevDataPos;
}

using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ToggleAudioSource : MonoBehaviour
{
    public void Start()
    {
        m_source = GetComponent<AudioSource>();
    }


    public void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            m_source.mute = !m_source.mute;
        }
    }

    protected AudioSource m_source;
}

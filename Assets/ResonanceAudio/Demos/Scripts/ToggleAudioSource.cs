using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class ToggleAudioSource : MonoBehaviour
{
    public InputActionProperty toggleAction;

    public void Start()
    {
        toggleAction.action.Enable();
        toggleAction.action.performed += delegate { m_source.mute = !m_source.mute; };

        m_source = GetComponent<AudioSource>();
    }

    protected AudioSource m_source;
}

#region Copyright Information
// Sentience Lab Unity Framework
// (C) Sentience Lab (sentiencelab@aut.ac.nz), Auckland University of Technology, Auckland, New Zealand 
#endregion Copyright Information

using UnityEngine;

public class PopUpDelay : MonoBehaviour
{
	[Tooltip("The game object to appear/disappear")]
	public GameObject PopupObject;

	[Tooltip("Time in seconds from the start signal to the game object appearing")]
	public float AppearanceDelay    = 0.5f;

	[Tooltip("Time in seconds from the stop signal to the game object appearing")]
	public float DisappearanceDelay = 2.0f;

	public void Start()
	{
		m_state = EPopupState.HIDDEN;
		PopupObject?.SetActive(false);
	}

	public void TriggerAppearance()
	{
		if (m_state == EPopupState.HIDDEN)
		{
			m_timer = AppearanceDelay;
			m_state = EPopupState.APPEARING;
		}
		else if (m_state == EPopupState.DISAPPEARING)
		{
			// stop disappearance timer
			m_state = EPopupState.VISIBLE;
		}
	}

	public void TriggerImmediateAppearance()
	{
		if (m_state == EPopupState.HIDDEN || m_state == EPopupState.APPEARING)
		{
			m_timer = 0;
			m_state = EPopupState.APPEARING;
		}
		else if (m_state == EPopupState.DISAPPEARING)
		{
			// stop disappearance timer
			m_state = EPopupState.VISIBLE;
		}
	}

	public void TriggerDisappearance()
	{
		if (m_state == EPopupState.VISIBLE)
		{
			m_timer = DisappearanceDelay;
			m_state = EPopupState.DISAPPEARING;
		}
		else if (m_state == EPopupState.APPEARING)
		{
			// stop appearance timer
			m_state = EPopupState.HIDDEN;
		}
	}

	public void TriggerImmediateDisappearance()
	{
		if (m_state == EPopupState.VISIBLE || m_state == EPopupState.DISAPPEARING)
		{
			m_timer = 0;
			m_state = EPopupState.DISAPPEARING;
		}
		else if (m_state == EPopupState.APPEARING)
		{
			// stop appearance timer
			m_state = EPopupState.HIDDEN;
		}
	}

	public void Update()
	{
		if (m_state == EPopupState.APPEARING)
		{
			m_timer -= Time.deltaTime;
			if (m_timer < 0)
			{
				m_state = EPopupState.VISIBLE;
				PopupObject?.SetActive(true);
			}
		}
		else if (m_state == EPopupState.DISAPPEARING)
		{
			m_timer -= Time.deltaTime;
			if (m_timer < 0)
			{
				m_state = EPopupState.HIDDEN;
				PopupObject?.SetActive(false);
			}
		}
	}

	private enum EPopupState
	{
		HIDDEN, APPEARING, VISIBLE, DISAPPEARING
	}

	private EPopupState m_state;
	private float       m_timer;
}

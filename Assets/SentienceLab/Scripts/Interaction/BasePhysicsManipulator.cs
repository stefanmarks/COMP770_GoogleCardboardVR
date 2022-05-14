﻿#region Copyright Information
// SentienceLab Unity Framework
// (C) SentienceLab (sentiencelab@aut.ac.nz), Auckland University of Technology, Auckland, New Zealand 
#endregion Copyright Information

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace SentienceLab
{
	/// <summary>
	/// Component for moving a physical object by clicking and moving it.
	/// When clicked, the script will try to maintain the relative position of the rigid body using forces applied to its centre.
	/// </summary>
	///
	public abstract class BasePhysicsManipulator : MonoBehaviour
	{
		[Tooltip("Input action for grabbing")]
		public InputActionProperty GrabAction;

		[Tooltip("Grab PID controller")]
		public PID_Controller3D PID;

		[Tooltip("Turn off gravity for manipulated object")]
		public bool DisableGravityOnGrab = true;


		[System.Serializable]
		public class Events
		{
			[Tooltip("Event fired when the manipulator touches an object")]
			public UnityEvent<Rigidbody> OnTouchStart;

			[Tooltip("Event fired when the manipulator stops touching an object")]
			public UnityEvent<Rigidbody> OnTouchEnd;

			[Tooltip("Event fired when the manipulator grabs an object")]
			public UnityEvent<Rigidbody> OnGrabStart;

			[Tooltip("Event fired when the manipulator releases an object")]
			public UnityEvent<Rigidbody> OnGrabEnd;
		}

		public Events events;


		public virtual void Start()
		{
			if (GrabAction != null)
			{
				GrabAction.action.performed += OnGrabStart;
				GrabAction.action.canceled  += OnGrabEnd;
				GrabAction.action.Enable();
			}
			else
			{
				Debug.LogWarning("No action defined for grab");
				this.enabled = false;
			}
		}


		/// <summary>
		/// Called by superclasses to set/clear a candidate and a world grab point.
		/// </summary>
		/// <param name="_candidate">potential rigidbody candidate or <c>null</c> if there is no candidate</param>
		/// <param name="_grabPoint">world coordinate of grab point</param>
		/// 
		protected void SetCandidate(Rigidbody _candidate, Vector3 _grabPoint)
		{
			// don't change candidate while holding onto an object
			if (!IsManipulatingRigidbody())
			{
				if (m_candidateBody != _candidate)
				{
					// did we touch another object before?
					if (m_candidateBody != null)
					{
						// fire touch end events
						var irb = m_candidateBody.GetComponent<InteractiveRigidbody>();
						if (irb    != null) irb.InvokeTouchEnd(this.gameObject);
						if (events != null) events.OnTouchEnd.Invoke(m_candidateBody);
					}

					m_candidateBody      = _candidate;
					m_candidateGrabPoint = _grabPoint;

					// are we touching an object now?
					if (m_candidateBody != null)
					{
						// fire touch start events
						var irb = m_candidateBody.GetComponent<InteractiveRigidbody>();
						if (irb    != null) irb.InvokeTouchStart(this.gameObject);
						if (events != null) events.OnTouchStart.Invoke(m_candidateBody);
					}
				}
			}
		}


		public Rigidbody GetActiveRigidbody()
		{
			return m_activeBody;
		}


		public Vector3 GetGrabPoint()
		{
			return m_activeBody.transform.TransformPoint(m_relTargetPoint);
		}


		public bool IsManipulatingRigidbody()
		{
			return m_activeBody != null;
		}


		protected void SetDefaultRigidbody(Rigidbody _default)
		{
			m_defaultBody = _default;
		}


		protected void OnGrabStart(InputAction.CallbackContext obj)
		{
			if ((m_candidateBody != null) || (m_defaultBody != null))
			{
				m_activeBody = (m_candidateBody != null) ? m_candidateBody : m_defaultBody;
				RigidbodyConstraints c = m_activeBody.constraints;
				if (c == RigidbodyConstraints.None)
				{
					// body can move freely - apply forces at centre
					m_relBodyPoint = Vector3.zero;
					m_relTargetPoint = transform.InverseTransformPoint(m_activeBody.transform.position);
					m_relTargetOrientation = Quaternion.Inverse(transform.rotation) * m_activeBody.transform.rotation;
				}
				else
				{
					// body is restrained - apply forces on contact point
					m_relBodyPoint = m_activeBody.transform.InverseTransformPoint(m_candidateGrabPoint);
					m_relTargetPoint = transform.InverseTransformPoint(m_candidateGrabPoint);
					m_relTargetOrientation = Quaternion.Inverse(transform.rotation) * m_activeBody.transform.rotation;
				}

				if (DisableGravityOnGrab)
				{
					// make target object weightless
					m_previousGravityFlag = m_activeBody.useGravity;
					m_activeBody.useGravity = false;
				}

				// fire grab start events
				var irb = m_activeBody.GetComponent<InteractiveRigidbody>();
				if (irb    != null) irb.InvokeGrabStart(this.gameObject);
				if (events != null) events.OnGrabStart.Invoke(m_activeBody);
			}
			else
			{
				m_activeBody = null;
			}
		}


		protected void OnGrabEnd(InputAction.CallbackContext obj)
		{
			if (m_activeBody != null)
			{
				if (DisableGravityOnGrab)
				{
					// restore gravity flag
					m_activeBody.useGravity = m_previousGravityFlag;
				}

				// fire grab end events
				var irb = m_activeBody.GetComponent<InteractiveRigidbody>();
				if (irb    != null) irb.InvokeGrabEnd(this.gameObject);
				if (events != null) events.OnGrabEnd.Invoke(m_activeBody);

				m_activeBody = null;
			}
		}


		public void FixedUpdate()
		{
			// moving a rigid body: apply the right force to get that body to the new target position
			if (m_activeBody != null)
			{
				// set new target position
				PID.Setpoint    = transform.TransformPoint(m_relTargetPoint); // target point in world coordinates
				Vector3 bodyPos = m_activeBody.transform.TransformPoint(m_relBodyPoint); // body point in world coordinates
				// let PID controller work
				Vector3 force = PID.Process(bodyPos);
				m_activeBody.AddForceAtPosition(force, bodyPos, ForceMode.Force);
			}
		}


		protected Rigidbody  m_candidateBody, m_activeBody, m_defaultBody;
		protected bool       m_previousGravityFlag;
		protected Vector3    m_candidateGrabPoint, m_relTargetPoint, m_relBodyPoint;
		protected Quaternion m_relTargetOrientation;
	}
}

#region Copyright Information
// Sentience Lab Unity Framework
// (C) Sentience Lab (sentiencelab@aut.ac.nz), Auckland University of Technology, Auckland, New Zealand 
#endregion Copyright Information

using System;
using System.Collections.Generic;
using UnityEngine;
using SentienceLab.PostProcessing;

namespace SentienceLab
{
	/// <summary>
	/// Class for executing a teleport.
	/// </summary>

	[AddComponentMenu("Locomotion/Teleporter")]
	[DisallowMultipleComponent]

	public class Teleporter : MonoBehaviour
	{
		[Tooltip("Type of the teleport transition")]
		public TransitionType transitionType = TransitionType.MoveLinear;

		[Tooltip("Duration of the teleport transition in seconds")]
		public float transitionTime = 0.1f;

		[Tooltip("Sound to play during teleport (optional)")]
		public AudioSource teleportSound;


		public enum TransitionType
		{
			MoveLinear,
			MoveSmooth,
			Fade,
			// Blink
		}


		void Start()
		{
			transition = null;
		}


		void Update()
		{
			if (transition != null)
			{
				transition.Update(this.transform);
				if (transition.IsFinished())
				{
					transition.Cleanup();
					transition = null;
				}
			}
		}


		/// <summary>
		/// Activates the Teleport function to a specific point and a specific orientation.
		/// </summary>
		/// <param name="originPoint">the point to teleport from</param>
		/// <param name="originOrientation">the orientation to teleport from</param>
		/// <param name="targetPoint">the point to teleport to</param>
		/// <param name="targetOrientation">the orientation to teleport to</param>
		/// 
		protected void Activate(Vector3 originPoint, Quaternion originOrientation, Vector3 targetPoint, Quaternion targetOrientation)
		{
			if (!IsReady()) return;

			if (teleportSound != null)
			{
				teleportSound.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
				teleportSound.Play();
			}

			// activate transition
			switch (transitionType)
			{
				case TransitionType.Fade:
					transition = new Transition_Fade(targetPoint, targetOrientation, transitionTime, this.gameObject);
					break;

				case TransitionType.MoveLinear:
					transition = new Transition_Move(originPoint, originOrientation, targetPoint, targetOrientation, transitionTime, false);
					break;

				case TransitionType.MoveSmooth:
					transition = new Transition_Move(originPoint, originOrientation, targetPoint, targetOrientation, transitionTime, true);
					break;
			}
		}


		/// <summary>
		/// Checks if the Teleporter is ready.
		/// </summary>
		/// <returns><c>true</c> if teleport can be triggered</returns>
		/// 
		public bool IsReady()
		{
			return transition == null;
		}


		public void ResetPosition()
		{
			Activate(this.transform.position, this.transform.rotation, Vector3.zero, Quaternion.identity);
		}


		public void TeleportPosition(Transform target)
		{
			TeleportPosition(this.transform, target);
		}


		public void TeleportPosition(Transform source, Transform target)
		{
			TeleportPosition(source.position, target.position);
		}


		protected void TeleportPosition(Vector3 source, Vector3 target)
		{
			Activate(source, this.transform.rotation, target, this.transform.rotation);
		}


		public void TeleportMainCamera(Vector3 target)
		{
			Vector3 relPos = this.transform.InverseTransformPoint(Camera.main.transform.position);
			relPos.y = 0;
			Activate(this.transform.position, this.transform.rotation, target-relPos, this.transform.rotation);
		}


		public void TeleportMainCamera(Transform target)
		{
			TeleportMainCamera(target.position);
		}


		public void TeleportPositionAndOrientation(Transform source, Transform target)
		{
			Activate(source.position, source.rotation, target.position, target.rotation);
		}


		public void TeleportMainCameraPositionAndOrientation(Transform target)
		{
			// How do we get from the current camera direction to the target direction?
			
			// Determine camera position on floor (no Y) relative to source
			Vector3 camPos    = Camera.main.transform.position;
			Vector3 relCamPosFloor = this.transform.InverseTransformPoint(camPos);
			relCamPosFloor.y = 0;

			// determine camera direction relative to source
			Vector3 camFwd    = Camera.main.transform.forward;
			Vector3 relCamFwd = this.transform.InverseTransformDirection(camFwd);
			relCamFwd.y = 0; relCamFwd.Normalize();
			float camAngle = Mathf.Atan2(relCamFwd.x, relCamFwd.z) * Mathf.Rad2Deg;

			// calculate difference rotation so camera faces forwards after teleport
			Quaternion targetRot = target.rotation;
			targetRot = Quaternion.AngleAxis(-camAngle, target.up) * targetRot;

			// execute
			Activate(
				this.transform.InverseTransformPoint(relCamPosFloor), this.transform.rotation, 
				target.TransformPoint(relCamPosFloor), targetRot);
		}


		private ITransition transition;


		private interface ITransition
		{
			void Update(Transform offsetObject);
			bool IsFinished();
			void Cleanup();
		}


		private class Transition_Fade : ITransition
		{
			public Transition_Fade(Vector3 endPoint, Quaternion endRot, float duration, GameObject parent)
			{
				this.endPoint    = endPoint;
				this.endRotation = endRot;
				this.duration    = duration;

				progress = 0;
				moved = false;

				// create fade effect
				fadeEffects = ScreenFade.AttachToAllCameras();
				foreach (ScreenFade fade in fadeEffects)
				{
					fade.FadeColour = Color.black;
				}
			}

			public void Update(Transform offsetObject)
			{
				// move immediately to B when fade is half way (all black)
				progress += Time.deltaTime / duration;
				progress  = Math.Min(1, progress);
				if ((progress >= 0.5f) && !moved)
				{
					offsetObject.position = endPoint;
					offsetObject.rotation = endRotation;
					moved = true; // only move once
				}

				float fadeFactor = 1.0f - Math.Abs(progress * 2 - 1); // from [0....1....0]
				foreach (ScreenFade fade in fadeEffects)
				{
					fade.FadeFactor = fadeFactor;
				}
			}

			public bool IsFinished()
			{
				return progress >= 1; // movement has finished
			}

			public void Cleanup()
			{
				foreach (ScreenFade fade in fadeEffects)
				{
					GameObject.Destroy(fade);
				}
			}


			private Vector3    endPoint;
			private Quaternion endRotation;
			private float      duration, progress;
			private bool       moved;
			private List<ScreenFade> fadeEffects;
		}


		private class Transition_Move : ITransition
		{
			public Transition_Move(
				Vector3 startPoint, Quaternion startRot, 
				Vector3 endPoint,   Quaternion endRot, 
				float   duration, 
				bool    smooth)
			{
				this.startPoint    = startPoint;
				this.startRotation = startRot;
				this.endPoint      = endPoint;
				this.endRotation   = endRot;
				this.duration      = duration;
				this.smooth        = smooth;

				progress = 0;
			}

			public void Update(Transform offsetObject)
			{
				// move from A to B
				progress += Time.deltaTime / duration;
				progress = Math.Min(1, progress);
				// linear: lerpFactor = progress. smooth: lerpFactor = sin(progress * PI/2) ^ 2
				float lerpFactor = smooth ? (float)Math.Pow(Math.Sin(progress * Math.PI / 2), 2) : progress;
				offsetObject.position = Vector3.Lerp(startPoint, endPoint, lerpFactor);
				offsetObject.rotation = Quaternion.Slerp(startRotation, endRotation, lerpFactor);
			}

			public bool IsFinished()
			{
				return progress >= 1; // movement has finished
			}

			public void Cleanup()
			{
				// nothing to do
			}

			private Vector3    startPoint, endPoint;
			private Quaternion startRotation, endRotation;
			private float      duration, progress;
			private bool       smooth;
		}
	}
}

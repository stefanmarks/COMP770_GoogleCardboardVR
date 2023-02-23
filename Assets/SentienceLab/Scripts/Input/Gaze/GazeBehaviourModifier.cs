#region Copyright Information
// SentienceLab Unity Framework
// (C) SentienceLab (sentiencelab@aut.ac.nz), Auckland University of Technology, Auckland, New Zealand 
#endregion Copyright Information

using UnityEngine;

namespace SentienceLab
{
	/// <summary>
	/// Component to modify gaze fuse times and gaze ranges.
	/// </summary>
	/// 
	[AddComponentMenu("SentienceLab/Interaction/Gaze/Gaze Behaviour Modifier")]
	public class GazeBehaviourModifier : MonoBehaviour
	{
		[Tooltip("0: Use default fuse time\n>0: Override fuse time\nInfinity: No fuse")]
		public float fuseTimeOverride         = 0;

		[Tooltip("0: Use default minimum gaze range\n>0: Override minimum gaze range")]
		public float minimumGazeRangeOverride = 0;

		[Tooltip("0: Use default maximum gaze range\n>0: Override maximum gaze range")]
		public float maximumGazeRangeOverride = 0;
	}
}
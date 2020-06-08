#region Copyright Information
// Sentience Lab - Unity Framework
// (C) Sentience Lab (sentiencelab@aut.ac.nz), Auckland University of Technology, Auckland, New Zealand 
#endregion Copyright Information

using System.Collections.Generic;
using UnityEngine;

namespace SentienceLab
{
	public class DontDestroyOnLoad : MonoBehaviour
	{
		public List<GameObject> ObjectsToPreserve;

		public void Awake()
		{
			if (ObjectsToPreserve.Count == 0)
			{
				ObjectsToPreserve.Add(gameObject);
			}

			foreach (var gameObject in ObjectsToPreserve)
			{
				if (gameObject != null) DontDestroyOnLoad(gameObject);
			}
		}
	}
}

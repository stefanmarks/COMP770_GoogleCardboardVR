using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Example script that is called whenever a scene is loaded
/// and, e.g., adjusts directional lightsources to keep the time-of-day appearance cosistent.
/// </summary>
public class SceneSetup : MonoBehaviour
{
	public void Start()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	
	public void OnSceneLoaded(Scene _scene, LoadSceneMode _mode)
	{
		Debug.LogFormat("SceneSetup: scene '{0}' loaded in mode {1}", _scene.name, _mode);
		
		/** 
		 * Example to manipulate light sources
		Light[] lights = FindObjectsOfType<Light>();
		foreach(Light l in lights)
		{
			if (l.type == LightType.Directional)
			{
				// adapt settings
				Debug.Log("fiddling with light source " + l.name);
			}
		}
		*/
	}
}

#region Copyright Information
// Sentience Lab - Unity Framework
// (C) Sentience Lab (sentiencelab@aut.ac.nz), Auckland University of Technology, Auckland, New Zealand 
#endregion Copyright Information

using SentienceLab.PostProcessing;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SentienceLab
{
	public class SceneLoader : MonoBehaviour
	{
		public List<string>   ScenesToLoad   = null;
		public LoadSceneMode  SceneMode      = LoadSceneMode.Single;
		public bool           LoadOnStart    = false;
		public List<string>   ScenesToUnload = null;
		public float          FadeTime       = 0.5f;
		public Color          FadeColour     = Color.black;


		public void Start()
		{
			CheckSceneList(ScenesToLoad);
			CheckSceneList(ScenesToUnload);

			if (LoadOnStart)
			{
				StartLoading();
			}
		}


		protected void CheckSceneList(List<string> sceneList)
		{
			if (sceneList == null) return;
#if UNITY_EDITOR
			List<string> toRemove = new List<string>();

			foreach (var asset in sceneList)
			{
				bool found = false;
				foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
				{
					if (scene.enabled && ScenesToLoad != null && scene.path.Contains(asset))
					{
						found = true;
						break;
					}
				}
				if (!found)
				{
					Debug.LogWarningFormat("Could not find scene '{0}' in build list", asset);
					toRemove.Add(asset);
				}
			}

			foreach (var asset in toRemove)
			{
				sceneList.Remove(asset);
			}
#endif
		}


		public void StartLoading()
		{
			if ( (ScenesToLoad   == null || ScenesToLoad.Count   == 0) &&
			     (ScenesToUnload == null || ScenesToUnload.Count == 0) ) return;

			StartCoroutine(LoadSceneWorker());
		}


		public IEnumerator LoadSceneWorker()
		{
			// create fade effect
			List<ScreenFade> fadeEffects = ScreenFade.AttachToAllCameras();
			foreach (ScreenFade fade in fadeEffects)
			{
				fade.FadeColour = FadeColour;
			}

			float fadeFactor = 0;
			while (fadeFactor < 1)
			{
				fadeFactor += Time.deltaTime / FadeTime;
				foreach (ScreenFade fade in fadeEffects)
				{
					fade.FadeFactor = fadeFactor;
				}
				yield return null;
			}

			string lastLoadedScene = "";
			foreach (var asset in ScenesToLoad)
			{
				SceneManager.LoadScene(asset, SceneMode);
				yield return null;
				lastLoadedScene = asset;
			}
			// cameras might have changed
			fadeEffects = ScreenFade.AttachToAllCameras();
			foreach (ScreenFade fade in fadeEffects)
			{
				fade.FadeColour = FadeColour;
				fade.FadeFactor = 1;
			}
			foreach (var asset in ScenesToUnload)
			{
				SceneManager.UnloadSceneAsync(asset);
				yield return null;
			}
			if (lastLoadedScene != "")
			{
				SceneManager.SetActiveScene(SceneManager.GetSceneByName(lastLoadedScene));
			}
			
			while (fadeFactor > 0)
			{
				fadeFactor -= Time.deltaTime / FadeTime;
				foreach (ScreenFade fade in fadeEffects)
				{
					fade.FadeFactor = fadeFactor;
				}
				yield return null;
			}

			foreach (ScreenFade fade in fadeEffects)
			{
				GameObject.Destroy(fade);
			}
		}
	}
}

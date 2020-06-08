using UnityEngine;
using UnityEngine.SceneManagement;

public class Timekeeper : MonoBehaviour
{
    // Start is called before the first frame update
    public void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // called second
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoaded: " + scene.name);
        Debug.Log(mode);
        Light[] lights = FindObjectsOfType<Light>();
        foreach(Light l in lights)
		{
            if (l.type == LightType.Directional)
			{
                // adapt settings
                Debug.Log("fiddling with light sournce " + l.name);
			}
        }
    }
}

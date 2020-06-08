using UnityEngine;


[RequireComponent(typeof(AudioSource))]
public class SoundOnCollision : MonoBehaviour
{
    public AudioSource sound;

    public void Start()
    {
        if (sound == null)
        {
            sound = GetComponent<AudioSource>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        sound.Play();
    }
    
    private void OnTriggerExit(Collider other)
    {
        sound.Stop();
    }



    void Update()
    {
        // 
    }
}

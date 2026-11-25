using UnityEngine;

public class SomBotao : MonoBehaviour
{
    public AudioSource fonte;
    public AudioClip som;
    [Range(0f, 1f)] public float volume = 1f;

    public void Tocar()
    {
        if (fonte != null && som != null)
            fonte.PlayOneShot(som, volume);
    }
}
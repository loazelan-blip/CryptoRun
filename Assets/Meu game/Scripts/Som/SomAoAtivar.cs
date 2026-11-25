using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SomAoAtivar : MonoBehaviour
{
    public AudioClip somParaTocar;
    [Range(0f, 1f)] public float volume = 1f;

    private AudioSource audioSource;

    private void Awake()
    {
        // Pega ou adiciona o AudioSource no objeto
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Garante que é som 2D (perfeito para Canvas/UI)
        audioSource.spatialBlend = 0f;
        audioSource.playOnAwake = false;
    }

    private void OnEnable()
    {
        if (somParaTocar != null && audioSource != null)
        {
            audioSource.PlayOneShot(somParaTocar, volume);
        }
    }
}
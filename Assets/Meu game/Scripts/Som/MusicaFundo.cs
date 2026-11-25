using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MusicaFundo : MonoBehaviour
{
    public AudioMixer mixer;
    public Slider slider;
    const string parametro = "VolumeMusica";

    void Start()
    {
        float v = PlayerPrefs.GetFloat(parametro, 0.8f);
        slider.value = v;
        Aplicar(v);
        slider.onValueChanged.AddListener(Aplicar);
    }

    public void Aplicar(float v)
    {
        mixer.SetFloat(parametro, Mathf.Log10(Mathf.Clamp(v, 0.0001f, 1f)) * 20f);
        PlayerPrefs.SetFloat(parametro, v);
    }
}
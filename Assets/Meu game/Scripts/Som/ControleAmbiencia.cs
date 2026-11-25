using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class ControleAmbiencia : MonoBehaviour
{
    public AudioMixer mixer;
    public Slider slider;
    const string chave = "VolumeAmbiencia";
    string[] parametros = { "VolumeAmbiencia", "VolumeCarro" };

    void Start()
    {
        float v = PlayerPrefs.GetFloat(chave, 0.8f);
        slider.value = v;
        Aplicar(v);
        slider.onValueChanged.AddListener(Aplicar);
    }

    public void Aplicar(float v)
    {
        float db = Mathf.Log10(Mathf.Clamp(v, 0.0001f, 1f)) * 20f;
        foreach (string p in parametros)
            mixer.SetFloat(p, db);
        PlayerPrefs.SetFloat(chave, v);
    }
}
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

[RequireComponent(typeof(Slider))]
public class SliderVolume : MonoBehaviour
{
    public AudioMixer mixer;
    public string parametro = "VolumeMusica";

    private Slider slider;

    void Awake()
    {
        slider = GetComponent<Slider>();
    }

    void OnEnable()
    {
        slider.SetValueWithoutNotify(PlayerPrefs.GetFloat(parametro, 1f));
        slider.onValueChanged.AddListener(Mudou);
    }

    void OnDisable()
    {
        slider.onValueChanged.RemoveListener(Mudou);
        PlayerPrefs.Save();
    }

    void Mudou(float valor)
    {
        GerenciadorAudio.Aplicar(mixer, parametro, valor);
        PlayerPrefs.SetFloat(parametro, valor);
    }
}
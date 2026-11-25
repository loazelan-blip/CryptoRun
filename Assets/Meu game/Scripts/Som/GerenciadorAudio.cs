using UnityEngine;
using UnityEngine.Audio;

public class GerenciadorAudio : MonoBehaviour
{
    public AudioMixer mixer;
    public AudioMixerGroup grupoAmbiencia;
    public AudioMixerGroup grupoCarro;

    public static AudioMixerGroup GrupoAmbiencia;
    public static AudioMixerGroup GrupoCarro;

    static readonly string[] parametros = { "VolumeMusica", "VolumeAmbiencia", "VolumeCarro" };

    void Awake()
    {
        GrupoAmbiencia = grupoAmbiencia;
        GrupoCarro = grupoCarro;
    }

    void Start()
    {
        // Aplica os volumes salvos assim que o jogo começa
        foreach (string p in parametros)
            Aplicar(mixer, p, PlayerPrefs.GetFloat(p, 1f));
    }

    // Converte o valor do slider (0.0001 a 1) para decibéis
    public static void Aplicar(AudioMixer m, string parametro, float valor)
    {
        if (m == null) return;
        m.SetFloat(parametro, Mathf.Log10(Mathf.Clamp(valor, 0.0001f, 1f)) * 20f);
    }
}
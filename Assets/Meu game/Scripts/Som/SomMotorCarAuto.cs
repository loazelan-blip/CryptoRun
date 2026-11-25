using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SomMotorCarAuto : MonoBehaviour
{
    [Header("Conexão com o Carro")]
    [Tooltip("Arraste o objeto do carro que possui o script CarAutomatico.")]
    public CarAutomatico carro;

    [Header("Configurações de Áudio")]
    [Tooltip("AudioSource com o clipe de som do motor em Loop.")]
    public AudioSource audioSourceMotor;

    [Header("Ajuste do Tom (Pitch)")]
    [Tooltip("Pitch do som com o carro parado/lenta (ex: 0.7 ou 0.8).")]
    public float pitchMinimo = 0.75f;

    [Tooltip("Pitch do som no limite de velocidade (ex: 2.0 ou 2.2).")]
    public float pitchMaximo = 2.1f;

    void Start()
    {
        // Se não atribuiu manualmente, tenta pegar no próprio objeto
        if (carro == null)
        {
            carro = GetComponent<CarAutomatico>();
        }

        if (audioSourceMotor == null)
        {
            audioSourceMotor = GetComponent<AudioSource>();
        }

        // Garante que o som comece tocando em loop
        if (audioSourceMotor != null)
        {
            audioSourceMotor.loop = true;
            if (!audioSourceMotor.isPlaying)
            {
                audioSourceMotor.Play();
            }
        }
    }

    void Update()
    {
        if (carro == null || audioSourceMotor == null) return;

        // Pega a velocidade atual ignorando o sinal (funciona pra frente e pra ré)
        float velAtualAbsoluta = Mathf.Abs(carro.velocidadeAtual);

        // Se o limite de velocidade for zero ou inválido, mantém no pitch mínimo
        float limiteVelocidade = carro.veloxLimit > 0 ? carro.veloxLimit : 1f;

        // Normaliza entre 0.0 (parado) e 1.0 (velocidade máxima)
        float fatorVelocidade = Mathf.Clamp01(velAtualAbsoluta / limiteVelocidade);

        // Calcula o tom de forma proporcional
        float pitchAlvo = Mathf.Lerp(pitchMinimo, pitchMaximo, fatorVelocidade);

        // Aplica no áudio do motor
        audioSourceMotor.pitch = pitchAlvo;
    }
}
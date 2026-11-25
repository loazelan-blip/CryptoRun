using UnityEngine;

public class SomBatidaTraseiraCarro : MonoBehaviour
{
    [Header("Conexão Direta com o Carro")]
    [Tooltip("Arraste aqui o script CarAutomatico do seu carro")]
    public CarAutomatico carro;

    [Header("Sons de Batida Traseira")]
    [Tooltip("Som para impacto na ré em baixa velocidade")]
    public AudioClip batidaLeve;
    [Range(0f, 1f)] public float volumeLeve = 0.03f;

    [Space(5)]
    [Tooltip("Som para impacto na ré em velocidade média")]
    public AudioClip batidaMedia;
    [Range(0f, 1f)] public float volumeMedia = 0.53f;

    [Space(5)]
    [Tooltip("Som para impacto na ré em alta velocidade")]
    public AudioClip batidaPesada;
    [Range(0f, 1f)] public float volumePesada = 1.0f;

    [Header("Configuração de Velocidade das Batidas")]
    [Tooltip("Velocidade máxima de referência do carro. O script dividirá esse valor por 3 para definir Leve, Média e Pesada.")]
    public float velocidadeMaximaReferencia = 30f;

    [Header("Configurações do Sensor Traseiro (Raycast)")]
    [Tooltip("Offset/Posição de onde o raio sai (traseira/para-choque traseiro)")]
    public Vector3 posicaoSensor = new Vector3(0f, 0.5f, -1.5f);

    [Tooltip("Distância máxima que o raio alcança para detectar objetos atrás")]
    public float distanciaSensor = 3.0f;

    [Tooltip("Camadas de objetos que o sensor deve detectar")]
    public LayerMask camadasObstaculos = ~0;

    [Header("Controle de Disparo")]
    [Tooltip("Tempo mínimo em segundos entre um disparo de som e outro")]
    public float tempoMinimoEntreBatidas = 0.4f;

    private AudioSource audioSourceBatidas;
    private float ultimoTempoBatida;

    private void Awake()
    {
        // Cria um AudioSource dedicado exclusivo para as batidas traseiras sem alterar o tom do motor
        audioSourceBatidas = gameObject.AddComponent<AudioSource>();
        audioSourceBatidas.spatialBlend = 0f;
        audioSourceBatidas.playOnAwake = false;
        audioSourceBatidas.pitch = 1.0f; // Tom fixo original
    }

    private void Start()
    {
        // Liga o som das batidas traseiras ao grupo Carro do Mixer
        if (GerenciadorAudio.GrupoCarro != null)
            audioSourceBatidas.outputAudioMixerGroup = GerenciadorAudio.GrupoCarro;
    }

    private void Update()
    {
        VerificarSensorImpacto();
    }

    private void VerificarSensorImpacto()
    {
        // Respeita o intervalo mínimo de tempo
        if (Time.time - ultimoTempoBatida < tempoMinimoEntreBatidas) return;

        Vector3 origemRaio = transform.TransformPoint(posicaoSensor);
        Vector3 direcaoRaio = -transform.forward; // Aponta diretamente para trás do carro

        // Dispara o Raycast na traseira do carro
        if (Physics.Raycast(origemRaio, direcaoRaio, out RaycastHit hit, distanciaSensor, camadasObstaculos))
        {
            // Ignora o próprio carro
            if (hit.transform.IsChildOf(transform) || hit.transform == transform) return;

            ultimoTempoBatida = Time.time;

            // Lê O NÚMERO REAL da variável do script CarAutomatico
            float velocidadeLida = 0f;

            if (carro != null)
            {
                velocidadeLida = Mathf.Abs(carro.velocidadeAtual);
            }
            else
            {
                Debug.LogWarning("[Batida Traseira] O script CarAutomatico NÃO foi arrastado para o Inspector!");
            }

            ProcessarETocarBatida(velocidadeLida);
        }
    }

    private void ProcessarETocarBatida(float velocidade)
    {
        audioSourceBatidas.pitch = 1.0f; // Garante o tom original do áudio

        // Divide o valor máximo de referência em 3 terços
        float terco = velocidadeMaximaReferencia / 3f;

        AudioClip clipParaTocar = null;
        float volumeParaTocar = 1f;
        string tipoBatida = "";

        if (velocidade < terco)
        {
            clipParaTocar = batidaLeve;
            volumeParaTocar = volumeLeve;
            tipoBatida = "LEVE";
        }
        else if (velocidade < terco * 2f)
        {
            clipParaTocar = batidaMedia;
            volumeParaTocar = volumeMedia;
            tipoBatida = "MÉDIA";
        }
        else
        {
            clipParaTocar = batidaPesada;
            volumeParaTocar = volumePesada;
            tipoBatida = "PESADA";
        }

        if (clipParaTocar != null)
        {
            audioSourceBatidas.PlayOneShot(clipParaTocar, volumeParaTocar);
            Debug.Log($"[Batida Traseira] Acionada batida <b>{tipoBatida}</b> | Velocidade real do carro: {velocidade:F1} | Volume: {volumeParaTocar}");
        }
        else
        {
            Debug.LogWarning($"[Batida Traseira] Batida <b>{tipoBatida}</b> acionada, mas o AudioClip não foi atribuído no Inspector!");
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Desenha a linha verde na traseira no Scene View para fácil ajuste
        Gizmos.color = Color.green;
        Vector3 origem = transform.TransformPoint(posicaoSensor);
        Vector3 destino = origem + (-transform.forward * distanciaSensor);

        Gizmos.DrawLine(origem, destino);
        Gizmos.DrawSphere(origem, 0.05f);
        Gizmos.DrawSphere(destino, 0.08f);
    }
}
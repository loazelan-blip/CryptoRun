using UnityEngine;

public class SomRaspaParede : MonoBehaviour
{
    [Header("Conexão Direta com o Carro")]
    [Tooltip("Arraste aqui o script CarAutomatico do seu carro")]
    public CarAutomatico carro;

    [Tooltip("Velocidade mínima do carro para permitir o som de raspão")]
    public float velocidadeMinimaParaSom = 0.2f;

    [Header("Som do Raspão")]
    [Tooltip("Som de raspão (deve ser um loop contínuo)")]
    public AudioClip somRaspao;

    [Range(0f, 1f)]
    [Tooltip("Volume do som")]
    public float volumeSom = 0.8f;

    [Header("Ajustes da Área do Sensor (Sem Collider)")]
    [Tooltip("Posição do centro da caixa em relação ao carro (X, Y, Z)")]
    public Vector3 centroSensor = new Vector3(0f, 0.5f, 0f);

    [Tooltip("Tamanho/Escala total da caixa (X = Largura, Y = Altura, Z = Comprimento)")]
    public Vector3 tamanhoSensor = new Vector3(2f, 1f, 4f);

    [Tooltip("Camadas de objetos que o sensor deve detectar")]
    public LayerMask camadasObstaculos = ~0;

    private AudioSource audioSource;

    private void Awake()
    {
        // Configura o AudioSource para reprodução contínua em loop
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f; // Som 2D limpo
        audioSource.playOnAwake = false;
        audioSource.loop = true; // Permite cortar e manter em execução contínua
        audioSource.clip = somRaspao;
    }

    private void Start()
    {
        // Liga o som do raspão ao grupo Carro do Mixer
        if (GerenciadorAudio.GrupoCarro != null)
            audioSource.outputAudioMixerGroup = GerenciadorAudio.GrupoCarro;
    }

    private void Update()
    {
        VerificarEAtivarSom();
    }

    private void VerificarEAtivarSom()
    {
        // 1. Atualiza o clipe caso tenha mudado no Inspector
        if (audioSource.clip != somRaspao)
        {
            audioSource.clip = somRaspao;
        }

        // 2. Validação da presença e velocidade do carro
        bool carroEmMovimento = false;
        if (carro != null)
        {
            carroEmMovimento = Mathf.Abs(carro.velocidadeAtual) >= velocidadeMinimaParaSom;
        }
        else
        {
            PararSom();
            return;
        }

        // Se o carro estiver parado, corta o som na hora
        if (!carroEmMovimento)
        {
            PararSom();
            return;
        }

        // 3. Detecção da caixa invisível (OverlapBox)
        Vector3 centroMundo = transform.TransformPoint(centroSensor);
        Vector3 metadeTamanho = tamanhoSensor / 2f;
        Quaternion rotacao = transform.rotation;

        Collider[] objetosDetectados = Physics.OverlapBox(centroMundo, metadeTamanho, rotacao, camadasObstaculos);

        bool estaEncostandoEmAlgo = false;

        foreach (Collider col in objetosDetectados)
        {
            // Ignora o próprio carro e seus filhos
            if (col.transform.IsChildOf(transform) || col.transform == transform) continue;

            // Se detectou algum objeto válido na área
            estaEncostandoEmAlgo = true;
            break;
        }

        // 4. Controle estrito de Play / Stop instantâneo
        if (estaEncostandoEmAlgo && somRaspao != null)
        {
            // Ajusta o pitch dinamicamente com base na velocidade
            float velocidadeRatio = Mathf.Clamp01(Mathf.Abs(carro.velocidadeAtual) / 30f);
            audioSource.pitch = Mathf.Lerp(0.85f, 1.15f, velocidadeRatio);
            audioSource.volume = volumeSom;

            // Inicia o som se ainda não estiver tocando
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
        else
        {
            // Perdeu contato com o objeto: corta o som imediatamente
            PararSom();
        }
    }

    private void PararSom()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.25f);
        Matrix4x4 matrizMundo = Matrix4x4.TRS(transform.TransformPoint(centroSensor), transform.rotation, Vector3.one);
        Gizmos.matrix = matrizMundo;

        Gizmos.DrawCube(Vector3.zero, tamanhoSensor);
        Gizmos.color = Color.orange;
        Gizmos.DrawWireCube(Vector3.zero, tamanhoSensor);
    }
}
using UnityEngine;

public class SomSensorNPC : MonoBehaviour
{
    [Header("Som do NPC")]
    [Tooltip("Som que o NPC vai tocar ao detectar o automóvel com a tag Player")]
    public AudioClip somNPC;

    [Range(0f, 1f)]
    [Tooltip("Volume do som do NPC")]
    public float volumeSom = 0.8f;

    [Header("Ajustes de Alcance do Som 3D")]
    [Tooltip("Distância (em metros) onde o som do NPC para de ser ouvido completamente.")]
    [Min(0f)]
    public float distanciaMaximaDeAudio = 35f;

    [Header("Regras de Disparo")]
    [Tooltip("Se marcado, o sensor reseta quando o automóvel sai da área (toca de novo a cada encontro). Se desmarcado, toca 1 única vez em todo o jogo.")]
    public bool resetarSeSairDaArea = true;

    [Header("Tag que aciona o som")]
    public string tagPlayer = "Player";

    [Header("Ajustes do Sensor Invisível (Sem Collider)")]
    [Tooltip("Posição do centro do sensor em relação ao NPC (X, Y, Z). Deixe em volta do corpo do NPC.")]
    public Vector3 centroSensor = new Vector3(0f, 1f, 0f);

    [Tooltip("Tamanho/Escala da caixa do sensor (X = Largura, Y = Altura, Z = Comprimento). Deixe um pouco maior que o corpo do NPC.")]
    public Vector3 tamanhoSensor = new Vector3(1.2f, 2f, 1.2f);

    [Tooltip("Camadas de objetos que o NPC deve detectar")]
    public LayerMask camadasObstaculos = ~0;

    private AudioSource audioSource;
    private bool jaTocouSom = false;

    private void Awake()
    {
        // Configura o AudioSource exclusivo do NPC em 3D
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f; // Som 3D
        audioSource.minDistance = 2f;
        audioSource.maxDistance = distanciaMaximaDeAudio;
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
    }

    private void Start()
    {
        // Liga o som do NPC ao grupo Ambiencia do Mixer
        if (GerenciadorAudio.GrupoAmbiencia != null)
            audioSource.outputAudioMixerGroup = GerenciadorAudio.GrupoAmbiencia;
    }

    private void Update()
    {
        VerificarAreaSensor();
    }

    private void VerificarAreaSensor()
    {
        // Se já tocou e a opção de resetar estiver desligada, encerra o script para economizar processamento
        if (jaTocouSom && !resetarSeSairDaArea) return;

        // 1. Calcula a área matemática da caixa invisível (OverlapBox)
        Vector3 centroMundo = transform.TransformPoint(centroSensor);
        Vector3 metadeTamanho = tamanhoSensor / 2f;
        Quaternion rotacao = transform.rotation;

        Collider[] objetosDetectados = Physics.OverlapBox(centroMundo, metadeTamanho, rotacao, camadasObstaculos);

        bool estaDetectandoPlayer = false;

        foreach (Collider col in objetosDetectados)
        {
            // Ignora o próprio NPC e seus filhos
            if (col.transform.IsChildOf(transform) || col.transform == transform) continue;

            // Só conta se o objeto detectado possui a tag "Player"
            if (col.CompareTag(tagPlayer))
            {
                estaDetectandoPlayer = true;

                // Mostra no Console quem disparou o som (para achar o que está acionando)
                if (!jaTocouSom)
                {
                    Debug.Log("[SomSensorNPC] " + name + " detectou o objeto '" + col.name + "' (tag " + col.tag + ", pai: " + (col.transform.parent != null ? col.transform.parent.name : "nenhum") + ")", col);
                }
                break;
            }
        }

        // 2. Disparo do Som
        if (estaDetectandoPlayer)
        {
            if (!jaTocouSom && somNPC != null)
            {
                jaTocouSom = true;
                audioSource.PlayOneShot(somNPC, volumeSom);
            }
        }
        else
        {
            // Se a área esvaziar e a opção de resetar estiver ativa, permite um novo disparo no futuro
            if (resetarSeSairDaArea)
            {
                jaTocouSom = false;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Desenha a caixa do sensor no Scene View
        Gizmos.color = new Color(1f, 0.92f, 0.01f, 0.25f); // Amarelo transparente
        Matrix4x4 matrizMundo = Matrix4x4.TRS(transform.TransformPoint(centroSensor), transform.rotation, Vector3.one);
        Gizmos.matrix = matrizMundo;

        Gizmos.DrawCube(Vector3.zero, tamanhoSensor);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(Vector3.zero, tamanhoSensor);

        // Desenha a esfera visual de alcance máximo do áudio
        if (distanciaMaximaDeAudio > 0)
        {
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = new Color(0f, 0.5f, 1f, 0.1f); // Azul claro transparente
            Gizmos.DrawSphere(transform.position, distanciaMaximaDeAudio);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, distanciaMaximaDeAudio);
        }
    }
}
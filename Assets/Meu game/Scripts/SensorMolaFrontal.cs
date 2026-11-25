using UnityEngine;

public class SensorMolaFrontal : MonoBehaviour
{
    [Header("Referências")]
    public CarAutomatico carro;
    private Rigidbody rb;

    [Header("Ajustes do Sensor")]
    public Vector3 posicaoSensor = new Vector3(0f, 0.8f, 1.8f);
    public Vector3 tamanhoSensor = new Vector3(1.4f, 0.6f, 0.3f);

    [Header("Configurações do Efeito Mola (Impacto)")]
    [Range(0.1f, 2.0f)] public float forcaDeRetorno = 0.8f;

    [Tooltip("Força que joga o carro para cima na batida, simulando o pulo do chassi.")]
    public float forcaSubidaBatida = 4f;

    [Tooltip("Força que levanta o bico do carro para trás (efeito de amasso/empinada).")]
    public float forcaLevantarBico = 2f;

    public float tempoBloqueioControle = 0.25f;

    [Header("Filtros de Detecção")]
    public LayerMask camadaObstaculos = ~0;

    [Header("Visualização")]
    public Color corSensorGizmo = Color.cyan;

    private float temporizadorBloqueio = 0f;

    void Start()
    {
        if (carro == null) carro = GetComponent<CarAutomatico>();
        if (carro != null) rb = carro.GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (carro == null || rb == null) return;

        if (temporizadorBloqueio > 0f)
        {
            temporizadorBloqueio -= Time.deltaTime;
            return;
        }

        // Só ativa se estiver andando para frente rápido o suficiente
        if (carro.velocidadeAtual > 2f)
        {
            Vector3 centroMundo = transform.TransformPoint(posicaoSensor);
            Collider[] hits = Physics.OverlapBox(centroMundo, tamanhoSensor / 2f, transform.rotation, camadaObstaculos);

            foreach (Collider hit in hits)
            {
                if (hit.transform.root == transform.root) continue;

                // DIAGNÓSTICO: mostra em que objeto o sensor "bateu"
                Debug.Log("[SensorMola] BATEU em '" + hit.name + "' | trigger = " + hit.isTrigger
                          + " | velocidade = " + carro.velocidadeAtual.ToString("F1"), hit.gameObject);

                AplicarBatida();
                break;
            }
        }
    }

    // Efeito da batida (antes ficava dentro do Update). Agora pode ser chamado de fora.
    public void AplicarBatida()
    {
        if (carro == null || rb == null) return;
        if (temporizadorBloqueio > 0f) return;

        // 1. Inverte a velocidade do motor para trás
        carro.velocidadeAtual = -carro.velocidadeAtual * forcaDeRetorno;
        carro.marchaAtual = 1;
        carro.cronometro = 0f;

        // 2. Aplica o impacto físico (Pulo e Rotação)
        // Zeramos a velocidade Y atual para garantir que o pulo seja sempre consistente
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // Joga o carro para cima (VelocityChange ignora a massa, facilitando o ajuste)
        rb.AddForce(Vector3.up * forcaSubidaBatida, ForceMode.VelocityChange);

        // Levanta o bico do carro (rotaciona no eixo X local)
        rb.AddTorque(-transform.right * forcaLevantarBico, ForceMode.VelocityChange);

        // 3. Bloqueia os controles temporariamente
        temporizadorBloqueio = tempoBloqueioControle;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = corSensorGizmo;
        Vector3 centroMundo = transform.TransformPoint(posicaoSensor);
        Gizmos.matrix = Matrix4x4.TRS(centroMundo, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, tamanhoSensor);
    }
}
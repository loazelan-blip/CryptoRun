using UnityEngine;

public class MelvemSensorNPCTraseiro : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Ajustes do Sensor MELVEM")]
    public Vector3 posicaoSensor = new Vector3(0f, 0.8f, -1.8f);
    public Vector3 tamanhoSensor = new Vector3(1.4f, 0.6f, 0.3f);

    [Header("Gatilho de Velocidade do Transform")]
    [Tooltip("Coloque aqui a velocidade limite lida no Transform. Se a velocidade do Player passar deste número, o impacto dispara.")]
    public float limiteVelocidadeTransform = 2.0f;

    [Header("Força Fixa do Impacto (Definida no Inspector)")]
    [Tooltip("Força exata de empurrão para trás quando passar do limite de velocidade.")]
    public float forcaEmpurraoFixa = 12.0f;

    [Tooltip("Força exata do pulo (eixo Y) quando passar do limite.")]
    public float forcaPuloFixa = 3.0f;

    [Tooltip("Torque exato que levanta a traseira/bico do carro.")]
    public float forcaTorqueFixa = 2.0f;

    [Tooltip("Tempo que o sensor fica bloqueado após disparar o impacto.")]
    public float tempoBloqueio = 0.4f;

    [Header("Filtros de Detecção")]
    public string tagAlvo = "Player";
    public LayerMask camadaObstaculos = ~0;

    [Header("Visualização")]
    public Color corSensorGizmo = Color.yellow;

    private Vector3 posicaoPlayerUltimoFrame;
    private Transform playerTransform;
    private float temporizadorBloqueio = 0f;

    void Start()
    {
        // Procura o Rigidbody neste objeto, depois nos pais, depois nos filhos
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = GetComponentInParent<Rigidbody>();
        if (rb == null) rb = GetComponentInChildren<Rigidbody>();
    }

    void Update()
    {
        if (rb == null) return;

        if (temporizadorBloqueio > 0f)
        {
            temporizadorBloqueio -= Time.deltaTime;
            return;
        }

        Vector3 centroMundo = transform.TransformPoint(posicaoSensor);
        Collider[] hits = Physics.OverlapBox(centroMundo, tamanhoSensor / 2f, transform.rotation, camadaObstaculos);

        foreach (Collider hit in hits)
        {
            if (hit.transform.root == transform.root) continue;

            if (hit.CompareTag(tagAlvo) || hit.transform.root.CompareTag(tagAlvo))
            {
                Transform atualPlayer = hit.transform.root;

                if (playerTransform != atualPlayer)
                {
                    playerTransform = atualPlayer;
                    posicaoPlayerUltimoFrame = playerTransform.position;
                    continue;
                }

                float velocidadeTransform = (playerTransform.position - posicaoPlayerUltimoFrame).magnitude / Time.deltaTime;
                posicaoPlayerUltimoFrame = playerTransform.position;

                if (velocidadeTransform >= limiteVelocidadeTransform)
                {
                    AplicarImpactoFixo();
                    break;
                }
            }
        }
    }

    private void AplicarImpactoFixo()
    {
        Vector3 direcaoEmpurrao = transform.forward;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.AddForce(direcaoEmpurrao * forcaEmpurraoFixa, ForceMode.VelocityChange);
        rb.AddForce(Vector3.up * forcaPuloFixa, ForceMode.VelocityChange);
        rb.AddTorque(transform.right * forcaTorqueFixa, ForceMode.VelocityChange);

        temporizadorBloqueio = tempoBloqueio;

        this.enabled = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = corSensorGizmo;
        Vector3 centroMundo = transform.TransformPoint(posicaoSensor);
        Gizmos.matrix = Matrix4x4.TRS(centroMundo, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, tamanhoSensor);
    }
}
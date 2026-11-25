using UnityEngine;

public class AutoDescapotavel : MonoBehaviour
{
    [Header("Referências")]
    public CarAutomatico carro;
    private Rigidbody rb;

    [Header("Detecção de Capotamento")]
    [Tooltip("Inclinação limite (1 = em pé, 0 = de lado, -1 = de ponta-cabeça).")]
    [Range(-1f, 0.5f)] public float limiteInclinacao = 0.2f;

    [Tooltip("Velocidade máxima do carro para considerar que está parado/preso capotado.")]
    public float velocidadeMaximaParaAtivar = 2f;

    [Tooltip("Tempo que o carro precisa ficar capotado e parado antes de agir.")]
    public float tempoEsperaParaAtivar = 1f;

    [Header("Força de Suavização")]
    [Tooltip("Força do torque aplicado para desvirar o carro no chão.")]
    public float forçaTorqueDesvirar = 80f;

    [Tooltip("Impulso para cima suave (só dispara se estiver parado e totalmente travado).")]
    public float puloSuaveAuxiliar = 2.5f;

    [Header("Monitoramento")]
    public bool estaCapotado = false;
    public float cronometroInclinado = 0f;
    public bool podeAplicarPulo = true;

    void Start()
    {
        if (carro == null) carro = GetComponent<CarAutomatico>();
        if (carro != null) rb = carro.GetComponent<Rigidbody>();
        if (rb == null) rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        // 1. Calcula o quão "em pé" o carro está em relação ao mundo
        float alinhamentoVertical = Vector3.Dot(transform.up, Vector3.up);
        bool estaParadoOuLento = rb.linearVelocity.magnitude <= velocidadeMaximaParaAtivar;

        // 2. Se estiver virado de lado/teto E com velocidade baixa
        if (alinhamentoVertical < limiteInclinacao && estaParadoOuLento)
        {
            cronometroInclinado += Time.fixedDeltaTime;

            if (cronometroInclinado >= tempoEsperaParaAtivar)
            {
                estaCapotado = true;
                DesvirarComFisicaSuave();
            }
        }
        else
        {
            // Reseta contadores quando o carro volta para a orientação correta ou está correndo
            if (alinhamentoVertical >= limiteInclinacao + 0.1f)
            {
                cronometroInclinado = 0f;
                estaCapotado = false;
                podeAplicarPulo = true;
            }
        }
    }

    private void DesvirarComFisicaSuave()
    {
        // 1. Dá apenas um pulinho leve inicial se o carro estiver preso
        if (podeAplicarPulo)
        {
            // Amortece rotações descontroladas antes do levante
            rb.angularVelocity *= 0.2f;

            // Aplica impulso vertical suave
            rb.AddForce(Vector3.up * puloSuaveAuxiliar, ForceMode.Impulse);
            podeAplicarPulo = false;
        }

        // 2. Calcula o vetor de torque necessário para alinhar o teto (Up) com o Up do mundo
        Vector3 eixoDeGiro = Vector3.Cross(transform.up, Vector3.up);

        // 3. Aplica o torque usando a física do Rigidbody
        rb.AddTorque(eixoDeGiro * forçaTorqueDesvirar, ForceMode.Acceleration);

        // 4. Amortecimento constante de rotação nos eixos errados
        Vector3 velAngularLocal = transform.InverseTransformDirection(rb.angularVelocity);
        velAngularLocal.z *= 0.85f; // Estabiliza a rolagem
        rb.angularVelocity = transform.TransformDirection(velAngularLocal);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = estaCapotado ? Color.red : Color.green;
        Gizmos.DrawRay(transform.position, transform.up * 2f);
    }
}
using UnityEngine;

public class DetectorRodaNPC : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Configuração das Rodas 3D")]
    public Transform rodaDianteiraEsquerda;
    public Transform rodaDianteiraDireita;
    public Transform rodaTraseiraEsquerda;
    public Transform rodaTraseiraDireita;

    [Header("Ajustes de Detecção")]
    [Tooltip("Comprimento do raio. Se o chão estiver mais longe que este valor, a força liga.")]
    [Range(0.1f, 10.0f)]
    public float distanciaLimiteChao = 0.5f;
    public LayerMask camadaChao = ~0;

    [Header("Mecânica do Ímã Individual")]
    [Tooltip("Força exercida para baixo quando a roda perde o contato com o chão.")]
    public float forcaInicialIma = 20f;

    [Header("Alinhamento Suave (Físico)")]
    [Tooltip("Força do torque para desvirar o carro fisicamente sem quebrar a colisão.")]
    public float forcaTorqueDesviro = 50.0f;

    [Tooltip("Ângulo limite de tombo lateral para acionar o alinhamento.")]
    public float anguloAtivacaoDesviro = 20f;

    public float amortecimentoGiro = 1.5f;

    [Header("Monitoramento")]
    public float anguloInclinacao;
    public bool precisaAlinhar;
    public bool capotadoDePontaCabeca;
    public int rodasNoAr;

    void Start()
    {
        // Procura o Rigidbody neste objeto, depois nos pais, depois nos filhos
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = GetComponentInParent<Rigidbody>();
        if (rb == null) rb = GetComponentInChildren<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        // Orientação real do corpo que tem a física
        Transform corpo = rb.transform;

        // 1. Checa se o raio de cada roda está tocando o chão
        bool chaoFLEsq = ChecarRodaNoChao(rodaDianteiraEsquerda, corpo);
        bool chaoRLEsq = ChecarRodaNoChao(rodaTraseiraEsquerda, corpo);
        bool chaoFRDir = ChecarRodaNoChao(rodaDianteiraDireita, corpo);
        bool chaoRRDir = ChecarRodaNoChao(rodaTraseiraDireita, corpo);

        // Conta quantas rodas estão fora do chão
        rodasNoAr = 0;
        if (!chaoFLEsq) rodasNoAr++;
        if (!chaoRLEsq) rodasNoAr++;
        if (!chaoFRDir) rodasNoAr++;
        if (!chaoRRDir) rodasNoAr++;

        bool todasRodasNoAr = (rodasNoAr == 4);

        // 2. Calcula a inclinação total em relação ao topo do mundo
        anguloInclinacao = Vector3.Angle(corpo.up, Vector3.up);
        capotadoDePontaCabeca = anguloInclinacao > 90f;
        precisaAlinhar = anguloInclinacao > anguloAtivacaoDesviro;

        // 3. Aplicação individual do Ímã nas rodas no ar
        AplicarImaRoda(rodaDianteiraEsquerda, chaoFLEsq);
        AplicarImaRoda(rodaTraseiraEsquerda, chaoRLEsq);
        AplicarImaRoda(rodaDianteiraDireita, chaoFRDir);
        AplicarImaRoda(rodaTraseiraDireita, chaoRRDir);

        // 4. Sistema Físico de Desviro / Alinhamento (Torque)
        if (precisaAlinhar)
        {
            // Reduz o giro descontrolado
            rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, Time.fixedDeltaTime * 5f);

            // Calcula o vetor de torque necessário para apontar o eixo UP do carro para cima
            Vector3 vetorCruzado = Vector3.Cross(corpo.up, Vector3.up);
            rb.AddTorque(vetorCruzado * forcaTorqueDesviro, ForceMode.Acceleration);
        }
        else if (todasRodasNoAr)
        {
            // Amortecimento quando o carro está saltando reto
            rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, Time.fixedDeltaTime * amortecimentoGiro);
        }
    }

    private bool ChecarRodaNoChao(Transform roda, Transform corpo)
    {
        if (roda == null) return true;

        // Se o carro não estiver totalmente de ponta-cabeça, faz a checagem dupla
        if (!capotadoDePontaCabeca)
        {
            return Physics.Raycast(roda.position, Vector3.down, distanciaLimiteChao, camadaChao) ||
                   Physics.Raycast(roda.position, -corpo.up, distanciaLimiteChao, camadaChao);
        }

        // Se estiver de cabeça para baixo, checa apenas em direção ao chão real do mundo
        return Physics.Raycast(roda.position, Vector3.down, distanciaLimiteChao, camadaChao);
    }

    private void AplicarImaRoda(Transform roda, bool estaNoChao)
    {
        if (roda == null) return;

        // Puxa a roda para baixo se perder contato
        if (!estaNoChao)
        {
            rb.AddForceAtPosition(Vector3.down * forcaInicialIma, roda.position, ForceMode.Acceleration);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Transform[] todasAsRodas = { rodaDianteiraEsquerda, rodaDianteiraDireita, rodaTraseiraEsquerda, rodaTraseiraDireita };
        Transform corpo = rb != null ? rb.transform : transform;

        Gizmos.color = Color.cyan;
        foreach (Transform roda in todasAsRodas)
        {
            if (roda != null)
            {
                Gizmos.DrawLine(roda.position, roda.position + Vector3.down * distanciaLimiteChao);
                Gizmos.DrawLine(roda.position, roda.position - corpo.up * distanciaLimiteChao);
            }
        }
    }
}
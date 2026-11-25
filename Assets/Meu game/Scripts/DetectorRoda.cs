using UnityEngine;

public class DetectorRoda : MonoBehaviour
{
    [Header("Referências")]
    public CarAutomatico carro;
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

    [Header("Alinhamento Suave (Transform)")]
    [Tooltip("Velocidade com que o carro desvira e se alinha de volta ao normal.")]
    public float velocidadeAlinhamento = 4.0f;

    [Tooltip("Ângulo limite de tombo lateral para acionar o alinhamento.")]
    public float anguloAtivacaoDesviro = 20f;

    public float amortecimentoGiro = 1.5f;

    [Header("Monitoramento")]
    public float anguloInclinacao;
    public bool precisaAlinhar;
    public bool capotadoDePontaCabeca;

    void Start()
    {
        if (carro == null) carro = GetComponent<CarAutomatico>();
        if (carro != null) rb = carro.GetComponent<Rigidbody>();
        if (rb == null) rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        // 1. Checa se o raio de cada roda está tocando o chão
        bool chaoFLEsq = ChecarRodaNoChao(rodaDianteiraEsquerda);
        bool chaoRLEsq = ChecarRodaNoChao(rodaTraseiraEsquerda);
        bool chaoFRDir = ChecarRodaNoChao(rodaDianteiraDireita);
        bool chaoRRDir = ChecarRodaNoChao(rodaTraseiraDireita);

        bool todasRodasNoAr = !chaoFLEsq && !chaoRLEsq && !chaoFRDir && !chaoRRDir;

        // 2. Calcula a inclinação total em relação ao topo do mundo
        anguloInclinacao = Vector3.Angle(transform.up, Vector3.up);
        capotadoDePontaCabeca = anguloInclinacao > 90f;
        precisaAlinhar = anguloInclinacao > anguloAtivacaoDesviro;

        // 3. Se ultrapassou o ângulo de tombo, aplica alinhamento por rotação
        if (precisaAlinhar)
        {
            rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, Time.fixedDeltaTime * 10f);

            Vector3 direcaoAtualfwd = transform.forward;
            direcaoAtualfwd.y = 0;
            if (direcaoAtualfwd.sqrMagnitude < 0.001f) direcaoAtualfwd = transform.forward;

            Quaternion rotacaoAlvo = Quaternion.LookRotation(direcaoAtualfwd, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacaoAlvo, Time.fixedDeltaTime * velocidadeAlinhamento);
        }
        else
        {
            // 4. LIGA A FORÇA SE A RODA SAIR DO CHÃO / DESLIGA SE A RODA ENCOSTAR
            AplicarImaRoda(rodaDianteiraEsquerda, chaoFLEsq);
            AplicarImaRoda(rodaTraseiraEsquerda, chaoRLEsq);
            AplicarImaRoda(rodaDianteiraDireita, chaoFRDir);
            AplicarImaRoda(rodaTraseiraDireita, chaoRRDir);

            if (todasRodasNoAr)
            {
                rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, Time.fixedDeltaTime * amortecimentoGiro);
            }
        }
    }

    private bool ChecarRodaNoChao(Transform roda)
    {
        if (roda == null) return true;

        // Retorna true se o raio encostar no chão (abaixo ou na orientação do carro)
        return Physics.Raycast(roda.position, Vector3.down, distanciaLimiteChao, camadaChao) ||
               Physics.Raycast(roda.position, -transform.up, distanciaLimiteChao, camadaChao);
    }

    private void AplicarImaRoda(Transform roda, bool estaNoChao)
    {
        if (roda == null) return;

        // Se NÃO estiver no chão (raio desconectou), ativa a força de puxão
        if (!estaNoChao)
        {
            rb.AddForceAtPosition(Vector3.down * forcaInicialIma, roda.position, ForceMode.Acceleration);
        }
        // Se estiver no chão, nenhuma força é aplicada (desligado)
    }

    private void OnDrawGizmosSelected()
    {
        Transform[] todasAsRodas = { rodaDianteiraEsquerda, rodaDianteiraDireita, rodaTraseiraEsquerda, rodaTraseiraDireita };

        Gizmos.color = Color.cyan;
        foreach (Transform roda in todasAsRodas)
        {
            if (roda != null)
            {
                Gizmos.DrawLine(roda.position, roda.position + Vector3.down * distanciaLimiteChao);
                Gizmos.DrawLine(roda.position, roda.position - transform.up * distanciaLimiteChao);
            }
        }
    }
}
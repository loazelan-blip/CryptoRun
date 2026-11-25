using UnityEngine;

public class DesativarPorCapotamento : MonoBehaviour
{
    [Header("Script de Movimento Alvo")]
    [Tooltip("Arrasta AQUI qualquer script de movimento do carro que deva ser desativado.")]
    public MonoBehaviour scriptMovimento;

    [Tooltip("Arrasta AQUI qualquer outro script adicional que também deva ser desativado.")]
    public MonoBehaviour scriptAdicional;

    [Header("Configurações do Capotamento")]
    [Tooltip("Ângulo limite para considerar virado (em graus). Valores entre 75 e 90 funcionam muito bem.")]
    [Range(45f, 135f)]
    public float anguloLimiteInclinacao = 80f;

    [Tooltip("Tempo em segundos sustentado de ponta-cabeça antes de desativar (evita disparar em pulos ou rampas).")]
    public float tempoSeguranca = 1.0f;

    [Header("Monitoramento")]
    public bool estaCapotado = false;
    public float anguloAtual = 0f;

    private float temporizadorCapotado = 0f;
    private bool jaDesativou = false;
    private Transform corpo;

    void Start()
    {
        // Mede a inclinação no objeto que tem o Rigidbody (neste objeto, nos pais ou nos filhos)
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) rb = GetComponentInParent<Rigidbody>();
        if (rb == null) rb = GetComponentInChildren<Rigidbody>();

        corpo = (rb != null) ? rb.transform : transform;
    }

    void Update()
    {
        if (jaDesativou) return;

        // Mede o ângulo entre o eixos 'up' (teto) do carro e o 'up' do mundo (céu)
        anguloAtual = Vector3.Angle(corpo.up, Vector3.up);

        // Se o ângulo passar do limite configurado (carro inclinado demais ou de ponta-cabeça)
        if (anguloAtual >= anguloLimiteInclinacao)
        {
            temporizadorCapotado += Time.deltaTime;
            estaCapotado = true;

            if (temporizadorCapotado >= tempoSeguranca)
            {
                DesativarScriptsMovimento();
            }
        }
        else
        {
            // Se o carro descapotar antes de esgotar o tempo, reseta o temporizador
            temporizadorCapotado = 0f;
            estaCapotado = false;
        }
    }

    private void DesativarScriptsMovimento()
    {
        jaDesativou = true;

        if (scriptMovimento != null)
        {
            scriptMovimento.enabled = false;
            Debug.LogWarning($"<color=red>[CAPOTAMENTO]</color> Script '{scriptMovimento.GetType().Name}' foi desativado em {gameObject.name}.");
        }

        if (scriptAdicional != null)
        {
            scriptAdicional.enabled = false;
            Debug.LogWarning($"<color=red>[CAPOTAMENTO]</color> Script adicional '{scriptAdicional.GetType().Name}' foi desativado em {gameObject.name}.");
        }

        // Desativa a si mesmo após cumprir o objetivo
        this.enabled = false;
    }

    private void OnDrawGizmosSelected()
    {
        Transform t = (corpo != null) ? corpo : transform;

        // Desenha uma linha verde (se em pé) ou vermelha (se capotado) indicando a orientação do teto
        Gizmos.color = (Vector3.Angle(t.up, Vector3.up) >= anguloLimiteInclinacao) ? Color.red : Color.green;
        Gizmos.DrawRay(t.position, t.up * 2f);
    }
}
using UnityEngine;
using UnityEngine.InputSystem;

public class CarAutomatico : MonoBehaviour
{
    [Header("Controles (Input System)")]
    public InputActionReference acelerar;
    public InputActionReference re;
    public InputActionReference esquerda;
    public InputActionReference direita;

    [Header("Configurações do Veículo")]
    [Tooltip("Se colocar 0, o carro ZERA e NÃO ANDA.")]
    public float veloxLimit = 0f;

    [Tooltip("Taxa de ganho de velocidade ao acelerar para frente. Valores menores deixam a aceleração mais suave.")]
    public float controleDeAceleracao = 3f;

    [Tooltip("Força de parada para zerar a ré instantaneamente ao apertar W.")]
    public float respostaAceleracaoFrente = 15f;

    [Tooltip("Aceleração da ré.")]
    public float aceleracaoRe = 5f;

    [Tooltip("Limite da ré.")]
    public float limiteVelocidadeRe = 12f;

    [Tooltip("Inércia ao soltar teclas.")]
    public float desaceleracaoInercia = 3f;

    public float velocidadeGiro = 80f;

    [Header("Verificação de Capotamento")]
    [Range(-1f, 1f)] public float alinhamentoMinimoParaAndar = 0.7f;

    [Header("Tempo entre Marchas (Segundos)")]
    public float tempoMarcha1Para2 = 2f;
    public float tempoMarcha2Para3 = 2f;
    public float tempoMarcha3Para4 = 2f;
    public float tempoMarcha4Para5 = 2f;

    [Header("Monitoramento")]
    public bool estaCapotado = false;
    public int marchaAtual = 1;
    public float limiteDaMarchaAtual;
    public float velocidadeAtual = 0f;
    public float cronometro = 0f;

    void Update()
    {
        float alinhamento = Vector3.Dot(transform.up, Vector3.up);
        estaCapotado = alinhamento < alinhamentoMinimoParaAndar;

        if (estaCapotado || veloxLimit <= 0f)
        {
            velocidadeAtual = 0f;
            cronometro = 0f;
            return;
        }

        float tamanhoDaMarcha = veloxLimit / 5f;
        limiteDaMarchaAtual = tamanhoDaMarcha * marchaAtual;

        // Leitura de entradas pelas actions configuráveis (teclado + joystick).
        // Se os campos estiverem vazios, usa o sistema antigo.
        float inputVertical = (acelerar != null && re != null)
            ? acelerar.action.ReadValue<float>() - re.action.ReadValue<float>()
            : Input.GetAxis("Vertical");

        float inputHorizontal = (direita != null && esquerda != null)
            ? direita.action.ReadValue<float>() - esquerda.action.ReadValue<float>()
            : Input.GetAxis("Horizontal");

        // Acelerando para FRENTE
        if (inputVertical > 0.1f)
        {
            // Se estiver em ré, zera a velocidade rápido com a resposta dedicada
            if (velocidadeAtual < 0f)
            {
                velocidadeAtual = Mathf.MoveTowards(velocidadeAtual, 0f, respostaAceleracaoFrente * Time.deltaTime);
            }
            else
            {
                // Acelera gradualmente usando a taxa do 'controleDeAceleracao'
                velocidadeAtual = Mathf.MoveTowards(velocidadeAtual, limiteDaMarchaAtual, controleDeAceleracao * Time.deltaTime);

                if (velocidadeAtual >= limiteDaMarchaAtual && marchaAtual < 5)
                {
                    cronometro += Time.deltaTime;

                    float tempoNecessario = 0f;
                    if (marchaAtual == 1) tempoNecessario = tempoMarcha1Para2;
                    else if (marchaAtual == 2) tempoNecessario = tempoMarcha2Para3;
                    else if (marchaAtual == 3) tempoNecessario = tempoMarcha3Para4;
                    else if (marchaAtual == 4) tempoNecessario = tempoMarcha4Para5;

                    if (cronometro >= tempoNecessario)
                    {
                        marchaAtual++;
                        cronometro = 0f;
                    }
                }
            }
        }
        // Acelerando em RÉ
        else if (inputVertical < -0.1f && limiteVelocidadeRe > 0f)
        {
            cronometro = 0f;

            if (velocidadeAtual > 0f)
            {
                velocidadeAtual = Mathf.MoveTowards(velocidadeAtual, 0f, aceleracaoRe * Time.deltaTime);
            }
            else
            {
                marchaAtual = -1;
                velocidadeAtual = Mathf.MoveTowards(velocidadeAtual, -limiteVelocidadeRe, aceleracaoRe * Time.deltaTime);
            }
        }
        else
        {
            velocidadeAtual = Mathf.MoveTowards(velocidadeAtual, 0f, desaceleracaoInercia * Time.deltaTime);
        }

        if (velocidadeAtual == 0f)
        {
            marchaAtual = 1;
            cronometro = 0f;
        }

        if (velocidadeAtual != 0f)
        {
            float direcaoGiro = velocidadeAtual > 0f ? 1f : -1f;
            transform.Rotate(0, inputHorizontal * velocidadeGiro * direcaoGiro * Time.deltaTime, 0);
        }

        transform.position += transform.forward * velocidadeAtual * Time.deltaTime;
    }
}
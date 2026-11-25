using UnityEngine;
using UnityEngine.UI;

public class ControladorTimer : MonoBehaviour
{
    [Header("Componentes UI")]
    public Image imagemPreenchimento;       // Arraste o Timer Front (Image)
    public GameObject painelGameOver;       // Arraste a Canvas/Painel de Game Over

    [Header("Objeto para Desativar no Game Over")]
    public GameObject objetoParaDesativar;  // Arraste o velocimetro aqui

    [Header("Game Over: Desligar o Pause")]
    public MonoBehaviour menuPause;         // Arraste o componente MenuPause (ele será desligado no Game Over)

    [Header("Objetos 3D Extras para Piscar")]
    public Renderer objeto3DParaPiscar;      // Arraste o node_id56 (SETA 1)
    public Renderer objeto3DParaPiscar2;     // Arraste o segundo objeto 3D aqui (SETA 2)

    [Header("CONFIGURAÇÃO DO PISCA (Variáveis Públicas)")]
    public float velocidadePisca = 5f;       // Controla a velocidade da piscagem no Inspector
    public Color corVermelhoPisca = Color.red; // Cor do flash vermelho
    public Color corVermelhoEscuro = new Color(0.3f, 0f, 0f, 1f); // Vermelho escuro para a fase crítica

    [Header("Tempo Inicial (Antes de qualquer objeto ativar)")]
    public float tempoInicial = 60f;        // Tempo ao dar Play na cena

    [Header("Cores do Temporizador")]
    public Color corBom = Color.green;
    public Color corMedio = Color.yellow;
    public Color corCritico = Color.red;

    public float PercentualAtual => tempoTotalAtual > 0 ? Mathf.Clamp01(tempoRestante / tempoTotalAtual) : 0f;

    private float tempoRestante;
    private float tempoTotalAtual;
    private bool jogoAcabou = false;
    private Material materialInstanciado;
    private Material materialInstanciado2;

    void Start()
    {
        if (painelGameOver != null)
            painelGameOver.SetActive(false);

        if (objetoParaDesativar != null)
            objetoParaDesativar.SetActive(true);

        // Salva as referências dos materiais para evitar vazamento de memória e garantir a troca de cor
        if (objeto3DParaPiscar != null)
        {
            materialInstanciado = objeto3DParaPiscar.material;
        }

        if (objeto3DParaPiscar2 != null)
        {
            materialInstanciado2 = objeto3DParaPiscar2.material;
        }

        ResetarTimer(tempoInicial);
    }

    void Update()
    {
        if (jogoAcabou) return;

        if (tempoTotalAtual > 0)
        {
            tempoRestante -= Time.deltaTime;

            float percentual = Mathf.Clamp01(tempoRestante / tempoTotalAtual);

            // 1. Determina as cores base e de pisca conforme a fase do tempo
            Color corBase;
            Color corAlvoPisca;

            if (percentual <= 0.2f)
            {
                // FASE VERMELHA: Alterna entre Vermelho e Vermelho Escuro
                corBase = corCritico;
                corAlvoPisca = corVermelhoEscuro;
            }
            else if (percentual <= 0.5f)
            {
                // FASE AMARELA: Alterna entre Amarelo e Vermelho
                corBase = corMedio;
                corAlvoPisca = corVermelhoPisca;
            }
            else
            {
                // FASE VERDE: Alterna entre Verde e Vermelho
                corBase = corBom;
                corAlvoPisca = corVermelhoPisca;
            }

            // 2. Atualiza a barra da UI (Timer Front)
            if (imagemPreenchimento != null)
            {
                imagemPreenchimento.fillAmount = percentual;
                imagemPreenchimento.color = corBase;
            }

            // 3. EFEITO PISCA NOS OBJETOS 3D
            bool estadoPisca = (Mathf.Repeat(Time.time * velocidadePisca, 1f) > 0.5f);
            Color corExibida = estadoPisca ? corBase : corAlvoPisca;

            // Primeiro Objeto 3D
            if (materialInstanciado != null)
            {
                materialInstanciado.color = corExibida;
                if (materialInstanciado.HasProperty("_BaseColor"))
                {
                    materialInstanciado.SetColor("_BaseColor", corExibida);
                }
            }

            // Segundo Objeto 3D
            if (materialInstanciado2 != null)
            {
                materialInstanciado2.color = corExibida;
                if (materialInstanciado2.HasProperty("_BaseColor"))
                {
                    materialInstanciado2.SetColor("_BaseColor", corExibida);
                }
            }

            // Dispara Game Over quando o tempo zera
            if (tempoRestante <= 0)
            {
                DispararGameOver();
            }
        }
    }

    public void ResetarTimer(float novoTempo)
    {
        if (jogoAcabou) return;

        tempoTotalAtual = novoTempo;
        tempoRestante = novoTempo;

        if (imagemPreenchimento != null)
        {
            imagemPreenchimento.fillAmount = 1f;
            imagemPreenchimento.color = corBom;
        }
    }

    private void DispararGameOver()
    {
        if (jogoAcabou) return;

        jogoAcabou = true;
        tempoRestante = 0;

        if (objetoParaDesativar != null)
            objetoParaDesativar.SetActive(false);

        if (imagemPreenchimento != null)
            imagemPreenchimento.fillAmount = 0f;

        if (painelGameOver != null)
            painelGameOver.SetActive(true);

        // Desliga o MenuPause para o pause não abrir no Game Over
        if (menuPause != null)
            menuPause.enabled = false;

        Time.timeScale = 0f;
    }
}
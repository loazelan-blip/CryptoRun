using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LeitorDeCorDoTimer : MonoBehaviour
{
    [Header("Componentes de UI")]
    [Tooltip("Arraste o objeto 'Timer Front' aqui")]
    public Image imagemTimer;

    [Tooltip("Arraste o objeto 'PlacarBtc' (TextMeshPro) aqui")]
    public TextMeshProUGUI textoPlacarBtc;

    [Header("Pontuação por Cor (Configurável)")]
    public int pontosVerde = 100;
    public int pontosAmarelo = 50;
    public int pontosVermelho = 20;

    [Header("Ajuste de Tempo Antes do Reset")]
    [Tooltip("Quantos segundos antes da colisão o script deve ler a cor?")]
    public float tempoVoltarSegundos = 1.0f;

    [Header("Objetos 3D para Monitorar (4 Objetos)")]
    [Tooltip("Arraste os 4 objetos 3D das cartas aqui")]
    public GameObject[] objetosMonitorados = new GameObject[4];

    [Header("Intervalo do Print Contínuo no Console (em segundos)")]
    public float intervaloPrint = 1f;

    // Variáveis de controle
    private int pontuacaoTotal = 0;
    private float cronometro;
    private bool[] jaAvisou = new bool[4];
    private string ultimaCorValida = "VERDE"; // Cor de segurança caso o timer seja destruído

    private struct RegistroTimer
    {
        public float tempoCaptura;
        public Color cor;
        public float fill;
    }

    private Queue<RegistroTimer> historicoTimer = new Queue<RegistroTimer>();

    void Start()
    {
        AtualizarTextoPlacar();
    }

    void Update()
    {
        // 1. Grava o histórico do timer continuamente enquanto ele existir
        if (imagemTimer != null)
        {
            GuardarHistoricoFrame();
        }

        // 2. Checagem da ativação dos objetos (roda mesmo se imagemTimer virar null ao ser destruído)
        if (objetosMonitorados != null)
        {
            for (int i = 0; i < objetosMonitorados.Length; i++)
            {
                if (objetosMonitorados[i] == null) continue;

                if (objetosMonitorados[i].activeSelf)
                {
                    if (!jaAvisou[i])
                    {
                        jaAvisou[i] = true;

                        // Lê a cor do histórico (usando a última cor conhecida como fallback se o Timer sumiu)
                        string corSetada = ObterCorDoHistorico(tempoVoltarSegundos);

                        int valorCor = ObterValorDaCor(corSetada);
                        pontuacaoTotal += valorCor;

                        AtualizarTextoPlacar();

                        
                    }
                }
                else
                {
                    jaAvisou[i] = false;
                }
            }
        }

        // 3. Print contínuo do Timer no Console enquanto ele estiver ativo na cena
        if (imagemTimer != null)
        {
            cronometro += Time.deltaTime;
            if (cronometro >= intervaloPrint)
            {
                cronometro = 0f;
                PrintarCorAtual();
            }
        }
    }

    void AtualizarTextoPlacar()
    {
        if (textoPlacarBtc != null)
        {
            textoPlacarBtc.text = pontuacaoTotal.ToString();
        }
    }

    int ObterValorDaCor(string cor)
    {
        switch (cor)
        {
            case "VERDE": return pontosVerde;
            case "AMARELA": return pontosAmarelo;
            case "VERMELHA": return pontosVermelho;
            default: return 0;
        }
    }

    void GuardarHistoricoFrame()
    {
        RegistroTimer registroAtual;
        registroAtual.tempoCaptura = Time.time;
        registroAtual.cor = imagemTimer.color;
        registroAtual.fill = imagemTimer.fillAmount;

        historicoTimer.Enqueue(registroAtual);

        // Mantém atualizada a última cor lida antes de qualquer Destroy
        ultimaCorValida = ProcessarCor(registroAtual.cor, registroAtual.fill);

        while (historicoTimer.Count > 0 && Time.time - historicoTimer.Peek().tempoCaptura > 5.0f)
        {
            historicoTimer.Dequeue();
        }
    }

    string ObterCorDoHistorico(float segundosAtras)
    {
        // Se a fila tiver dados gravados nos segundos anteriores, recupera do histórico normalmente
        if (historicoTimer.Count > 0)
        {
            float tempoAlvo = Time.time - segundosAtras;
            RegistroTimer registroMaisProximo = default;
            bool encontrou = false;

            foreach (var reg in historicoTimer)
            {
                registroMaisProximo = reg;
                if (reg.tempoCaptura >= tempoAlvo)
                {
                    encontrou = true;
                    break;
                }
            }

            Color cor = encontrou ? registroMaisProximo.cor : historicoTimer.Peek().cor;
            float fill = encontrou ? registroMaisProximo.fill : historicoTimer.Peek().fill;

            return ProcessarCor(cor, fill);
        }

        // Se o Timer foi destruído e a fila esvaziou, retorna a última cor gravada no histórico
        return ultimaCorValida;
    }

    string ProcessarCor(Color cor, float fill)
    {
        if (fill > 0.5f || (cor.g > 0.6f && cor.r < 0.6f))
        {
            return "VERDE";
        }
        else if (fill > 0.2f || (cor.r > 0.5f && cor.g > 0.5f))
        {
            return "AMARELA";
        }
        else
        {
            return "VERMELHA";
        }
    }

    void PrintarCorAtual()
    {
        Color cor = imagemTimer.color;
        float fill = imagemTimer.fillAmount;

        string nomeCor = ProcessarCor(cor, fill);

       
    }

    string ObterTagCor(string cor)
    {
        switch (cor)
        {
            case "VERDE": return "green";
            case "AMARELA": return "yellow";
            case "VERMELHA": return "red";
            default: return "white";
        }
    }
}
using UnityEngine;
using TMPro;

/// <summary>
/// Calcula a velocidade atual de um objeto (baseado no deslocamento por frame)
/// e exibe o valor formatado em um TextMeshPro na tela, no formato "XX km/h".
/// </summary>
public class Velocimetro : MonoBehaviour
{
    #region Referências

    [Header("Referências")]
    [Tooltip("Transform do veículo cuja velocidade será medida.")]
    [SerializeField] private Transform carro;

    [Tooltip("Campo de texto (TextMeshPro) onde a velocidade será exibida.")]
    [SerializeField] private TextMeshProUGUI textoVelocidade;

    #endregion

    #region Configurações

    [Header("Configurações")]
    [Tooltip("Quanto MAIOR, mais suave (lenta) fica a leitura. Quanto MENOR, mais rápida (mas mais instável).")]
    [SerializeField, Range(0.05f, 2f)] private float tempoDeSuavizacao = 0.4f;

    #endregion

    #region Estado interno

    private Vector3 ultimaPosicao;
    private float velocidadeSuavizada;
    private float velocidadeReferencia; // usada internamente pelo SmoothDamp

    #endregion

    private void Start()
    {
        if (carro != null)
        {
            ultimaPosicao = carro.position;
        }
    }

    private void Update()
    {
        if (!ReferenciasValidas()) return;

        AtualizarVelocidade();
        AtualizarTexto();
    }

    /// <summary>
    /// Confirma que todas as referências obrigatórias foram atribuídas no Inspector.
    /// </summary>
    private bool ReferenciasValidas()
    {
        return carro != null && textoVelocidade != null;
    }

    /// <summary>
    /// Calcula a velocidade instantânea (km/h) com base no deslocamento
    /// desde o último frame, e aplica um SmoothDamp (suavização independente
    /// de framerate) para eliminar tremulação no HUD.
    /// </summary>
    private void AtualizarVelocidade()
    {
        float distancia = Vector3.Distance(carro.position, ultimaPosicao);
        float velocidadeInstantanea = (distancia / Time.deltaTime) * 3.6f; // m/s → km/h

        velocidadeSuavizada = Mathf.SmoothDamp(
            velocidadeSuavizada,
            velocidadeInstantanea,
            ref velocidadeReferencia,
            tempoDeSuavizacao
        );

        ultimaPosicao = carro.position;
    }

    /// <summary>
    /// Atualiza o texto exibido na UI com a velocidade calculada.
    /// O <mspace> trava a largura dos números, evitando que o texto "dance"
    /// conforme a quantidade de dígitos muda.
    /// </summary>
    private void AtualizarTexto()
    {
        int velocidadeArredondada = Mathf.RoundToInt(velocidadeSuavizada);
        textoVelocidade.text = $"<mspace=0.55em>{velocidadeArredondada}</mspace> km/h";
    }
}
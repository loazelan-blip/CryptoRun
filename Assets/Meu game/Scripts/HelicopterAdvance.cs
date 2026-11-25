using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelicopterAdvance : MonoBehaviour
{
    public enum AcaoAposAndar
    {
        VirarDireita,
        VirarEsquerda,
        SeguirReto
    }

    [System.Serializable]
    public struct PassoPatrulha
    {
        [Tooltip("1. Apenas Eixo: Inclinação visual do objeto (pitch/roll/yaw). NÃO altera a direção em que ele anda.")]
        public Vector3 apenasEixo;

        [Tooltip("2. Eixo de Rotação: Define a inclinação da trajetória do movimento.")]
        public Vector3 eixoRotacao;

        [Tooltip("3. Distância em metros que o objeto vai andar neste trecho.")]
        public float distancia;

        [Tooltip("4. O que o objeto deve fazer ao completar este trecho na IDA.")]
        public AcaoAposAndar acaoFinal;

        [Tooltip("5. Subida/descida vertical em metros durante este trecho. Positivo = sobe, negativo = desce, 0 = não muda a altura. Na volta da rota é desfeita.")]
        public float variacaoAltura;
    }

    [Header("Configurações de Movimento")]
    public float velocidade = 5.0f;
    public float velocidadeRotacao = 3.0f;

    [Tooltip("Marque para inverter completamente a direção de movimento e o sentido das curvas")]
    public bool inverterDirecao = false;

    [Header("Sequência da Rota (IDA)")]
    public List<PassoPatrulha> passosDaRota = new List<PassoPatrulha>();

    private Collider colisorPrincipal;
    private bool estaAtivo = true;
    private bool estaGirando = false;

    private int indicePassoAtual = 0;
    private bool indoIda = true;

    private float distanciaPercorridaNoPasso = 0f;

    // Ponto e orientação de partida originais
    private Vector3 posicaoInicialOriginal;
    private Quaternion orientacaoInicialOriginal;

    // Guarda a direção real de deslocamento no plano do mundo
    private Quaternion orientacaoTrajetoria;

    void Start()
    {
        colisorPrincipal = GetComponent<Collider>();

        posicaoInicialOriginal = transform.position;
        orientacaoInicialOriginal = transform.rotation;
        orientacaoTrajetoria = transform.rotation;

        if (passosDaRota == null || passosDaRota.Count == 0)
        {
            passosDaRota = new List<PassoPatrulha>
            {
                new PassoPatrulha
                {
                    apenasEixo = Vector3.zero,
                    eixoRotacao = Vector3.zero,
                    distancia = 10f,
                    acaoFinal = AcaoAposAndar.VirarDireita,
                    variacaoAltura = 0f
                }
            };
        }

        distanciaPercorridaNoPasso = 0f;

        // Aplica as rotações do primeiro passo no início
        AtualizarTransformObjeto(passosDaRota[0]);
    }

    private Vector3 ObterEixoTrajetoriaAjustado(Vector3 eixoOriginal)
    {
        if (indoIda) return eixoOriginal;

        // Na volta, invertemos a componente X/Z da trajetória para desfazer subidas/descidas e desvios
        return new Vector3(-eixoOriginal.x, eixoOriginal.y, -eixoOriginal.z);
    }

    private void AtualizarTransformObjeto(PassoPatrulha passo)
    {
        Vector3 eixoAjustado = ObterEixoTrajetoriaAjustado(passo.eixoRotacao);
        transform.rotation = orientacaoTrajetoria * Quaternion.Euler(eixoAjustado) * Quaternion.Euler(passo.apenasEixo);
    }

    private Vector3 ObterVetorDirecaoTrajetoria(PassoPatrulha passo)
    {
        Vector3 eixoAjustado = ObterEixoTrajetoriaAjustado(passo.eixoRotacao);
        Quaternion rotTrajetoria = orientacaoTrajetoria * Quaternion.Euler(eixoAjustado);
        return rotTrajetoria * Vector3.forward;
    }

    void Update()
    {
        if (!estaAtivo) return;

        float fatorInversao = inverterDirecao ? -1.0f : 1.0f;

        // Move no vetor de trajetória ajustado
        Vector3 direcaoMovimento = ObterVetorDirecaoTrajetoria(passosDaRota[indicePassoAtual]);
        Vector3 deslocamento = (direcaoMovimento * fatorInversao) * velocidade * Time.deltaTime;
        transform.position += deslocamento;

        if (!estaGirando && passosDaRota.Count > 0)
        {
            PassoPatrulha passoAtual = passosDaRota[indicePassoAtual];

            // Subida/descida vertical proporcional ao quanto do trecho foi andado neste frame
            if (passoAtual.distancia > 0f && passoAtual.variacaoAltura != 0f)
            {
                float restante = Mathf.Max(0f, passoAtual.distancia - distanciaPercorridaNoPasso);
                float andadoValido = Mathf.Min(deslocamento.magnitude, restante);
                float sinal = indoIda ? 1f : -1f;
                float dy = passoAtual.variacaoAltura * sinal * (andadoValido / passoAtual.distancia);
                transform.position += Vector3.up * dy;
            }

            distanciaPercorridaNoPasso += deslocamento.magnitude;

            if (distanciaPercorridaNoPasso >= passoAtual.distancia)
            {
                distanciaPercorridaNoPasso = 0f;
                StartCoroutine(ProcessarFimDoTrecho(passoAtual));
            }
        }
    }

    IEnumerator ProcessarFimDoTrecho(PassoPatrulha passoAtual)
    {
        estaGirando = true;

        float anguloGiro = 0f;

        if (indoIda && indicePassoAtual == passosDaRota.Count - 1)
        {
            anguloGiro = 180f;
            indoIda = false;
        }
        else if (!indoIda && indicePassoAtual == 0)
        {
            anguloGiro = 180f;
            indoIda = true;
        }
        else
        {
            if (indoIda)
            {
                if (passoAtual.acaoFinal == AcaoAposAndar.VirarDireita) anguloGiro = 90f;
                else if (passoAtual.acaoFinal == AcaoAposAndar.VirarEsquerda) anguloGiro = -90f;
                else if (passoAtual.acaoFinal == AcaoAposAndar.SeguirReto) anguloGiro = 0f;

                indicePassoAtual++;
            }
            else
            {
                indicePassoAtual--;
                PassoPatrulha passoAnterior = passosDaRota[indicePassoAtual];

                if (passoAnterior.acaoFinal == AcaoAposAndar.VirarDireita) anguloGiro = -90f;
                else if (passoAnterior.acaoFinal == AcaoAposAndar.VirarEsquerda) anguloGiro = 90f;
                else if (passoAnterior.acaoFinal == AcaoAposAndar.SeguirReto) anguloGiro = 0f;
            }
        }

        if (inverterDirecao)
        {
            anguloGiro *= -1f;
        }

        // Atualiza a orientação base da trajetória no plano Y
        orientacaoTrajetoria = orientacaoTrajetoria * Quaternion.Euler(0, anguloGiro, 0);

        PassoPatrulha proximoPasso = passosDaRota[indicePassoAtual];
        Quaternion rotacaoInicial = transform.rotation;

        Vector3 eixoProximoAjustado = ObterEixoTrajetoriaAjustado(proximoPasso.eixoRotacao);
        Quaternion rotacaoAlvo = orientacaoTrajetoria * Quaternion.Euler(eixoProximoAjustado) * Quaternion.Euler(proximoPasso.apenasEixo);

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * velocidadeRotacao;
            transform.rotation = Quaternion.Slerp(rotacaoInicial, rotacaoAlvo, t);
            yield return null;
        }

        transform.rotation = rotacaoAlvo;

        // Se completou a volta e regressou ao início do passo 0
        if (indoIda && indicePassoAtual == 0)
        {
            transform.position = posicaoInicialOriginal;
            orientacaoTrajetoria = orientacaoInicialOriginal;
            AtualizarTransformObjeto(passosDaRota[0]);
        }

        estaGirando = false;
    }

    public void DesativarCarro()
    {
        estaAtivo = false;
        StopAllCoroutines();

        if (colisorPrincipal != null)
        {
            colisorPrincipal.enabled = false;
        }
    }
}
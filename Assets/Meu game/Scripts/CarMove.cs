using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMove : MonoBehaviour
{
    public enum AcaoAposAndar
    {
        VirarDireita,
        VirarEsquerda
    }

    [System.Serializable]
    public struct PassoPatrulha
    {
        [Tooltip("Distância em metros que o carro vai andar neste trecho.")]
        public float distancia;

        [Tooltip("O que o carro deve fazer ao completar este trecho na IDA.")]
        public AcaoAposAndar acaoFinal;
    }

    [Header("Configurações de Movimento")]
    public float velocidade = 5.0f;
    public float velocidadeRotacao = 3.0f;

    [Tooltip("Marque para inverter completamente a direção de movimento e o sentido das curvas")]
    public bool inverterDirecao = true;

    [Header("Rodas do Veículo")]
    [Tooltip("Arraste os Transforms das 4 rodas aqui para girarem com o movimento")]
    public Transform rodaDianteiraEsquerda;
    public Transform rodaDianteiraDireita;
    public Transform rodaTraseiraEsquerda;
    public Transform rodaTraseiraDireita;

    [Tooltip("Multiplicador da velocidade de rotação das rodas")]
    public float velocidadeGiroRodas = 300.0f;

    [Header("Sequência da Rota (IDA)")]
    public List<PassoPatrulha> passosDaRota = new List<PassoPatrulha>();

    [Header("Obstáculo à frente")]
    [Tooltip("Distância (em metros) do pivô do carro até o para-choque, na direção em que ele anda (e para trás). A esfera de detecção começa aqui.")]
    public float afastamentoDoParachoque = 2.0f;
    [Tooltip("Distância (em metros) do pivô do carro até a lateral. Usada para olhar para a direita e para a esquerda quando está encurralado.")]
    public float afastamentoLateral = 1.0f;
    [Tooltip("Distância (em metros) à frente do ponto de partida da esfera em que ele enxerga um obstáculo. Ao ver, ele dá meia-volta.")]
    public float distanciaVisao = 1.5f;
    [Tooltip("Altura (em metros, a partir do pivô) de onde ele olha.")]
    public float alturaVisao = 0.8f;
    [Tooltip("Largura da esfera que ele lança para detectar obstáculos.")]
    public float raioVisao = 0.5f;
    [Tooltip("Normal (vetor perpendicular à superfície) acima deste valor = chão, não é obstáculo.")]
    [Range(0.3f, 1f)] public float limiteChao = 0.5f;

    [Header("Calçada (tag Obstaculo)")]
    [Tooltip("Tag dos objetos (ex: calçada) que a esfera baixa detecta. A esfera baixa SÓ reage a objetos com esta tag. A tag precisa estar no objeto que tem o colisor.")]
    public string tagObstaculo = "Obstaculo";
    [Tooltip("Altura (em metros, a partir do pivô) da esfera baixa. Diminua se o carro ainda subir na calçada.")]
    public float alturaVisaoBaixa = 0.3f;
    [Tooltip("Largura da esfera baixa.")]
    public float raioVisaoBaixa = 0.3f;

    [Header("Encurralado (obstáculos dos dois lados)")]
    [Tooltip("Backup: quantas meias-voltas seguidas ele aceita fazer. Se o sensor já vê obstáculo na frente E atrás, ele fica encurralado na hora, sem precisar virar. Este número só vale quando os obstáculos estão mais longe do que o sensor enxerga.")]
    public int maxViradasSeguidas = 2;
    [Tooltip("Tempo (em segundos) que ele espera procurando um caminho livre quando não existe carro por perto para seguir, ou quando não tem direção livre até o carro seguido.")]
    public float tempoEspera = 3.0f;
    [Tooltip("De quanto em quanto tempo (em segundos) ele confere de novo qual direção tomar para ir até o carro seguido.")]
    public float intervaloConferencia = 0.5f;
    [Tooltip("Se ele andar esta distância (em metros) sem achar obstáculo, a contagem de viradas volta a zero.")]
    public float distanciaParaZerar = 5.0f;
    [Tooltip("Quando encurralado, ele procura o carro mais próximo até esta distância (em metros). Se o carro seguido passar desta distância, ele procura outro.")]
    public float distanciaMaximaSeguir = 40.0f;

    [Header("Seguir o carro mais próximo (quando encurralado)")]
    [Tooltip("Distância (em metros) que ele mantém do carro que está seguindo.")]
    public float distanciaSeguir = 10.0f;
    [Tooltip("Velocidade (devagar) com que ele vai até o carro que vai seguir, antes de chegar na distância de seguir.")]
    public float velocidadeAproximacao = 2.0f;
    [Tooltip("Por quantos segundos ele segue o carro (conta depois que ele chegou na distância de seguir e saiu da calçada). Depois volta para a rota.")]
    public float tempoSeguir = 20.0f;

    [Header("Monitoramento (só leitura)")]
    public int viradasSeguidas = 0;
    public bool esperandoEncurralado = false;
    public bool seguindoCarro = false;
    public bool naCalcada = false;
    public float tempoRestanteSeguir = 0f;
    public string carroSeguido = "Nenhum";

    // Velocidade (em graus por segundo) com que ele vira enquanto segue outro carro
    private const float GRAUS_POR_SEGUNDO_SEGUIR = 120f;

    private Collider colisorPrincipal;
    private bool estaGirando = false;
    private bool girandoPorObstaculo = false;

    private int indicePassoAtual = 0;
    private bool indoIda = true;
    private Vector3 pontoInicialPasso;
    private float distanciaAlvo;
    private float distanciaDesdeUltimaVirada = 0f;

    private float proximaTentativaCalcada = 0f;
    private readonly RaycastHit[] bufferChao = new RaycastHit[8];

    void Start()
    {
        colisorPrincipal = GetComponent<Collider>();

        if (passosDaRota == null || passosDaRota.Count == 0)
        {
            passosDaRota = new List<PassoPatrulha>
            {
                new PassoPatrulha { distancia = 10f, acaoFinal = AcaoAposAndar.VirarDireita }
            };
        }

        pontoInicialPasso = transform.position;
        distanciaAlvo = passosDaRota[indicePassoAtual].distancia;
    }

    Vector3 DirecaoDeMovimento()
    {
        float fatorInversao = inverterDirecao ? -1.0f : 1.0f;
        return transform.forward * fatorInversao;
    }

    // Ângulo (em graus) de uma direção no plano horizontal do mundo
    static float YawDe(Vector3 direcao)
    {
        return Mathf.Atan2(direcao.x, direcao.z) * Mathf.Rad2Deg;
    }

    // Direção horizontal correspondente a um ângulo no mundo
    static Vector3 DirecaoDeYaw(float yaw)
    {
        return Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
    }

    // Arredonda um ângulo para o múltiplo de 90° mais próximo (0, 90, 180, 270)
    static float Arredondar90(float yaw)
    {
        return Mathf.Round(yaw / 90f) * 90f;
    }

    // Distância em linha reta (ignorando altura) deste carro até um ponto
    float DistanciaHorizontal(Vector3 ponto)
    {
        Vector3 d = ponto - transform.position;
        d.y = 0f;
        return d.magnitude;
    }

    // Joga um raio para baixo: se o que está embaixo do carro tem a tag, ele está em cima da calçada
    bool EstaNaCalcada()
    {
        Vector3 origem = transform.position + Vector3.up * 0.5f;

        int quantidade = Physics.RaycastNonAlloc(origem, Vector3.down, bufferChao, 1.5f, ~0, QueryTriggerInteraction.Ignore);

        float menorDistancia = float.MaxValue;
        Collider maisPerto = null;

        for (int i = 0; i < quantidade; i++)
        {
            RaycastHit h = bufferChao[i];

            if (h.collider.transform.IsChildOf(transform)) continue; // colisores do próprio carro

            if (h.distance < menorDistancia)
            {
                menorDistancia = h.distance;
                maisPerto = h.collider;
            }
        }

        return maisPerto != null && maisPerto.CompareTag(tagObstaculo);
    }

    void Update()
    {
        // Confere se está em cima da calçada
        naCalcada = EstaNaCalcada();

        // 0. Em cima da calçada: vai até o carro mais próximo (que está na rua) para sair dela.
        //    Senão, se tem obstáculo à frente, dá meia-volta (ou segue o carro mais próximo, se estiver encurralado)
        if (!estaGirando && naCalcada && Time.time >= proximaTentativaCalcada)
        {
            StartCoroutine(SairDaCalcada());
        }
        else if (!estaGirando && ObstaculoNaFrente())
        {
            StartCoroutine(ProcessarObstaculo());
        }

        float fatorInversao = inverterDirecao ? -1.0f : 1.0f;

        // 1. Move o carro no sentido ajustado (não anda enquanto gira ou espera por causa do obstáculo)
        if (!girandoPorObstaculo)
        {
            transform.position += DirecaoDeMovimento() * velocidade * Time.deltaTime;

            // 2. Faz as rodas girarem acompanhando a direção
            GirarRodas(fatorInversao, velocidade);

            // Se andou o suficiente sem achar obstáculo, zera a contagem de viradas
            if (viradasSeguidas > 0)
            {
                distanciaDesdeUltimaVirada += velocidade * Time.deltaTime;
                if (distanciaDesdeUltimaVirada >= distanciaParaZerar)
                {
                    viradasSeguidas = 0;
                    distanciaDesdeUltimaVirada = 0f;
                }
            }
        }

        // 3. Controle dos trechos da patrulha
        if (!estaGirando)
        {
            float distanciaPercorrida = Vector3.Distance(pontoInicialPasso, transform.position);
            PassoPatrulha passoAtual = passosDaRota[indicePassoAtual];

            if (distanciaPercorrida >= distanciaAlvo)
            {
                StartCoroutine(ProcessarFimDoTrecho(passoAtual));
            }
        }
    }

    bool ObstaculoNaFrente()
    {
        return ObstaculoNaDirecao(DirecaoDeMovimento(), afastamentoDoParachoque);
    }

    // Lança as esferas de detecção em uma direção e diz se tem obstáculo.
    // Esfera normal: ignora o próprio carro e o chão.
    // Esfera baixa: só reage a objetos com a tag Obstaculo (e é ignorada quando o carro está em cima da calçada).
    bool ObstaculoNaDirecao(Vector3 direcao, float afastamento)
    {
        // 1) Esfera na altura normal
        Vector3 origem = transform.position + Vector3.up * alturaVisao + direcao * afastamento;

        RaycastHit[] hits = Physics.SphereCastAll(origem, raioVisao, direcao, distanciaVisao, ~0, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit h = hits[i];

            if (h.collider.transform.IsChildOf(transform)) continue; // colisores do próprio carro
            if (h.normal.y > limiteChao) continue;                   // chão

            return true;
        }

        // Em cima da calçada, a esfera baixa só atrapalharia: ignora
        if (naCalcada) return false;

        // 2) Esfera baixa: só detecta objetos com a tag (calçada baixa)
        Vector3 origemBaixa = transform.position + Vector3.up * alturaVisaoBaixa + direcao * afastamento;

        RaycastHit[] hitsBaixos = Physics.SphereCastAll(origemBaixa, raioVisaoBaixa, direcao, distanciaVisao, ~0, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hitsBaixos.Length; i++)
        {
            RaycastHit h = hitsBaixos[i];

            if (h.collider.transform.IsChildOf(transform)) continue;

            if (h.collider.CompareTag(tagObstaculo)) return true;
        }

        return false;
    }

    // Acha o outro carro (com CarMove ligado e que não esteja encurralado) mais próximo
    CarMove EncontrarCarroMaisProximo()
    {
        CarMove[] todos = FindObjectsByType<CarMove>(FindObjectsSortMode.None);

        CarMove melhor = null;
        float melhorDistancia = distanciaMaximaSeguir;

        for (int i = 0; i < todos.Length; i++)
        {
            CarMove c = todos[i];

            if (c == this) continue;
            if (!c.enabled) continue;
            if (c.esperandoEncurralado) continue;

            float d = Vector3.Distance(transform.position, c.transform.position);
            if (d <= melhorDistancia)
            {
                melhorDistancia = d;
                melhor = c;
            }
        }
        return melhor;
    }

    // Escolhe, em linha reta (múltiplo de 90° do mundo), a direção livre para ir até o carro seguido.
    // Prefere continuar no eixo em que já está andando enquanto ainda falta distância nele
    // (evita zigue-zague); senão, usa o eixo com maior distância até o carro.
    bool EscolherYawParaLider(Vector3 posicaoLider, float yawMovimentoAtual, out float yawEscolhido)
    {
        Vector3 delta = posicaoLider - transform.position;
        delta.y = 0f;

        float yawX = delta.x >= 0f ? 90f : -90f;   // eixo X do mundo, em direção ao carro
        float yawZ = delta.z >= 0f ? 0f : 180f;    // eixo Z do mundo, em direção ao carro
        float absX = Mathf.Abs(delta.x);
        float absZ = Mathf.Abs(delta.z);

        float yawBase = Arredondar90(yawMovimentoAtual);

        List<float> candidatos = new List<float>();

        // 1) Continua no eixo atual se ainda falta mais de 1 m nele
        if (absX > 1f && Mathf.Abs(Mathf.DeltaAngle(yawBase, yawX)) < 1f) candidatos.Add(yawX);
        if (absZ > 1f && Mathf.Abs(Mathf.DeltaAngle(yawBase, yawZ)) < 1f) candidatos.Add(yawZ);

        // 2) Eixo com maior distância primeiro
        if (absX >= absZ)
        {
            if (absX > 1f) candidatos.Add(yawX);
            if (absZ > 1f) candidatos.Add(yawZ);
        }
        else
        {
            if (absZ > 1f) candidatos.Add(yawZ);
            if (absX > 1f) candidatos.Add(yawX);
        }

        for (int i = 0; i < candidatos.Count; i++)
        {
            float yaw = candidatos[i];
            Vector3 dir = DirecaoDeYaw(yaw);

            float diferenca = Mathf.Abs(Mathf.DeltaAngle(yawBase, yaw));
            float afastamento = (diferenca > 45f && diferenca < 135f) ? afastamentoLateral : afastamentoDoParachoque;

            if (!ObstaculoNaDirecao(dir, afastamento))
            {
                yawEscolhido = yaw;
                return true;
            }
        }

        yawEscolhido = yawBase;
        return false;
    }

    // Procura um caminho livre, sempre alinhado ao múltiplo de 90° do mundo (linha reta).
    // Usado quando está encurralado e NÃO existe nenhum carro por perto para seguir
    // (ou quando não tem direção livre até o carro seguido).
    // Devolve o ângulo de giro (em graus) para ir até esse caminho.
    bool EscolherCaminhoLivre(out float angulo)
    {
        Vector3 frente = DirecaoDeMovimento();
        frente.y = 0f;
        frente.Normalize();

        float yawAtual = YawDe(frente);
        float yawBase = Arredondar90(yawAtual);

        List<float> candidatos = new List<float>(); // ângulos absolutos no mundo

        // Direita/esquerda (sorteado), depois trás
        if (Random.value < 0.5f)
        {
            candidatos.Add(yawBase + 90f);
            candidatos.Add(yawBase - 90f);
        }
        else
        {
            candidatos.Add(yawBase - 90f);
            candidatos.Add(yawBase + 90f);
        }
        candidatos.Add(yawBase + 180f);

        for (int i = 0; i < candidatos.Count; i++)
        {
            float yawAlvo = candidatos[i];
            Vector3 dir = DirecaoDeYaw(yawAlvo);

            float diferencaDaBase = Mathf.Abs(Mathf.DeltaAngle(yawBase, yawAlvo));
            float afastamento = (diferencaDaBase > 45f && diferencaDaBase < 135f) ? afastamentoLateral : afastamentoDoParachoque;

            if (!ObstaculoNaDirecao(dir, afastamento))
            {
                // Ângulo que o carro precisa girar para ficar exatamente no yawAlvo
                angulo = Mathf.DeltaAngle(yawAtual, yawAlvo);
                return true;
            }
        }

        angulo = 0f;
        return false;
    }

    private void GirarRodas(float fatorInversao, float velocidadeDasRodas)
    {
        float grausParaGirar = velocidadeDasRodas * velocidadeGiroRodas * Time.deltaTime * fatorInversao;

        if (rodaDianteiraEsquerda != null) rodaDianteiraEsquerda.Rotate(Vector3.right, grausParaGirar);
        if (rodaDianteiraDireita != null) rodaDianteiraDireita.Rotate(Vector3.right, grausParaGirar);
        if (rodaTraseiraEsquerda != null) rodaTraseiraEsquerda.Rotate(Vector3.right, grausParaGirar);
        if (rodaTraseiraDireita != null) rodaTraseiraDireita.Rotate(Vector3.right, grausParaGirar);
    }

    IEnumerator Girar(float anguloGiro)
    {
        Quaternion rotacaoInicial = transform.rotation;
        Quaternion rotacaoAlvo = rotacaoInicial * Quaternion.Euler(0, anguloGiro, 0);

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * velocidadeRotacao;
            transform.rotation = Quaternion.Slerp(rotacaoInicial, rotacaoAlvo, t);

            if (!girandoPorObstaculo)
            {
                float fatorInversao = inverterDirecao ? -1.0f : 1.0f;
                GirarRodas(fatorInversao, velocidade);
            }

            yield return null;
        }

        transform.rotation = rotacaoAlvo;
    }

    // Em cima da calçada: vai até o carro mais próximo (que está na rua) para sair dela.
    // Se não existir nenhum carro por perto, continua normal (a esfera baixa fica ignorada enquanto estiver na calçada).
    IEnumerator SairDaCalcada()
    {
        estaGirando = true;
        girandoPorObstaculo = true;

        CarMove lider = EncontrarCarroMaisProximo();

        if (lider != null)
        {
            yield return StartCoroutine(SeguirCarro(lider));
        }
        else
        {
            // Sem carro por perto: tenta de novo daqui a 1 segundo
            proximaTentativaCalcada = Time.time + 1f;
        }

        girandoPorObstaculo = false;
        estaGirando = false;
    }

    // Vai devagar até o carro mais próximo e passa a segui-lo a "distanciaSeguir" metros por "tempoSeguir" segundos.
    // O tempo só conta depois que ele chegou perto do carro e saiu da calçada.
    // Depois recomeça o trecho atual da rota, na direção em que está virado.
    IEnumerator SeguirCarro(CarMove lider)
    {
        seguindoCarro = true;
        esperandoEncurralado = true;
        carroSeguido = lider.name;
        tempoRestanteSeguir = tempoSeguir;

        float fatorInversao = inverterDirecao ? -1.0f : 1.0f;

        float yawAlvo = YawDe(DirecaoDeMovimento());
        bool temAlvo = false;
        bool chegou = false;
        float tempoSeguindo = 0f;
        float tempoSemCaminho = 0f;
        float proximaConferencia = 0f;

        while (true)
        {
            // Perdeu o carro seguido? Procura outro. Se não houver, desiste de seguir.
            if (lider == null
                || !lider.gameObject.activeInHierarchy
                || lider.esperandoEncurralado
                || DistanciaHorizontal(lider.transform.position) > distanciaMaximaSeguir)
            {
                lider = EncontrarCarroMaisProximo();
                if (lider == null) break;

                carroSeguido = lider.name;
                chegou = false;
                temAlvo = false;
                proximaConferencia = 0f;
            }

            Vector3 posicaoLider = lider.transform.position;
            float distancia = DistanciaHorizontal(posicaoLider);

            // Precisa ir até o carro se está longe, ou se ainda está em cima da calçada
            bool precisaIr = distancia > distanciaSeguir || naCalcada;

            // Confere de tempos em tempos qual direção (em linha reta) tomar até o carro
            if (Time.time >= proximaConferencia)
            {
                proximaConferencia = Time.time + intervaloConferencia;

                if (precisaIr)
                {
                    temAlvo = EscolherYawParaLider(posicaoLider, YawDe(DirecaoDeMovimento()), out yawAlvo);
                }
            }

            if (!precisaIr)
            {
                // Já está na distância de seguir: fica parado até o carro se afastar
                chegou = true;
                tempoSemCaminho = 0f;
            }
            else if (temAlvo)
            {
                tempoSemCaminho = 0f;

                // Vira para a direção escolhida
                float yawMovimento = YawDe(DirecaoDeMovimento());
                float delta = Mathf.DeltaAngle(yawMovimento, yawAlvo);
                float passo = Mathf.MoveTowardsAngle(0f, delta, GRAUS_POR_SEGUNDO_SEGUIR * Time.deltaTime);
                transform.rotation = Quaternion.Euler(0f, passo, 0f) * transform.rotation;

                // Só anda quando já está virado para a direção
                if (Mathf.Abs(delta) <= 5f)
                {
                    // Devagar até chegar; depois acompanha na velocidade do carro seguido
                    float v = chegou ? lider.velocidade : velocidadeAproximacao;

                    transform.position += DirecaoDeMovimento() * v * Time.deltaTime;
                    GirarRodas(fatorInversao, v);
                }
            }
            else
            {
                // Sem direção livre até o carro: espera um pouco e, se continuar assim, escapa para um caminho livre
                tempoSemCaminho += Time.deltaTime;

                if (tempoSemCaminho >= tempoEspera)
                {
                    tempoSemCaminho = 0f;

                    float angulo;
                    if (EscolherCaminhoLivre(out angulo))
                    {
                        yield return StartCoroutine(Girar(angulo));
                        break;
                    }
                }
            }

            // Conta os segundos de seguir (só depois de chegar perto e sair da calçada)
            if (chegou && !naCalcada)
            {
                tempoSeguindo += Time.deltaTime;
                tempoRestanteSeguir = Mathf.Max(0f, tempoSeguir - tempoSeguindo);

                if (tempoSeguindo >= tempoSeguir) break;
            }

            yield return null;
        }

        // Terminou: recomeça o trecho atual daqui, na direção em que está virado
        seguindoCarro = false;
        esperandoEncurralado = false;
        carroSeguido = "Nenhum";
        tempoRestanteSeguir = 0f;
        viradasSeguidas = 0;
        distanciaDesdeUltimaVirada = 0f;

        pontoInicialPasso = transform.position;
        distanciaAlvo = passosDaRota[indicePassoAtual].distancia;
    }

    IEnumerator ProcessarObstaculo()
    {
        estaGirando = true;
        girandoPorObstaculo = true;

        // Encurralado: o sensor vê obstáculo atrás também (frente e trás bloqueadas),
        // ou (backup) ele já virou vezes demais seguidas.
        bool obstaculoAtras = ObstaculoNaDirecao(-DirecaoDeMovimento(), afastamentoDoParachoque);

        if (obstaculoAtras || viradasSeguidas >= maxViradasSeguidas)
        {
            // Existe um carro por perto? Vai até ele devagar e o segue a distanciaSeguir metros.
            CarMove lider = EncontrarCarroMaisProximo();
            if (lider != null)
            {
                yield return StartCoroutine(SeguirCarro(lider));

                girandoPorObstaculo = false;
                estaGirando = false;
                yield break;
            }

            // Nenhum carro por perto: procura um caminho livre
            esperandoEncurralado = true;

            float tempoEsperado = 0f;

            while (true)
            {
                float angulo;
                if (EscolherCaminhoLivre(out angulo))
                {
                    // Achou um caminho livre: vira para ele e recomeça o trecho atual nessa direção
                    esperandoEncurralado = false;
                    viradasSeguidas = 0;
                    distanciaDesdeUltimaVirada = 0f;

                    yield return StartCoroutine(Girar(angulo));

                    pontoInicialPasso = transform.position;
                    distanciaAlvo = passosDaRota[indicePassoAtual].distancia;
                    break;
                }

                if (tempoEsperado >= tempoEspera)
                {
                    // Nenhum caminho liberou: volta ao comportamento normal
                    esperandoEncurralado = false;
                    viradasSeguidas = 0;
                    distanciaDesdeUltimaVirada = 0f;
                    break;
                }

                yield return new WaitForSeconds(intervaloConferencia);
                tempoEsperado += intervaloConferencia;
            }

            girandoPorObstaculo = false;
            estaGirando = false;
            yield break;
        }

        viradasSeguidas++;
        distanciaDesdeUltimaVirada = 0f;

        // Volta exatamente o que já andou neste trecho, depois a rota segue no sentido contrário
        distanciaAlvo = Vector3.Distance(pontoInicialPasso, transform.position);
        indoIda = !indoIda;

        yield return StartCoroutine(Girar(180f));

        pontoInicialPasso = transform.position;
        girandoPorObstaculo = false;
        estaGirando = false;
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
                anguloGiro = (passoAtual.acaoFinal == AcaoAposAndar.VirarDireita) ? 90f : -90f;
                indicePassoAtual++;
            }
            else
            {
                indicePassoAtual--;
                PassoPatrulha passoAnterior = passosDaRota[indicePassoAtual];
                anguloGiro = (passoAnterior.acaoFinal == AcaoAposAndar.VirarDireita) ? -90f : 90f;
            }
        }

        // Se a direção está invertida, inverte também o sentido da curva para manter a rota coerente
        if (inverterDirecao)
        {
            anguloGiro *= -1f;
        }

        yield return StartCoroutine(Girar(anguloGiro));

        distanciaAlvo = passosDaRota[indicePassoAtual].distancia;
        pontoInicialPasso = transform.position;
        estaGirando = false;
    }

    void OnDrawGizmosSelected()
    {
        Vector3 direcao = DirecaoDeMovimento();

        Gizmos.color = Color.red;
        DesenharDeteccao(direcao, afastamentoDoParachoque);

        Gizmos.color = Color.yellow;
        DesenharDeteccao(Quaternion.Euler(0, 90f, 0) * direcao, afastamentoLateral);
        DesenharDeteccao(Quaternion.Euler(0, -90f, 0) * direcao, afastamentoLateral);

        // Raio para baixo que detecta a calçada
        Gizmos.color = Color.cyan;
        Vector3 origemChao = transform.position + Vector3.up * 0.5f;
        Gizmos.DrawLine(origemChao, origemChao + Vector3.down * 1.5f);
    }

    void DesenharDeteccao(Vector3 direcao, float afastamento)
    {
        // Esfera na altura normal
        Vector3 origem = transform.position + Vector3.up * alturaVisao + direcao * afastamento;

        Gizmos.DrawWireSphere(origem, raioVisao);
        Gizmos.DrawWireSphere(origem + direcao * distanciaVisao, raioVisao);
        Gizmos.DrawLine(origem, origem + direcao * distanciaVisao);

        // Esfera baixa (só tag)
        Vector3 origemBaixa = transform.position + Vector3.up * alturaVisaoBaixa + direcao * afastamento;

        Gizmos.DrawWireSphere(origemBaixa, raioVisaoBaixa);
        Gizmos.DrawWireSphere(origemBaixa + direcao * distanciaVisao, raioVisaoBaixa);
    }
}
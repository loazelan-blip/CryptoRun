using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoctorController : MonoBehaviour
{
    public enum AcaoAposAndar
    {
        VirarDireita,
        VirarEsquerda
    }

    [System.Serializable]
    public struct PassoPatrulha
    {
        [Tooltip("Distância em metros que o médico vai andar neste trecho.")]
        public float distancia;

        [Tooltip("O que o médico deve fazer ao completar este trecho na IDA.")]
        public AcaoAposAndar acaoFinal;
    }

    private enum FaseContorno
    {
        Nenhuma,
        Desviando,   // indo para o lado livre
        Passando,    // andando ao lado do obstáculo
        Voltando     // voltando para a linha da rota
    }

    [Header("Configurações de Movimento")]
    [Tooltip("Velocidade do médico. Se < 5 ele ANDA (Walk), se >= 5 ele CORRE (Run).")]
    public float velocidade = 2.0f;
    public float velocidadeRotacao = 3.0f;

    [Header("Sequência da Rota (IDA)")]
    public List<PassoPatrulha> passosDaRota = new List<PassoPatrulha>();

    [Header("Obstáculo à frente")]
    [Tooltip("Distância (em metros) à frente em que ele enxerga um obstáculo. Ao ver, ele contorna.")]
    public float distanciaVisao = 5.0f;
    [Tooltip("Altura (em metros, a partir dos pés) de onde ele olha para frente (esfera alta).")]
    public float alturaVisao = 2.0f;
    [Tooltip("Altura (em metros, a partir dos pés) da segunda esfera, na altura do corpo. Serve para enxergar carros, que passam por baixo da esfera alta.")]
    public float alturaVisaoMedia = 1.0f;
    [Tooltip("Largura da esfera que ele lança para frente para detectar obstáculos.")]
    public float raioVisao = 0.25f;
    [Tooltip("Normal (vetor perpendicular à superfície) acima deste valor = chão, não é obstáculo.")]
    [Range(0.3f, 1f)] public float limiteChao = 0.5f;

    [Header("Contornar obstáculos")]
    [Tooltip("Raio (em metros) da zona de segurança ao redor do corpo. O caminho do contorno precisa ter esta folga, e algo empurrável dentro dela é motivo para ele se afastar.")]
    public float raioSeguranca = 0.6f;
    [Tooltip("Desvio lateral máximo (em metros) que ele aceita fazer para contornar. Se não achar lado livre até aqui, dá meia-volta.")]
    public float maxDesvioLateral = 6.0f;
    [Tooltip("Quantos metros ele anda depois de passar do obstáculo, antes de voltar para a linha da rota.")]
    public float margemAposObstaculo = 1.5f;

    [Header("Monitoramento (só leitura)")]
    public bool contornando = false;

    private Animator animator;
    private Collider colisorPrincipal;
    private bool estaVivo = true;
    private bool estaGirando = false;
    private bool girandoPorObstaculo = false;

    private int indicePassoAtual = 0;
    private bool indoIda = true;
    private Vector3 pontoInicialPasso;
    private float distanciaAlvo;

    // Dados do contorno
    private FaseContorno fase = FaseContorno.Nenhuma;
    private Vector3 linhaOrigem;
    private Vector3 rotaFrente;
    private Vector3 rotaDireita;
    private float desvioAlvo;
    private float distanciaPassando;

    void Start()
    {
        animator = GetComponent<Animator>();
        colisorPrincipal = GetComponent<Collider>();

        if (animator != null)
        {
            animator.applyRootMotion = false;
            animator.speed = 1.0f; // Garante reprodução em velocidade normal
        }

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

    void Update()
    {
        if (!estaVivo) return;

        if (contornando)
        {
            // 0a. Está contornando um obstáculo
            AtualizarContorno();
        }
        else
        {
            // 0b. Obstáculo à frente (ou algo empurrável encostando na frente): contorna.
            //     Se não achar lado livre, dá meia-volta.
            if (!estaGirando && (ObstaculoNaFrente() || CorpoEmpurravelPerto(true)))
            {
                if (!IniciarContorno())
                {
                    StartCoroutine(ProcessarObstaculo());
                }
            }

            if (contornando)
            {
                AtualizarContorno();
            }
            else if (!girandoPorObstaculo)
            {
                // 1. Desloca o médico para frente (não anda enquanto gira por causa do obstáculo)
                transform.position += transform.forward * velocidade * Time.deltaTime;
            }
        }

        // 2. Mapeamento direto e suave da animação com base na velocidade do Inspector
        if (animator != null)
        {
            float valorTarget = (velocidade < 5.0f) ? 0.5f : 1.0f;

            if (velocidade <= 0.1f) valorTarget = 0f;

            float speedAtual = animator.GetFloat("Speed");
            animator.SetFloat("Speed", Mathf.MoveTowards(speedAtual, valorTarget, Time.deltaTime * 5f));
        }

        // 3. Controle dos trechos da patrulha
        if (!estaGirando && !contornando)
        {
            float distanciaPercorrida = Vector3.Distance(pontoInicialPasso, transform.position);
            PassoPatrulha passoAtual = passosDaRota[indicePassoAtual];

            if (distanciaPercorrida >= distanciaAlvo)
            {
                StartCoroutine(ProcessarFimDoTrecho(passoAtual));
            }
        }
    }

    // ---------------------------------------------------------------------
    // DETECÇÃO
    // ---------------------------------------------------------------------

    bool ObstaculoNaFrente()
    {
        // Duas alturas: a alta (paredes, muros) e a do corpo (carros, que passam por baixo da esfera alta)
        return ObstaculoNaAltura(alturaVisao) || ObstaculoNaAltura(alturaVisaoMedia);
    }

    bool ObstaculoNaAltura(float altura)
    {
        Vector3 origem = transform.position + Vector3.up * altura;

        RaycastHit[] hits = Physics.SphereCastAll(origem, raioVisao, transform.forward, distanciaVisao, ~0, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit h = hits[i];

            if (h.collider.transform.IsChildOf(transform)) continue; // colisores do próprio médico
            if (h.normal.y > limiteChao) continue;                   // chão

            return true;
        }
        return false;
    }

    // Caminho "largo" (com a folga da zona de segurança) ocupado a partir de um ponto, nas duas alturas.
    // origemBase = posição dos pés.
    bool CaminhoOcupado(Vector3 origemBase, Vector3 direcao, float distancia)
    {
        return CaminhoOcupadoNaAltura(origemBase, direcao, distancia, alturaVisao)
            || CaminhoOcupadoNaAltura(origemBase, direcao, distancia, alturaVisaoMedia);
    }

    bool CaminhoOcupadoNaAltura(Vector3 origemBase, Vector3 direcao, float distancia, float altura)
    {
        Vector3 origem = origemBase + Vector3.up * altura;

        RaycastHit[] hits = Physics.SphereCastAll(origem, raioSeguranca, direcao, distancia, ~0, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit h = hits[i];

            if (h.collider.transform.IsChildOf(transform)) continue;
            if (h.normal.y > limiteChao) continue;

            return true;
        }
        return false;
    }

    // Tem algo que a física empurra (carro, caixa...) dentro da zona de segurança do corpo?
    // apenasNaFrente = true: só conta o que está na frente.
    // apenasNaFrente = false: conta o que está em volta toda.
    bool CorpoEmpurravelPerto(bool apenasNaFrente)
    {
        Vector3 centro = transform.position + Vector3.up * alturaVisaoMedia;

        Collider[] colisores = Physics.OverlapSphere(centro, raioSeguranca, ~0, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < colisores.Length; i++)
        {
            Collider c = colisores[i];

            if (c.transform.IsChildOf(transform)) continue; // colisores do próprio médico

            // Só importa o que a física consegue empurrar
            Rigidbody rb = c.attachedRigidbody;
            if (rb == null || rb.isKinematic) continue;

            if (apenasNaFrente)
            {
                Vector3 pontoMaisPerto = c.bounds.ClosestPoint(centro);
                Vector3 para = pontoMaisPerto - centro;
                para.y = 0f;

                if (Vector3.Dot(transform.forward, para) <= 0f) continue;
            }

            return true;
        }

        return false;
    }

    // Se algo empurrável está encostando no corpo, anda para longe dele (sem esperar).
    // Devolve true se conseguiu se afastar neste frame.
    bool AfastarDoCorpoEmpurravel()
    {
        Vector3 centro = transform.position + Vector3.up * alturaVisaoMedia;

        Collider[] colisores = Physics.OverlapSphere(centro, raioSeguranca, ~0, QueryTriggerInteraction.Ignore);

        Rigidbody maisPerto = null;
        Vector3 pontoMaisPerto = Vector3.zero;
        float menorDistancia = float.MaxValue;

        for (int i = 0; i < colisores.Length; i++)
        {
            Collider c = colisores[i];

            if (c.transform.IsChildOf(transform)) continue;

            Rigidbody rb = c.attachedRigidbody;
            if (rb == null || rb.isKinematic) continue;

            Vector3 ponto = c.bounds.ClosestPoint(centro);
            float d = (ponto - centro).sqrMagnitude;

            if (d < menorDistancia)
            {
                menorDistancia = d;
                maisPerto = rb;
                pontoMaisPerto = ponto;
            }
        }

        if (maisPerto == null) return false;

        Vector3 afastar = centro - pontoMaisPerto;
        afastar.y = 0f;
        if (afastar.sqrMagnitude < 0.0001f) afastar = -transform.forward;
        afastar.Normalize();

        // Tenta se afastar direto; se não der, tenta para os lados
        Vector3[] opcoes =
        {
            afastar,
            Quaternion.Euler(0f, 90f, 0f) * afastar,
            Quaternion.Euler(0f, -90f, 0f) * afastar
        };

        for (int i = 0; i < opcoes.Length; i++)
        {
            if (CaminhoLivreAfastando(centro, opcoes[i], 0.5f, maisPerto))
            {
                transform.position += opcoes[i] * velocidade * Time.deltaTime;
                return true;
            }
        }

        return false;
    }

    // Caminho livre para andar um pouco, ignorando o corpo de quem ele está se afastando
    bool CaminhoLivreAfastando(Vector3 centro, Vector3 direcao, float distancia, Rigidbody ignorar)
    {
        RaycastHit[] hits = Physics.SphereCastAll(centro, raioVisao, direcao, distancia, ~0, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit h = hits[i];

            if (h.collider.transform.IsChildOf(transform)) continue;
            if (h.collider.attachedRigidbody == ignorar) continue;
            if (h.normal.y > limiteChao) continue;

            return false;
        }
        return true;
    }

    // ---------------------------------------------------------------------
    // CONTORNO
    // ---------------------------------------------------------------------

    // Procura o menor desvio (direita ou esquerda) com caminho livre. Devolve false se não achar.
    bool IniciarContorno()
    {
        Vector3 frente = transform.forward;
        frente.y = 0f;
        frente.Normalize();

        Vector3 direita = Vector3.Cross(Vector3.up, frente);
        Vector3 pos = transform.position;

        bool direitaPrimeiro = Random.value < 0.5f;

        for (float d = 1f; d <= maxDesvioLateral + 0.01f; d += 0.5f)
        {
            for (int k = 0; k < 2; k++)
            {
                float lado = ((k == 0) == direitaPrimeiro) ? 1f : -1f;

                // 1) O caminho até o lado está livre?
                if (CaminhoOcupado(pos, direita * lado, d)) continue;

                // 2) Dali para frente está livre?
                Vector3 pontoLateral = pos + direita * (d * lado);
                if (CaminhoOcupado(pontoLateral, frente, distanciaVisao)) continue;

                // Achou o lado livre: começa o contorno
                linhaOrigem = pos;
                rotaFrente = frente;
                rotaDireita = direita;
                desvioAlvo = (d + 0.2f) * lado;   // 0,2 m de folga extra
                distanciaPassando = 0f;

                fase = FaseContorno.Desviando;
                contornando = true;
                return true;
            }
        }

        return false;
    }

    void AtualizarContorno()
    {
        float dt = Time.deltaTime;

        float lateral = Vector3.Dot(transform.position - linhaOrigem, rotaDireita);

        // Chegou no lado livre: começa a passar pelo obstáculo
        if (fase == FaseContorno.Desviando && Mathf.Abs(desvioAlvo - lateral) < 0.3f)
        {
            fase = FaseContorno.Passando;
            distanciaPassando = 0f;
        }

        // Passando: anda ao lado do obstáculo até passar dele
        if (fase == FaseContorno.Passando)
        {
            // Apareceu outro obstáculo à frente? Termina este contorno; a lógica normal começa outro
            if (ObstaculoNaFrente())
            {
                FinalizarContorno(false);
                return;
            }

            if (ObstaculoDoLado())
                distanciaPassando = 0f;
            else
                distanciaPassando += velocidade * dt;

            if (distanciaPassando >= margemAposObstaculo)
                fase = FaseContorno.Voltando;
        }

        float alvoLateral = (fase == FaseContorno.Voltando) ? 0f : desvioAlvo;
        float erro = alvoLateral - lateral;

        // Voltou para a linha da rota: acabou
        if (fase == FaseContorno.Voltando && Mathf.Abs(erro) < 0.3f)
        {
            FinalizarContorno(true);
            return;
        }

        // Direção desejada: segue para frente na rota e corrige o lado
        Vector3 dirDesejada = rotaFrente + rotaDireita * Mathf.Clamp(erro, -1f, 1f);
        dirDesejada.Normalize();

        // Vira suavemente para a direção desejada
        Quaternion rotacaoDesejada = Quaternion.LookRotation(dirDesejada, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, rotacaoDesejada, velocidadeRotacao * 90f * dt);

        // Anda. Se tem algo colado na frente, anda só para o lado (não empurra nem atravessa).
        if (CaminhoOcupado(transform.position, transform.forward, 0.6f + velocidade * dt))
        {
            if (Mathf.Abs(erro) < 0.3f)
            {
                // Sem como seguir: termina; a lógica normal procura outro contorno
                FinalizarContorno(false);
                return;
            }

            transform.position += rotaDireita * Mathf.Sign(erro) * velocidade * dt;
        }
        else
        {
            transform.position += transform.forward * velocidade * dt;
        }
    }

    // Ainda tem obstáculo ao lado dele, no lado da linha da rota?
    bool ObstaculoDoLado()
    {
        Vector3 direcaoInterna = -Mathf.Sign(desvioAlvo) * rotaDireita;
        return CaminhoOcupado(transform.position, direcaoInterna, Mathf.Abs(desvioAlvo));
    }

    void FinalizarContorno(bool alinharComARota)
    {
        fase = FaseContorno.Nenhuma;
        contornando = false;

        if (alinharComARota)
        {
            transform.rotation = Quaternion.LookRotation(rotaFrente, Vector3.up);
        }
    }

    // ---------------------------------------------------------------------
    // GIROS E ROTA
    // ---------------------------------------------------------------------

    IEnumerator Girar(float anguloGiro)
    {
        Quaternion rotacaoInicial = transform.rotation;
        Quaternion rotacaoAlvo = rotacaoInicial * Quaternion.Euler(0, anguloGiro, 0);

        float t = 0;
        while (t < 1f)
        {
            // Algo empurrável encostando no corpo: afasta-se andando (sem esperar) e só gira quando liberar.
            // Se estiver cercado e não puder se afastar, gira mesmo assim.
            if (AfastarDoCorpoEmpurravel())
            {
                yield return null;
                continue;
            }

            t += Time.deltaTime * velocidadeRotacao;
            transform.rotation = Quaternion.Slerp(rotacaoInicial, rotacaoAlvo, t);
            yield return null;
        }

        transform.rotation = rotacaoAlvo;
    }

    // Só chamado quando não existe lado livre para contornar: dá meia-volta
    IEnumerator ProcessarObstaculo()
    {
        estaGirando = true;
        girandoPorObstaculo = true;

        // Volta só o que já andou neste trecho, depois a rota segue no sentido contrário
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

        yield return StartCoroutine(Girar(anguloGiro));

        distanciaAlvo = passosDaRota[indicePassoAtual].distancia;
        pontoInicialPasso = transform.position;
        estaGirando = false;
    }

    public void Morrer()
    {
        estaVivo = false;
        contornando = false;
        fase = FaseContorno.Nenhuma;
        StopAllCoroutines();

        if (animator != null)
        {
            animator.enabled = false;
        }

        if (colisorPrincipal != null)
        {
            colisorPrincipal.enabled = false;
        }
    }
}
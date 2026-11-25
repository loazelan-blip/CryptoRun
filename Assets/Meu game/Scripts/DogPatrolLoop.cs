using System;
using System.Collections.Generic;
using UnityEngine;

public class DogPatrolLoop : MonoBehaviour
{
    private Animator anim;

    [System.Serializable]
    public class PassoPatrulha
    {
        public string nomePasso = "Passo";
        public float distancia = 5.0f;
        public float velocidade = 2.0f;       // Se > 5, ativa corrida (ID 4)
        public float velocidadeGiro = 180f;

        public enum DirecaoGiro { Direita, Esquerda }
        public DirecaoGiro escolhaGiro = DirecaoGiro.Direita;
    }

    [Header("Sequência de Passos")]
    [Tooltip("Clique no botão + para adicionar novos passos de patrulha")]
    public List<PassoPatrulha> passos = new List<PassoPatrulha>();

    [Header("Obstáculo à frente")]
    [Tooltip("Distância (em metros) à frente em que ele enxerga um obstáculo. Ao ver, ele dá meia-volta, volta exatamente o que andou neste passo e segue para o próximo passo.")]
    public float distanciaVisao = 0.8f;
    [Tooltip("Altura (em metros, a partir dos pés) de onde ele olha para frente.")]
    public float alturaVisao = 0.3f;
    [Tooltip("Largura da esfera que ele lança para frente para detectar obstáculos.")]
    public float raioVisao = 0.15f;
    [Tooltip("Normal (vetor perpendicular à superfície) acima deste valor = chão, não é obstáculo.")]
    [Range(0.3f, 1f)] public float limiteChao = 0.5f;

    private int passoAtualIndex = 0;
    private Vector3 pontoInicial;
    private Vector3 pontoDestino;
    private Quaternion rotacaoInicial;
    private Quaternion rotacaoFinal;

    private bool obstaculoPendente = false; // girando por causa de um obstáculo
    private bool retornando = false;        // voltando o que andou até o começo do passo

    private enum EstadoDog { Caminhando, Girando }
    private EstadoDog estadoAtual = EstadoDog.Caminhando;

    void Start()
    {
        anim = GetComponent<Animator>();

        // Se a lista estiver vazia no Inspector, cria um passo padrão para não dar erro
        if (passos.Count == 0)
        {
            passos.Add(new PassoPatrulha());
        }

        passoAtualIndex = 0;
        IniciarCaminhada();
    }

    void Update()
    {
        if (passos.Count == 0) return;

        PassoPatrulha passoAtual = passos[passoAtualIndex];

        if (estadoAtual == EstadoDog.Caminhando)
        {
            AtualizarAnimacaoMovimento(passoAtual.velocidade);

            // Obstáculo à frente (só quando está indo, não quando está voltando)
            if (!retornando && ObstaculoNaFrente())
            {
                obstaculoPendente = true;
                IniciarGiro();
                return;
            }

            // Move o cachorro até o destino atual
            transform.position = Vector3.MoveTowards(transform.position, pontoDestino, passoAtual.velocidade * Time.deltaTime);

            // Chegou no destino
            if (Vector3.Distance(transform.position, pontoDestino) < 0.05f)
            {
                transform.position = pontoDestino;

                if (retornando)
                {
                    // Voltou até o começo do passo: segue para o próximo passo da lista
                    retornando = false;
                    passoAtualIndex = (passoAtualIndex + 1) % passos.Count;
                    IniciarCaminhada();
                }
                else
                {
                    IniciarGiro();
                }
            }
        }
        else if (estadoAtual == EstadoDog.Girando)
        {
            // Gira o cachorro de acordo com a velocidade do passo atual
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotacaoFinal, passoAtual.velocidadeGiro * Time.deltaTime);

            // Terminou de girar 180 graus
            if (Quaternion.Angle(transform.rotation, rotacaoFinal) < 0.1f)
            {
                transform.rotation = rotacaoFinal;

                if (obstaculoPendente)
                {
                    // Volta exatamente o que andou neste passo (até o ponto onde o passo começou)
                    obstaculoPendente = false;
                    retornando = true;
                    pontoDestino = pontoInicial;
                    estadoAtual = EstadoDog.Caminhando;
                }
                else
                {
                    // Avança para o próximo passo da lista (ou volta ao início quando chega no fim)
                    passoAtualIndex = (passoAtualIndex + 1) % passos.Count;
                    IniciarCaminhada();
                }
            }
        }
    }

    bool ObstaculoNaFrente()
    {
        Vector3 origem = transform.position + Vector3.up * alturaVisao;

        RaycastHit[] hits = Physics.SphereCastAll(origem, raioVisao, transform.forward, distanciaVisao, ~0, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit h = hits[i];

            if (h.collider.transform.IsChildOf(transform)) continue; // colisores do próprio cachorro
            if (h.normal.y > limiteChao) continue;                   // chão

            return true;
        }
        return false;
    }

    void IniciarCaminhada()
    {
        estadoAtual = EstadoDog.Caminhando;
        PassoPatrulha passoAtual = passos[passoAtualIndex];

        pontoInicial = transform.position;
        pontoDestino = pontoInicial + transform.forward * passoAtual.distancia;

        AtualizarAnimacaoMovimento(passoAtual.velocidade);
    }

    void IniciarGiro()
    {
        estadoAtual = EstadoDog.Girando;
        PassoPatrulha passoAtual = passos[passoAtualIndex];

        rotacaoInicial = transform.rotation;

        float angulo = (passoAtual.escolhaGiro == PassoPatrulha.DirecaoGiro.Direita) ? 180f : -180f;
        rotacaoFinal = rotacaoInicial * Quaternion.Euler(0, angulo, 0);

        AtualizarAnimacaoMovimento(passoAtual.velocidade);
    }

    void AtualizarAnimacaoMovimento(float vel)
    {
        if (anim == null) return;

        if (vel > 5f)
        {
            anim.SetInteger("AnimationID", 4); // Corrida
        }
        else
        {
            anim.SetInteger("AnimationID", 2); // Caminhada
        }
    }
}
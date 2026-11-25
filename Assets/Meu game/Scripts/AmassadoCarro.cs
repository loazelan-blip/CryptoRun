using UnityEngine;

public class AmassadoCarro : MonoBehaviour
{
    public enum Lado { Frente, Traseira }
    public enum Nivel { Fraca, Media, Forte }

    [Header("Filtro do chão")]
    public bool ignorarChao = true;
    [Range(0.3f, 1f)] public float limiteChao = 0.7f;

    [Header("FRENTE - força do impacto")]
    public float frenteFracaMinima = 5f;   // abaixo disso não conta
    public float frenteFracaMaxima = 50f;
    public float frenteMediaMaxima = 80f;  // acima disso = Forte

    [Header("TRASEIRA - força do impacto")]
    public float traseiraFracaMinima = 5f;
    public float traseiraFracaMaxima = 50f;
    public float traseiraMediaMaxima = 80f;

    [Header("Ajustes")]
    public float intervaloEntreBatidas = 0.3f;

    [Header("Malha com os Blend Shapes")]
    [Tooltip("Se ficar vazio, procura no Start dentro do carro.")]
    public SkinnedMeshRenderer malha;

    public int frenteShape1 = 0;
    public int frenteShape2 = 1;
    public int frenteShape3 = 2;
    public int traseiraShape1 = 3;
    public int traseiraShape2 = 4;
    public int traseiraShape3 = 5;

    [Header("Pontos de cada batida")]
    public int pontosFraca = 1;
    public int pontosMedia = 2;
    public int pontosForte = 4;

    [Header("Pontos para cada amassado (por lado)")]
    public int pontosParaAmassadoLeve = 3;
    public int pontosParaAmassadoMedio = 6;
    public int pontosParaAmassadoForte = 12;

    [Header("FUMAÇA controlada por este script")]
    [Tooltip("Arraste aqui o Particle System da fumaça (ex.: LineSmoke). Este script liga essa fumaça quando a FRENTE chega no amassado mais forte. Este é o único script que deve controlar essa fumaça.")]
    public ParticleSystem fumaca;
    [Tooltip("Só leitura: fica marcado quando a fumaça foi ligada por este script.")]
    public bool fumacaLigada;

    [Header("Contadores (só leitura)")]
    public int frenteFracas;
    public int frenteMedias;
    public int frenteFortes;
    public int frentePontos;
    public int amassadoFrente;      // 0 a 3

    public int traseiraFracas;
    public int traseiraMedias;
    public int traseiraFortes;
    public int traseiraPontos;
    public int amassadoTraseira;    // 0 a 3

    [Header("Velocidade do carro (só leitura)")]
    public float velocidadeAtualMS;
    public float velocidadeAtualKMH;

    [Header("Última batida (só leitura)")]
    public Lado ultimoLado;
    public Nivel ultimoNivel;
    public float ultimaForca;

    float tempoUltimaBatida = -999f;

    // Velocidade medida pela posição do carro (funciona mesmo que o carro ande por script)
    Vector3 posicaoAnterior;
    bool temPosicaoAnterior;
    readonly Vector3[] historicoVelocidade = new Vector3[7];
    int indiceHistorico;

    void Start()
    {
        if (malha == null) malha = GetComponentInChildren<SkinnedMeshRenderer>();
        ZerarAmassados();
        PararFumacas();
    }

    void Update()
    {
        Vector3 pos = transform.position;
        if (temPosicaoAnterior && Time.deltaTime > 0f)
        {
            Vector3 vel = (pos - posicaoAnterior) / Time.deltaTime;
            historicoVelocidade[indiceHistorico] = vel;
            indiceHistorico = (indiceHistorico + 1) % historicoVelocidade.Length;
            velocidadeAtualMS = vel.magnitude;
            velocidadeAtualKMH = velocidadeAtualMS * 3.6f;
        }
        posicaoAnterior = pos;
        temPosicaoAnterior = true;
    }

    // Mediana dos últimos frames (ignora picos isolados)
    Vector3 VelocidadeRecente()
    {
        int n = historicoVelocidade.Length;
        int[] ordem = new int[n];
        for (int i = 0; i < n; i++) ordem[i] = i;

        for (int i = 0; i < n - 1; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                if (historicoVelocidade[ordem[j]].sqrMagnitude < historicoVelocidade[ordem[i]].sqrMagnitude)
                {
                    int t = ordem[i];
                    ordem[i] = ordem[j];
                    ordem[j] = t;
                }
            }
        }
        return historicoVelocidade[ordem[n / 2]];
    }

    // Velocidade do carro na direção da superfície que ele bateu
    float ForcaDoImpacto(Collision colisao, ContactPoint c)
    {
        Vector3 v = VelocidadeRecente();
        if (v.magnitude < 0.5f)
        {
            // carro quase parado: usa a física
            return Mathf.Abs(Vector3.Dot(colisao.relativeVelocity, c.normal));
        }
        return Mathf.Max(0f, Vector3.Dot(v, -c.normal));
    }

    void OnCollisionEnter(Collision colisao)
    {
        if (colisao.contactCount == 0) return;

        // Primeiro contato que não é chão
        bool achou = false;
        ContactPoint contato = default;
        for (int i = 0; i < colisao.contactCount; i++)
        {
            ContactPoint c = colisao.GetContact(i);
            if (ignorarChao && c.normal.y > limiteChao) continue;
            contato = c;
            achou = true;
            break;
        }
        if (!achou) return;

        // Frente ou traseira
        bool frente = transform.InverseTransformPoint(contato.point).z > 0f;
        Lado lado = frente ? Lado.Frente : Lado.Traseira;

        float forca = ForcaDoImpacto(colisao, contato);

        float fracaMinima = frente ? frenteFracaMinima : traseiraFracaMinima;
        float fracaMaxima = frente ? frenteFracaMaxima : traseiraFracaMaxima;
        float mediaMaxima = frente ? frenteMediaMaxima : traseiraMediaMaxima;

        if (forca < fracaMinima) return;

        if (Time.time - tempoUltimaBatida < intervaloEntreBatidas) return;
        tempoUltimaBatida = Time.time;

        Nivel nivel;
        if (forca <= fracaMaxima) nivel = Nivel.Fraca;
        else if (forca <= mediaMaxima) nivel = Nivel.Media;
        else nivel = Nivel.Forte;

        ultimoLado = lado;
        ultimoNivel = nivel;
        ultimaForca = forca;

        RegistrarBatida(lado, nivel);
    }

    int PontosDoNivel(Nivel nivel)
    {
        if (nivel == Nivel.Fraca) return pontosFraca;
        if (nivel == Nivel.Media) return pontosMedia;
        return pontosForte;
    }

    int CalcularAmassado(int pontos)
    {
        if (pontos >= pontosParaAmassadoForte) return 3;
        if (pontos >= pontosParaAmassadoMedio) return 2;
        if (pontos >= pontosParaAmassadoLeve) return 1;
        return 0;
    }

    void RegistrarBatida(Lado lado, Nivel nivel)
    {
        int pontos = PontosDoNivel(nivel);

        if (lado == Lado.Frente)
        {
            if (nivel == Nivel.Fraca) frenteFracas++;
            else if (nivel == Nivel.Media) frenteMedias++;
            else frenteFortes++;

            frentePontos += pontos;
            int novo = CalcularAmassado(frentePontos);
            if (novo > amassadoFrente)   // o amassado nunca volta atrás
            {
                amassadoFrente = novo;
                AplicarShape(Lado.Frente, amassadoFrente);
            }
        }
        else
        {
            if (nivel == Nivel.Fraca) traseiraFracas++;
            else if (nivel == Nivel.Media) traseiraMedias++;
            else traseiraFortes++;

            traseiraPontos += pontos;
            int novo = CalcularAmassado(traseiraPontos);
            if (novo > amassadoTraseira)
            {
                amassadoTraseira = novo;
                AplicarShape(Lado.Traseira, amassadoTraseira);
            }
        }

        // Fumaça só quando a FRENTE chegou no amassado mais forte (traseira não conta)
        if (amassadoFrente >= 3)
        {
            LigarFumacas();
        }
    }

    void AplicarShape(Lado lado, int estagio)
    {
        if (malha == null || malha.sharedMesh == null) return;

        int s1, s2, s3;
        if (lado == Lado.Frente)
        {
            s1 = frenteShape1; s2 = frenteShape2; s3 = frenteShape3;
        }
        else
        {
            s1 = traseiraShape1; s2 = traseiraShape2; s3 = traseiraShape3;
        }

        DefinirShape(s1, estagio == 1 ? 100f : 0f);
        DefinirShape(s2, estagio == 2 ? 100f : 0f);
        DefinirShape(s3, estagio == 3 ? 100f : 0f);
    }

    void DefinirShape(int indice, float peso)
    {
        if (indice < 0 || indice >= malha.sharedMesh.blendShapeCount) return;
        malha.SetBlendShapeWeight(indice, peso);
    }

    void ZerarAmassados()
    {
        if (malha == null || malha.sharedMesh == null) return;
        DefinirShape(frenteShape1, 0f);
        DefinirShape(frenteShape2, 0f);
        DefinirShape(frenteShape3, 0f);
        DefinirShape(traseiraShape1, 0f);
        DefinirShape(traseiraShape2, 0f);
        DefinirShape(traseiraShape3, 0f);
    }

    void LigarFumacas()
    {
        if (fumaca == null) return;
        if (!fumaca.isPlaying) fumaca.Play();
        fumacaLigada = true;
    }

    void PararFumacas()
    {
        fumacaLigada = false;
        if (fumaca == null) return;
        fumaca.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}
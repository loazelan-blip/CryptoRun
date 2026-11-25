using UnityEngine;

public class CarNpcDano : MonoBehaviour
{
    [Header("Malha com os Blend Shapes")]
    [Tooltip("Se ficar vazio, procura no Start dentro do carro.")]
    public SkinnedMeshRenderer malha;

    [Header("Índices dos Blend Shapes - FRENTE")]
    public int frenteLeve = 0;
    public int frenteMedio = 1;
    public int frentePesado = 2;

    [Header("Índices dos Blend Shapes - TRASEIRA")]
    public int traseiraLeve = 3;
    public int traseiraMedio = 4;
    public int traseiraPesado = 5;

    [Header("Ajustes")]
    [Tooltip("Tempo mínimo entre duas batidas contadas (evita contar a mesma batida várias vezes).")]
    public float intervaloEntreBatidas = 0.3f;
    [Tooltip("Contatos com normal (vetor perpendicular à superfície) acima deste valor são chão e não contam.")]
    [Range(0.3f, 1f)] public float limiteChao = 0.7f;

    [Header("Contadores (só leitura)")]
    public int colisoesFrente;
    public int colisoesTraseira;

    float tempoUltimaBatida = -999f;

    void Start()
    {
        if (malha == null) malha = GetComponentInChildren<SkinnedMeshRenderer>();
        AplicarShape(frenteLeve, frenteMedio, frentePesado, 0);
        AplicarShape(traseiraLeve, traseiraMedio, traseiraPesado, 0);
    }

    void OnCollisionEnter(Collision colisao)
    {
        if (Time.time - tempoUltimaBatida < intervaloEntreBatidas) return;

        for (int i = 0; i < colisao.contactCount; i++)
        {
            ContactPoint c = colisao.GetContact(i);
            if (c.normal.y > limiteChao) continue; // chão

            tempoUltimaBatida = Time.time;

            bool frente = transform.InverseTransformPoint(c.point).z > 0f;

            if (frente)
            {
                colisoesFrente++;
                AplicarShape(frenteLeve, frenteMedio, frentePesado, colisoesFrente);
            }
            else
            {
                colisoesTraseira++;
                AplicarShape(traseiraLeve, traseiraMedio, traseiraPesado, colisoesTraseira);
            }
            return;
        }
    }

    // estagio: 0 = sem amassado, 1 = leve, 2 = médio, 3 ou mais = pesado
    void AplicarShape(int leve, int medio, int pesado, int estagio)
    {
        if (malha == null || malha.sharedMesh == null) return;

        DefinirShape(leve, estagio == 1 ? 100f : 0f);
        DefinirShape(medio, estagio == 2 ? 100f : 0f);
        DefinirShape(pesado, estagio >= 3 ? 100f : 0f);
    }

    void DefinirShape(int indice, float peso)
    {
        if (indice < 0 || indice >= malha.sharedMesh.blendShapeCount) return;
        malha.SetBlendShapeWeight(indice, peso);
    }
}
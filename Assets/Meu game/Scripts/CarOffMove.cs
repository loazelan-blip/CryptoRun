using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarOffMove : MonoBehaviour
{
    [Header("Alvo da Malha")]
    [Tooltip("Arraste o SkinnedMeshRenderer do carro (hotdog_truck_seperate) aqui")]
    public SkinnedMeshRenderer corpoCarro;

    [Header("Nomes dos BlendShapes de Perda Total")]
    public string nomeFrente3 = "frente 3";
    public string nomeTras3 = "tras 3";

    [Header("Monitor de Estado")]
    public bool carroDesligado = false;

    private int indexFrente3 = -1;
    private int indexTras3 = -1;
    private CarMove carMove;

    private void Start()
    {
        // Procura o CarMove neste objeto, depois nos pais, depois nos filhos
        carMove = GetComponent<CarMove>();
        if (carMove == null) carMove = GetComponentInParent<CarMove>();
        if (carMove == null) carMove = GetComponentInChildren<CarMove>();

        if (corpoCarro == null)
        {
            corpoCarro = GetComponentInChildren<SkinnedMeshRenderer>();
        }

        if (corpoCarro != null)
        {
            // Mapeia os índices dos BlendShapes no início
            indexFrente3 = corpoCarro.sharedMesh.GetBlendShapeIndex(nomeFrente3);
            indexTras3 = corpoCarro.sharedMesh.GetBlendShapeIndex(nomeTras3);
        }
    }

    private void Update()
    {
        if (carroDesligado || corpoCarro == null) return;

        // Checa se frente 3 ou tras 3 foram ativados (peso maior que 0)
        bool frente3Ativo = (indexFrente3 >= 0) && (corpoCarro.GetBlendShapeWeight(indexFrente3) > 0f);
        bool tras3Ativo = (indexTras3 >= 0) && (corpoCarro.GetBlendShapeWeight(indexTras3) > 0f);

        if (frente3Ativo || tras3Ativo)
        {
            ReduzirVelocidade();
        }
    }

    private void ReduzirVelocidade()
    {
        carroDesligado = true; // marca que já aconteceu, para reduzir só uma vez

        if (carMove != null)
        {
            carMove.velocidade *= 0.95f; // diminui 5%
        }
    }
}
using UnityEngine;

public class SensorTraseiro : MonoBehaviour
{
    public enum Severidade { Nenhum, Leve, Medio, Pesado }
    public enum LocalSensor { Frente, Traseira }

    [Header("Configuração do Sensor")]
    public LocalSensor localDoSensor = LocalSensor.Frente;

    [Header("Alvos")]
    [Tooltip("Arraste o Transform do Carro (CarDanoFrontBack) aqui")]
    public Transform carroTransform;

    [Tooltip("Arraste a malha do carro (SkinnedMeshRenderer) com BlendShapes")]
    public SkinnedMeshRenderer corpoCarro;

    [Header("Tamanho do Carro (Sem Collider)")]
    [Tooltip("Distância do centro até o para-choque da frente")]
    public float offsetParaChoqueFrente = 2.2f;

    [Tooltip("Distância do centro até o para-choque da traseira")]
    public float offsetParaChoqueTraseira = 2.2f;

    [Header("Área de Alcance do Sensor (Sem Collider)")]
    public Vector3 tamanhoCaixa = new Vector3(20f, 10f, 50f);
    public Vector3 posicaoCaixaOffset = Vector3.zero;

    [Header("REGULAGEM DE KM/H DOS AMASSADOS")]
    public float velMinimaEstagio1 = 10f;
    public float velMinimaEstagio2 = 20f;
    public float velMinimaEstagio3 = 30f;
    public float velDanoMaximoDireto = 40f;

    [Header("Mapeamento das BlendShapes (Índices)")]
    public int indexFrente1 = 0;
    public int indexFrente2 = 1;
    public int indexFrente3 = 2;

    public int indexTras1 = 3;
    public int indexTras2 = 4;
    public int indexTras3 = 5;

    [Header("Cooldown")]
    public float cooldownImpacto = 0.5f;

    [Header("Painel do Sensor (Monitores no Inspector)")]
    public bool carroNaArea;
    public float velocidadeLidaKMH;
    public int toquesRegistrados = 0;
    public int estagioAtualDano = 0;

    private float ultimoImpactoTime = -999f;
    private Vector3 ultimaPosicaoCarro;

    private void Start()
    {
        if (carroTransform != null)
        {
            ultimaPosicaoCarro = carroTransform.position;
        }
    }

    private void Update()
    {
        if (carroTransform == null) return;

        // 1. OBTÉM OS 3 PONTOS CHAVE DO VEÍCULO (Frente, Centro e Traseira)
        Vector3 pontaFrente = carroTransform.position + (carroTransform.forward * offsetParaChoqueFrente);
        Vector3 centroCarro = carroTransform.position;
        Vector3 pontaTraseira = carroTransform.position - (carroTransform.forward * offsetParaChoqueTraseira);

        // 2. DETETA SE QUALQUER UMA DAS PARTES ENTROU NA CAIXA AMARELA
        bool frenteDentro = ChecarPresencaMatematica(pontaFrente);
        bool centroDentro = ChecarPresencaMatematica(centroCarro);
        bool trasDentro = ChecarPresencaMatematica(pontaTraseira);

        carroNaArea = frenteDentro || centroDentro || trasDentro;

        if (carroNaArea)
        {
            // Calcula a velocidade do veículo em tempo real
            float dist = Vector3.Distance(carroTransform.position, ultimaPosicaoCarro);
            if (Time.deltaTime > 0f)
            {
                velocidadeLidaKMH = (dist / Time.deltaTime) * 3.6f;
            }

            Debug.Log($"📡 SENSOR REGISTROU | Vel: {velocidadeLidaKMH:F1} km/h | Dentro da Área: SIM");

            ProcessarTabelaDano(velocidadeLidaKMH);
        }
        else
        {
            velocidadeLidaKMH = 0f;
        }

        ultimaPosicaoCarro = carroTransform.position;
    }

    private bool ChecarPresencaMatematica(Vector3 ponto)
    {
        Vector3 centroMundo = transform.position + transform.TransformDirection(posicaoCaixaOffset);
        Vector3 pontoLocal = Quaternion.Inverse(transform.rotation) * (ponto - centroMundo);
        Vector3 metadeTamanho = tamanhoCaixa * 0.5f;

        return (Mathf.Abs(pontoLocal.x) <= metadeTamanho.x) &&
               (Mathf.Abs(pontoLocal.y) <= metadeTamanho.y) &&
               (Mathf.Abs(pontoLocal.z) <= metadeTamanho.z);
    }

    private void ProcessarTabelaDano(float vel)
    {
        if (vel < velMinimaEstagio1) return;

        if (Time.time - ultimoImpactoTime < cooldownImpacto) return;

        int novoEstagio = estagioAtualDano;

        if (vel >= velDanoMaximoDireto)
        {
            novoEstagio = 3;
        }
        else if (vel >= velMinimaEstagio3)
        {
            if (estagioAtualDano < 2) novoEstagio = 2;
            else if (estagioAtualDano == 2) novoEstagio = 3;
        }
        else if (vel >= velMinimaEstagio2)
        {
            if (estagioAtualDano < 1) novoEstagio = 1;
            else if (estagioAtualDano == 1) novoEstagio = 2;
            else if (estagioAtualDano == 2) novoEstagio = 3;
        }
        else if (vel >= velMinimaEstagio1)
        {
            toquesRegistrados++;
            ultimoImpactoTime = Time.time;

            if (toquesRegistrados >= 2 && estagioAtualDano < 1) novoEstagio = 1;
            else if (estagioAtualDano == 1) novoEstagio = 2;
            else if (estagioAtualDano == 2) novoEstagio = 3;

            if (novoEstagio > estagioAtualDano)
            {
                estagioAtualDano = novoEstagio;
                AplicarBlendShape(estagioAtualDano);
            }
            return;
        }

        if (novoEstagio > estagioAtualDano)
        {
            ultimoImpactoTime = Time.time;
            toquesRegistrados++;
            estagioAtualDano = novoEstagio;
            AplicarBlendShape(estagioAtualDano);
        }
    }

    private void AplicarBlendShape(int estagio)
    {
        if (corpoCarro == null) return;

        int indexParaAtivar = -1;

        if (localDoSensor == LocalSensor.Frente)
        {
            if (estagio == 1) indexParaAtivar = indexFrente1;
            else if (estagio == 2) indexParaAtivar = indexFrente2;
            else if (estagio == 3) indexParaAtivar = indexFrente3;
        }
        else if (localDoSensor == LocalSensor.Traseira)
        {
            if (estagio == 1) indexParaAtivar = indexTras1;
            else if (estagio == 2) indexParaAtivar = indexTras2;
            else if (estagio == 3) indexParaAtivar = indexTras3;
        }

        if (indexParaAtivar >= 0 && indexParaAtivar < corpoCarro.sharedMesh.blendShapeCount)
        {
            ZerarBlendShapesDoLado(localDoSensor);
            corpoCarro.SetBlendShapeWeight(indexParaAtivar, 100f);

            string nome = corpoCarro.sharedMesh.GetBlendShapeName(indexParaAtivar);
            Debug.Log($"💥 SENSOR IMPACTO | Vel: {velocidadeLidaKMH:F1} km/h | Estágio: {estagio} | Shape: {nome}");
        }
    }

    private void ZerarBlendShapesDoLado(LocalSensor lado)
    {
        if (lado == LocalSensor.Frente)
        {
            if (indexFrente1 >= 0 && indexFrente1 < corpoCarro.sharedMesh.blendShapeCount) corpoCarro.SetBlendShapeWeight(indexFrente1, 0f);
            if (indexFrente2 >= 0 && indexFrente2 < corpoCarro.sharedMesh.blendShapeCount) corpoCarro.SetBlendShapeWeight(indexFrente2, 0f);
            if (indexFrente3 >= 0 && indexFrente3 < corpoCarro.sharedMesh.blendShapeCount) corpoCarro.SetBlendShapeWeight(indexFrente3, 0f);
        }
        else if (lado == LocalSensor.Traseira)
        {
            if (indexTras1 >= 0 && indexTras1 < corpoCarro.sharedMesh.blendShapeCount) corpoCarro.SetBlendShapeWeight(indexTras1, 0f);
            if (indexTras2 >= 0 && indexTras2 < corpoCarro.sharedMesh.blendShapeCount) corpoCarro.SetBlendShapeWeight(indexTras2, 0f);
            if (indexTras3 >= 0 && indexTras3 < corpoCarro.sharedMesh.blendShapeCount) corpoCarro.SetBlendShapeWeight(indexTras3, 0f);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = (localDoSensor == LocalSensor.Frente) ? Color.yellow : Color.red;
        Matrix4x4 matOriginal = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(posicaoCaixaOffset, tamanhoCaixa);
        Gizmos.matrix = matOriginal;

        // Desenha 3 esferas ciano na Scene representando a Frente, Centro e Traseira do carro
        if (carroTransform != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(carroTransform.position + (carroTransform.forward * offsetParaChoqueFrente), 0.2f);
            Gizmos.DrawSphere(carroTransform.position, 0.2f);
            Gizmos.DrawSphere(carroTransform.position - (carroTransform.forward * offsetParaChoqueTraseira), 0.2f);
        }
    }
}
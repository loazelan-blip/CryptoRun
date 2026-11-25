using UnityEngine;

/// <summary>
/// Coloque este script no CAMINHÃO (objeto raiz, onde também deve estar o Rigidbody
/// que o SpeedImpactSensor vai ler). Ele guarda a referência do SkinnedMeshRenderer
/// e liga/desliga os blend shapes de amassado conforme a severidade do impacto.
/// </summary>
public class VehicleDentReceiver : MonoBehaviour
{
    [Header("Mesh com os 6 amassados (blend shapes)")]
    public SkinnedMeshRenderer skinnedMesh;

    [System.Serializable]
    public struct AmassadoConfig
    {
        [Tooltip("Nome exato do Blend Shape no SkinnedMeshRenderer")]
        public string nomeBlendShape;
    }

    [Header("Amassados - Leve (peso 100 quando ativado)")]
    public AmassadoConfig[] amassadosLeve;

    [Header("Amassados - Medio")]
    public AmassadoConfig[] amassadosMedio;

    [Header("Amassados - Pesado")]
    public AmassadoConfig[] amassadosPesado;

    public void AplicarAmassado(SpeedImpactSensor.Severidade severidade, float velocidadeDetectada)
    {
        if (skinnedMesh == null)
        {
            Debug.LogWarning($"[VehicleDentReceiver] '{name}' nao tem SkinnedMeshRenderer configurado.");
            return;
        }

        AmassadoConfig[] grupo = severidade switch
        {
            SpeedImpactSensor.Severidade.Leve => amassadosLeve,
            SpeedImpactSensor.Severidade.Medio => amassadosMedio,
            SpeedImpactSensor.Severidade.Pesado => amassadosPesado,
            _ => null
        };

        if (grupo == null) return;

        foreach (var amassado in grupo)
        {
            AtivarBlendShape(amassado.nomeBlendShape, 100f);
        }

        Debug.Log($"[VehicleDentReceiver] Impacto a {velocidadeDetectada:F1} — severidade: {severidade}");
    }

    private void AtivarBlendShape(string nome, float peso)
    {
        int index = skinnedMesh.sharedMesh.GetBlendShapeIndex(nome);
        if (index < 0)
        {
            Debug.LogWarning($"[VehicleDentReceiver] Blend shape '{nome}' nao encontrado na mesh.");
            return;
        }

        skinnedMesh.SetBlendShapeWeight(index, peso);
    }

    // Util para resetar o caminhao (ex: reaproveitar em pool de objetos)
    [ContextMenu("Resetar Todos os Amassados")]
    public void ResetarAmassados()
    {
        if (skinnedMesh == null) return;

        foreach (var a in amassadosLeve) AtivarBlendShape(a.nomeBlendShape, 0f);
        foreach (var a in amassadosMedio) AtivarBlendShape(a.nomeBlendShape, 0f);
        foreach (var a in amassadosPesado) AtivarBlendShape(a.nomeBlendShape, 0f);
    }
}
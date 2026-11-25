using UnityEngine;

public class SetaOnOff : MonoBehaviour
{
    [Header("Objeto para Monitorar (Referência)")]
    public GameObject objetoMonitorado;

    [Header("Objeto 3D para Controlar")]
    public GameObject objeto3D;

    void Update()
    {
        if (objetoMonitorado == null || objeto3D == null) return;

        // O objeto3D será ativado se o objetoMonitorado estiver ativo, e desativado caso contrário
        bool deveEstarAtivo = objetoMonitorado.activeSelf;

        if (objeto3D.activeSelf != deveEstarAtivo)
        {
            objeto3D.SetActive(deveEstarAtivo);
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

public class Sensor : MonoBehaviour
{
    [Header("Scripts a serem Controlados")]
    public List<MonoBehaviour> scriptsParaControlar = new List<MonoBehaviour>();

    [Header("Configurações do Sensor (Sem Collider)")]
    public float distanciaAlcance = 10f;
    [Tooltip("Aumente este valor se o raio estiver a bater no próprio cubo/carro")]
    public float distanciaMinima = 1.0f;
    public LayerMask camadasParaDetectar = ~0;

    [Header("Posição Vertical do Sensor")]
    [Tooltip("Positivo = para cima | Negativo = para baixo")]
    public float alturaSensor = 0f;

    [Header("Direção do Raio")]
    [Tooltip("Marque esta caixa se o raio estiver a apontar para trás!")]
    public bool inverterDirecao = true;

    [Header("Estado Atual")]
    public bool objetoDetectado = false;
    public string objetoAtingido = "Nenhum";

    private Vector3 pontoFinalGizmo;

    private Vector3 ObterDirecaoFrente()
    {
        return inverterDirecao ? -transform.forward : transform.forward;
    }

    private void Update()
    {
        Vector3 direcaoFrente = ObterDirecaoFrente();

        Vector3 deslocamentoVertical = Vector3.up * alturaSensor;

        Vector3 pontoInicial = transform.position + deslocamentoVertical + (direcaoFrente * distanciaMinima);

        float alcanceEfetivo = Mathf.Max(0.1f, distanciaAlcance - distanciaMinima);

        RaycastHit[] hits = Physics.RaycastAll(pontoInicial, direcaoFrente, alcanceEfetivo, camadasParaDetectar);

        bool detectouObstaculo = false;
        string nomeObjeto = "Nenhum";
        pontoFinalGizmo = pontoInicial + (direcaoFrente * alcanceEfetivo);

        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.IsChildOf(transform.root)) continue;

            detectouObstaculo = true;
            nomeObjeto = hit.collider.gameObject.name;
            pontoFinalGizmo = hit.point;
            break;
        }

        objetoAtingido = nomeObjeto;

        if (detectouObstaculo && !objetoDetectado)
        {
            objetoDetectado = true;
            AlternarScripts(false);
            Debug.LogWarning($"📡 OBSTÁCULO DETECTADO: [{nomeObjeto}]!");
        }
        else if (!detectouObstaculo && objetoDetectado)
        {
            objetoDetectado = false;
            AlternarScripts(true);
            Debug.Log($"✅ CAMINHO LIVRE!");
        }
    }

    private void AlternarScripts(bool estado)
    {
        foreach (MonoBehaviour script in scriptsParaControlar)
        {
            if (script != null)
            {
                script.enabled = estado;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 direcaoFrente = ObterDirecaoFrente();

        Vector3 deslocamentoVertical = Vector3.up * alturaSensor;

        Vector3 pontoInicial = transform.position + deslocamentoVertical + (direcaoFrente * distanciaMinima);

        Vector3 pontoDestinoFinal = transform.position + deslocamentoVertical + (direcaoFrente * distanciaAlcance);

        if (!Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(pontoInicial, pontoDestinoFinal);
            Gizmos.DrawWireSphere(pontoDestinoFinal, 0.2f);
        }
        else
        {
            Gizmos.color = objetoDetectado ? Color.red : Color.green;
            Gizmos.DrawLine(pontoInicial, pontoFinalGizmo);
            Gizmos.DrawWireSphere(pontoFinalGizmo, 0.15f);
        }
    }
}
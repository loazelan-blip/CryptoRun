using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CaminhoLinha : MonoBehaviour
{
    private LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    // Retorna todos os pontos definidos no LineRenderer
    public Vector3[] ObterPontos()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        Vector3[] pontos = new Vector3[lineRenderer.positionCount];
        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            pontos[i] = lineRenderer.GetPosition(i);
        }
        return pontos;
    }
}
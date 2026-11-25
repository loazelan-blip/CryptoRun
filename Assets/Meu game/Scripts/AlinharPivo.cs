using UnityEngine;

[DefaultExecutionOrder(200)]
public class AlinharPivo : MonoBehaviour
{
    [Header("CONFIGURAÇÃO DE ALVO")]
    public Transform objetoAlvo;

    [Header("COMPENSAÇÃO DE POSIÇÃO (OFFSET)")]
    public Vector3 offsetPosicao = Vector3.zero;

    [Header("COMPENSAÇÃO DE ROTAÇÃO (OFFSET)")]
    public Vector3 offsetRotacao = Vector3.zero;

    [Header("CONFIGURAÇÕES DE SEGUIMENTO")]
    public bool seguirPosicao = true;
    public bool seguirRotacao = true;

    void LateUpdate()
    {
        if (objetoAlvo == null) return;

        // 1. COPIA POSIÇÃO MUNDIAL
        if (seguirPosicao)
        {
            Vector3 posicaoDesejada = (offsetPosicao == Vector3.zero)
                ? objetoAlvo.position
                : objetoAlvo.TransformPoint(offsetPosicao);

            transform.position = posicaoDesejada;
        }

        // 2. COPIA ROTAÇÃO MUNDIAL
        if (seguirRotacao)
        {
            Quaternion rotacaoDesejada = objetoAlvo.rotation * Quaternion.Euler(offsetRotacao);
            transform.rotation = rotacaoDesejada;
        }
    }
}
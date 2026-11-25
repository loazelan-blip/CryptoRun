using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;       // Arraste o seu Player aqui
    public float distance = 5f;    // Distância exata atrás do carro
    public float height = 2f;      // Altura exata da câmera em relação ao carro
    public float rotationSmoothing = 5f; // Suavidade apenas para o giro da câmera

    [Header("CONTROLE MANUAL EXTRA (INSPECTOR)")]
    [Tooltip("Ajuste os eixos X, Y e Z para mover a posição da câmera manualmente em tempo real.")]
    public Vector3 posicaoExtra = Vector3.zero;

    [Tooltip("Ajuste os eixos X, Y e Z no Inspector para controlar a rotação adicional da câmera.")]
    public Vector3 rotacaoExtra = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        // 1. Pega o ângulo atual do carro e suaviza o GIRO para a câmera fazer a curva de forma fluida
        float targetAngle = target.eulerAngles.y;
        float currentAngle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, rotationSmoothing * Time.deltaTime);

        // 2. Transforma esse ângulo em uma rotação do Unity
        Quaternion currentRotation = Quaternion.Euler(0f, currentAngle, 0f);

        // 3. POSIÇÃO TRAVADA: Calcula a posição exata da traseira do carro multiplicando a rotação pelo vetor de trás
        Vector3 targetPosition = target.position + (currentRotation * Vector3.back * distance);

        // 4. Aplica a altura correta baseada na posição do carro
        targetPosition.y = target.position.y + height;

        // 5. APLICA A POSIÇÃO BASE + A POSIÇÃO EXTRA RELATIVA À CÂMERA
        transform.position = targetPosition + (currentRotation * posicaoExtra);

        // 6. Obriga a câmera a olhar diretamente para o centro do carro
        transform.LookAt(target);

        // 7. APLICA A ROTAÇÃO EXTRA DO INSPECTOR
        transform.Rotate(rotacaoExtra, Space.Self);
    }
}
using UnityEngine;

public class ArrowDrive01 : MonoBehaviour
{
    [Header("Configurações do Movimento")]
    public float floatSpeed = 3f;     // Velocidade do vai e vem
    public float floatHeight = 1f;    // Altura do deslocamento (1 metro para cima e 1 para baixo)
    public float rotationSpeed = 50f; // Velocidade de rotação da seta

    private Vector3 startPosition;

    void Start()
    {
        // Guarda a posição original onde você colocou a seta no mapa
        startPosition = transform.position;
    }

    void Update()
    {
        // O Seno varia entre -1 e 1. Multiplicado por floatHeight (1), varia exatamente 1 metro para cima e para baixo
        float newY = startPosition.y + (Mathf.Sin(Time.time * floatSpeed) * floatHeight);

        // Aplica a nova posição mantendo os eixos X e Z fixos
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);

        // Mantém a seta girando no próprio eixo Y
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
}
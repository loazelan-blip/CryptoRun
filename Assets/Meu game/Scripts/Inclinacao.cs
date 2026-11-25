using UnityEngine;

public class Inclinacao : MonoBehaviour
{
    // Referências aos Transforms do jogador e alvos possíveis
    public Transform player;
    public Transform target;
    public Transform target2;
    public Transform target3;
    public Transform target4;

    // Ângulo limite (em graus) para a inclinação no eixo X
    public float anguloMaximo = 30f;

    // Guarda a inclinação X real sem ler da Unity para eliminar o tremor
    private float anguloXAtual = 0f;

    void Update()
    {
        // Sistema de fallback
        if (target == null && target2 != null) target = target2;
        if (target == null && target3 != null) target = target3;
        if (target == null && target4 != null) target = target4;

        if (player == null || target == null) return;

        // 1. Calcula a direção do player até o alvo
        Vector3 direcaoAlvo = target.position - player.position;
        direcaoAlvo.y = 0;

        // 2. INVERSÃO DE DIREÇÃO: Invertemos o vetor do player (-player.forward)
        // Isso inverte o lado do ângulo com 100% de precisão!
        float dot = Vector3.Dot(direcaoAlvo.normalized, -player.forward.normalized);

        // 3. Mantém a estrutura matemática original que você já usava
        float inclinacao = Mathf.Lerp(anguloMaximo, -anguloMaximo, (dot + 1f) / 2f);

        // 4. Obtém os eixos Y e Z do objeto
        Vector3 euler = transform.eulerAngles;

        // 5. Interpola suavemente a variável interna (acaba com a tremedeira)
        anguloXAtual = Mathf.Lerp(anguloXAtual, inclinacao, Time.deltaTime * 5f);

        // 6. Aplica a rotação final
        transform.eulerAngles = new Vector3(anguloXAtual, euler.y, euler.z);
    }
}
using UnityEngine;
using UnityEngine.InputSystem;

public class RodaGiro : MonoBehaviour
{
    [Header("Controles (Input System)")]
    [Tooltip("Arraste aqui Carro/Esquerda (a mesma do CarAutomatico).")]
    public InputActionReference esquerda;
    [Tooltip("Arraste aqui Carro/Direita (a mesma do CarAutomatico).")]
    public InputActionReference direita;

    [Header("CONFIGURAÇÃO DE GIRO DA RODA")]
    [Tooltip("Ângulo máximo que a roda vira (em graus)")]
    public float anguloMaximo = 30f;

    [Tooltip("Velocidade da transição/suavidade da curva")]
    public float velocidadeSuavidade = 8f;

    private float anguloAtual = 0f;

    void Update()
    {
        // Lê as mesmas actions do CarAutomatico (seguem a troca de teclas do jogador).
        // Se os campos estiverem vazios, usa o sistema antigo.
        float entradaHorizontal = (direita != null && esquerda != null)
            ? direita.action.ReadValue<float>() - esquerda.action.ReadValue<float>()
            : Input.GetAxis("Horizontal");

        // Calcula o ângulo alvo baseado na entrada do jogador
        float anguloAlvo = entradaHorizontal * anguloMaximo;

        // Suaviza a transição do ângulo atual para o ângulo alvo
        anguloAtual = Mathf.Lerp(anguloAtual, anguloAlvo, Time.deltaTime * velocidadeSuavidade);

        // Aplica a rotação no eixo Y (Manter X e Z locais originais)
        transform.localRotation = Quaternion.Euler(0f, anguloAtual, 0f);
    }
}
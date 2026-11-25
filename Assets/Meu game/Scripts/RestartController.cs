using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class RestartController : MonoBehaviour
{
    [Header("Configurações da UI")]
    public GameObject painelGameOver;        // Arraste o seu PainelGameOver aqui

    [Header("Controles (Input System)")]
    public InputActionReference botaoRestart; // Arraste o Carro/Pause (o mesmo botão do pause)

    void Update()
    {
        if (painelGameOver == null || !painelGameOver.activeSelf) return;
        if (botaoRestart == null || botaoRestart.action == null) return;

        // Garante que o botão está ligado enquanto o Game Over está na tela
        if (!botaoRestart.action.enabled)
            botaoRestart.action.Enable();

        // Usa o botão que o jogador configurou no menu de controles
        if (botaoRestart.action.WasPressedThisFrame())
        {
            RestartGame();
        }
    }

    // Também pode ser chamada por um botão da tela (OnClick)
    public void RestartGame()
    {
        // Volta o tempo do jogo ao normal (descongela)
        Time.timeScale = 1f;

        // Recarrega a cena atual
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
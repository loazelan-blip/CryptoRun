using UnityEngine;

public class FecharJogo : MonoBehaviour
{
    // Esta função será chamada pelo botão "Close Game"
    public void Fechar()
    {
#if UNITY_EDITOR
        // Dentro do Unity (Editor), Application.Quit não faz nada. Por isso, aqui ele para o Play.
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // No jogo já compilado (build), fecha o jogo de verdade.
        Application.Quit();
#endif
    }
}
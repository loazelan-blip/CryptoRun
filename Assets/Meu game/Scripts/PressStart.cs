using UnityEngine;
using UnityEngine.UI; // Se for imagem ou Text normal
using TMPro; // Se for TextMeshPro

public class PressStart : MonoBehaviour
{
    [Range(0.1f, 2.0f)]
    public float velocidade = 0.5f;

    private float timer;
    private Behaviour visualComponent; // Variável genérica para o componente visual

    void Start()
    {
        // Tenta achar o componente de texto ou imagem
        visualComponent = GetComponent<TextMeshProUGUI>(); // Se usar TextMeshPro
        if (visualComponent == null) visualComponent = GetComponent<Image>(); // Se usar Imagem
    }

    void Update()
    {
        // Usamos unscaledDeltaTime para ignorar a pausa do jogo
        timer += Time.unscaledDeltaTime;

        if (timer >= velocidade)
        {
            // Liga/Desliga apenas o componente visual, não o objeto
            if (visualComponent != null)
            {
                visualComponent.enabled = !visualComponent.enabled;
            }

            timer = 0;
        }
    }
}
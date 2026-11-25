using UnityEngine;

public class MovimentoGeral : MonoBehaviour
{
    private Quaternion inclinacaoOriginalSeta;

    void Start()
    {
        // Salva a posição exata da seta que você configurou na Scene
        inclinacaoOriginalSeta = transform.localRotation;
    }

    void Update()
    {
        if (transform.parent == null) return;

        // Copia a rotação que o pai sofre e aplica por cima da inclinação da seta
        transform.localRotation = transform.parent.localRotation * inclinacaoOriginalSeta;
    }
}
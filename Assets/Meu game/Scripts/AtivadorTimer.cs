using UnityEngine;

public class AtivadorTimer : MonoBehaviour
{
    [Header("Configuração do Timer")]
    public ControladorTimer referenciaDoTimer; // Arraste o objeto do Temporizador aqui
    public float tempoParaEsteObjeto = 10f;    // Defina o tempo específico deste objeto

    void OnEnable()
    {
        // Dispara o reset no instante exato em que o Inspector desse objeto for marcado/ativado
        if (referenciaDoTimer != null)
        {
            referenciaDoTimer.ResetarTimer(tempoParaEsteObjeto);
        }
    }
}
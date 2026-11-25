using UnityEngine;
using UnityEngine.InputSystem;

public class ControleCamerasCarro : MonoBehaviour
{
    [Header("Câmeras do Veículo")]
    public GameObject cameraPrincipal;
    public GameObject cameraEsquerda;
    public GameObject cameraDireita;
    public GameObject cameraFrente;
    public GameObject cameraInterna;

    [Header("Controles (Input System)")]
    public InputActionReference acaoToggleInterna;
    public InputActionReference acaoEsquerda;
    public InputActionReference acaoDireita;
    public InputActionReference acaoFrente;

    [Header("Mapeamento de Teclas / Botões (usado se as ações acima estiverem vazias)")]
    [Tooltip("Tecla ou botão para alternar a visão interna/externa (Toggle).")]
    public KeyCode teclaToggleInterna = KeyCode.Keypad0;

    [Tooltip("Tecla ou botão para olhar à esquerda.")]
    public KeyCode teclaEsquerda = KeyCode.Keypad7;

    [Tooltip("Tecla ou botão para olhar à direita.")]
    public KeyCode teclaDireita = KeyCode.Keypad8;

    [Tooltip("Tecla ou botão para olhar para a frente.")]
    public KeyCode teclaFrente = KeyCode.Keypad9;

    private bool modoVisaoInterna = false;

    void Start()
    {
        // Garante que o jogo inicie com a câmera principal ligada
        AtivarSomenteCamera(cameraPrincipal);
    }

    bool Apertou(InputActionReference acao, KeyCode tecla)
    {
        return acao != null ? acao.action.WasPressedThisFrame() : Input.GetKeyDown(tecla);
    }

    bool Segurando(InputActionReference acao, KeyCode tecla)
    {
        return acao != null ? acao.action.IsPressed() : Input.GetKey(tecla);
    }

    void Update()
    {
        // --- TOGGLE: Alterna entre visão Interna e visão Externa ---
        if (Apertou(acaoToggleInterna, teclaToggleInterna))
        {
            modoVisaoInterna = !modoVisaoInterna;

            if (modoVisaoInterna)
            {
                AtivarSomenteCamera(cameraInterna);
            }
            else
            {
                AtivarSomenteCamera(cameraPrincipal);
            }
        }

        // Se estiver dentro do carro (Visão Interna), ignora os espiões das laterais/frente
        if (modoVisaoInterna) return;

        // --- Câmeras temporárias (só enquanto segura a tecla configurada) ---
        if (Segurando(acaoEsquerda, teclaEsquerda))
        {
            AtivarSomenteCamera(cameraEsquerda);
        }
        else if (Segurando(acaoDireita, teclaDireita))
        {
            AtivarSomenteCamera(cameraDireita);
        }
        else if (Segurando(acaoFrente, teclaFrente))
        {
            AtivarSomenteCamera(cameraFrente);
        }
        else
        {
            // Quando soltar qualquer botão, volta para a Câmera Principal
            AtivarSomenteCamera(cameraPrincipal);
        }
    }

    /// Desativa todas as câmeras e ativa somente a que for informada no parâmetro
    void AtivarSomenteCamera(GameObject cameraParaAtivar)
    {
        if (cameraPrincipal != null) cameraPrincipal.SetActive(cameraPrincipal == cameraParaAtivar);
        if (cameraEsquerda != null) cameraEsquerda.SetActive(cameraEsquerda == cameraParaAtivar);
        if (cameraDireita != null) cameraDireita.SetActive(cameraDireita == cameraParaAtivar);
        if (cameraFrente != null) cameraFrente.SetActive(cameraFrente == cameraParaAtivar);
        if (cameraInterna != null) cameraInterna.SetActive(cameraInterna == cameraParaAtivar);
    }
}
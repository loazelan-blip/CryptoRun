using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class MenuPause : MonoBehaviour
{
    [Header("Painéis do Menu")]
    public GameObject painelPause;
    public GameObject painelControles;
    public GameObject painelSom;

    [Header("Navegação (Teclado / Comando)")]
    public Button primeiroBotaoSelecionado;     // primeiro botão do menu pause
    public Button botaoControle;                // botão Controle do PainelPause
    public Button botaoVoltarControles;         // BackControles
    public Button botaoSom;                     // botão sound do PainelPause
    public Selectable primeiroSelecionavelSom;  // SliderMusica

    [Header("Controles (Input System)")]
    public InputActionReference pause;          // Carro/Pause
    public InputActionReference confirmar;      // Carro/Acelerar (SPEED)
    public InputActionReference voltar;         // Action do BRAKE

    private bool jogoPausado = false;

    private void Start()
    {
        if (painelPause != null) painelPause.SetActive(false);
        if (painelControles != null) painelControles.SetActive(false);
        if (painelSom != null) painelSom.SetActive(false);
        Time.timeScale = 1f;

        // SPEED passa a ser o "confirmar" dos menus
        var modulo = EventSystem.current != null ? EventSystem.current.GetComponent<InputSystemUIInputModule>() : null;
        if (modulo != null && confirmar != null) modulo.submit = confirmar;
    }

    private void Update()
    {
        // Com o painel de controles aberto, não abre/fecha o pause
        // (evita fechar o menu ao trocar a tecla ou ao cancelar com Esc)
        bool emControles = painelControles != null && painelControles.activeSelf;

        bool apertouPause = Input.GetKeyDown(KeyCode.Escape)
            || (pause != null ? pause.action.WasPressedThisFrame() : Input.GetKeyDown(KeyCode.P));

        if (apertouPause && !emControles)
        {
            if (jogoPausado) Continuar();
            else Pausar();
        }

        // BRAKE = voltar
        if (jogoPausado && voltar != null && voltar.action.WasPressedThisFrame() && !RebindButton.Ocupado)
        {
            if (painelControles != null && painelControles.activeSelf) FecharControles();
            else if (painelSom != null && painelSom.activeSelf) FecharSom();
            else Continuar();
        }

        // Se pausado e a seleção sumiu (null ou objeto inativo), seleciona de novo
        if (jogoPausado)
        {
            var sel = EventSystem.current.currentSelectedGameObject;
            if (sel == null || !sel.activeInHierarchy)
                SelecionarDoPainelAtivo();
        }
    }

    void SelecionarDoPainelAtivo()
    {
        Selectable alvo;

        if (painelControles != null && painelControles.activeSelf)
            alvo = botaoVoltarControles;
        else if (painelSom != null && painelSom.activeSelf)
            alvo = primeiroSelecionavelSom;
        else
            alvo = primeiroBotaoSelecionado;

        Selecionar(alvo);
    }

    void Selecionar(Selectable s)
    {
        if (s == null) return;
        EventSystem.current.SetSelectedGameObject(null);
        s.Select();
    }

    public void Pausar()
    {
        if (painelPause != null) painelPause.SetActive(true);
        Time.timeScale = 0f;
        jogoPausado = true;
        Selecionar(primeiroBotaoSelecionado);
    }

    public void Continuar()
    {
        if (painelPause != null) painelPause.SetActive(false);
        if (painelControles != null) painelControles.SetActive(false);
        if (painelSom != null) painelSom.SetActive(false);
        Time.timeScale = 1f;
        jogoPausado = false;
    }

    public void AbrirControles()
    {
        if (painelPause != null) painelPause.SetActive(false);
        if (painelControles != null) painelControles.SetActive(true);
        Selecionar(botaoVoltarControles);
    }

    public void FecharControles()
    {
        if (painelControles != null) painelControles.SetActive(false);
        if (painelPause != null) painelPause.SetActive(true);
        Selecionar(botaoControle);
    }

    public void AbrirSom()
    {
        if (painelPause != null) painelPause.SetActive(false);
        if (painelSom != null) painelSom.SetActive(true);
        Selecionar(primeiroSelecionavelSom);
    }

    public void FecharSom()
    {
        if (painelSom != null) painelSom.SetActive(false);
        if (painelPause != null) painelPause.SetActive(true);
        Selecionar(botaoSom);
    }

    public void SairDoJogo()
    {
        Debug.Log("[Menu Pause] Saindo do jogo...");
        Application.Quit();
    }
}
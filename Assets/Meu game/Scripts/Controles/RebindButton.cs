
using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class RebindButton : MonoBehaviour
{
    public InputActionReference acao;
    public TMP_Text label;
    public string titulo;

    [Header("Posição do texto no Joystick")]
    [Tooltip("Sobe (positivo) ou desce (negativo) o texto só quando está no modo joystick. 0 = não muda.")]
    public float deslocamentoYJoystick = 0f;

    // true enquanto alguma tecla está sendo trocada (e por 0.3s depois que termina)
    public static bool Ocupado => rebindAtivo || Time.unscaledTime - fimRebind < 0.3f;
    static bool rebindAtivo = false;
    static float fimRebind = -10f;
    bool meuRebind = false;

    InputActionRebindingExtensions.RebindingOperation op;
    Vector2 posicaoOriginal;
    bool guardouPosicao = false;

    void Awake() => GuardarPosicao();

    void OnEnable() => Atualizar();

    void OnDisable()
    {
        op?.Dispose();
        op = null;
        if (meuRebind)
        {
            meuRebind = false;
            rebindAtivo = false;
        }
    }

    // Aplica a posição o tempo todo, para você ver o ajuste do Inspector com o jogo rodando
    void Update() => AplicarPosicao();

    void GuardarPosicao()
    {
        if (guardouPosicao || label == null) return;
        posicaoOriginal = label.rectTransform.anchoredPosition;
        guardouPosicao = true;
    }

    void AplicarPosicao()
    {
        if (label == null) return;
        GuardarPosicao();

        // Ajusta a altura do texto só no joystick; no teclado volta ao original
        label.rectTransform.anchoredPosition = ControlesLoader.Joystick
            ? posicaoOriginal + new Vector2(0f, deslocamentoYJoystick)
            : posicaoOriginal;
    }

    int Indice()
    {
        var bs = acao.action.bindings;
        for (int i = 0; i < bs.Count; i++)
        {
            if (bs[i].isComposite || bs[i].isPartOfComposite) continue;
            bool teclado = bs[i].path.StartsWith("<Keyboard>");
            if (teclado != ControlesLoader.Joystick) return i;
        }
        return -1;
    }

    public void Atualizar()
    {
        int i = Indice();
        string tecla = i < 0 ? "-" : acao.action.GetBindingDisplayString(i);

        // No joystick, tira a palavra "Button" do texto
        if (i >= 0 && ControlesLoader.Joystick)
            tecla = Regex.Replace(tecla, "button", "", RegexOptions.IgnoreCase).Trim();

        label.text = titulo + "\n" + tecla;

        AplicarPosicao();
    }

    public void Iniciar()
    {
        if (op == null && !Ocupado) StartCoroutine(Rotina());
    }

    IEnumerator Rotina()
    {
        int i = Indice();
        if (i < 0) yield break;

        rebindAtivo = true;
        meuRebind = true;

        label.text = titulo + "\n...";
        yield return new WaitForSecondsRealtime(0.2f);

        var a = acao.action;
        a.Disable();

        var o = a.PerformInteractiveRebinding(i)
            .WithCancelingThrough("<Keyboard>/escape")
            .WithTimeout(5f)
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(x => Terminar(a))
            .OnCancel(x => Terminar(a));

        if (ControlesLoader.Joystick)
            o.WithControlsExcluding("<Keyboard>").WithControlsExcluding("<Mouse>");
        else
            o.WithControlsHavingToMatchPath("<Keyboard>");

        op = o.Start();
    }

    void Terminar(InputAction a)
    {
        op?.Dispose();
        op = null;
        a.Enable();
        ControlesLoader.Salvar(a.actionMap.asset);
        Atualizar();

        rebindAtivo = false;
        meuRebind = false;
        fimRebind = Time.unscaledTime;
    }
}
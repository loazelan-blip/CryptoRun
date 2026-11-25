using TMPro;
using UnityEngine;

public class AlternarDispositivo : MonoBehaviour
{
    public TMP_Text label;

    void OnEnable() => AtualizarTudo();

    public void Alternar()
    {
        ControlesLoader.Joystick = !ControlesLoader.Joystick;
        AtualizarTudo();
    }

    void AtualizarTudo()
    {
        if (label != null)
            label.text = ControlesLoader.Joystick ? "JOYSTICK" : "keyboard";

        var botoes = FindObjectsByType<RebindButton>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var b in botoes) b.Atualizar();
    }
}
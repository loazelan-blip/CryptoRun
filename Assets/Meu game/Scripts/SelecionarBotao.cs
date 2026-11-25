using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelecionarBotao : MonoBehaviour
{
    public GameObject alvo;

    public void Selecionar()
    {
        StartCoroutine(Rotina());
    }

    IEnumerator Rotina()
    {
        yield return null; // espera o painel ativar
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(alvo);
    }
}
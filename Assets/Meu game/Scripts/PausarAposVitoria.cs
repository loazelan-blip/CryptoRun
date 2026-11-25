using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PausarAposVitoria : MonoBehaviour
{
    [Header("Tempo de espera antes de voltar ao menu (segundos)")]
    public float tempoDeEspera = 3f;

    [Header("Cena para onde voltar")]
    [Tooltip("Nome exato da cena da tela inicial. A cena precisa estar na lista de cenas em Build Profiles.")]
    public string nomeDaCena = "Menu Inicial";

    private bool jaIniciouContagem = false;

    // O Unity chama esta função automaticamente toda vez que o objeto é ativado (SetActive(true))
    private void OnEnable()
    {
        if (!jaIniciouContagem)
        {
            jaIniciouContagem = true;
            StartCoroutine(EsperarEVoltarAoMenu());
        }
    }

    private IEnumerator EsperarEVoltarAoMenu()
    {
        // Espera o tempo configurado, contando em tempo real
        yield return new WaitForSecondsRealtime(tempoDeEspera);

        Debug.Log("[Vitória] Voltando para a cena: " + nomeDaCena);

        // Carrega a tela inicial
        SceneManager.LoadScene(nomeDaCena);
    }
}
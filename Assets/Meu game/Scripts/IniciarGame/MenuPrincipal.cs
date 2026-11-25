using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{
    public GameObject painelOptions;

    public void IniciarGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void AbrirOptions()
    {
        painelOptions.SetActive(true);
    }

    public void FecharOptions()
    {
        painelOptions.SetActive(false);
    }
}
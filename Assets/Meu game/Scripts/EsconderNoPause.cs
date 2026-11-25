using UnityEngine;

public class EsconderNoPause : MonoBehaviour
{
    public GameObject[] objetos;

    bool desligado = false;

    void Update()
    {
        if (desligado) return;

        bool pausado = Time.timeScale == 0f;

        foreach (GameObject obj in objetos)
        {
            if (obj != null && obj.activeSelf == pausado)
                obj.SetActive(!pausado);
        }
    }

    public void Desligar()
    {
        desligado = true;

        foreach (GameObject obj in objetos)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }
}
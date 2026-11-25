using UnityEngine;

public class CampainhaSeta : MonoBehaviour
{
    [Header("Objeto 3D para Controlar")]
    public GameObject objeto3D;

    void Update()
    {
        if (objeto3D == null) return;

        // Enquanto estiver segurando a tecla 9 do teclado numérico (lado direito), DESLIGA o objeto
        if (Input.GetKeyDown(KeyCode.Keypad9))
        {
            objeto3D.SetActive(false);
        }

        // Quando soltar a tecla 9 do teclado numérico, LIGA o objeto novamente
        if (Input.GetKeyUp(KeyCode.Keypad9))
        {
            objeto3D.SetActive(true);
        }
    }
}
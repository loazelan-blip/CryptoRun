using UnityEngine;

public class Trigger01Home01 : MonoBehaviour
{
    //public CompassArrow3D seta;
    public Transform proximoAlvo;
    public GameObject arrowToDestroy;
    public GameObject PeterMoney;
    public GameObject TerceiraSeta; // a seta da casa do cliente
    public GameObject TiggerDrive02;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Some com a seta flutuante da lanchonete
            if (arrowToDestroy != null)
            {
                Destroy(arrowToDestroy);
            }

            // Mostra o ícone do hambúrguer na tela
            if (PeterMoney != null)
            {
                PeterMoney.SetActive(true);
                TiggerDrive02.SetActive(true);
            }

            // Ativa a seta da casa do cliente
            if (TerceiraSeta != null)
            {
                TerceiraSeta.SetActive(true);
            }

            Destroy(gameObject);

            Debug.Log("Coletado com sucesso através do objeto vazio!");
        }
    }
}
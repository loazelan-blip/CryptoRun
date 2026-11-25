using UnityEngine;

public class TriggerDrive02 : MonoBehaviour
{
   // public CompassArrow3D seta;
    public Transform proximoAlvo;
    public GameObject arrowToDestroy;
    public GameObject iconeHamburguer;
    public GameObject TerceiraSeta;
    public GameObject TiggerSucker02;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Some com a seta flutuante da lanchonete
            if (arrowToDestroy != null)
            {
                Destroy(arrowToDestroy);
                Debug.Log("destruido");
            }

            // Mostra o ícone do hambúrguer na tela
            if (iconeHamburguer != null)
            {
                iconeHamburguer.SetActive(true);
                Debug.Log("ativo");
                TiggerSucker02.SetActive(true);
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
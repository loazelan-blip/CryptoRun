using UnityEngine;

public class DriveTigger1 : MonoBehaviour
{
   // public CompassArrow3D seta;
    public Transform proximoAlvo;
    public GameObject arrowToDestroy;
    public GameObject iconeHamburguer;
    public GameObject segundaSeta; // a seta da casa do cliente
    public GameObject TiggerSucker01;

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
            if (iconeHamburguer != null)
            {
                iconeHamburguer.SetActive(true);
                TiggerSucker01.SetActive(true);
            }

            // Ativa a seta da casa do cliente
            if (segundaSeta != null)
            {
                segundaSeta.SetActive(true);
            }

            Destroy(gameObject);

           
        }
    }
}
using UnityEngine;

public class TriggerSucker02 : MonoBehaviour
{
    //public CompassArrow3D seta;
    
    public GameObject arrowToDestroy;
    public GameObject PeterMoney;
    public GameObject PointerArrow;
    public GameObject DestroyFundoTimer;


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

            // Mostra Agrana na Tela
            if (PeterMoney != null)
            {
                PeterMoney.SetActive(true);
                Debug.Log("ativo");
                Destroy(PointerArrow);
                Destroy(DestroyFundoTimer);
                
            }

            
            

            Destroy(gameObject);

            Debug.Log("Coletado com sucesso através do objeto vazio!");
        }
    }
}
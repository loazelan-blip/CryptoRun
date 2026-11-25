using UnityEngine;

public class FireCollision : MonoBehaviour
{
    [Header("Configurações do Efeito")]
    public GameObject prefabParticula;

    [Tooltip("Força mínima do impacto para disparar a faísca (Ex: 0.5 para leve, 5.0 para forte)")]
    public float forcaMinimaColisao = 0.5f;

    [Tooltip("Tempo em segundos antes de deletar a partícula criada")]
    public float tempoVidaParticula = 1.0f;

    void OnCollisionEnter(Collision collision)
    {
        // Ignora o chão se ele tiver a tag Ground
        if (collision.gameObject.CompareTag("Ground")) return;

        // Compara a força real do impacto com o valor público que você definiu
        if (collision.relativeVelocity.magnitude >= forcaMinimaColisao)
        {
            Vector3 pontoDeImpacto = collision.contacts[1].point;
            GameObject novaParticula = Instantiate(prefabParticula, pontoDeImpacto, Quaternion.identity);

            Destroy(novaParticula, tempoVidaParticula);
        }
    }
}
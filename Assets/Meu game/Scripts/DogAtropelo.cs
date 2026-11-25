using UnityEngine;
using UnityEngine.AI;

public class DogAtropelo : MonoBehaviour
{
    [Header("Configurações do Atropelamento")]
    public string tagDoCarro = "Player";
    public float forcaDoImpacto = 12f;

    [Header("Script de Movimentação do Personagem")]
    public MonoBehaviour scriptDeMovimento;

    private Animator animator;
    private Collider colisorPai;
    private Rigidbody rbPai;
    private NavMeshAgent agent;
    private bool jaAtropelado = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        colisorPai = GetComponent<Collider>();
        rbPai = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();

        // No início, deixa os ossos como Kinematic para o cachorro andar/animar normal
        ConfigurarKinematicOssos(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!jaAtropelado && other.CompareTag(tagDoCarro))
        {
            Atropelar(other.transform.forward);
        }
    }

    public void Atropelar(Vector3 direcaoDoCarro)
    {
        jaAtropelado = true;

        // 1. Desliga os scripts de andar/navegação
        if (scriptDeMovimento != null) scriptDeMovimento.enabled = false;
        if (agent != null) agent.enabled = false;

        // 2. Desliga o Animator para que a pose da animação solte os ossos
        if (animator != null) animator.enabled = false;

        // 3. DESLIGA O RIGIDBODY E COLLIDER DO PAI (Crucial!)
        // Isso impede a cápsula de colidir com os próprios ossos e fazer o cachorro voar ou afundar.
        if (colisorPai != null) colisorPai.enabled = false;
        if (rbPai != null) rbPai.isKinematic = true;

        // 4. Ativa a física real nos ossos (Ragdoll)
        ConfigurarKinematicOssos(false);

        // 5. Aplica a força de impacto nos ossos
        Rigidbody[] ossos = GetComponentsInChildren<Rigidbody>();
        Vector3 vetorForca = (direcaoDoCarro + Vector3.up * 0.2f).normalized * forcaDoImpacto;

        foreach (Rigidbody rb in ossos)
        {
            // Ignora o próprio pai para não aplicar força dupla
            if (rb.gameObject != this.gameObject)
            {
                rb.AddForce(vetorForca, ForceMode.Impulse);
            }
        }
    }

    private void ConfigurarKinematicOssos(bool estadoKinematic)
    {
        Rigidbody[] ossos = GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rb in ossos)
        {
            if (rb.gameObject != this.gameObject)
            {
                rb.isKinematic = estadoKinematic;
            }
        }
    }
}
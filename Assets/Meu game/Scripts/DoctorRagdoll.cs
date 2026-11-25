using UnityEngine;

public class DoctorRagdoll : MonoBehaviour
{
    private Rigidbody[] ossosRagdoll;
    private Collider[] colisoresRagdoll;
    private Animator animator;

    void Awake()
    {
        ossosRagdoll = GetComponentsInChildren<Rigidbody>();
        colisoresRagdoll = GetComponentsInChildren<Collider>();
        animator = GetComponent<Animator>();

        DesativarRagdoll();
    }

    public void DesativarRagdoll()
    {
        foreach (var rb in ossosRagdoll)
        {
            rb.isKinematic = true;
        }
    }

    public void AtivarRagdollEArremessar(Vector3 direcaoImpacto, float forcaImpacto)
    {
        if (animator != null)
        {
            animator.enabled = false;
        }

        foreach (var rb in ossosRagdoll)
        {
            rb.isKinematic = false;

            Vector3 vetorForca = (direcaoImpacto + Vector3.up * 0.5f).normalized * forcaImpacto;
            rb.AddForce(vetorForca, ForceMode.Impulse);
        }
    }
}
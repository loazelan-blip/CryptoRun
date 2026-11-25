using UnityEngine;

public class CarImpactDoctor : MonoBehaviour
{
    [Header("Configuração de Impacto")]
    [Tooltip("Velocidade mínima do impacto necessária para atropelar o médico. Abaixo disso, ele não reage.")]
    public float limiarMinimoImpacto = 5f;

    [Tooltip("Multiplicador da força de arremesso proporcional à velocidade da colisão.")]
    public float multiplicadorDeForca = 25f;

    [Tooltip("Impulso vertical adicionado para fazer o corpo voar em parábola.")]
    public float impulsoVertical = 0.4f;

    private Rigidbody rbCarro;

    void Start()
    {
        rbCarro = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verifica se colidiu com o médico
        if (other.CompareTag("Player") || other.name.Contains("Doctor") || other.transform.root.name.Contains("Doctor"))
        {
            // Busca o Rigidbody do médico (se houver) para calcular a velocidade relativa de colisão
            Rigidbody rbDoctor = other.GetComponentInParent<Rigidbody>();
            if (rbDoctor == null) rbDoctor = other.GetComponent<Rigidbody>();

            // Calcula a velocidade do impacto
            Vector3 velCarro = rbCarro != null ? rbCarro.linearVelocity : Vector3.zero;
            Vector3 velDoctor = rbDoctor != null ? rbDoctor.linearVelocity : Vector3.zero;

            // Velocidade relativa entre o carro e o médico
            float velocidadeImpacto = (velCarro - velDoctor).magnitude;

            // Busca os componentes do médico
            DoctorController controller = other.GetComponentInParent<DoctorController>();
            DoctorRagdoll ragdoll = other.GetComponentInParent<DoctorRagdoll>();

            if (controller == null) controller = other.GetComponent<DoctorController>();
            if (ragdoll == null) ragdoll = other.GetComponent<DoctorRagdoll>();

            // 2. Para a patrulha/IA do médico
            if (controller != null)
            {
                controller.Morrer();
            }

            // 3. Direção em que o médico voará
            Vector3 direcaoImpacto = (other.transform.position - transform.position).normalized;
            direcaoImpacto.y = 0; // Zera para aplicar a subida de forma controlada

            // Adiciona elevação vertical ao vetor de impulso
            Vector3 vetorArremesso = (direcaoImpacto + Vector3.up * impulsoVertical).normalized;

            // 4. Calcula a força total do arremesso proporcional à velocidade da batida
            //    Impacto fraco (abaixo do limiar): ele só desaba, sem ser arremessado
            float forcaTotal = 0f;
            if (velocidadeImpacto >= limiarMinimoImpacto)
            {
                forcaTotal = velocidadeImpacto * multiplicadorDeForca;
            }

            // 5. Ativa o Ragdoll e aplica o impacto proporcional
            if (ragdoll != null)
            {
                ragdoll.AtivarRagdollEArremessar(vetorArremesso, forcaTotal);
            }
        }
    }
}
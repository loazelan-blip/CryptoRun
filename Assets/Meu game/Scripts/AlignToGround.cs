using UnityEngine;

namespace ithappy.Animals_FREE
{
    public class AlignToGround : MonoBehaviour
    {
        [Header("Configurações de Ajuste de Altura")]
        [Tooltip("Velocidade com que o personagem gruda no chão ao subir ou descer.")]
        [SerializeField] private float snapSpeed = 10f;

        private void Update()
        {
            AjustarAlturaAutomaticamente();
        }

        private void AjustarAlturaAutomaticamente()
        {
            // Origem do raio (um pouco acima do centro do personagem)
            Vector3 rayOrigin = transform.position + Vector3.up * 1.5f;

            // Dispara o raio para baixo. 
            // Usamos RaycastAll ou verificamos o hit para ignorar o collider do próprio personagem se necessário.
            RaycastHit[] hits = Physics.RaycastAll(rayOrigin, Vector3.down, 5f);

            float highestFloorY = -Mathf.Infinity;
            bool foundGround = false;

            foreach (var hit in hits)
            {
                // Ignora o próprio personagem e seus filhos para não travar nele mesmo
                if (hit.collider.transform.IsChildOf(transform)) continue;

                // Pega o ponto mais alto de colisão abaixo dele (o chão real ou a calçada)
                if (hit.point.y > highestFloorY)
                {
                    highestFloorY = hit.point.y;
                    foundGround = true;
                }
            }

            if (foundGround)
            {
                // Posição atual
                Vector3 targetPosition = transform.position;

                // Interpola suavemente a altura Y atual para a altura exata do chão detectado
                targetPosition.y = Mathf.Lerp(targetPosition.y, highestFloorY, snapSpeed * Time.deltaTime);

                transform.position = targetPosition;
            }
        }
    }
}
using UnityEngine;

// Força este script a executar DEPOIS da câmera e do player (evita a trepidação de frame desfasado)
[DefaultExecutionOrder(100)]
public class CompassArrow3D : MonoBehaviour
{
    public Transform player;
    public Transform target;
    public Transform target2;
    public Transform target3;
    public Transform target4;
    public float radius = 2f;
    public float tiltAngle = 45f;
    public float tiltAngle2 = 10f;
    public float fixoX = 0f;
    public float fixoY = 0f;
    public float fixoZ = 2f;
    public Transform cameraPrincipal;
    public float suavidade = 20f;

    void LateUpdate()
    {
        if (target == null && target2 != null) target = target2;
        if (target == null && target3 != null) target = target3;
        if (target == null && target4 != null) target = target4;

        if (target == null || player == null) return;

        // POSICIONAMENTO: Atualiza a posição sincronizada com a câmera finalizada
        if (cameraPrincipal != null)
        {
            Vector3 posicaoAlvo = cameraPrincipal.position +
                                  cameraPrincipal.right * fixoX +
                                  cameraPrincipal.up * fixoY +
                                  cameraPrincipal.forward * fixoZ;

            transform.position = posicaoAlvo;
        }

        // SUA ROTAÇÃO ORIGINAL (Mantida 100% igual)
        Vector3 lookDir = player.position - target.position;
        lookDir.y = 0;

        if (lookDir.sqrMagnitude > 0.001f)
        {
            float anguloY = Quaternion.LookRotation(lookDir).eulerAngles.y;
            float anguloX = transform.eulerAngles.x;

            Quaternion rotacaoAlvo = Quaternion.Euler(anguloX, anguloY, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation,
                                                 rotacaoAlvo,
                                                 suavidade * Time.deltaTime);
        }
    }

    public void SetTarget(Transform novoAlvo)
    {
        target = novoAlvo;
    }
}
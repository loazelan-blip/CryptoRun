using UnityEngine;

public class PivoSegueCorpo : MonoBehaviour
{
    [Tooltip("O corpo do carro (o filho que tem o Rigidbody). Se ficar vazio, procura no Start dentro do carro.")]
    public Transform corpo;

    Vector3 posLocalOriginal;
    Quaternion rotLocalOriginal;

    void Start()
    {
        if (corpo == null)
        {
            Rigidbody rb = GetComponentInChildren<Rigidbody>();
            if (rb != null) corpo = rb.transform;
        }

        if (corpo == null || corpo == transform) { enabled = false; return; }

        posLocalOriginal = corpo.localPosition;
        rotLocalOriginal = corpo.localRotation;
    }

    void LateUpdate()
    {
        // Pose (posição e rotação) em que o corpo está agora no mundo
        Vector3 posCorpo = corpo.position;
        Quaternion rotCorpo = corpo.rotation;

        // Leva o pai para onde ele precisa estar para o corpo ficar na posição original dentro dele
        transform.rotation = rotCorpo * Quaternion.Inverse(rotLocalOriginal);
        transform.position = posCorpo - transform.rotation * posLocalOriginal;

        // O corpo volta a ficar na mesma posição local de antes (no mundo ele não se mexe)
        corpo.localPosition = posLocalOriginal;
        corpo.localRotation = rotLocalOriginal;
    }
}
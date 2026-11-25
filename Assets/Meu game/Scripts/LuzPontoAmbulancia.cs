using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class LuzPontoAmbulancia : MonoBehaviour
{
    [Header("Configurações do Pisca")]
    [Tooltip("Tempo em segundos que a luz fica acesa em cada flash.")]
    public float tempoAcesa = 0.08f;

    [Tooltip("Tempo de pausa entre os dois flashes rápidos.")]
    public float intervaloCurto = 0.08f;

    [Tooltip("Tempo de espera entre cada ciclo completo.")]
    public float intervaloLongo = 0.4f;

    [Tooltip("Se marcado, a luz começa a piscar assim que o jogo inicia.")]
    public bool ligarNoInicio = true;

    private Light luz;
    private Coroutine corrotinaLuz;
    private bool estaAtiva = false;

    void Awake()
    {
        // Pega o componente Light anexado a este mesmo GameObject
        luz = GetComponent<Light>();
    }

    void Start()
    {
        if (ligarNoInicio)
        {
            LigarLuz();
        }
        else
        {
            DesligarLuz();
        }
    }

    public void LigarLuz()
    {
        if (!estaAtiva)
        {
            estaAtiva = true;
            corrotinaLuz = StartCoroutine(RotinaPisca());
        }
    }

    public void DesligarLuz()
    {
        estaAtiva = false;

        if (corrotinaLuz != null)
        {
            StopCoroutine(corrotinaLuz);
        }

        if (luz != null)
        {
            luz.enabled = false;
        }
    }

    IEnumerator RotinaPisca()
    {
        while (estaAtiva)
        {
            // Primeiro Flash
            if (luz != null) luz.enabled = true;
            yield return new WaitForSeconds(tempoAcesa);
            if (luz != null) luz.enabled = false;
            yield return new WaitForSeconds(intervaloCurto);

            // Segundo Flash (Duplo Pisca)
            if (luz != null) luz.enabled = true;
            yield return new WaitForSeconds(tempoAcesa);
            if (luz != null) luz.enabled = false;

            // Pausa entre os ciclos
            yield return new WaitForSeconds(intervaloLongo);
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

public class BuzinaColisao : MonoBehaviour
{
    [Header("Buzina")]
    [Tooltip("Som da buzina")]
    public AudioClip buzina;

    [Range(0f, 1f)]
    public float volume = 1f;

    [Tooltip("Tempo mínimo (em segundos) entre uma buzinada e outra, para o som não empilhar se houver várias colisões seguidas.")]
    public float intervaloEntreBuzinas = 1.5f;

    [Header("Tags que acionam a buzina")]
    public string tagPlayer = "Player";
    public string tagCar = "Car";

    private AudioSource audioSource;
    private float ultimoTempoBuzina = -999f;

    // Rigidbodies deste carro (para ignorar colisões entre as próprias partes dele)
    private readonly HashSet<Rigidbody> meusRigidbodies = new HashSet<Rigidbody>();

    private void Awake()
    {
        // Cria um AudioSource próprio para a buzina (som 3D: se ouve melhor perto do carro)
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 1f;
    }

    private void Start()
    {
        // Liga a buzina ao grupo Carro do Mixer (para o volume das opções valer para ela)
        if (GerenciadorAudio.GrupoCarro != null)
            audioSource.outputAudioMixerGroup = GerenciadorAudio.GrupoCarro;

        BuscarRigidbodies();
    }

    // Procura os Rigidbody neste objeto, nos filhos e no pai.
    // A colisão só chega ao objeto que tem o Rigidbody, então liga um ajudante em cada um
    // que não seja este objeto, para repassar as colisões para cá.
    private void BuscarRigidbodies()
    {
        Rigidbody[] encontrados = GetComponentsInChildren<Rigidbody>(true);

        for (int i = 0; i < encontrados.Length; i++)
            meusRigidbodies.Add(encontrados[i]);

        Rigidbody doPai = GetComponentInParent<Rigidbody>();
        if (doPai != null) meusRigidbodies.Add(doPai);

        foreach (Rigidbody rb in meusRigidbodies)
        {
            // Se o Rigidbody é deste mesmo objeto, o OnCollisionEnter daqui já recebe a colisão
            if (rb.gameObject == gameObject) continue;

            BuzinaColisaoRepasse repasse = rb.GetComponent<BuzinaColisaoRepasse>();
            if (repasse == null) repasse = rb.gameObject.AddComponent<BuzinaColisaoRepasse>();

            repasse.dono = this;
        }

        if (meusRigidbodies.Count == 0)
        {
            Debug.LogWarning("[Buzina] Nenhum Rigidbody encontrado em " + name + ". A buzina não vai receber colisões.");
        }
    }

    // Recebe a colisão quando o Rigidbody está neste mesmo objeto
    private void OnCollisionEnter(Collision colisao)
    {
        ProcessarColisao(colisao);
    }

    // Chamado também pelo ajudante, quando o Rigidbody está em outro objeto (pai ou filho)
    public void ProcessarColisao(Collision colisao)
    {
        // Ignora colisão entre as próprias partes deste carro
        if (colisao.rigidbody != null && meusRigidbodies.Contains(colisao.rigidbody)) return;
        if (colisao.collider.transform.IsChildOf(transform)) return;

        if (!EhPlayerOuCar(colisao)) return;
        if (buzina == null) return;
        if (Time.time - ultimoTempoBuzina < intervaloEntreBuzinas) return;

        ultimoTempoBuzina = Time.time;
        audioSource.PlayOneShot(buzina, volume);
    }

    // Confere a tag no colisor, no objeto com o Rigidbody e no objeto raiz
    // (a tag pode estar no pai, mesmo que o colisor esteja em um filho)
    private bool EhPlayerOuCar(Collision colisao)
    {
        return TemTag(colisao.collider.gameObject)
            || TemTag(colisao.gameObject)
            || TemTag(colisao.collider.transform.root.gameObject);
    }

    private bool TemTag(GameObject objeto)
    {
        return objeto.CompareTag(tagPlayer) || objeto.CompareTag(tagCar);
    }
}

// Ajudante criado automaticamente pelo BuzinaColisao no objeto que tem o Rigidbody.
// Ele só repassa a colisão para o script da buzina.
public class BuzinaColisaoRepasse : MonoBehaviour
{
    [HideInInspector] public BuzinaColisao dono;

    private void OnCollisionEnter(Collision colisao)
    {
        if (dono != null) dono.ProcessarColisao(colisao);
    }
}
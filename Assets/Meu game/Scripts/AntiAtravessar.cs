using UnityEngine;

public class AntiAtravessar : MonoBehaviour
{
    [Header("Referências (se vazio, procura no próprio carro)")]
    public CarAutomatico carro;
    public BoxCollider caixa;
    [Tooltip("Sensor da mola. Quando o carro é segurado de frente e rápido, este sensor faz o recuo.")]
    public SensorMolaFrontal sensorMola;

    [Header("Recuo ao bater em qualquer parede")]
    [Tooltip("Velocidade mínima do recuo quando o carro bate (rápido ou devagar). Aumente para recuar mais, diminua para recuar menos.")]
    public float recuoMinimo = 3f;

    [Header("Ajustes")]
    [Tooltip("A checagem só vê obstáculos acima dessa altura (em metros, a partir da base do collider). Calçadas mais baixas que isso continuam passando.")]
    public float alturaMinimaParede = 0.4f;
    public LayerMask camadasObstaculos = ~0;

    [Header("NPCs")]
    [Tooltip("Marcado = objetos que têm o script DoctorController (os NPCs a pé) NÃO seguram o carro. Eles reagem pela física normal.")]
    public bool ignorarNPCs = true;

    [Header("Diagnóstico (Debug Log)")]
    public bool mostrarLogs = false;
    [Tooltip("Mostra logs também quando o carro anda para frente. Desmarcado = só ré.")]
    public bool logarFrente = false;
    [Tooltip("Tempo mínimo entre logs de 'nada detectado', para não encher o Console.")]
    public float intervaloLogSemDeteccao = 0.5f;

    [Header("Monitoramento (só leitura)")]
    public bool bloqueouNesteFrame;
    public string ultimoObjetoBloqueado;

    Vector3 posicaoAnterior;
    float tempoUltimoLogSemDeteccao = -999f;

    void Awake()
    {
        if (carro == null) carro = GetComponent<CarAutomatico>();
        if (caixa == null) caixa = GetComponent<BoxCollider>();
        if (sensorMola == null) sensorMola = GetComponent<SensorMolaFrontal>();
    }

    void Start()
    {
        posicaoAnterior = transform.position;

        if (mostrarLogs)
        {
            Debug.Log("[AntiAtravessar] Start | carro=" + (carro != null) + " | caixa=" + (caixa != null) + " | sensorMola=" + (sensorMola != null) + " | camadas=" + camadasObstaculos.value);
            if (caixa != null)
            {
                Debug.Log("[AntiAtravessar] Box Collider size=" + caixa.size + " center=" + caixa.center + " escala=" + transform.lossyScale + " isTrigger=" + caixa.isTrigger);
            }
        }
    }

    // Roda depois de todos os Update, ou seja, depois do CarAutomatico mover o carro
    void LateUpdate()
    {
        bloqueouNesteFrame = false;

        Vector3 delta = transform.position - posicaoAnterior;
        float passo = Vector3.Dot(delta, transform.forward);

        if (caixa == null)
        {
            if (mostrarLogs && Mathf.Abs(passo) > 0.0001f)
                Debug.Log("[AntiAtravessar] SEM CHECAGEM: o carro não tem Box Collider no mesmo objeto.");
        }
        else if (Mathf.Abs(passo) > 0.0001f)
        {
            float permitido = DistanciaPermitida(passo);
            float distancia = Mathf.Abs(passo);

            if (permitido < distancia)
            {
                float sinal = passo > 0f ? 1f : -1f;
                float excesso = distancia - permitido;

                // Puxa o carro de volta até encostar na parede
                transform.position -= transform.forward * sinal * excesso;
                bloqueouNesteFrame = true;

                if (mostrarLogs && (passo < 0f || logarFrente))
                {
                    Debug.Log("[AntiAtravessar] BLOQUEOU " + (passo > 0f ? "FRENTE" : "RÉ")
                              + " | objeto '" + ultimoObjetoBloqueado + "'"
                              + " | passo " + passo.ToString("F3")
                              + " | permitido " + permitido.ToString("F3")
                              + " | velocidadeAtual " + (carro != null ? carro.velocidadeAtual.ToString("F2") : "-"));
                }

                if (carro != null)
                {
                    if (passo > 0f)
                    {
                        // De frente e rápido: o sensor da mola faz o recuo
                        if (sensorMola != null && carro.velocidadeAtual > 2f)
                            sensorMola.AplicarBatida();

                        // Em qualquer batida de frente: garante o recuo mínimo
                        if (carro.velocidadeAtual > -recuoMinimo)
                            carro.velocidadeAtual = -recuoMinimo;
                    }
                    else
                    {
                        // De ré: recua para frente com o recuo mínimo
                        if (carro.velocidadeAtual < recuoMinimo)
                            carro.velocidadeAtual = recuoMinimo;
                    }
                }
            }
        }

        posicaoAnterior = transform.position;
    }

    // Quanto o carro pode andar nessa direção antes de bater numa parede
    float DistanciaPermitida(float passo)
    {
        float sinal = passo > 0f ? 1f : -1f;
        bool logar = mostrarLogs && (passo < 0f || logarFrente);
        string lado = passo > 0f ? "FRENTE" : "RÉ";
        Vector3 dir = transform.forward * sinal;
        float distancia = Mathf.Abs(passo);

        Vector3 escala = transform.lossyScale;
        float meiaLargura = caixa.size.x * Mathf.Abs(escala.x) * 0.5f;
        float meiaAltura = caixa.size.y * Mathf.Abs(escala.y) * 0.5f;
        float meioComprimento = caixa.size.z * Mathf.Abs(escala.z) * 0.5f;

        float baseY = -meiaAltura + alturaMinimaParede;
        float alturaSensor = meiaAltura - baseY;
        if (alturaSensor < 0.1f)
        {
            if (logar)
                Debug.Log("[AntiAtravessar] " + lado + " SEM CHECAGEM: alturaSensor=" + alturaSensor.ToString("F3") + " (menor que 0.1). Diminua 'Altura Minima Parede' ou aumente a altura do Box Collider.");
            return distancia;
        }

        float centroY = (baseY + meiaAltura) * 0.5f;

        // Posição do carro ANTES de mover (para lançar a caixa a partir dali)
        Vector3 centroAntes = transform.TransformPoint(caixa.center) - transform.forward * passo;
        Vector3 centro = centroAntes
                         + transform.up * centroY
                         + dir * (meioComprimento - 0.05f);
        Vector3 meiaCaixa = new Vector3(meiaLargura * 0.95f, alturaSensor * 0.5f, 0.05f);

        RaycastHit[] hits = Physics.BoxCastAll(centro, meiaCaixa, dir, transform.rotation,
                                               distancia, camadasObstaculos, QueryTriggerInteraction.Ignore);

        float menor = float.MaxValue;
        int ignoradosProprios = 0;
        int ignoradosChao = 0;
        int ignoradosNPC = 0;

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit h = hits[i];

            if (h.collider.transform.IsChildOf(transform))
            {
                ignoradosProprios++;
                if (logar)
                    Debug.Log("[AntiAtravessar] " + lado + " ignorou '" + h.collider.name + "': é do próprio carro.");
                continue;
            }

            if (ignorarNPCs && h.collider.GetComponentInParent<DoctorController>() != null)
            {
                ignoradosNPC++;
                if (logar)
                    Debug.Log("[AntiAtravessar] " + lado + " ignorou '" + h.collider.name + "': é um NPC (tem DoctorController).");
                continue;
            }

            if (h.normal.y > 0.5f)
            {
                ignoradosChao++;
                if (logar)
                    Debug.Log("[AntiAtravessar] " + lado + " ignorou '" + h.collider.name + "': superfície deitada (normal.y " + h.normal.y.ToString("F2") + "), tratada como chão.");
                continue;
            }

            if (logar)
                Debug.Log("[AntiAtravessar] " + lado + " VIU '" + h.collider.name + "' | distância " + h.distance.ToString("F3") + " | normal.y " + h.normal.y.ToString("F2"));

            if (h.distance < menor)
            {
                menor = h.distance;
                ultimoObjetoBloqueado = h.collider.name;
            }
        }

        if (menor == float.MaxValue)
        {
            if (logar && Time.time - tempoUltimoLogSemDeteccao >= intervaloLogSemDeteccao)
            {
                tempoUltimoLogSemDeteccao = Time.time;
                Debug.Log("[AntiAtravessar] " + lado + " NADA BLOQUEOU | passo " + distancia.ToString("F3")
                          + " | encontrados " + hits.Length + " (próprios " + ignoradosProprios + ", NPCs " + ignoradosNPC + ", chão " + ignoradosChao + ")"
                          + " | centro da caixa de detecção " + centro
                          + " | tamanho " + (meiaCaixa * 2f)
                          + " | velocidadeAtual " + (carro != null ? carro.velocidadeAtual.ToString("F2") : "-"));
            }
            return distancia;
        }

        return Mathf.Min(menor + 0.02f, distancia);
    }
}
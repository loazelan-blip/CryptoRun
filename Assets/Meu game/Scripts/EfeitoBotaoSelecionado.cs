using UnityEngine;
using UnityEngine.EventSystems;

public class EfeitoBotaoSelecionado : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler
{
    public float escalaAumentada = 1.2f;

    Vector3 escalaOriginal;
    bool iniciado;

    void Init()
    {
        if (iniciado) return;
        escalaOriginal = transform.localScale;
        iniciado = true;
    }

    void Awake() => Init();

    void OnDisable()
    {
        // devolve a escala ao fechar o painel
        if (iniciado) transform.localScale = escalaOriginal;
    }

    public void OnSelect(BaseEventData eventData)
    {
        Init();
        transform.localScale = escalaOriginal * escalaAumentada;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        Init();
        transform.localScale = escalaOriginal;
    }

    // mouse por cima = seleciona o botão, e aí o OnSelect faz o efeito
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(gameObject);
    }
}
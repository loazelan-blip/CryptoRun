using UnityEngine;
using UnityEngine.InputSystem;

public class ControleFumaca : MonoBehaviour
{
    public ParticleSystem fumaca;
    public InputActionReference acelerar;

    private ParticleSystem.EmissionModule emissionModule;
    private bool emitindo;

    void OnEnable()
    {
        if (acelerar != null) acelerar.action.Enable();
    }

    void Start()
    {
        if (fumaca != null)
        {
            emissionModule = fumaca.emission;
            emissionModule.enabled = false;
            if (!fumaca.isPlaying) fumaca.Play();
        }
    }

    void Update()
    {
        if (fumaca == null || acelerar == null) return;

        bool acelerando = acelerar.action.IsPressed();

        if (acelerando != emitindo)
        {
            emitindo = acelerando;
            emissionModule.enabled = emitindo;
        }
    }
}